using iAssist.Hubs;
using iAssist.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace iAssist.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

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
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
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
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Task
        //Creating Task in General or Posting Task
        public ActionResult CreateTaskIndex(int? jobid)
        {
            var user = User.Identity.GetUserId();
            var taskposting = new TaskDetailsViewModel();
            taskposting.JobList = new SelectList(db.JobCategories, "Id", "JobName");
            taskposting.check = 0;
            if(jobid != null)
            {
                taskposting.check = 1;
                taskposting.JobList = new SelectList(db.JobCategories, "Id", "JobName",jobid);
                taskposting.Address = db.Locations.Where(x => x.UserId == user && x.JobId == null).Select(p => p.Loc_Address).FirstOrDefault();
                taskposting.Longitude = db.Locations.Where(x => x.UserId == user && x.JobId == null).Select(p => p.Geolocation.Longitude.ToString()).FirstOrDefault();
                taskposting.Latitude = db.Locations.Where(x => x.UserId == user && x.JobId == null).Select(p => p.Geolocation.Latitude.ToString()).FirstOrDefault();
                ViewBag.SkillsSelect = new SelectList(db.Skills.Where(x => x.Jobid == jobid), "Id", "Skillname",jobid);
                ViewBag.r = jobid;
            }
            return View(taskposting);
        }
        [HttpPost]
        public ActionResult CreateTaskIndex(TaskDetailsViewModel model, params string[] selectedSkills)
        {
            if (ModelState.IsValid)
            {
                var checkdate = model.taskdet_sched.ToString();
                if (checkdate == "1/1/0001 12:00:00 AM")
                {
                    ModelState.AddModelError("", "Please Fill up the form correctly");
                    model.JobList = new SelectList(db.JobCategories, "Id", "JobName",model.JobId);
                    ViewBag.SkillsSelect = new SelectList(db.Skills.Where(x => x.Jobid == model.JobId), "Id", "Skillname");
                    return View(model);
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
                    filename = Path.Combine(Server.MapPath("~/image/"), filename);
                    userposttask.TaskImage = model.TaskImage;
                    model.ImageFile.SaveAs(filename);
                }
                userposttask.JobId = model.JobId;
                model.JobList = new SelectList(db.JobCategories, "Id", "JobName", userposttask.JobId);
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
                if (selectedSkills != null)
                {
                    for (int i = 0; i < selectedSkills.Count(); i++)
                    {
                        var skillofservice = new SkillServiceTask();
                        skillofservice.Jobid = model.JobId;
                        skillofservice.Skillname = selectedSkills[i];
                        skillofservice.UserId = model.UserId;
                        skillofservice.Taskdet = taskdetid.Id;
                        db.SkillServiceTasks.Add(skillofservice);
                        db.SaveChanges();
                    }
                }
                if(selectedSkills == null)
                {
                    ModelState.AddModelError("", "Please select service needed");
                    model.JobList = new SelectList(db.JobCategories, "Id", "JobName");
                    return View(model);
                }
                return RedirectToAction("ShowMyTaskPost");
            }
            model.JobList = new SelectList(db.JobCategories, "Id", "JobName");
            return View(model);
        }
        //Show User TaskPost
        public ActionResult ShowMyTaskPost(string category)
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
            ViewBag.Category = new SelectList(categor.Select(p=>p.CategoryName).ToList().Distinct());
            return View(taskpostview);
        }
        //Posting the task
        public ActionResult PostTheTask(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Task_Book taskbook = db.TaskBook.Where(x => x.TaskDetId == id).FirstOrDefault();
            if (taskbook == null)
            {
                return HttpNotFound();
            }
            taskbook.Taskbook_Status = 1;
            db.SaveChanges();
            return RedirectToAction("ShowMyTaskPost");
        }
        //Edit User TaskPost
        public ActionResult EditMyPostTask(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskDetails taskdetails = db.TaskDetails.Where(x => x.Id == id).FirstOrDefault();
            if (taskdetails == null)
            {
                return HttpNotFound();
            }
            var service = (from st in db.SkillServiceTasks where st.Taskdet == taskdetails.Id select st.Skillname).ToList();
            var taskpostlist = new TaskDetailsViewModel();
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
            taskpostlist.JobList = new SelectList(db.JobCategories, "Id", "JobName", taskpostlist.JobId);
            taskpostlist.Skilltasks = db.Skills.Where(x => x.Jobid == id).ToList().Select(x => new SelectListItem() { Selected = service.Contains(x.Skillname), Text = x.Skillname, Value = x.Skillname });
            return View(taskpostlist);
        }
        [HttpPost]
        public ActionResult EditMyPostTask(TaskDetailsViewModel model, params string[] selectedSkills)
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
                    filename = Path.Combine(Server.MapPath("~/image/"), filename);
                    userposttask.TaskImage = model.TaskImage;
                    model.ImageFile.SaveAs(filename);
                }
                userposttask.JobId = model.JobId;
                model.JobList = new SelectList(db.JobCategories, "Id", "JobName", userposttask.JobId);
                userposttask.Loc_Address = model.Address;
                userposttask.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
                userposttask.taskdet_name = model.TaskTitle;
                userposttask.taskdet_desc = model.TaskDesc;
                userposttask.taskdet_sched = model.taskdet_sched;
                userposttask.taskdet_Updated_at = DateTime.Now;
                if (selectedSkills != null)
                {
                    var a = db.SkillServiceTasks.Where(x => x.Taskdet == userposttask.Id).ToList();
                    db.SkillServiceTasks.RemoveRange(a);
                    db.SaveChanges();
                    for (int i = 0; i < selectedSkills.Count(); i++)
                    {
                        var skillofservice = new SkillServiceTask();
                        skillofservice.Jobid = model.JobId;
                        skillofservice.Skillname = selectedSkills[i];
                        skillofservice.UserId = model.UserId;
                        skillofservice.Taskdet = userposttask.Id;
                        db.SkillServiceTasks.Add(skillofservice);
                        db.SaveChanges();
                    }
                }
                if (selectedSkills == null)
                {
                    ModelState.AddModelError("", "Please Select Service Needed");
                    model.JobList = new SelectList(db.JobCategories, "Id", "JobName");
                    return View(model);
                }
                db.SaveChanges();
                return RedirectToAction("ShowMyTaskPost");
            }
            ModelState.AddModelError("", "Please Fill up the form correctly");
            model.JobList = new SelectList(db.JobCategories, "Id", "JobName");
            return View(model);
        }
        //Cancel User TaskPost
        public ActionResult CancelMyPostTask(int? id,int? cancel)
        {
            if(cancel == null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Task_Book taskbook = db.TaskBook.Where(x => x.TaskDetId == id).FirstOrDefault();
                if (taskbook == null)
                {
                    return HttpNotFound();
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
                    return RedirectToAction("ShowMyTaskPost");
                }
                taskbook.Taskbook_Status = 4;
                db.SaveChanges();
                return RedirectToAction("ShowMyTaskPost");
            }
            if (cancel != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Task_Book taskbook = db.TaskBook.Where(x => x.TaskDetId == id).FirstOrDefault();
                if (taskbook == null)
                {
                    return HttpNotFound();
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
                if(cancel == 1)
                {
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
                    return RedirectToAction("ViewUserRequestedTask");
                }
                if(cancel == 2)
                {
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
                    return RedirectToAction("ViewContractTask");
                }
            }
            return View("Error");
        }
        //Request Booking
        public ActionResult RequestBooking(int? id, int? jobid, int? a)
        {
            if(id == null || jobid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userid = User.Identity.GetUserId();
            var workerid = db.RegistWork.Where(x => x.Userid == userid).FirstOrDefault();
            if(workerid != null &&id == workerid.Id)
            {
                return View("Error");
            }
            if(a == 1)
            {
                ModelState.AddModelError("", "Please Fill up the form correctly");
            }
            if(a == 2)
            {
                ModelState.AddModelError("", "Please select Service ");
            }
            var taskposting = new TaskDetailsViewModel();
            ViewBag.id = id;
            ViewBag.Jobid = jobid;
            taskposting.JobList = new SelectList(db.JobCategories, "Id", "JobName", jobid);
            taskposting.Address = db.Locations.Where(x => x.UserId == userid && x.JobId == null).Select(p => p.Loc_Address).FirstOrDefault();
            taskposting.Longitude = db.Locations.Where(x => x.UserId == userid && x.JobId == null).Select(p => p.Geolocation.Longitude.ToString()).FirstOrDefault();
            taskposting.Latitude = db.Locations.Where(x => x.UserId == userid && x.JobId == null).Select(p => p.Geolocation.Latitude.ToString()).FirstOrDefault();
            ViewBag.SkillsSelect = new SelectList(db.Skills.Where(x => x.Jobid == jobid), "Id", "Skillname", jobid);
            return View(taskposting);
        }
        [HttpPost]
        public ActionResult RequestBooking(TaskDetailsViewModel model, params string[] selectedSkills)
        {
            if (ModelState.IsValid)
            {
                var checkdate = model.taskdet_sched.ToString();
                if (selectedSkills == null)
                {
                    return RedirectToAction("RequestBooking", new { id = model.workerid, jobid = model.JobId, a = 2 });
                }
                if (checkdate == "1/1/0001 12:00:00 AM")
                {
                    return RedirectToAction("RequestBooking", new { id = model.workerid, jobid = model.JobId, a = 1 });
                }
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
                    filename = Path.Combine(Server.MapPath("~/image/"), filename);
                    userposttask.TaskImage = model.TaskImage;
                    model.ImageFile.SaveAs(filename);
                }
                userposttask.JobId = model.JobId;
                model.JobList = new SelectList(db.JobCategories, "Id", "JobName", userposttask.JobId);
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
                if (selectedSkills != null)
                {
                    for (int i = 0; i < selectedSkills.Count(); i++)
                    {
                        var skillofservice = new SkillServiceTask();
                        skillofservice.Jobid = model.JobId;
                        skillofservice.Skillname = selectedSkills[i];
                        skillofservice.UserId = model.UserId;
                        skillofservice.Taskdet = taskdetid.Id;
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
                return RedirectToAction("ShowMyTaskPost");
            }
            return RedirectToAction("RequestBooking", new { id = model.workerid, jobid = model.JobId, a = 1 });
        }

        public ActionResult FindWorkerRequestBooking(int? id, int? task)
        {
            if (id == null)
            {
                return View("Error");
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
            return RedirectToAction("ShowMyTaskPost");
        }



        //User Requested Tasks
        [Authorize(Roles = "Worker")]
        public ActionResult ViewUserRequestedTask()
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
            return View(taskpostview);
        }
        [Authorize(Roles = "Worker")]
        public ActionResult ViewBiddedRequestTask()
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
            return View(taskpostview);
        }
        [Authorize(Roles = "Worker")]
        public ActionResult ViewContractTask()
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
            return View(taskpostview);
        }
        public ActionResult MarkasWorking(int? id)
        {
            if(id == null)
            {
                return View("Error");
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
            return RedirectToAction("ViewContractTask");
        }
        public ActionResult MarkasDone(int? id)
        {
            if (id == null)
            {
                return View("Error");
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
            return RedirectToAction("ViewContractTask");
        }
        public ActionResult MarkasCompleteTask(int? id)
        {
            if (id == null)
            {
                return View("Error");
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
                DetailsURL = $"",
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
            var transactions = new TransactionHistory();
            transactions.BidAmount = taskeds.TaskPayable.ToString();
            transactions.tasktitle = taskdet.taskdet_name;
            transactions.TotalAmount = remaining.ToString();
            transactions.Commission = commission.ToString();
            transactions.Created_At = DateTime.Now;
            transactions.Payer = useres.UserName;
            transactions.Reciever = admin.UserName;
            db.TransactionHistories.Add(transactions);
            db.SaveChanges();
            return RedirectToAction("ShowMyTaskPost");
        }
        public ActionResult ViewCompletedTask()
        {
            var user = User.Identity.GetUserId();
            var work = db.RegistWork.Where(x => x.Userid == user).FirstOrDefault();
            var worker = db.Locations.Where(x => x.UserId == user).FirstOrDefault();
            var taskpostlist = (from u in db.Taskeds
                                where u.WorkerId == work.Id
                                join t in db.TaskDetails on u.TaskDetId equals t.Id
                                join d in db.Users on worker.UserId equals d.Id
                                join
                                tu in db.TaskBook on u.TaskDetId equals tu.TaskDetId
                                where tu.Taskbook_Status == 3
                                join
                                job in db.JobCategories on work.JobId equals job.Id
                                orderby u.TaskCreated_at descending
                                select new
                                {
                                    id = u.Id,
                                    taskbookstatus = tu.Taskbook_Status,
                                    taskname = t.taskdet_name,
                                    taskdesc = t.taskdet_desc,
                                    tasksched = t.taskdet_sched,
                                    taskimage = t.TaskImage,
                                    taskaddress = t.Loc_Address,
                                    jobname = job.JobName,
                                    userid = t.UserId,
                                    workerid = tu.workerId,
                                    taskedid = (from td in db.Taskeds where td.TaskDetId == u.Id select td.Id).FirstOrDefault(),
                                    taskedstatus = (from td in db.Taskeds where td.TaskDetId == t.Id select td.TaskStatus).FirstOrDefault(),
                                    taskedTaskPayable = (from tp in db.Taskeds where tp.TaskDetId == t.Id select tp.TaskPayable).FirstOrDefault(),
                                    taskedWorkerfname = (from tp in db.Taskeds where tp.TaskDetId == t.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Firstname).FirstOrDefault(),
                                    taskedWorkerlname = (from tp in db.Taskeds where tp.TaskDetId == t.Id join uw in db.RegistWork on tp.WorkerId equals uw.Id join us in db.UsersIdentities on uw.Userid equals us.Userid select us.Lastname).FirstOrDefault(),
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
            return View(taskpostview);
        }

        //Task Schedule
        public ActionResult TaskScheduler()
        {
            return View();
        }
        public JsonResult GetSchedule()
        {
            var user = User.Identity.GetUserId();
            var worker = db.RegistWork.Where(x => x.Userid == user).FirstOrDefault();
            List<TaskScheduleViewModel> eve = new List<TaskScheduleViewModel>();
            var task = db.Taskeds.Where(x => x.WorkerId == worker.Id).ToList();
            foreach(var item in task)
            {
                var taskbook = db.TaskBook.Where(x => x.TaskDetId == item.TaskDetId).FirstOrDefault();
                if(taskbook.Taskbook_Status == 2)
                {
                    var taskdet = db.TaskDetails.Where(x => x.Id == item.TaskDetId).FirstOrDefault();
                    var saveevents = new TaskScheduleViewModel();
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
            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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