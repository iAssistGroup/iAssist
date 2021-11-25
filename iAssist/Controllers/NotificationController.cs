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
    [Authorize]
    public class NotificationController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public NotificationController()
        {
        }

        public NotificationController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: Admin
        
        // GET: Notification
        public ActionResult Index()
        {
            var users = User.Identity.GetUserName();
            var seennotif = db.Notifications.Where(x => x.Receiver == users).ToList();
            foreach(var t in seennotif)
            {
                t.IsRead = true;
            }
            db.SaveChanges();
            var notif = (from t in db.Notifications
                         where t.Receiver == users
                         orderby t.Date descending
                         select new 
                         { 
                            details = t.Details,
                            title = t.Title,
                            detailurl = t.DetailsURL,
                            receiver = t.Receiver,
                            date = t.Date,
                            isread = t.IsRead
                         })
                        .ToList().Select(p => new NotificationViewModel() 
                        { 
                            Details = p.details,
                            Title = p.title,
                            DetailsURL = p.detailurl,
                            Receiver = p.receiver,
                            Date = p.date,
                            IsRead = p.isread
                        });
            return View(notif);
        }
    }
}