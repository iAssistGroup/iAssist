using iAssist.Hubs;
using iAssist.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace iAssist.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public UserController()
        {
        }

        public UserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: User
        //Start Functions Applying User to be SkilledWorker
        public ActionResult ApplySkilledWorker()
        {
            var UserId = User.Identity.GetUserId();
            var data = db.RegistWork.Where(x => x.Userid == UserId && x.worker_status == 1).FirstOrDefault();
            if(data == null)
            {
                var selectjob = new SelectJobViewModel();
                selectjob.JobList = new SelectList(db.JobCategories, "Id", "JobName");
                return View(selectjob);
            }
            else
            {
                return RedirectToAction("ViewSubmittedWorker");
            }
        }
        [HttpGet]
        public ActionResult RegisterSkilledWorker(SelectJobViewModel model)
        {
            var UserId = User.Identity.GetUserId();
            var data = db.RegistWork.Where(x => x.Userid == UserId && x.JobId == model.JobId).FirstOrDefault();
            var Useridentity = db.UsersIdentities.Where(x => x.Userid == UserId).FirstOrDefault();
            if (data == null)
            {
                var Userinform = new RegisterSkilledWorker();
                Userinform.Firstname = Useridentity.Firstname;
                Userinform.Lastname = Useridentity.Lastname;
                Userinform.ProfilePicture = Useridentity.ProfilePicture;
                Userinform.Phonenumber = UserManager.GetPhoneNumber(UserId);
                Userinform.Address = (from l in db.Locations where l.UserId == UserId select l.Loc_Address).FirstOrDefault();
                Userinform.Longitude = (from l in db.Locations where l.UserId == UserId select l.Geolocation.Longitude.ToString()).FirstOrDefault();
                Userinform.Latitude = (from l in db.Locations where l.UserId == UserId select l.Geolocation.Latitude.ToString()).FirstOrDefault();
                Userinform.JobId = model.JobId;
                ViewBag.SkillsSelect = new SelectList(db.Skills.Where(x => x.Jobid == model.JobId), "Id", "Skillname");
                return View(Userinform);
            }
            TempData["Error1"] = "You already Apply This job";
            return RedirectToAction("ApplySkilledWorker");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterSkilledWorker(RegisterSkilledWorker model, params string[] selectedSkills)
        {
            //var user = User.Identity.GetUserId();
            //var userident = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
            //if (!ModelState.IsValid && selectedSkills == null)
            //{
            //    ViewBag.SkillsSelect = new SelectList(db.Skills.Where(x => x.Jobid == model.JobId), "Id", "Skillname");
            //    return View(model);
            //}
            //if (model.Firstname != null)
            //{
            //    userident.Firstname = model.Firstname;
            //}
            //if (model.Lastname != null)
            //{
            //    userident.Lastname = model.Lastname;
            //}
            //if (model.Phonenumber != null)
            //{
            //    var changePhoneNumberToken = UserManager.GenerateChangePhoneNumberToken(user, model.Phonenumber);
            //    var result = UserManager.ChangePhoneNumber(user, model.Phonenumber, changePhoneNumberToken);
            //}
            //userident.Updated_At = DateTime.Now;
            var user = User.Identity.GetUserId();
            var userident = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
            if (model.ImageFile != null && ValidateFile(model.ImageFile) == true)
            {
                string filename = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                string extension = Path.GetExtension(model.ImageFile.FileName);
                filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                model.ProfilePicture = "" + filename;
                filename = Path.Combine(Server.MapPath("~/image/"), filename);
                userident.ProfilePicture = model.ProfilePicture;
                model.ImageFile.SaveAs(filename);
                userident.Updated_At = DateTime.Now;
            }
            //var w = db.RegistWork.Where(x => x.Userid == user && model.JobId == x.JobId).FirstOrDefault();
            //if (w == null)
            //{
            //    Work worker = new Work();
            //    worker.worker_overview = model.Overview;
            //    worker.worker_status = 3;
            //    worker.Created_At = DateTime.Now;
            //    worker.Updated_At = DateTime.Now;
            //    worker.Userid = User.Identity.GetUserId();
            //    worker.JobId = model.JobId;
            //    db.RegistWork.Add(worker);
            //    db.SaveChanges();
            //}
            //if (w != null)
            //{
            //    w.worker_overview = model.Overview;
            //    w.worker_status = 3;
            //    w.Updated_At = DateTime.Now;
            //    w.JobId = model.JobId;
            //    db.SaveChanges();
            //}
            //var l = db.Locations.Where(x => x.UserId == user).FirstOrDefault();
            //if (l == null)
            //{
            //    var location = new Location();
            //    location.Loc_Address = model.Address;
            //    location.UserId = user;
            //    location.Created_At = DateTime.Now;
            //    location.Updated_At = DateTime.Now;
            //    location.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
            //    db.Locations.Add(location);
            //    db.SaveChanges();
            //}
            //if (l != null)
            //{
            //    l.Loc_Address = model.Address;
            //    l.Updated_At = DateTime.Now;
            //    l.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
            //    db.SaveChanges();
            //}
            //var s = db.SkillsOfWorkers.Where(x => x.UserId == user && model.JobId == x.Jobid).ToList();
            //if (s == null)
            //{
            //    if (selectedSkills != null)
            //    {
            //        for (int i = 0; i < selectedSkills.Count(); i++)
            //        {
            //            var skills = new SkillsOfWorker();
            //            skills.Jobid = model.JobId;
            //            skills.Skillname = selectedSkills[i];
            //            skills.UserId = user;
            //            db.SkillsOfWorkers.Add(skills);
            //            db.SaveChanges();
            //        }
            //    }
            //}
            //if (s != null)
            //{
            //    db.SkillsOfWorkers.RemoveRange(s);
            //    db.SaveChanges();
            //    if (selectedSkills != null)
            //    {
            //        for (int i = 0; i < selectedSkills.Count(); i++)
            //        {
            //            var skills = new SkillsOfWorker();
            //            skills.Jobid = model.JobId;
            //            skills.Skillname = selectedSkills[i];
            //            skills.UserId = user;
            //            db.SkillsOfWorkers.Add(skills);
            //            db.SaveChanges();
            //        }
            //    }
            //}
            if (selectedSkills != null)
            {
                model.workerskills = new List<worskills>();
                for (int i = 0; i < selectedSkills.Count(); i++)
                {
                    model.workerskills.Add(new worskills { Jobid = model.JobId, Skillname = selectedSkills[i], UserId = user });
                }
            }
            if(selectedSkills == null)
            {
                ViewBag.SkillsSelect = new SelectList(db.Skills.Where(x => x.Jobid == model.JobId), "Id", "Skillname");
                ModelState.AddModelError("", "Please specify Skills / Service Offer");
                return View(model);
            }
            Session["Model"] = model;
            return RedirectToAction("GetSkilledWOrkerFile");
        }
        public ActionResult EditRegisterSkilledWorker(int id)
        {
            var UserId = User.Identity.GetUserId();
            var data = db.RegistWork.Where(x => x.Userid == UserId && x.JobId == id).FirstOrDefault();
            var skills = (from u in db.SkillsOfWorkers where u.UserId == UserId && u.Jobid == id select u.Skillname).ToList();
            var Useridentity = db.UsersIdentities.Where(x => x.Userid == UserId).FirstOrDefault();
            var Userinform = new RegisterSkilledWorker();
            Userinform.Firstname = Useridentity.Firstname;
            Userinform.Lastname = Useridentity.Lastname;
            Userinform.ProfilePicture = Useridentity.ProfilePicture;
            Userinform.Phonenumber = UserManager.GetPhoneNumber(UserId);
            Userinform.Address = (from l in db.Locations where l.UserId == UserId select l.Loc_Address).FirstOrDefault();
            Userinform.Longitude = (from l in db.Locations where l.UserId == UserId select l.Geolocation.Longitude.ToString()).FirstOrDefault();
            Userinform.Latitude = (from l in db.Locations where l.UserId == UserId select l.Geolocation.Latitude.ToString()).FirstOrDefault();
            Userinform.JobId = id;
            Userinform.Overview = data.worker_overview;
            Userinform.workskill = db.Skills.Where(x=>x.Jobid == id).ToList().Select(x => new SelectListItem() { Selected = skills.Contains(x.Skillname) , Text = x.Skillname, Value = x.Skillname });
            return View(Userinform);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRegisterSkilledWorker(RegisterSkilledWorker model, params string[] selectedSkills)
        {
            if(ModelState.IsValid)
            {
                //var user = User.Identity.GetUserId();
                //var userident = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
                //if (!ModelState.IsValid && selectedSkills == null)
                //{
                //    ViewBag.SkillsSelect = new SelectList(db.Skills.Where(x => x.Jobid == model.JobId), "Id", "Skillname");
                //    return View(model);
                //}
                //if (model.Firstname != null)
                //{
                //    userident.Firstname = model.Firstname;
                //}
                //if (model.Lastname != null)
                //{
                //    userident.Lastname = model.Lastname;
                //}
                //if (model.Phonenumber != null)
                //{
                //    var changePhoneNumberToken = UserManager.GenerateChangePhoneNumberToken(user, model.Phonenumber);
                //    var result = UserManager.ChangePhoneNumber(user, model.Phonenumber, changePhoneNumberToken);
                //}
                //userident.Updated_At = DateTime.Now;
                var user = User.Identity.GetUserId();
                var userident = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
                if (model.ImageFile != null && ValidateFile(model.ImageFile) == true)
                {
                    string filename = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                    string extension = Path.GetExtension(model.ImageFile.FileName);
                    filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                    model.ProfilePicture = "" + filename;
                    filename = Path.Combine(Server.MapPath("~/image/"), filename);
                    userident.ProfilePicture = model.ProfilePicture;
                    model.ImageFile.SaveAs(filename);
                    userident.Updated_At = DateTime.Now;
                }
                //var w = db.RegistWork.Where(x => x.Userid == user && model.JobId == x.JobId).FirstOrDefault();
                //if (w == null)
                //{
                //    Work worker = new Work();
                //    worker.worker_overview = model.Overview;
                //    worker.worker_status = 3;
                //    worker.Created_At = DateTime.Now;
                //    worker.Updated_At = DateTime.Now;
                //    worker.Userid = User.Identity.GetUserId();
                //    worker.JobId = model.JobId;
                //    db.RegistWork.Add(worker);
                //    db.SaveChanges();
                //}
                //if (w != null)
                //{
                //    w.worker_overview = model.Overview;
                //    w.worker_status = 3;
                //    w.Updated_At = DateTime.Now;
                //    w.JobId = model.JobId;
                //    db.SaveChanges();
                //}
                //var l = db.Locations.Where(x => x.UserId == user).FirstOrDefault();
                //if (l == null)
                //{
                //    var location = new Location();
                //    location.Loc_Address = model.Address;
                //    location.UserId = user;
                //    location.Created_At = DateTime.Now;
                //    location.Updated_At = DateTime.Now;
                //    location.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
                //    db.Locations.Add(location);
                //    db.SaveChanges();
                //}
                //if (l != null)
                //{
                //    l.Loc_Address = model.Address;
                //    l.Updated_At = DateTime.Now;
                //    l.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
                //    db.SaveChanges();
                //}
                //var s = db.SkillsOfWorkers.Where(x => x.UserId == user && model.JobId == x.Jobid).ToList();
                //if (s == null)
                //{
                //    if (selectedSkills != null)
                //    {
                //        for (int i = 0; i < selectedSkills.Count(); i++)
                //        {
                //            var skills = new SkillsOfWorker();
                //            skills.Jobid = model.JobId;
                //            skills.Skillname = selectedSkills[i];
                //            skills.UserId = user;
                //            db.SkillsOfWorkers.Add(skills);
                //            db.SaveChanges();
                //        }
                //    }
                //}
                //if (s != null)
                //{
                //    db.SkillsOfWorkers.RemoveRange(s);
                //    db.SaveChanges();
                //    if (selectedSkills != null)
                //    {
                //        for (int i = 0; i < selectedSkills.Count(); i++)
                //        {
                //            var skills = new SkillsOfWorker();
                //            skills.Jobid = model.JobId;
                //            skills.Skillname = selectedSkills[i];
                //            skills.UserId = user;
                //            db.SkillsOfWorkers.Add(skills);
                //            db.SaveChanges();
                //        }
                //    }
                //}
                if (selectedSkills != null)
                {
                    model.workerskills = new List<worskills>();
                    for (int i = 0; i < selectedSkills.Count(); i++)
                    {
                        model.workerskills.Add(new worskills { Jobid = model.JobId, Skillname = selectedSkills[i], UserId = user });
                    }
                }
                if (selectedSkills == null)
                {
                    ViewBag.SkillsSelect = new SelectList(db.Skills.Where(x => x.Jobid == model.JobId), "Id", "Skillname");
                    ModelState.AddModelError("", "Please specify Skills / Service Offer");
                    return View(model);
                }
                Session["Model"] = model;
                return RedirectToAction("EditGetSkilledWorkerFile");
            }
            ViewBag.SkillsSelect = new SelectList(db.Skills.Where(x => x.Jobid == model.JobId), "Id", "Skillname");
            return View(model);
        }
        public ActionResult EditGetSkilledWorkerFile()
        {
            RegisterSkilledWorker workerdata = (RegisterSkilledWorker)Session["Model"];
            var Userid = User.Identity.GetUserId();
            var data = db.RegistWork.Where(x => x.Userid == Userid).FirstOrDefault();
                var workerfileimage = (from fileimage in db.RegistWorkFile
                                       where fileimage.Userid == Userid && fileimage.Jobid == workerdata.JobId
                                       select new
                                       {
                                           FileId = fileimage.Id,
                                           Fileimage = fileimage.FileName,
                                       }).ToList().Select(p => new SkilledWorkerFileImage()
                                       {
                                           id = p.FileId,
                                           FileName = p.Fileimage,
                                       });

                return View(workerfileimage);
        }
        public ActionResult GetSkilledWorkerFile()
        {
            RegisterSkilledWorker workerdata = (RegisterSkilledWorker)Session["Model"];
            var Userid = User.Identity.GetUserId();
            var data = db.RegistWork.Where(x => x.Userid == Userid).FirstOrDefault();
                var workerfileimage = (from fileimage in db.RegistWorkFile
                                       where fileimage.Userid == Userid && fileimage.Jobid == workerdata.JobId
                                       select new
                                       {
                                           FileId = fileimage.Id,
                                           Fileimage = fileimage.FileName,
                                       }).ToList().Select(p => new SkilledWorkerFileImage()
                                       {
                                           id = p.FileId,
                                           FileName = p.Fileimage,
                                       });

                return View(workerfileimage);
        }
        [HttpPost]
        public ActionResult EditGetSkilledWorkerFiles()
        {
            RegisterSkilledWorker workerdata = (RegisterSkilledWorker)Session["Model"];
            var UserId = User.Identity.GetUserId();
            var username = User.Identity.GetUserName();
            var data = db.RegistWorkFile.Where(x => x.Userid == UserId).FirstOrDefault();
            if (data != null)
            {
                var user = User.Identity.GetUserId();
                var userident = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
                if (workerdata.Firstname != null)
                {
                    userident.Firstname = workerdata.Firstname;
                }
                if (workerdata.Lastname != null)
                {
                    userident.Lastname = workerdata.Lastname;
                }
                if (workerdata.Phonenumber != null)
                {
                    var changePhoneNumberToken = UserManager.GenerateChangePhoneNumberToken(user, workerdata.Phonenumber);
                    var result = UserManager.ChangePhoneNumber(user, workerdata.Phonenumber, changePhoneNumberToken);
                }
                userident.Updated_At = DateTime.Now;
                if(workerdata.ProfilePicture != null)
                {
                    userident.ProfilePicture = workerdata.ProfilePicture;
                }
                var w = db.RegistWork.Where(x => x.Userid == user && workerdata.JobId == x.JobId).FirstOrDefault();
                if (w != null)
                {
                    w.worker_overview = workerdata.Overview;
                    w.worker_status = 1;
                    w.Updated_At = DateTime.Now;
                    w.Userid = User.Identity.GetUserId();
                    w.JobId = workerdata.JobId;
                    db.SaveChanges();
                }
                //if (w != null)
                //{
                //    w.worker_overview = model.Overview;
                //    w.worker_status = 3;
                //    w.Updated_At = DateTime.Now;
                //    w.JobId = model.JobId;
                //    db.SaveChanges();
                //}
                var l = db.Locations.Where(x => x.UserId == user && x.JobId == workerdata.JobId).FirstOrDefault();
                if (l != null)
                {
                    l.Loc_Address = workerdata.Address;
                    l.UserId = user;
                    l.Updated_At = DateTime.Now;
                    l.Geolocation = DbGeography.FromText("POINT( " + workerdata.Longitude + " " + workerdata.Latitude + " )");
                    db.SaveChanges();
                }
                //if (l != null)
                //{
                //    l.Loc_Address = model.Address;
                //    l.Updated_At = DateTime.Now;
                //    l.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
                //    db.SaveChanges();
                //}
                var s = db.SkillsOfWorkers.Where(x => x.UserId == user && workerdata.JobId == x.Jobid).Count();
                if (s != 0)
                {
                    var ss = db.SkillsOfWorkers.Where(x => x.UserId == user && workerdata.JobId == x.Jobid).ToList();
                    db.SkillsOfWorkers.RemoveRange(ss);
                    db.SaveChanges();
                    if (workerdata.workerskills != null)
                    {
                        for (int i = 0; i < workerdata.workerskills.Count(); i++)
                        {
                            var skills = new SkillsOfWorker();
                            skills.Jobid = workerdata.JobId;
                            skills.Skillname = workerdata.workerskills[i].Skillname;
                            skills.UserId = user;
                            db.SkillsOfWorkers.Add(skills);
                            db.SaveChanges();
                        }
                    }
                }
                //if (s != null)
                //{
                //    db.SkillsOfWorkers.RemoveRange(s);
                //    db.SaveChanges();
                //    if (selectedSkills != null)
                //    {
                //        for (int i = 0; i < selectedSkills.Count(); i++)
                //        {
                //            var skills = new SkillsOfWorker();
                //            skills.Jobid = model.JobId;
                //            skills.Skillname = selectedSkills[i];
                //            skills.UserId = user;
                //            db.SkillsOfWorkers.Add(skills);
                //            db.SaveChanges();
                //        }
                //    }
                //}
                var role = (from rolename in db.Roles where rolename.Name.Contains("admin") select rolename).FirstOrDefault();
                var admin = (from useres in db.Users where useres.Roles.Any(r => r.RoleId == role.Id) select new { username = useres.UserName }).FirstOrDefault();
                var notification = new NotificationModel
                {
                    Receiver = admin.username,
                    Title = $"A worker Editted His application form submitted by {username}",
                    Details = $"{username} Editted his worker application form and requested to be part of iAssist skilled worker",
                    DetailsURL = $"/Admin/ViewDetailsSkilledWorker?id={UserId}",
                    Date = DateTime.Now,
                    IsRead = false
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                NotificationHub objNotifHub = new NotificationHub();
                objNotifHub.SendNotification(notification.Receiver);
                return RedirectToAction("ViewSubmittedWorker");
            }
            TempData["Error"] = "Please Submitted Image of Proof";
            return RedirectToAction("GetSkilledWorkerFile");
        }
        public ActionResult EditUploadWorkerFile()
        {
            RegisterSkilledWorker workerdata = (RegisterSkilledWorker)Session["Model"];
            var Userid = User.Identity.GetUserId();

            return View();
        }
        [HttpPost]
        public ActionResult EditUploadWorkerFile(HttpPostedFileBase file)
        {
            RegisterSkilledWorker workerdata = (RegisterSkilledWorker)Session["Model"];
            //check the user has entered a file
            if (file != null)
            {
                //check if the file is valid
                if (ValidateFile(file))
                {
                    try
                    {
                        SaveFileToDisk(file);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("FileName", "Sorry an error occurred saving the file to disk, please try again");
                    }
                }
                else
                {
                    ModelState.AddModelError("FileName", "The file must be gif, png, jpeg or jpg and less than 2MB in size");
                }
            }
            else
            {
                //if the user has not entered a file return an error message
                ModelState.AddModelError("FileName", "Please choose a file");
            }
            if (ModelState.IsValid)
            {
                db.RegistWorkFile.Add(new WorkerRegImages { FileName = file.FileName, Userid = User.Identity.GetUserId(), Jobid = workerdata.JobId });
                db.SaveChanges();
                return RedirectToAction("EditGetSkilledWorkerFile");
            }
            return View();
        }
        public ActionResult EditDeleteSkilledWorkerFiles(int id)
        {
            var imageid = db.RegistWorkFile.Find(id);
            if (imageid != null)
            {
                db.RegistWorkFile.Remove(imageid);
            }
            db.SaveChanges();
            return RedirectToAction("EditGetSkilledWorkerFile");
        }
        public ActionResult DeleteSkilledWorkerFiles(int id)
        {
            var imageid = db.RegistWorkFile.Find(id);
            if (imageid != null)
            {
                db.RegistWorkFile.Remove(imageid);
            }
            db.SaveChanges();
            return RedirectToAction("GetSkilledWorkerFile");
        }
        [HttpPost]
        public ActionResult GetSkilledWorkerFiles()
        {
            RegisterSkilledWorker workerdata = (RegisterSkilledWorker)Session["Model"];
            var UserId = User.Identity.GetUserId();
            var username = User.Identity.GetUserName();
            var data = db.RegistWorkFile.Where(x => x.Userid == UserId).FirstOrDefault();
            if (data != null)
            {
                var user = User.Identity.GetUserId();
                var userident = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
                if (workerdata.Firstname != null)
                {
                    userident.Firstname = workerdata.Firstname;
                }
                if (workerdata.Lastname != null)
                {
                    userident.Lastname = workerdata.Lastname;
                }
                if (workerdata.Phonenumber != null)
                {
                    var changePhoneNumberToken = UserManager.GenerateChangePhoneNumberToken(user, workerdata.Phonenumber);
                    var result = UserManager.ChangePhoneNumber(user, workerdata.Phonenumber, changePhoneNumberToken);
                }
                userident.Updated_At = DateTime.Now;
                if(workerdata.ProfilePicture != null)
                {
                    userident.ProfilePicture = workerdata.ProfilePicture;
                }
                var w = db.RegistWork.Where(x => x.Userid == user && workerdata.JobId == x.JobId).FirstOrDefault();
                if (w == null)
                {
                    Work worker = new Work();
                    worker.worker_overview = workerdata.Overview;
                    worker.worker_status = 1;
                    worker.Created_At = DateTime.Now;
                    worker.Updated_At = DateTime.Now;
                    worker.Userid = User.Identity.GetUserId();
                    worker.JobId = workerdata.JobId;
                    db.RegistWork.Add(worker);
                    db.SaveChanges();
                }
                //if (w != null)
                //{
                //    w.worker_overview = model.Overview;
                //    w.worker_status = 3;
                //    w.Updated_At = DateTime.Now;
                //    w.JobId = model.JobId;
                //    db.SaveChanges();
                //}
                var l = db.Locations.Where(x => x.UserId == user && x.JobId == workerdata.JobId).FirstOrDefault();
                if (l == null)
                {
                    var location = new Location();
                    location.JobId = workerdata.JobId;
                    location.Loc_Address = workerdata.Address;
                    location.UserId = user;
                    location.Created_At = DateTime.Now;
                    location.Updated_At = DateTime.Now;
                    location.Geolocation = DbGeography.FromText("POINT( " + workerdata.Longitude + " " + workerdata.Latitude + " )");
                    db.Locations.Add(location);
                    db.SaveChanges();
                }
                //if (l != null)
                //{
                //    l.Loc_Address = model.Address;
                //    l.Updated_At = DateTime.Now;
                //    l.Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )");
                //    db.SaveChanges();
                //}
                var s = db.SkillsOfWorkers.Where(x => x.UserId == user && workerdata.JobId == x.Jobid).Count();
                if (s == 0)
                {
                    if (workerdata.workerskills != null)
                    {
                        for (int i = 0; i < workerdata.workerskills.Count(); i++)
                        {
                            var skills = new SkillsOfWorker();
                            skills.Jobid = workerdata.JobId;
                            skills.Skillname = workerdata.workerskills[i].Skillname;
                            skills.UserId = user;
                            db.SkillsOfWorkers.Add(skills);
                            db.SaveChanges();
                        }
                    }
                }
                //if (s != null)
                //{
                //    db.SkillsOfWorkers.RemoveRange(s);
                //    db.SaveChanges();
                //    if (selectedSkills != null)
                //    {
                //        for (int i = 0; i < selectedSkills.Count(); i++)
                //        {
                //            var skills = new SkillsOfWorker();
                //            skills.Jobid = model.JobId;
                //            skills.Skillname = selectedSkills[i];
                //            skills.UserId = user;
                //            db.SkillsOfWorkers.Add(skills);
                //            db.SaveChanges();
                //        }
                //    }
                //}
                var role = (from rolename in db.Roles where rolename.Name.Contains("admin") select rolename).FirstOrDefault();
                var admin = (from useres in db.Users where useres.Roles.Any(r => r.RoleId == role.Id) select new { username = useres.UserName }).FirstOrDefault();
                var notification = new NotificationModel
                {
                    Receiver = admin.username,
                    Title = $"New worker application form submitted by {username}",
                    Details = $"{username} submitted a worker application form and requested to be part of iAssist skilled worker",
                    DetailsURL = $"/Admin/ViewDetailsSkilledWorker?id={UserId}",
                    Date = DateTime.Now,
                    IsRead = false
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                NotificationHub objNotifHub = new NotificationHub();
                objNotifHub.SendNotification(notification.Receiver);
                return RedirectToAction("ViewSubmittedWorker");
            }
            TempData["Error"] = "Please Submitted Image of Proof";
            return RedirectToAction("GetSkilledWorkerFile");
        }
        public ActionResult UploadWorkerFile()
        {
            RegisterSkilledWorker workerdata = (RegisterSkilledWorker)Session["Model"];
            var Userid = User.Identity.GetUserId();
            var data = db.RegistWork.Where(x => x.Userid == Userid && x.JobId == workerdata.JobId).FirstOrDefault();
            if (data != null && data.worker_status == 1)
            {
                return RedirectToAction("ViewSubmittedWorker");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadWorkerFile(HttpPostedFileBase file)
        {
            RegisterSkilledWorker workerdata = (RegisterSkilledWorker)Session["Model"];
            //check the user has entered a file
            if (file != null)
            {
                //check if the file is valid
                if (ValidateFile(file))
                {
                    try
                    {
                        SaveFileToDisk(file);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("FileName", "Sorry an error occurred saving the file to disk, please try again");
                    }
                }
                else
                {
                    ModelState.AddModelError("FileName", "The file must be gif, png, jpeg or jpg and less than 2MB in size");
                }
            }
            else
            {
                //if the user has not entered a file return an error message
                ModelState.AddModelError("FileName", "Please choose a file");
            }
            if (ModelState.IsValid)
            {
                db.RegistWorkFile.Add(new WorkerRegImages { FileName = file.FileName, Userid = User.Identity.GetUserId(), Jobid = workerdata.JobId });
                db.SaveChanges();
                return RedirectToAction("GetSkilledWorkerFile");
            }
            return View();
        }
        public ActionResult ViewSubmittedWorker()
        {
            var UserId = User.Identity.GetUserId();
            var users = db.UsersIdentities.Where(x => x.Userid == UserId).FirstOrDefault();
            var useres = db.Users.Where(x => x.Id == UserId).FirstOrDefault();
            var loc = db.Locations.Where(x => x.UserId == UserId).FirstOrDefault();
            var inforegit = db.RegistWork.Where(x => x.Userid == UserId && x.worker_status == 1).FirstOrDefault();
            if(inforegit == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var infoR = db.JobCategories.Where(x => x.Id == inforegit.JobId).FirstOrDefault();
            var inforegist = new ViewSubmittedWorker();
            inforegist.JobId = inforegit.JobId;
            inforegist.Firstname = users.Firstname;
            inforegist.Phonenumber = useres.PhoneNumber;
            inforegist.ProfilePicture = users.ProfilePicture;
            inforegist.Lastname = users.Lastname;
            inforegist.Address = loc.Loc_Address;
            inforegist.Longitude = loc.Geolocation.Longitude.ToString();
            inforegist.Latitude = loc.Geolocation.Latitude.ToString();
            inforegist.Overview = inforegit.worker_overview;
            if (inforegit.worker_status == 1)
            {
                inforegist.Workerstatus = "Pending";
            }
            inforegist.Created_at = inforegit.Created_At;
            inforegist.Updated_at = inforegit.Updated_At;
            inforegist.Jobname = infoR.JobName;
            inforegist.workerskills = db.SkillsOfWorkers.Where(x => x.UserId == UserId && x.Jobid == inforegit.JobId).ToList().Select(p => new worskills { Jobid = p.Jobid, Skillname = p.Skillname, UserId = p.UserId });
            inforegist.SkilledWorkerImageFile = db.RegistWorkFile.Where(x => x.Userid == UserId && x.Jobid == inforegit.JobId).ToList().Select(p => new SkilledWorkerFileImage { FileName = p.FileName, id = p.Id });
            return View(inforegist);
        }
        private void SaveFileToDisk(HttpPostedFileBase file)
        {
            WebImage img = new WebImage(file.InputStream);
            img.Save("~/image/RegisterSkilledFile/Thumbnails" + file.FileName);
            if (img.Width > 190)
            {
                img.Resize(190, img.Height);
            }
            img.Save("~/image/RegisterSkilledFile/" + file.FileName);
        }
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
        //End Functions Applying User to be SkilledWorker
    }
}