using iAssist.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iAssist.Controllers
{
    [Authorize]
    public class ComplainController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ComplainController()
        {
        }

        public ComplainController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: Report
        public ActionResult ReportWorker(int? id)
        {
            var user = User.Identity.GetUserId();
            var worker = db.RegistWork.Where(x => x.Id == id).FirstOrDefault();
            if(worker != null && worker.Userid == user)
            {
                return View("Error");
            }
            var complaints = new ComplainViews();
            ViewBag.Id = id;
            complaints.Workerid = ViewBag.Id;
            return View(complaints);
        }
        [HttpPost]
        public ActionResult ReportWorker(ComplainViews model)
        {
            if(ModelState.IsValid)
            {
                var complaints = new Complaint();
                var user = User.Identity.GetUserId();
                if (model.ImageFile != null && ValidateFile(model.ImageFile) == true)
                {
                    string filename = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                    string extension = Path.GetExtension(model.ImageFile.FileName);
                    filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                    model.image = "~/Image/" + filename;
                    filename = Path.Combine(Server.MapPath("~/image/"), filename);
                    complaints.compimage = model.image;
                    model.ImageFile.SaveAs(filename);
                }
                complaints.ComplaintTitle = model.ComplainType;
                complaints.Created_at = DateTime.Now;
                complaints.Desc = model.Description;
                complaints.Updated_at = DateTime.Now;
                complaints.UserId = user;
                complaints.WorkerId = model.Workerid;
                db.Complaints.Add(complaints);
                db.SaveChanges();
                var role = (from rolename in db.Roles where rolename.Name.Contains("admin") select rolename).FirstOrDefault();
                var admin = (from us in db.Users where us.Roles.Any(r => r.RoleId == role.Id) select new { username = us.UserName }).FirstOrDefault();
                var use = User.Identity.GetUserId();
                var ue = db.Users.Where(x => x.Id == use).FirstOrDefault();
                var notification = new NotificationModel
                {
                    Receiver = admin.username,
                    Title = $"{ue} Submitted a report",
                    Details = $"{ue} submitted a report / complain on a worker",
                    DetailsURL = $"/Admin/ManageUserComplaints",
                    Date = DateTime.Now,
                    IsRead = false
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                return RedirectToAction("ViewDetailsChoosenWorker","Home", new { id = model.Workerid});
            }
            return View(model);
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
    }
}