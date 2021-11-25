using iAssist.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iAssist.Controllers
{
    public class FeedBackAndRateController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

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
        // GET: FeedBackAndRate
        public ActionResult CreateFeedbackAndRateWorker(int id, int taskid, int jobid)
        {
            var rate = new RateandFeedback();
            rate.WorkerId = id;
            ViewBag.Worid = id;
            ViewBag.Taskid = taskid;
            ViewBag.Job = jobid;
            rate.taskid = taskid;
            return View(rate);
        }
        [HttpPost]
        public ActionResult CreateFeedbackAndRateWorker(RateandFeedback model, int? id,int? taskid)
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
                return RedirectToAction("ShowMyTaskPost", "Task");
            }
            ViewBag.Worid = id;
            ViewBag.Taskid = taskid;
            return View(model);
        }
        public ActionResult DontRate(int id, int taskid)
        {
                var task = db.Taskeds.Where(x => x.TaskDetId == taskid).FirstOrDefault();
                task.TaskType = "Done";
                db.SaveChanges();
                return RedirectToAction("ShowMyTaskPost", "Task");
        }
    }
}