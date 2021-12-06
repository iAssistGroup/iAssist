using iAssist.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace iAssist.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            var searchedWorker = new SearchNearSkilledWorkerView();
            searchedWorker.JobList = new SelectList(db.JobCategories, "Id", "JobName");
            return View(searchedWorker);
        }
        public ActionResult SearchNearSkilledView(SearchNearSkilledWorkerView model, string category)
        {
            if (category == "Distance" || category == null)
            {
                //if(Session["lng"].ToString() != "" && Session["lat"].ToString() != "" && Session["jid"].ToString() != "")
                //{
                //    model.Longitude = Session["lng"].ToString();
                //    model.Latitude = Session["lat"].ToString();
                //    model.JobId = int.Parse(Session["jid"].ToString());
                //    Session["lat"] = "";
                //    Session["lng"] = "";
                //    Session["jid"] = "";
                //}
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
                                    u in db.Locations on userwork.Userid equals u.UserId
                                  where u.JobId != null
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
                                      rate = db.Ratings.Where(x => x.WorkerID == userwork.Id).ToList().Average(a => a.Rate),
                                      distance = u.Geolocation.Distance(currentLocation),
                                  }).Select(x => new SearchNearSkilledWorkerView() { Firstname = x.Firstname, Lastname = x.Lastname, Profile = x.Profile, Jobname = x.Jobtitle, nearaddress = x.address, WorkerId = x.workerId, UserId = x.userid, JobId = x.jobid, distance = x.distance.ToString(), Rate = x.rate }).Distinct().OrderBy(x => x.distance).ToList();
                    //This was only for filter dropdown value
                    List<ShowposttaskcategoryViewModel> categor = new List<ShowposttaskcategoryViewModel>();
                    var cat = new ShowposttaskcategoryViewModel();
                    cat.CategoryName = "Distance";
                    cat.Id = 0;
                    categor.Add(cat);
                    cat = new ShowposttaskcategoryViewModel();
                    cat.CategoryName = "Rating";
                    cat.Id = 1;
                    categor.Add(cat);
                    ViewBag.Category = new SelectList(categor.Select(p => p.CategoryName).ToList().Distinct()); 
                    Session["lng"] = model.Longitude;
                    Session["lat"] = model.Latitude;
                    Session["jid"] = model.JobId;
                    return View(places);
                   
                }
            }
            else if(category == "Rating")
            {
                if (Session["lng"] != null && Session["lat"] != null && Session["jid"] != null)
                {
                    model.Longitude = Session["lng"].ToString();
                    model.Latitude = Session["lat"].ToString();
                    model.JobId = int.Parse(Session["jid"].ToString());
                    Session["lat"] = null;
                    Session["lng"] = null;
                    Session["jid"] = null;
                }
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
                                    u in db.Locations on userwork.Userid equals u.UserId
                                  where u.JobId != null
                                  join userworkprof in db.UsersIdentities on u.UserId equals userworkprof.Userid
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
                                      rate = db.Ratings.Where(x=>x.WorkerID == userwork.Id).ToList().Average(a => a.Rate),
                                      distance = u.Geolocation.Distance(currentLocation),
                                  }).Select(x => new SearchNearSkilledWorkerView() { Firstname = x.Firstname, Lastname = x.Lastname, Profile = x.Profile, Jobname = x.Jobtitle, nearaddress = x.address, WorkerId = x.workerId, UserId = x.userid, JobId = x.jobid, distance = x.distance.ToString(), Rate = x.rate}).ToList().OrderByDescending(x=>x.Rate);
                    //This was only for filter dropdown value
                    List<ShowposttaskcategoryViewModel> categor = new List<ShowposttaskcategoryViewModel>();
                    var cat = new ShowposttaskcategoryViewModel();
                    cat.CategoryName = "Distance";
                    cat.Id = 0;
                    categor.Add(cat);
                    cat = new ShowposttaskcategoryViewModel();
                    cat.CategoryName = "Rating";
                    cat.Id = 1;
                    categor.Add(cat);
                    ViewBag.Category = new SelectList(categor.Select(p => p.CategoryName).ToList().Distinct());
                    Session["lng"] = model.Longitude;
                    Session["lat"] = model.Latitude;
                    Session["jid"] = model.JobId;
                    return View(places);
                }
            }
            return View("Error");
        }
        public ActionResult FindWorkerList(int? id, string category)
        {
            //if (Session["tid"] != null)
            //{
            //    id = int.Parse(Session["tid"].ToString());
            //    Session["tid"] = null;
            //}
            var model = db.TaskDetails.Where(x => x.Id == id).FirstOrDefault();
            if (category == "Distance" || category == null)
            {
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
                                  where u.JobId != null
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
                                      rate = db.Ratings.Where(x => x.WorkerID == userwork.Id).ToList().Average(a => a.Rate),
                                      distance = u.Geolocation.Distance(currentLocation),
                                  }).Select(x => new SearchNearSkilledWorkerView() { Firstname = x.Firstname, Lastname = x.Lastname, Profile = x.Profile, Jobname = x.Jobtitle, nearaddress = x.address, WorkerId = x.workerId, UserId = x.userid, JobId = x.jobid, distance = x.distance.ToString(), Rate = x.rate, Taskdet = model.Id }).Distinct().OrderBy(x=>x.distance).ToList();
                    //This was only for filter dropdown value
                    List<ShowposttaskcategoryViewModel> categor = new List<ShowposttaskcategoryViewModel>();
                    var cat = new ShowposttaskcategoryViewModel();
                    cat.CategoryName = "Distance";
                    cat.Id = 0;
                    categor.Add(cat);
                    cat = new ShowposttaskcategoryViewModel();
                    cat.CategoryName = "Rating";
                    cat.Id = 1;
                    categor.Add(cat);
                    ViewBag.Category = new SelectList(categor.Select(p => p.CategoryName).ToList().Distinct());
                    Session["tid"] = id;
                    return View(places);

                }
            }
            else if (category == "Rating")
            {
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
                                  where u.JobId != null
                                  join userworkprof in db.UsersIdentities on u.UserId equals userworkprof.Userid
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
                                      rate = db.Ratings.Where(x => x.WorkerID == userwork.Id).ToList().Average(a => a.Rate),
                                      distance = u.Geolocation.Distance(currentLocation),
                                  }).Select(x => new SearchNearSkilledWorkerView() { Firstname = x.Firstname, Lastname = x.Lastname, Profile = x.Profile, Jobname = x.Jobtitle, nearaddress = x.address, WorkerId = x.workerId, UserId = x.userid, JobId = x.jobid, distance = x.distance.ToString(), Rate = x.rate, Taskdet = model.Id }).ToList().OrderByDescending(x => x.Rate);
                    //This was only for filter dropdown value
                    List<ShowposttaskcategoryViewModel> categor = new List<ShowposttaskcategoryViewModel>();
                    var cat = new ShowposttaskcategoryViewModel();
                    cat.CategoryName = "Distance";
                    cat.Id = 0;
                    categor.Add(cat);
                    cat = new ShowposttaskcategoryViewModel();
                    cat.CategoryName = "Rating";
                    cat.Id = 1;
                    categor.Add(cat);
                    ViewBag.Category = new SelectList(categor.Select(p => p.CategoryName).ToList().Distinct());
                    Session["tid"] = id;
                    return View(places);
                }
            }
            return View("Error");
        }
        public ActionResult ViewDetailsChoosenWorker(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
            return View(model);
        }
        public ActionResult ViewFindWorker(int? id, int? task)
        {
            if (id == null || task == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
            return View(model);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}