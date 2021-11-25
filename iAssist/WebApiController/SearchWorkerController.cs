using iAssist.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using System.Data.Entity.Spatial;
using System.Web.Routing;
using iAssist.WebApiModels;
using System.Dynamic;

namespace iAssist.WebApiControllers
{
    [Authorize]
    [RoutePrefix("api/SearchWorker")]
    public class SearchWorkerController : ApiController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private string _errorMessage = "An Error has occurred.";
        private string _errorMessageNotFound = "Corresponding Data not found.";
        private string _successMessage = "Success";


        public IEnumerable<JobListModel> GetJobList()
        {
            List<JobListModel> output = new List<JobListModel>();
            foreach (var data in db.JobCategories.ToList())
            {
                output.Append(new JobListModel { Id = data.Id, JobName = data.JobName });
            };
            return output;
        }

        public IEnumerable<SkillListModel> GetSkillList(int jobid)
        {
            List<SkillListModel> output = new List<SkillListModel>();

            foreach (var data in db.Skills.Where(x => x.Jobid == jobid).ToList())
            {
                output.Append(new SkillListModel { Id = data.Id, Skillname = data.Skillname });
            };
            return output;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IHttpActionResult> Index()
        {
            var searchedWorker = new WebApiModels.SearchNearSkilledWorkerView();
            searchedWorker.JobList = GetJobList();
            return Ok(searchedWorker);
        }

        [HttpGet]
        [Route("SearchNearSkilledView")]
        public async Task<IHttpActionResult> SearchNearSkilledView(WebApiModels.SearchNearSkilledWorkerView model)
        {
            if (model.Latitude != null && model.Longitude != null)
            {
                var currentLocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");

                //var currentLocation = DbGeography.FromText("POINT( 78.3845534 17.4343666 )");

                var places = (from userwork in db.RegistWork
                              where userwork.JobId == model.JobId && userwork.worker_status == 0
                              join
                                job in db.JobCategories on model.JobId equals job.Id
                              join
                              username in db.Users on userwork.Userid equals username.Id
                              join
                                u in db.Locations on userwork.Userid equals u.UserId where u.JobId != null
                              join userworkprof in db.UsersIdentities on u.UserId equals userworkprof.Userid
                              orderby u.Geolocation.Distance(currentLocation) ascending
                              select new
                              {
                                  workerId = userwork.Id,
                                  Firstname = userworkprof.Firstname,
                                  Lastname = userworkprof.Lastname,
                                  Profile = userworkprof.ProfilePicture,
                                  Jobtitle = job.JobName,
                                  address = u.Loc_Address,
                                  userid = username.UserName,
                                  jobid = job.Id,
                                  distance = u.Geolocation.Distance(currentLocation),
                              }).Select(x => new WebApiModels.SearchNearSkilledWorkerView() { Firstname = x.Firstname, Lastname = x.Lastname, Profile = x.Profile, Jobname = x.Jobtitle, nearaddress = x.address,WorkerId = x.workerId, UserId = x.userid, JobId = x.jobid, distance = x.distance.ToString() }).ToList();
                return Ok(places);
            }
            return BadRequest(_errorMessage);
        }
        [HttpGet]
        [Route("FindWorkerList")]
        public async Task<IHttpActionResult> FindWorkerList(int? id)
        {
            var model = db.TaskDetails.Where(x => x.Id == id).FirstOrDefault();
            if (model.Geolocation.Latitude != null && model.Geolocation.Longitude != null)
            {
                var currentLocation = DbGeography.FromText("POINT( " + model.Geolocation.Longitude + " " + model.Geolocation.Latitude + " )");

                //var currentLocation = DbGeography.FromText("POINT( 78.3845534 17.4343666 )");

                var places = (from userwork in db.RegistWork
                              where userwork.JobId == model.JobId && userwork.worker_status == 0
                              join
                                job in db.JobCategories on model.JobId equals job.Id
                              join
                              username in db.Users on userwork.Userid equals username.Id
                              join
                                u in db.Locations on userwork.Userid equals u.UserId
                              join userworkprof in db.UsersIdentities on u.UserId equals userworkprof.Userid
                              orderby u.Geolocation.Distance(currentLocation) ascending
                              select new
                              {
                                  workerId = userwork.Id,
                                  Firstname = userworkprof.Firstname,
                                  Lastname = userworkprof.Lastname,
                                  Profile = userworkprof.ProfilePicture,
                                  Jobtitle = job.JobName,
                                  address = u.Loc_Address,
                                  userid = username.UserName,
                                  jobid = job.Id,
                                  distance = u.Geolocation.Distance(currentLocation),
                              }).Take(10).Select(x => new WebApiModels.SearchNearSkilledWorkerView() { Firstname = x.Firstname, Lastname = x.Lastname, Profile = x.Profile, Jobname = x.Jobtitle, nearaddress = x.address, WorkerId = x.workerId, UserId = x.userid, JobId = x.jobid, distance = x.distance.ToString(), Taskdet = model.Id }).ToList();
                return Ok(places);
            }
            return BadRequest(_errorMessage);
        }
        [HttpGet]
        [Route("ViewDetailsChoosenWorker")]
        public async Task<IHttpActionResult> ViewDetailsChoosenWorker(int? id)
        {
            if (id == null)
            {
                return BadRequest(_errorMessageNotFound);
            }
            var viewprofileworker = (from a in db.RegistWork
                                     where a.Id == id
                                     join b in db.UsersIdentities on a.Userid equals b.Userid
                                     join c in db.JobCategories on a.JobId equals c.Id
                                     select new ProfileViewOfSkilledWorker
                                     {
                                         Firstname = b.Firstname,
                                         Lastname = b.Lastname,
                                         ProfilePicture = b.ProfilePicture,
                                         worker_overview = a.worker_overview,
                                         Jobname = c.JobName,
                                         Userid = (from u in db.Users where a.Userid == u.Id select u.UserName).FirstOrDefault(),
                                         Jobid = c.Id,
                                         WorkerId = a.Id,
                                     }).FirstOrDefault();
            var Rateandfeedback = (from r in db.Ratings
                                   where viewprofileworker.WorkerId == r.WorkerID
                                   select new
                                   {
                                       Rate = r.Rate,
                                       Feedback = r.Feedback,
                                       Username = r.UsernameFeedback,
                                   }).ToList().Select(p => new RateandFeedback
                                   {
                                       Rate = p.Rate,
                                       Feedback = p.Feedback,
                                       Username = p.Username,
                                   });
            dynamic model = new ExpandoObject();
            model.viewprofile = viewprofileworker;
            model.rate = Rateandfeedback;
            return Ok(model);
        }
        [HttpGet]
        [Route("ViewDetailsChoosenWorker")]
        public async Task<IHttpActionResult> ViewFindWorker(int? id, int? task)
        {
            if (id == null || task == null)
            {
                return BadRequest(_errorMessageNotFound);
            }
            var viewprofileworker = (from a in db.RegistWork
                                     where a.Id == id
                                     join b in db.UsersIdentities on a.Userid equals b.Userid
                                     join c in db.JobCategories on a.JobId equals c.Id
                                     select new ProfileViewOfSkilledWorker
                                     {
                                         Firstname = b.Firstname,
                                         Lastname = b.Lastname,
                                         ProfilePicture = b.ProfilePicture,
                                         worker_overview = a.worker_overview,
                                         Jobname = c.JobName,
                                         Userid = (from u in db.Users where a.Userid == u.Id select u.UserName).FirstOrDefault(),
                                         Jobid = c.Id,
                                         WorkerId = a.Id,
                                         taskdet = task,
                                     }).FirstOrDefault();
            var Rateandfeedback = (from r in db.Ratings
                                   where viewprofileworker.WorkerId == r.WorkerID
                                   select new
                                   {
                                       Rate = r.Rate,
                                       Feedback = r.Feedback,
                                       Username = r.UsernameFeedback,
                                   }).ToList().Select(p => new RateandFeedback
                                   {
                                       Rate = p.Rate,
                                       Feedback = p.Feedback,
                                       Username = p.Username,
                                   });
            dynamic model = new ExpandoObject();
            model.viewprofile = viewprofileworker;
            model.rate = Rateandfeedback;
            return Ok(model);
        }
    }
}