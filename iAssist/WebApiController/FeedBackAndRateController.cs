using iAssist.Models;
using iAssist.WebApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Data.Entity.Spatial;
using System.IO;
using System.Globalization;
using System.EnterpriseServices;

namespace iAssist.WebApiControllers
{
    [Authorize]
    [RoutePrefix("api/Feedback")]
    public class FeedBackAndRateController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private string _errorMessage = "An Error has occurred.";
        private string _errorMessageNotFound = "Corresponding Data not found.";
        private string _successMessage = "Success";

        public FeedBackAndRateController()
        {
        }

        public FeedBackAndRateController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: FeedBackAndRate
        [HttpGet]
        [Route("CreateFeedbackAndRateWorker")]
        public async Task<IHttpActionResult> CreateFeedbackAndRateWorker(int id, int taskid, int jobid)
        {
            var rate = new RateandFeedback();
            rate.WorkerId = id;
            rate.taskid = taskid;
            rate.jobid = jobid;
            return Ok(rate);
        }
        [HttpPost]
        [Route("CreateFeedbackAndRateWorker")]
        public async Task<IHttpActionResult> CreateFeedbackAndRateWorker(RateandFeedback model)
        {
           if(ModelState.IsValid)
            {
                var tasks = db.Taskeds.Where(x => x.TaskDetId == model.taskid).FirstOrDefault();
                tasks.TaskType = "Done";
                db.SaveChanges();
                var user = User.Identity.GetUserId();
                var rated = new Rating();
                rated.Feedback = model.Feedback;
                rated.Rate = model.Rate;
                rated.UsernameFeedback = (from u in db.Users where u.Id == user select u.Email).FirstOrDefault();
                rated.WorkerID = model.WorkerId;
                rated.Jobid = model.jobid;
                db.Ratings.Add(rated);
                db.SaveChanges();
                return Ok(_successMessage);
            }
            return BadRequest("Please fill up the form correctly.");
        }
        [HttpGet]
        [Route("DontRate")]
        public async Task<IHttpActionResult> DontRate(int taskid)
        {
            var task = db.Taskeds.Where(x => x.TaskDetId == taskid).FirstOrDefault();
            task.TaskType = "Done";
            db.SaveChanges();
            return Ok(_successMessage);
        }
    }
}