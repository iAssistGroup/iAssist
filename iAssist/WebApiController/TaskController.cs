using iAssist.Models;
using iAssist.WebApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Data.Entity.Spatial;
using System.IO;
using iAssist.Hubs;

namespace iAssist.WebApiControllers
{
    [Authorize]
    [RoutePrefix("api/Task")]
    public class TaskController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private string _errorMessage = "An Error has occurred.";
        private string _errorMessageNotFound = "Corresponding Data not found.";
        private string _successMessage = "Success";


        public TaskController()
        {
        }

        public TaskController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? Request.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private ApplicationDbContext db = new ApplicationDbContext();

        public IEnumerable<JobListModel> GetJobList()
        {
            List<JobListModel> output = new List<JobListModel>();
            //var jobList = db.JobCategories.ToList();
            foreach (var data in db.JobCategories.ToList())
            {
                output.Add(new JobListModel { Id = data.Id, JobName = data.JobName });
            };
            return output;
        }

        public IEnumerable<SkillListModel> GetSkillList(int jobid)
        {
            List<SkillListModel> output = new List<SkillListModel>();

            foreach (var data in db.Skills.Where(x => x.Jobid == jobid).ToList())
            {
                output.Add(new SkillListModel { Id = data.Id, Skillname = data.Skillname });
            };
            return output;
        }

        [HttpGet]
        [Route("GetJobList")]
        public async Task<IHttpActionResult> GetJobListApi()
        {
            return Ok(GetJobList());
        }

        [HttpGet]
        [Route("GetSkillList")]
        public async Task<IHttpActionResult> GetSkillListApi(int id)
        {
            return Ok(GetSkillList(id));
        }

        // GET: Task
        //Creating Task in General or Posting Task
        [HttpGet]
        [Route("CreateTaskIndex")]
        public async Task<IHttpActionResult> CreateTaskIndex(int? jobid)
        {
            var user = User.Identity.GetUserId();
            var taskposting = new WebApiModels.TaskDetailsViewModel();
            taskposting.JobList = GetJobList();
            if(jobid != null)
            {
                taskposting.Address = db.Locations.Where(x => x.UserId == user && x.JobId == null).Select(p => p.Loc_Address).FirstOrDefault();
                taskposting.Longitude = db.Locations.Where(x => x.UserId == user && x.JobId == null).Select(p => p.Geolocation.Longitude.ToString()).FirstOrDefault();
                taskposting.Latitude = db.Locations.Where(x => x.UserId == user && x.JobId == null).Select(p => p.Geolocation.Latitude.ToString()).FirstOrDefault();
                taskposting.SkillList = GetSkillList((int)jobid);
                taskposting.JobId = (int)jobid;
            }
            return Ok(taskposting);
        }

