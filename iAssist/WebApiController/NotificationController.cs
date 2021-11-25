using iAssist.Models;
using iAssist.WebApiModels;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace iAssist.WebApiControllers
{
    [Authorize]
    [RoutePrefix("api/Notifications")]
    public class NotificationController : ApiController
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
        // GET: Admin

        // GET: Notification

        [HttpGet]
        [Route("Notifications")]
        public async Task<IHttpActionResult> Notifications()
        {
            var users = User.Identity.GetUserName();
            var seennotif = db.Notifications.Where(x => x.Receiver == users).ToList();
            foreach(var t in seennotif)
            {
                t.IsRead = true;
            }
            db.SaveChanges();
            List<NotificationViewModel> notif = (from t in db.Notifications
                                                 where t.Receiver == users
                                                 orderby t.Date descending
                                                 select new NotificationViewModel
                                                 {
                                                     Details = t.Details,
                                                     Title = t.Title,
                                                     DetailsURL = t.DetailsURL,
                                                     Receiver = t.Receiver,
                                                     Date = t.Date,
                                                     IsRead = t.IsRead
                                                 }).ToList();
            return Ok(notif);
        }
    }
}