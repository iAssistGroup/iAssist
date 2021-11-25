using iAssist.Models;
using iAssist.WebApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Data.Entity.Spatial;
using System.IO;
using iAssist.Hubs;

namespace iAssist.WebApiControllers
{
    [Authorize]
    [RoutePrefix("api/Transactions")]
    public class TransactionsController : ApiController
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
        // GET: Transactions
        [HttpGet]
        [Route("Transactions")]
        public async Task<IHttpActionResult> Transactions()
        {
            var user = User.Identity.GetUserId();
            var userinfo = db.Users.Where(x => x.Id == user).FirstOrDefault();
            if(User.IsInRole("admin"))
            {
                var transaction = db.TransactionHistories.ToList();
                return Ok(transaction);
            }
               var utransaction = db.TransactionHistories.Where(x => x.Payer == userinfo.UserName || x.Reciever == userinfo.UserName).ToList();
            return Ok(utransaction);
        }
    }
}