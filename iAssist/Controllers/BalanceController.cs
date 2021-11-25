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
    public class BalanceController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public BalanceController()
        {
        }

        public BalanceController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: Balance
        public ActionResult Index()
        {
            var userid = User.Identity.GetUserId();
            var balance = db.Balance.Where(x => x.UserId == userid).FirstOrDefault();
            return View(balance);
        }
        public PartialViewResult Summary()
        {
            var userid = User.Identity.GetUserId();
            var balance = db.Balance.Where(x => x.UserId == userid).FirstOrDefault();
            return PartialView(balance);
        }
        public ActionResult AddBalance()
        {
            return View();
        }
        public ActionResult WithdrawBalance()
        {
            var userid = User.Identity.GetUserId();
            var user = db.Users.Where(x => x.Id == userid).FirstOrDefault();
            var balance = db.Balance.Where(x => x.UserId == userid).FirstOrDefault();
            var withmodel = new WithDrawRequest();
            withmodel.UserId = userid;
            withmodel.Money = balance.Money;
            withmodel.Username = user.UserName;
            return View(withmodel);
        }
        [HttpPost]
        public ActionResult WithdrawBalance(WithDrawRequest model)
        {
            var userid = User.Identity.GetUserId();
            var user = db.Users.Where(x => x.Id == userid).FirstOrDefault();
            var balance = db.Balance.Where(x => x.UserId == userid).FirstOrDefault();
            var admin = db.Users.ToList().Where(x => UserManager.IsInRole(x.Id, "admin")).FirstOrDefault();
            if (model.Money > balance.Money||model.Money == 0)
            {
                ModelState.AddModelError("", "Invalid Withdraw Amount");
                return View(model);
            }
            model.status = false;
            db.Withdraw.Add(model);
            db.SaveChanges();
            var notification = new NotificationModel
            {
                Receiver = admin.UserName,
                Title = $"Worker {user.UserName} request to withdraw is balance",
                Details = $"The worker request to withdraw his balance amount of {model.Money}",
                DetailsURL = $"",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
            return View("Index");
        }

    }
}