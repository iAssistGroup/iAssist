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
    public class TransactionsController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public TransactionsController()
        {
        }

        public TransactionsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: Transactions
        public ActionResult TransIndex()
        {
            var user = User.Identity.GetUserId();
            var userinfo = db.Users.Where(x => x.Id == user).FirstOrDefault();
            if(User.IsInRole("admin"))
            {
                var transaction = db.TransactionHistories.ToList();
                return View(transaction);
            }
               var utransaction = db.TransactionHistories.Where(x => x.Payer == userinfo.UserName || x.Reciever == userinfo.UserName).ToList();
            return View(utransaction);
        }
    }
}