        [Authorize]
        [HttpPost]
        [Route("CreateTaskIndex")]
        public async Task<IHttpActionResult> CreateTaskIndex(WebApiModels.TaskDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var checkdate = model.taskdet_sched.ToString();
                if (checkdate == "1/1/0001 12:00:00 AM")
                {
                    return BadRequest("Please Fill up the form correctly");
                }
                var taskbook = new Task_Book();
                var user = User.Identity.GetUserId();
                var userposttask = new TaskDetails();
                if (model.ImageFile != null && ValidateFile(model.ImageFile) == true)
                {
                    string filename = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                    string extension = Path.GetExtension(model.ImageFile.FileName);
                    filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                    model.TaskImage = "" + filename;
                    filename = ConstantVariables.BaseURL + "image/" + filename;
                    userposttask.TaskImage = model.TaskImage;
                    model.ImageFile.SaveAs(filename);
                }
                userposttask.JobId = model.JobId;
                model.JobList = GetJobList();
                userposttask.Loc_Address = model.Address;
                userposttask.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
                userposttask.taskdet_name = model.TaskTitle;
                userposttask.taskdet_desc = model.TaskDesc;
                userposttask.taskdet_sched = model.taskdet_sched;
                model.taskdet_Created_at = DateTime.Now;
                userposttask.taskdet_Created_at = model.taskdet_Created_at;
                userposttask.taskdet_Updated_at = userposttask.taskdet_Created_at;
                userposttask.UserId = user;
                db.TaskDetails.Add(userposttask);
                db.SaveChanges();
                var taskdetid = db.TaskDetails.OrderByDescending(p => p.taskdet_Created_at).First();
                var check = db.TaskBook.Find(taskdetid.Id);
                if (check == null)
                {
                    taskbook.TaskDetId = taskdetid.Id;
                }
                taskbook.Taskbook_Created_at = userposttask.taskdet_Created_at;
                taskbook.Taskbook_Updated_at = userposttask.taskdet_Created_at;
                taskbook.Taskbook_Status = 0;
                db.TaskBook.Add(taskbook);
                db.SaveChanges();
                if (model.SelectedSkills != null)
            {
                    foreach(string data in model.SelectedSkills)
                    {
                        var skillofservice = new SkillServiceTask
                        {
                            Jobid = model.JobId,
                            Skillname = data,
                            UserId = model.UserId,
                            Taskdet = taskdetid.Id
                        };
                        db.SkillServiceTasks.Add(skillofservice);
                        db.SaveChanges();
                    }
                }
                return Ok("Task Created Successfully");
            }
            return BadRequest("Please Fill up the form correctly");
        }
        //Show User TaskPost
        [Authorize]
        [HttpGet]
        [Route("ShowMyTaskPost")]
        public async Task<IHttpActionResult> ShowMyTaskPost(string category)
        {
            var user = User.Identity.GetUserId();
                var taskpostlist = (from u in db.TaskDetails
                                    where u.UserId == user
                                    join
                                    tu in db.TaskBook on u.Id equals tu.TaskDetId
                                    join
                                    job in db.JobCategories on u.JobId equals job.Id
                                    orderby u.taskdet_Created_at descending
                                    select new
                                    {
                                        id = u.Id,
                                        taskbookstatus = tu.Taskbook_Status,
                                        taskname = u.taskdet_name,
                                        taskdesc = u.taskdet_desc,
                                        tasktype = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskType).FirstOrDefault(),
                                        tasksched = u.taskdet_sched,
                                        taskimage = u.TaskImage,
                                        taskaddress = u.Loc_Address,
                                        jobname = job.JobName,
                                        jobid = job.Id,
                                        userid = u.UserId,
                                        specificworkerid = tu.workerId,
                                        workerid = (from td in db.Taskeds where td.TaskDetId == u.Id select td.WorkerId).FirstOrDefault(),
                                        taskedstatus = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskStatus).FirstOrDefault(),
                                        taskedTaskPayable = (from tp in db.Taskeds where tp.TaskDetId == u.Id select tp.TaskPayable).FirstOrDefault(),
                                        taskedWorkerfname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Firstname).FirstOrDefault(),
                                        taskedWorkerlname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Lastname).FirstOrDefault(),
                                    }).ToList().Select(p => new TaskPostListView
                                    {
                                        Id = p.id,
                                        Taskbook_Status = p.taskbookstatus,
                                        taskdet_name = p.taskname,
                                        taskdet_desc = p.taskdesc,
                                        taskdet_sched = p.tasksched,
                                        TaskImage = p.taskimage,
                                        Loc_Address = p.taskaddress,
                                        jobid = p.jobid,
                                        Jobname = p.jobname,
                                        UserId = p.userid,
                                        taskedWorkerfname = p.taskedWorkerfname,
                                        taskedWorkerlname = p.taskedWorkerlname,
                                        taskedstatus = p.taskedstatus,
                                        specificworkerid = p.specificworkerid,
                                        workerid = p.workerid,
                                        taskedTaskPayable = p.taskedTaskPayable,
                                        Tasktype = p.tasktype,
                                    });
            if(!String.IsNullOrEmpty(category))
            {
                if(category == "Pending")
                {
                    taskpostlist = (from u in db.TaskDetails
                                        where u.UserId == user
                                        join
                                        tu in db.TaskBook on u.Id equals tu.TaskDetId where tu.Taskbook_Status == 0
                                        join
                                        job in db.JobCategories on u.JobId equals job.Id
                                        orderby u.taskdet_Created_at descending
                                        select new
                                        {
                                            id = u.Id,
                                            taskbookstatus = tu.Taskbook_Status,
                                            taskname = u.taskdet_name,
                                            taskdesc = u.taskdet_desc,
                                            tasktype = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskType).FirstOrDefault(),
                                            tasksched = u.taskdet_sched,
                                            taskimage = u.TaskImage,
                                            taskaddress = u.Loc_Address,
                                            jobname = job.JobName,
                                            jobid = job.Id,
                                            userid = u.UserId,
                                            specificworkerid = tu.workerId,
                                            workerid = (from td in db.Taskeds where td.TaskDetId == u.Id select td.WorkerId).FirstOrDefault(),
                                            taskedstatus = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskStatus).FirstOrDefault(),
                                            taskedTaskPayable = (from tp in db.Taskeds where tp.TaskDetId == u.Id select tp.TaskPayable).FirstOrDefault(),
                                            taskedWorkerfname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Firstname).FirstOrDefault(),
                                            taskedWorkerlname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Lastname).FirstOrDefault(),
                                        }).ToList().Select(p => new TaskPostListView
                                        {
                                            Id = p.id,
                                            Taskbook_Status = p.taskbookstatus,
                                            taskdet_name = p.taskname,
                                            taskdet_desc = p.taskdesc,
                                            taskdet_sched = p.tasksched,
                                            TaskImage = p.taskimage,
                                            Loc_Address = p.taskaddress,
                                            jobid = p.jobid,
                                            Jobname = p.jobname,
                                            UserId = p.userid,
                                            taskedWorkerfname = p.taskedWorkerfname,
                                            taskedWorkerlname = p.taskedWorkerlname,
                                            taskedstatus = p.taskedstatus,
                                            specificworkerid = p.specificworkerid,
                                            workerid = p.workerid,
                                            taskedTaskPayable = p.taskedTaskPayable,
                                            Tasktype = p.tasktype,
                                        });
                }
                if(category == "Posted")
                {
                    taskpostlist = (from u in db.TaskDetails
                                    where u.UserId == user
                                    join
                                    tu in db.TaskBook on u.Id equals tu.TaskDetId
                                    where tu.Taskbook_Status == 1
                                    join
                                    job in db.JobCategories on u.JobId equals job.Id
                                    orderby u.taskdet_Created_at descending
                                    select new
                                    {
                                        id = u.Id,
                                        taskbookstatus = tu.Taskbook_Status,
                                        taskname = u.taskdet_name,
                                        taskdesc = u.taskdet_desc,
                                        tasktype = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskType).FirstOrDefault(),
                                        tasksched = u.taskdet_sched,
                                        taskimage = u.TaskImage,
                                        taskaddress = u.Loc_Address,
                                        jobname = job.JobName,
                                        jobid = job.Id,
                                        userid = u.UserId,
                                        specificworkerid = tu.workerId,
                                        workerid = (from td in db.Taskeds where td.TaskDetId == u.Id select td.WorkerId).FirstOrDefault(),
                                        taskedstatus = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskStatus).FirstOrDefault(),
                                        taskedTaskPayable = (from tp in db.Taskeds where tp.TaskDetId == u.Id select tp.TaskPayable).FirstOrDefault(),
                                        taskedWorkerfname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Firstname).FirstOrDefault(),
                                        taskedWorkerlname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Lastname).FirstOrDefault(),
                                    }).ToList().Select(p => new TaskPostListView
                                    {
                                        Id = p.id,
                                        Taskbook_Status = p.taskbookstatus,
                                        taskdet_name = p.taskname,
                                        taskdet_desc = p.taskdesc,
                                        taskdet_sched = p.tasksched,
                                        TaskImage = p.taskimage,
                                        Loc_Address = p.taskaddress,
                                        jobid = p.jobid,
                                        Jobname = p.jobname,
                                        UserId = p.userid,
                                        taskedWorkerfname = p.taskedWorkerfname,
                                        taskedWorkerlname = p.taskedWorkerlname,
                                        taskedstatus = p.taskedstatus,
                                        specificworkerid = p.specificworkerid,
                                        workerid = p.workerid,
                                        taskedTaskPayable = p.taskedTaskPayable,
                                        Tasktype = p.tasktype,
                                    });
                }
                if(category == "Ongoing")
                {
                    taskpostlist = (from u in db.TaskDetails
                                    where u.UserId == user
                                    join
                                    tu in db.TaskBook on u.Id equals tu.TaskDetId
                                    where tu.Taskbook_Status == 2
                                    join
                                    job in db.JobCategories on u.JobId equals job.Id
                                    orderby u.taskdet_Created_at descending
                                    select new
                                    {
                                        id = u.Id,
                                        taskbookstatus = tu.Taskbook_Status,
                                        taskname = u.taskdet_name,
                                        taskdesc = u.taskdet_desc,
                                        tasktype = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskType).FirstOrDefault(),
                                        tasksched = u.taskdet_sched,
                                        taskimage = u.TaskImage,
                                        taskaddress = u.Loc_Address,
                                        jobname = job.JobName,
                                        jobid = job.Id,
                                        userid = u.UserId,
                                        specificworkerid = tu.workerId,
                                        workerid = (from td in db.Taskeds where td.TaskDetId == u.Id select td.WorkerId).FirstOrDefault(),
                                        taskedstatus = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskStatus).FirstOrDefault(),
                                        taskedTaskPayable = (from tp in db.Taskeds where tp.TaskDetId == u.Id select tp.TaskPayable).FirstOrDefault(),
                                        taskedWorkerfname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Firstname).FirstOrDefault(),
                                        taskedWorkerlname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Lastname).FirstOrDefault(),
                                    }).ToList().Select(p => new TaskPostListView
                                    {
                                        Id = p.id,
                                        Taskbook_Status = p.taskbookstatus,
                                        taskdet_name = p.taskname,
                                        taskdet_desc = p.taskdesc,
                                        taskdet_sched = p.tasksched,
                                        TaskImage = p.taskimage,
                                        Loc_Address = p.taskaddress,
                                        jobid = p.jobid,
                                        Jobname = p.jobname,
                                        UserId = p.userid,
                                        taskedWorkerfname = p.taskedWorkerfname,
                                        taskedWorkerlname = p.taskedWorkerlname,
                                        taskedstatus = p.taskedstatus,
                                        specificworkerid = p.specificworkerid,
                                        workerid = p.workerid,
                                        taskedTaskPayable = p.taskedTaskPayable,
                                        Tasktype = p.tasktype,
                                    });
                }
                if(category == "Completed")
                {
                    taskpostlist = (from u in db.TaskDetails
                                    where u.UserId == user
                                    join
                                    tu in db.TaskBook on u.Id equals tu.TaskDetId
                                    where tu.Taskbook_Status == 3
                                    join
                                    job in db.JobCategories on u.JobId equals job.Id
                                    orderby u.taskdet_Created_at descending
                                    select new
                                    {
                                        id = u.Id,
                                        taskbookstatus = tu.Taskbook_Status,
                                        taskname = u.taskdet_name,
                                        taskdesc = u.taskdet_desc,
                                        tasktype = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskType).FirstOrDefault(),
                                        tasksched = u.taskdet_sched,
                                        taskimage = u.TaskImage,
                                        taskaddress = u.Loc_Address,
                                        jobname = job.JobName,
                                        jobid = job.Id,
                                        userid = u.UserId,
                                        specificworkerid = tu.workerId,
                                        workerid = (from td in db.Taskeds where td.TaskDetId == u.Id select td.WorkerId).FirstOrDefault(),
                                        taskedstatus = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskStatus).FirstOrDefault(),
                                        taskedTaskPayable = (from tp in db.Taskeds where tp.TaskDetId == u.Id select tp.TaskPayable).FirstOrDefault(),
                                        taskedWorkerfname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Firstname).FirstOrDefault(),
                                        taskedWorkerlname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Lastname).FirstOrDefault(),
                                    }).ToList().Select(p => new TaskPostListView
                                    {
                                        Id = p.id,
                                        Taskbook_Status = p.taskbookstatus,
                                        taskdet_name = p.taskname,
                                        taskdet_desc = p.taskdesc,
                                        taskdet_sched = p.tasksched,
                                        TaskImage = p.taskimage,
                                        Loc_Address = p.taskaddress,
                                        jobid = p.jobid,
                                        Jobname = p.jobname,
                                        UserId = p.userid,
                                        taskedWorkerfname = p.taskedWorkerfname,
                                        taskedWorkerlname = p.taskedWorkerlname,
                                        taskedstatus = p.taskedstatus,
                                        specificworkerid = p.specificworkerid,
                                        workerid = p.workerid,
                                        taskedTaskPayable = p.taskedTaskPayable,
                                        Tasktype = p.tasktype,
                                    });
                }
                if(category == "Cancelled")
                {
                    taskpostlist = (from u in db.TaskDetails
                                    where u.UserId == user
                                    join
                                    tu in db.TaskBook on u.Id equals tu.TaskDetId
                                    where tu.Taskbook_Status != 1 && tu.Taskbook_Status != 2 && tu.Taskbook_Status != 3 && tu.Taskbook_Status != 0
                                    join
                                    job in db.JobCategories on u.JobId equals job.Id
                                    orderby u.taskdet_Created_at descending
                                    select new
                                    {
                                        id = u.Id,
                                        taskbookstatus = tu.Taskbook_Status,
                                        taskname = u.taskdet_name,
                                        taskdesc = u.taskdet_desc,
                                        tasktype = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskType).FirstOrDefault(),
                                        tasksched = u.taskdet_sched,
                                        taskimage = u.TaskImage,
                                        taskaddress = u.Loc_Address,
                                        jobname = job.JobName,
                                        jobid = job.Id,
                                        userid = u.UserId,
                                        specificworkerid = tu.workerId,
                                        workerid = (from td in db.Taskeds where td.TaskDetId == u.Id select td.WorkerId).FirstOrDefault(),
                                        taskedstatus = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskStatus).FirstOrDefault(),
                                        taskedTaskPayable = (from tp in db.Taskeds where tp.TaskDetId == u.Id select tp.TaskPayable).FirstOrDefault(),
                                        taskedWorkerfname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Firstname).FirstOrDefault(),
                                        taskedWorkerlname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Lastname).FirstOrDefault(),
                                    }).ToList().Select(p => new TaskPostListView
                                    {
                                        Id = p.id,
                                        Taskbook_Status = p.taskbookstatus,
                                        taskdet_name = p.taskname,
                                        taskdet_desc = p.taskdesc,
                                        taskdet_sched = p.tasksched,
                                        TaskImage = p.taskimage,
                                        Loc_Address = p.taskaddress,
                                        jobid = p.jobid,
                                        Jobname = p.jobname,
                                        UserId = p.userid,
                                        taskedWorkerfname = p.taskedWorkerfname,
                                        taskedWorkerlname = p.taskedWorkerlname,
                                        taskedstatus = p.taskedstatus,
                                        specificworkerid = p.specificworkerid,
                                        workerid = p.workerid,
                                        taskedTaskPayable = p.taskedTaskPayable,
                                        Tasktype = p.tasktype,
                                    });
                }
            }
            var taskpostview = new taskViewPost();
            taskpostview.Taskpostlistview = taskpostlist;
            taskpostview.TaskViewPost = db.SkillServiceTasks.ToList();
            return Ok(taskpostview);
        }
        
        public List<ShowposttaskcategoryViewModel> TaskCategoryInit()
        {
            List<ShowposttaskcategoryViewModel> categor = new List<ShowposttaskcategoryViewModel>();
            var cat = new ShowposttaskcategoryViewModel();
            cat.CategoryName = "Pending";
            cat.Id = 0;
            categor.Add(cat);
            cat = new ShowposttaskcategoryViewModel();
            cat.CategoryName = "Posted";
            cat.Id = 1;
            categor.Add(cat);
            cat = new ShowposttaskcategoryViewModel();
            cat.CategoryName = "Ongoing";
            cat.Id = 2;
            categor.Add(cat);
            cat = new ShowposttaskcategoryViewModel();
            cat.CategoryName = "Completed";
            cat.Id = 3;
            categor.Add(cat);
            cat = new ShowposttaskcategoryViewModel();
            cat.CategoryName = "Cancelled";
            cat.Id = 4;
            categor.Add(cat);
            return categor;
        }
        //Posting the task
        [HttpGet]
        [Route("PostTheTask")]
        public async Task<IHttpActionResult> PostTheTask(int? id)
        {
            if (id == null)
            {
                return BadRequest(_errorMessage);
            }
            Task_Book taskbook = db.TaskBook.Where(x => x.TaskDetId == id).FirstOrDefault();
            if (taskbook == null)
            {
                return BadRequest(_errorMessageNotFound);
            }
            taskbook.Taskbook_Status = 1;
            db.SaveChanges();
            return Ok("Post Task Successful.");
        }
        //Edit User TaskPost
        [HttpGet]
        [Route("EditMyPostTask")]
        public async Task<IHttpActionResult> EditMyPostTask(int? id)
        {
            if (id == null)
            {
                return BadRequest(_errorMessage);
            }
            TaskDetails taskdetails = db.TaskDetails.Where(x => x.Id == id).FirstOrDefault();
            if (taskdetails == null)
            {
                return BadRequest(_errorMessageNotFound);
            }
            var service = (from st in db.SkillServiceTasks where st.Taskdet == taskdetails.Id select st.Skillname).ToList();
            var taskpostlist = new WebApiModels.TaskDetailsViewModel();
            taskpostlist.Address = taskdetails.Loc_Address;
            taskpostlist.TaskImage = taskdetails.TaskImage;
            taskpostlist.Longitude = taskdetails.Geolocation.Longitude.ToString();
            taskpostlist.Latitude = taskdetails.Geolocation.Latitude.ToString();
            taskpostlist.TaskTitle = taskdetails.taskdet_name;
            taskpostlist.taskdet_sched = taskdetails.taskdet_sched;
            taskpostlist.JobId = taskdetails.JobId;
            taskpostlist.TaskDesc = taskdetails.taskdet_desc;
            taskpostlist.taskdet_Created_at = taskdetails.taskdet_Created_at;
            taskpostlist.UserId = taskdetails.UserId;
            taskpostlist.JobList = GetJobList();
            taskpostlist.SkillList = GetSkillList((int)id);
            return Ok(taskpostlist);
        }
        [HttpPost]
        [Route("EditMyPostTask")]
        public async Task<IHttpActionResult> EditMyPostTask(WebApiModels.TaskDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var taskbook = new Task_Book();
                var userposttask = db.TaskDetails.Where(x=>x.Id == model.Id).FirstOrDefault();
                if (model.ImageFile != null && ValidateFile(model.ImageFile) == true)
                {
                    string filename = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                    string extension = Path.GetExtension(model.ImageFile.FileName);
                    filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                    model.TaskImage = "" + filename;
                    filename = ConstantVariables.BaseURL + "image/" + filename;
                    userposttask.TaskImage = model.TaskImage;
                    model.ImageFile.SaveAs(filename);
                }
                userposttask.JobId = model.JobId;
                model.JobList = GetJobList();
                userposttask.Loc_Address = model.Address;
                userposttask.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
                userposttask.taskdet_name = model.TaskTitle;
                userposttask.taskdet_desc = model.TaskDesc;
                userposttask.taskdet_sched = model.taskdet_sched;
                userposttask.taskdet_Updated_at = DateTime.Now;

                if (model.SelectedSkills != null)
                {
                    var a = db.SkillServiceTasks.Where(x => x.Taskdet == userposttask.Id).ToList();
                    db.SkillServiceTasks.RemoveRange(a);
                    db.SaveChanges();
                    foreach (string data in model.SelectedSkills)
                    {
                        var skillofservice = new SkillServiceTask
                        {
                            Jobid = model.JobId,
                            Skillname = data,
                            UserId = model.UserId,
                            Taskdet = userposttask.Id
                        };
                        db.SkillServiceTasks.Add(skillofservice);
                        db.SaveChanges();
                    }
                }
                db.SaveChanges();
                return Ok(_successMessage);
            }
            return BadRequest("Please Fill up the form correctly");
        }
        //Cancel User TaskPost
        [HttpGet]
        [Route("CancelMyPostTask")]
        public async Task<IHttpActionResult> CancelMyPostTask(int? id,int? cancel)
        {
            if(cancel == 0)
            {
                if (id == null)
                {
                    return BadRequest(_errorMessage);
                }
                Task_Book taskbook = db.TaskBook.Where(x => x.TaskDetId == id).FirstOrDefault();
                if (taskbook == null)
                {
                    return BadRequest(_errorMessageNotFound);
                }
                Tasked taskeds = db.Taskeds.Where(x => x.TaskDetId == id).FirstOrDefault();
                if(taskeds != null && taskeds.TaskStatus != 4 && taskeds.TaskStatus != 3 && taskeds.TaskStatus != 2)
                {
                    taskeds.TaskStatus = 0;
                }
                if(taskbook.Taskbook_Status == 1)
                {
                    var bid = db.Bids.Where(x => x.TaskDetId == taskbook.TaskDetId).ToList();
                    if(bid != null)
                    {
                        db.Bids.RemoveRange(bid);
                        db.SaveChanges();
                    }
                    taskbook.Taskbook_Status = 0;
                    taskbook.workerId = 0;
                    db.SaveChanges();
                    return Ok(_successMessage);
                }
                taskbook.Taskbook_Status = 4;
                db.SaveChanges();
                return Ok(_successMessage);
            }
            if (cancel == 1)
            {
                if (id == null)
                {
                    return BadRequest(_errorMessage);
                }
                Task_Book taskbook = db.TaskBook.Where(x => x.TaskDetId == id).FirstOrDefault();
                if (taskbook == null)
                {
                    return BadRequest(_errorMessageNotFound);
                }
                taskbook.Taskbook_Status = 5;
                taskbook.workerId = 0;
                db.SaveChanges();
                Tasked taskeds = db.Taskeds.Where(x => x.TaskDetId == id).FirstOrDefault();
                if (taskeds != null && taskeds.TaskStatus != 4 && taskeds.TaskStatus != 3 && taskeds.TaskStatus != 2)
                {
                    taskeds.TaskStatus = 0;
                    db.SaveChanges();
                }
                var taskdet = db.TaskDetails.Where(x => x.Id == id).FirstOrDefault();
                var user = db.Users.Where(x => x.Id == taskdet.UserId).FirstOrDefault();

                var notification = new NotificationModel
                {
                    Receiver = user.UserName,
                    Title = $"Worker cancell your task posted",
                    Details = $"The Worker cancel the task you posted",
                    DetailsURL = $"/Task/ShowMyTaskPost",
                    Date = DateTime.Now,
                    IsRead = false
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                return Ok(_successMessage);
            }
            return BadRequest(_errorMessage);
        }
        //Request Booking
        [HttpGet]
        [Route("RequestBooking")]
        public async Task<IHttpActionResult> RequestBooking(int? inWorkerid, int? jobid)
        {
            if(inWorkerid == null || jobid == null)
            {
                return BadRequest(_errorMessage);
            }
            var userid = User.Identity.GetUserId();
            var workerid = db.RegistWork.Where(x => x.Userid == userid).FirstOrDefault();
            if(workerid != null && inWorkerid == workerid.Id)
            {
                return BadRequest(_errorMessage);
            }
            var taskposting = new WebApiModels.TaskDetailsViewModel();
            taskposting.workerid = inWorkerid;
            taskposting.JobId = (int)jobid;
            taskposting.JobList = GetJobList();
            taskposting.Address = db.Locations.Where(x => x.UserId == userid && x.JobId == null).Select(p => p.Loc_Address).FirstOrDefault();
            taskposting.Longitude = db.Locations.Where(x => x.UserId == userid && x.JobId == null).Select(p => p.Geolocation.Longitude.ToString()).FirstOrDefault();
            taskposting.Latitude = db.Locations.Where(x => x.UserId == userid && x.JobId == null).Select(p => p.Geolocation.Latitude.ToString()).FirstOrDefault();
            taskposting.SkillList = GetSkillList((int)jobid);
            return Ok(taskposting);
        }
        [HttpPost]
        [Route("RequestBooking")]
        public async Task<IHttpActionResult> RequestBooking(WebApiModels.TaskDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var taskbook = new Task_Book();
                var user = User.Identity.GetUserId();
                var username = db.Users.Where(x => x.Id == user).FirstOrDefault();
                var userposttask = new TaskDetails();
                if (model.ImageFile != null && ValidateFile(model.ImageFile) == true)
                {
                    string filename = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                    string extension = Path.GetExtension(model.ImageFile.FileName);
                    filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                    model.TaskImage = "" + filename;
                    filename = ConstantVariables.BaseURL + "image/" + filename;
                    userposttask.TaskImage = model.TaskImage;
                    model.ImageFile.SaveAs(filename);
                }
                userposttask.JobId = model.JobId;
                model.JobList = GetJobList();
                userposttask.Loc_Address = model.Address;
                userposttask.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
                userposttask.taskdet_name = model.TaskTitle;
                userposttask.taskdet_desc = model.TaskDesc;
                userposttask.taskdet_sched = model.taskdet_sched;
                model.taskdet_Created_at = DateTime.Now;
                userposttask.taskdet_Created_at = model.taskdet_Created_at;
                userposttask.taskdet_Updated_at = userposttask.taskdet_Created_at;
                userposttask.UserId = user;
                db.TaskDetails.Add(userposttask);
                db.SaveChanges();
                var taskdetid = db.TaskDetails.OrderByDescending(p => p.taskdet_Created_at).First();
                var check = db.TaskBook.Find(taskdetid.Id);
                if (check == null)
                {
                    taskbook.TaskDetId = taskdetid.Id;
                }
                taskbook.Taskbook_Created_at = userposttask.taskdet_Created_at;
                taskbook.Taskbook_Updated_at = userposttask.taskdet_Created_at;
                taskbook.Taskbook_Status = 1;
                if(model.workerid != null)
                {
                    taskbook.workerId = model.workerid;
                }
                db.TaskBook.Add(taskbook);
                db.SaveChanges();

                if (model.SelectedSkills != null)
                {
                    foreach (string data in model.SelectedSkills)
                    {
                        var skillofservice = new SkillServiceTask
                        {
                            Jobid = model.JobId,
                            Skillname = data,
                            UserId = model.UserId,
                            Taskdet = taskdetid.Id
                    };
                        db.SkillServiceTasks.Add(skillofservice);
                        db.SaveChanges();
                    }
                }
                var userworkid = db.RegistWork.Where(x => x.Id == model.workerid).FirstOrDefault();
                var usernameid = db.Users.Where(x => x.Id == userworkid.Userid).FirstOrDefault();
                var notification = new NotificationModel
                {
                    Receiver = usernameid.UserName,
                    Title = $" {username.UserName} Request a book on you",
                    Details = $"{username.UserName} Requesting a book on you. You need To check it.",
                    DetailsURL = "",
                    Date = DateTime.Now,
                    IsRead = false
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                NotificationHub objNotifHub = new NotificationHub();
                objNotifHub.SendNotification(notification.Receiver);
                return Ok(_successMessage);
            }
            return BadRequest("Please Fill up the form correctly");
        }

        [HttpGet]
        [Route("FindWorkerRequestBooking")]
        public async Task<IHttpActionResult> FindWorkerRequestBooking(int? id, int? task)
        {
            if (id == null)
            {
                return BadRequest("_errorMessage");
            }
            var tasker = db.TaskBook.Where(x => x.TaskDetId == task).FirstOrDefault();
            tasker.workerId = id;
            tasker.Taskbook_Status = 1;
            db.SaveChanges();
            var userworkid = db.RegistWork.Where(x => x.Id == tasker.workerId).FirstOrDefault();
            var user = User.Identity.GetUserId();
            var username = db.Users.Where(x => x.Id == user).FirstOrDefault();
            var usernameid = db.Users.Where(x => x.Id == userworkid.Userid).FirstOrDefault();
            var notification = new NotificationModel
            {
                Receiver = usernameid.UserName,
                Title = $" {username.UserName} Request a book on you",
                Details = $"{username.UserName} Requesting a book on you. You need To check it.",
                DetailsURL = "",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
            return Ok("ShowMyTaskPost");
        }



        //User Requested Tasks
        [Authorize(Roles = "Worker")]
        [HttpGet]
        [Route("ViewUserRequestedTask")]
        public async Task<IHttpActionResult> ViewUserRequestedTask()
        {
            //Note para di ko kalimot: echeck ni if ang bid is nana or wala pa.. ang bidded ID basihan sa worker
            var user = User.Identity.GetUserId();
            var work = db.RegistWork.Where(x => x.Userid == user).FirstOrDefault();
            var worker = db.Locations.Where(x => x.UserId == user).FirstOrDefault();
            var currentlocation = DbGeography.FromText("POINT( " + worker.Geolocation.Longitude + " " + worker.Geolocation.Latitude + " )");
            var taskpostlist1 = (from u in db.TaskDetails
                                where u.UserId != user && u.JobId == work.JobId
                                join d in db.Users on u.UserId equals d.Id
                                join
                                tu in db.TaskBook on u.Id equals tu.TaskDetId
                                where tu.Taskbook_Status == 1
                                join
                                job in db.JobCategories on u.JobId equals job.Id
                                orderby u.Geolocation.Distance(currentlocation)
                                select new
                                {
                                    id = u.Id,
                                    taskbookstatus = tu.Taskbook_Status,
                                    taskname = u.taskdet_name,
                                    taskdesc = u.taskdet_desc,
                                    tasksched = u.taskdet_sched,
                                    taskimage = u.TaskImage,
                                    taskaddress = u.Loc_Address,
                                    jobname = job.JobName,
                                    userid = u.UserId,
                                    username = d.UserName,
                                    workerid = tu.workerId
                                }).ToList().Select(p => new TaskPostListView
                                {
                                    Id = p.id,
                                    Taskbook_Status = p.taskbookstatus,
                                    taskdet_name = p.taskname,
                                    taskdet_desc = p.taskdesc,
                                    taskdet_sched = p.tasksched,
                                    TaskImage = p.taskimage,
                                    Loc_Address = p.taskaddress,
                                    Jobname = p.jobname,
                                    UserId = p.userid,
                                    Username = p.username,
                                    workerid = p.workerid,
                                });
            var taskpostlist2 = (from u in db.TaskDetails
                                where u.UserId != user
                                join d in db.Users on u.UserId equals d.Id
                                join
                                tu in db.TaskBook on u.Id equals tu.TaskDetId
                                where tu.Taskbook_Status == 1
                                join
                                bu in db.Bids on u.Id equals bu.TaskDetId
                                 where bu.WorkerId == work.Id && bu.bid_status != 1 && bu.bid_status != 2
                                 join
                                job in db.JobCategories on u.JobId equals job.Id
                                orderby u.Geolocation.Distance(currentlocation)
                                select new
                                {
                                    id = u.Id,
                                    taskbookstatus = tu.Taskbook_Status,
                                    taskname = u.taskdet_name,
                                    taskdesc = u.taskdet_desc,
                                    tasksched = u.taskdet_sched,
                                    taskimage = u.TaskImage,
                                    taskaddress = u.Loc_Address,
                                    jobname = job.JobName,
                                    userid = u.UserId,
                                    username = d.UserName,
                                    workerid = tu.workerId
                                }).ToList().Select(p => new TaskPostListView
                                {
                                    Id = p.id,
                                    Taskbook_Status = p.taskbookstatus,
                                    taskdet_name = p.taskname,
                                    taskdet_desc = p.taskdesc,
                                    taskdet_sched = p.tasksched,
                                    TaskImage = p.taskimage,
                                    Loc_Address = p.taskaddress,
                                    Jobname = p.jobname,
                                    UserId = p.userid,
                                    Username = p.username,
                                    workerid = p.workerid,
                                });
            List<TaskPostListView> filteredtaskpostlist = (from e in taskpostlist1 where !(from m in taskpostlist2 select m.Id).Contains(e.Id) select e).ToList();
            var taskpostview = new taskViewPost();
            taskpostview.Taskpostlistview = filteredtaskpostlist;
            taskpostview.TaskViewPost = db.SkillServiceTasks.ToList();
            return Ok(taskpostview);
        }
        [Authorize(Roles = "Worker")]
        [HttpGet]
        [Route("ViewBiddedRequestTask")]
        public async Task<IHttpActionResult> ViewBiddedRequestTask()
        {
            //Note para di ko kalimot: echeck ni if ang bid is nana or wala pa.. ang bidded ID basihan sa worker
            var user = User.Identity.GetUserId();
            var work = db.RegistWork.Where(x => x.Userid == user).FirstOrDefault();
            var worker = db.Locations.Where(x => x.UserId == user).FirstOrDefault();
            var currentlocation = DbGeography.FromText("POINT( " + worker.Geolocation.Longitude + " " + worker.Geolocation.Latitude + " )");
            var taskpostlist = (from u in db.TaskDetails
                                where u.UserId != user && u.JobId == work.JobId
                                join d in db.Users on u.UserId equals d.Id
                                join
                                tu in db.TaskBook on u.Id equals tu.TaskDetId
                                where tu.Taskbook_Status == 1
                                join
                                bu in db.Bids on u.Id equals bu.TaskDetId
                                where bu.WorkerId == work.Id && bu.bid_status != 1 && bu.bid_status != 2
                                join
                                job in db.JobCategories on u.JobId equals job.Id
                                orderby bu.Created_at descending
                                select new
                                {
                                    id = u.Id,
                                    taskbookstatus = tu.Taskbook_Status,
                                    taskname = u.taskdet_name,
                                    taskdesc = u.taskdet_desc,
                                    tasksched = u.taskdet_sched,
                                    taskimage = u.TaskImage,
                                    taskaddress = u.Loc_Address,
                                    jobname = job.JobName,
                                    userid = u.UserId,
                                    username = d.UserName,
                                    workerid = tu.workerId
                                }).ToList().Select(p => new TaskPostListView
                                {
                                    Id = p.id,
                                    Taskbook_Status = p.taskbookstatus,
                                    taskdet_name = p.taskname,
                                    taskdet_desc = p.taskdesc,
                                    taskdet_sched = p.tasksched,
                                    TaskImage = p.taskimage,
                                    Loc_Address = p.taskaddress,
                                    Jobname = p.jobname,
                                    UserId = p.userid,
                                    Username = p.username,
                                    workerid = p.workerid,
                                });
            var taskpostview = new taskViewPost();
            taskpostview.Taskpostlistview = taskpostlist;
            taskpostview.TaskViewPost = db.SkillServiceTasks.ToList();
            return Ok(taskpostview);
        }
        [Authorize(Roles = "Worker")]
        [HttpGet]
        [Route("ViewContractTask")]
        public async Task<IHttpActionResult> ViewContractTask()
        {
            var user = User.Identity.GetUserId();
            var work = db.RegistWork.Where(x => x.Userid == user).FirstOrDefault();
            var worker = db.Locations.Where(x => x.UserId == user).FirstOrDefault();
                var taskpostlist = (from u in db.TaskDetails
                                    where u.UserId != user && u.JobId == work.JobId
                                    join d in db.Users on u.UserId equals d.Id
                                    join
                                    tu in db.TaskBook on u.Id equals tu.TaskDetId
                                    where tu.Taskbook_Status != 1 && tu.Taskbook_Status != 0 && tu.Taskbook_Status != 3
                                    join
                                    au in db.Taskeds on u.Id equals au.TaskDetId where au.WorkerId == work.Id
                                    join
                                    job in db.JobCategories on u.JobId equals job.Id
                                    orderby u.taskdet_Created_at descending
                                    select new
                                    {
                                        id = u.Id,
                                        taskbookstatus = tu.Taskbook_Status,
                                        taskname = u.taskdet_name,
                                        taskdesc = u.taskdet_desc,
                                        tasksched = u.taskdet_sched,
                                        taskimage = u.TaskImage,
                                        taskaddress = u.Loc_Address,
                                        jobname = job.JobName,
                                        userid = u.UserId,
                                        workerid = tu.workerId,
                                        taskedid = (from td in db.Taskeds where td.TaskDetId == u.Id select td.Id).FirstOrDefault(),
                                        taskedstatus = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskStatus).FirstOrDefault(),
                                        taskedTaskPayable = (from tp in db.Taskeds where tp.TaskDetId == u.Id select tp.TaskPayable).FirstOrDefault(),
                                        taskedWorkerfname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Firstname).FirstOrDefault(),
                                        taskedWorkerlname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Lastname).FirstOrDefault(),
                                    }).ToList().Select(p => new TaskPostListView
                                    {
                                        Id = p.id,
                                        Taskbook_Status = p.taskbookstatus,
                                        taskdet_name = p.taskname,
                                        taskdet_desc = p.taskdesc,
                                        taskdet_sched = p.tasksched,
                                        TaskImage = p.taskimage,
                                        Loc_Address = p.taskaddress,
                                        Jobname = p.jobname,
                                        UserId = p.userid,
                                        taskedWorkerfname = p.taskedWorkerfname,
                                        taskedWorkerlname = p.taskedWorkerlname,
                                        taskedstatus = p.taskedstatus,
                                        workerid = p.workerid,
                                        taskedid = p.taskedid,
                                        taskedTaskPayable = p.taskedTaskPayable
                                    });
            var taskpostview = new taskViewPost();
            taskpostview.Taskpostlistview = taskpostlist;
            taskpostview.TaskViewPost = db.SkillServiceTasks.ToList();
            return Ok(taskpostview);
        }
        [HttpGet]
        [Route("MarkasWorking")]
        public async Task<IHttpActionResult> MarkasWorking(int? id)
        {
            if(id == null)
            {
                return BadRequest(_errorMessage);
            }
            var taskeds = db.Taskeds.Where(x => x.Id == id).FirstOrDefault();
            taskeds.TaskStatus = 3;
            db.SaveChanges();
            var taskdet = db.TaskDetails.Where(x => x.Id == taskeds.TaskDetId).FirstOrDefault();
            var user = db.Users.Where(x => x.Id == taskdet.UserId).FirstOrDefault();
            var notification = new NotificationModel
            {
                Receiver = user.UserName,
                Title = $"Worker mark your task as working",
                Details = $"The worker mark your task as working and you can`t cancel it now",
                DetailsURL = $"/Task/ShowMyTaskPost",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
            return Ok(_successMessage);
        }
        [HttpGet]
        [Route("MarkasDone")]
        public async Task<IHttpActionResult> MarkasDone(int? id)
        {
            if (id == null)
            {
                return BadRequest(_errorMessage);
            }
            var taskeds = db.Taskeds.Where(x => x.Id == id).FirstOrDefault();
            taskeds.TaskStatus = 4;
            db.SaveChanges();
            var taskdet = db.TaskDetails.Where(x => x.Id == taskeds.TaskDetId).FirstOrDefault();
            var user = db.Users.Where(x => x.Id == taskdet.UserId).FirstOrDefault();
            var notification = new NotificationModel
            {
                Receiver = user.UserName,
                Title = $"Worker mark your task as Done",
                Details = $"The worker mark your task as done. Click the Mark as Complete button to pay the worker",
                DetailsURL = $"/Task/ShowMyTaskPost",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
            return Ok(_successMessage);
        }
        [HttpGet]
        [Route("MarkasCompleteTask")]
        public async Task<IHttpActionResult> MarkasCompleteTask(int? id)
        {
            if (id == null)
            {
                return BadRequest("ID is cannot be null");
            }
            var taskeds = db.Taskeds.Where(x => x.TaskDetId == id).FirstOrDefault();
            taskeds.TaskStatus = 5;
            var taskdet = db.TaskDetails.Where(x => x.Id == id).FirstOrDefault();
            var taskbook = db.TaskBook.Where(x => x.TaskDetId == id).FirstOrDefault();
            taskbook.Taskbook_Status = 3;
            var sender = db.Balance.Where(x => x.UserId == taskdet.UserId).FirstOrDefault();
            var balancesender = sender.Money - taskeds.TaskPayable;
            sender.Money = balancesender;
            var commission = (taskeds.TaskPayable * 5) / 100;
            var remaining = taskeds.TaskPayable - commission;
            var workerrecieve = db.RegistWork.Where(x => x.Id == taskeds.WorkerId).FirstOrDefault();
            var workeruserid = db.Users.Where(x => x.Id == workerrecieve.Userid).FirstOrDefault();
            var reciever = db.Balance.Where(x => x.UserId == workeruserid.Id).FirstOrDefault();
            reciever.Money = reciever.Money + remaining;
            var admin = db.Users.ToList().Where(x => UserManager.IsInRole(x.Id, "admin")).FirstOrDefault();
            var adminbalance = db.Balance.Where(x => x.UserId == admin.Id).FirstOrDefault();
            adminbalance.Money = adminbalance.Money + commission;
            db.SaveChanges();
            var user = db.Users.Where(x => x.Id == taskdet.UserId).FirstOrDefault();
            var notification = new NotificationModel
            {
                Receiver = user.UserName,
                Title = $"Worker recieve your payment",
                Details = $"The worker recieved the payment amount of {taskeds.TaskPayable}",
                DetailsURL = $"/Task/ShowMyTaskPost",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
            var worker = db.RegistWork.Where(x => x.Id == taskeds.WorkerId).FirstOrDefault();
            var useres = db.Users.Where(x => x.Id == worker.Userid).FirstOrDefault();
            var notif = new NotificationModel
            {
                Receiver = useres.UserName,
                Title = $"You recieved payment from {user.UserName}",
                Details = $"You recieved {taskeds.TaskPayable} from your previous task",
                DetailsURL = $"/Task/ShowMyTaskPost",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notif);
            db.SaveChanges();
            var transaction = new TransactionHistory();
            transaction.BidAmount = taskeds.TaskPayable.ToString();
            transaction.tasktitle = taskdet.taskdet_name;
            transaction.TotalAmount = remaining.ToString();
            transaction.Commission = commission.ToString();
            transaction.Created_At = DateTime.Now;
            transaction.Payer = user.UserName;
            transaction.Reciever = useres.UserName;
            db.TransactionHistories.Add(transaction);
            db.SaveChanges();

            return Ok(_successMessage);
        }
        [HttpGet]
        [Route("ViewCompletedTask")]
        public async Task<IHttpActionResult> ViewCompletedTask()
        {
            var user = User.Identity.GetUserId();
            var work = db.RegistWork.Where(x => x.Userid == user).FirstOrDefault();
            var worker = db.Locations.Where(x => x.UserId == user).FirstOrDefault();
            var taskpostlist = (from u in db.TaskDetails
                                where u.UserId == user && u.JobId == work.JobId
                                join d in db.Users on u.UserId equals d.Id
                                join
                                tu in db.TaskBook on u.Id equals tu.TaskDetId
                                where tu.Taskbook_Status == 3
                                join
                                job in db.JobCategories on u.JobId equals job.Id
                                orderby u.taskdet_Created_at descending
                                select new
                                {
                                    id = u.Id,
                                    taskbookstatus = tu.Taskbook_Status,
                                    taskname = u.taskdet_name,
                                    taskdesc = u.taskdet_desc,
                                    tasksched = u.taskdet_sched,
                                    taskimage = u.TaskImage,
                                    taskaddress = u.Loc_Address,
                                    jobname = job.JobName,
                                    userid = u.UserId,
                                    workerid = tu.workerId,
                                    taskedid = (from td in db.Taskeds where td.TaskDetId == u.Id select td.Id).FirstOrDefault(),
                                    taskedstatus = (from td in db.Taskeds where td.TaskDetId == u.Id select td.TaskStatus).FirstOrDefault(),
                                    taskedTaskPayable = (from tp in db.Taskeds where tp.TaskDetId == u.Id select tp.TaskPayable).FirstOrDefault(),
                                    taskedWorkerfname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Firstname).FirstOrDefault(),
                                    taskedWorkerlname = (from tp in db.Taskeds where tp.TaskDetId == u.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Lastname).FirstOrDefault(),
                                }).ToList().Select(p => new TaskPostListView
                                {
                                    Id = p.id,
                                    Taskbook_Status = p.taskbookstatus,
                                    taskdet_name = p.taskname,
                                    taskdet_desc = p.taskdesc,
                                    taskdet_sched = p.tasksched,
                                    TaskImage = p.taskimage,
                                    Loc_Address = p.taskaddress,
                                    Jobname = p.jobname,
                                    UserId = p.userid,
                                    taskedWorkerfname = p.taskedWorkerfname,
                                    taskedWorkerlname = p.taskedWorkerlname,
                                    taskedstatus = p.taskedstatus,
                                    workerid = p.workerid,
                                    taskedid = p.taskedid,
                                    taskedTaskPayable = p.taskedTaskPayable
                                });
            var taskpostview = new taskViewPost();
            taskpostview.Taskpostlistview = taskpostlist;
            taskpostview.TaskViewPost = db.SkillServiceTasks.ToList();
            return Ok(taskpostview);
        }


        [HttpGet]
        [Route("GetSchedule")]
        public async Task<IHttpActionResult> GetSchedule()
        {
            var user = User.Identity.GetUserId();
            var worker = db.RegistWork.Where(x => x.Userid == user).FirstOrDefault();
            List<TaskScheduleModel> eve = new List<TaskScheduleModel>();
            var task = db.Taskeds.Where(x => x.WorkerId == worker.Id).ToList();
            foreach(var item in task)
            {
                var taskbook = db.TaskBook.Where(x => x.TaskDetId == item.TaskDetId).FirstOrDefault();
                if(taskbook.Taskbook_Status == 2)
                {
                    var taskdet = db.TaskDetails.Where(x => x.Id == item.TaskDetId).FirstOrDefault();
                    var saveevents = new TaskScheduleModel();
                    saveevents.TaskId = item.Id;
                    saveevents.Description = taskdet.taskdet_desc;
                    saveevents.End = null;
                    saveevents.Start = taskdet.taskdet_sched;
                    saveevents.Subject = taskdet.taskdet_name;
                    saveevents.IsFullDay = true;
                    saveevents.ThemeColor = "green";
                    eve.Add(saveevents);
                }
            }
            var events = eve.ToList();
            return Ok(events);
        }


        //Bool to validate ImageFile
        private bool ValidateFile(HttpPostedFileBase file)
        {
            string fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
            string[] allowedFileTypes = { ".gif", ".png", ".jpeg", ".jpg" };
            if ((file.ContentLength > 0 && file.ContentLength < 2097152) && allowedFileTypes.Contains(fileExtension))
            {
                return true;
            }
            return false;
        }
    }
}