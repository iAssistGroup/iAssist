using iAssist.Models;
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
using System.Web.Http.Description;
using System.Web.Routing;
using iAssist.WebApiModels;
using System.Runtime.Remoting.Messaging;
using Microsoft.Ajax.Utilities;
using iAssist.Utility;
using System.IO;

namespace iAssist.WebApiControllers
{
    [Authorize]
    [RoutePrefix("api/Mobile/Account")]
    public class AccountMobileController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountMobileController()
        {
        }

        public AccountMobileController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        [ResponseType(typeof(string))]
        ///This function is not in use
        public async Task<IHttpActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            // Require the user to have a confirmed email before they can log on.

            var users = await UserManager.FindByNameAsync(model.Email);
            if (users != null)
            {
                if (!await UserManager.IsEmailConfirmedAsync(users.Id))
                {
                    return BadRequest("You must have a confirmed email to log on.");
                }
                var user = await UserManager.FindAsync(model.Email, model.Password);
                var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:
                        if (UserManager.IsInRole(user.Id, "Worker"))
                        {
                            return Ok("Worker");
                        }
                        else
                        {
                            return Ok("Employer");
                        }
                    case SignInStatus.LockedOut:
                        return BadRequest("Lockout");
                    case SignInStatus.RequiresVerification:
                        return Ok("Email must be verified to log on.");
                    case SignInStatus.Failure:
                    default:
                        return BadRequest("Invalid login attempt.");
                }
            }
            return Ok("Model State Error");
        }




        //GET: / Account/Role
        [HttpGet]
        [AllowAnonymous]
        [Route("Role")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> Role(string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model State Invalid.");
            }
            var user = await UserManager.FindByNameAsync(email);
            if (user == null)
            {
                return BadRequest("User does not exist.");
            }
            else
            {
                if (UserManager.IsInRole(user.Id, "Worker"))
                {
                    UserRole data = new UserRole();
                    data.Role = "Worker";
                    data.WorkerId = (from t in db.RegistWork
                         where t.Userid == user.Id
                         select t.Id).First();

                    return Ok(data);
                }
                else
                {
                    UserRole data = new UserRole();
                    data.Role = "Employer";

                    return Ok(data);
                }
            }
        }

        // POST: /Account/GetBalance
        [HttpGet]
        [AllowAnonymous]
        [Route("GetBalance")]
        public async Task<IHttpActionResult> GetBalance()
        {
            string userid = User.Identity.GetUserId();
            decimal balance = db.Balance.Where(x => x.UserId == userid).FirstOrDefault().Money;
            return Ok(balance);
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterViewModel model)
        {
            string output = "";
            if (ModelState.IsValid)
            {
                var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var roleManager = Request.GetOwinContext().Get<ApplicationRoleManager>();
                var roleName = "User";
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber };
                var result = await UserManager.CreateAsync(user, model.Password);
                var role = roleManager.FindByName(roleName);
                if (role == null)
                {
                    role = new IdentityRole(roleName);
                    var roleresult = roleManager.Create(role);
                }
                if (result.Succeeded)
                {
                    var db = new ApplicationDbContext();
                    var usersidentityinformation = new users { Firstname = model.Firstname, Lastname = model.Lastname, Userid = user.Id, Created_At = DateTime.Now, Updated_At = DateTime.Now };
                    db.UsersIdentities.Add(usersidentityinformation);
                    db.SaveChanges();
                    var userwallet = new Wallet { Money = 0, UserId = user.Id };
                    db.Balance.Add(userwallet);
                    db.SaveChanges();
                    var location = new Location { Loc_Address = model.Address, UserId = user.Id, Created_At = DateTime.Now, Updated_At = DateTime.Now, Geolocation = DbGeography.FromText("POINT( " + model.Longitude + " " + model.Latitude + " )") };
                    db.Locations.Add(location);
                    db.SaveChanges();
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    var rolesForuser = userManager.GetRoles(user.Id);
                    if (!rolesForuser.Contains(role.Name))
                    {
                        var results = userManager.AddToRole(user.Id, role.Name);
                    }

                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                    System.Web.Mvc.UrlHelper urlHelper = new System.Web.Mvc.UrlHelper(HttpContext.Current.Request.RequestContext, RouteTable.Routes);
                    string callbackUrl = urlHelper.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, code = code },
                    HttpContext.Current.Request.Url.Scheme
                    );
                    //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/MailTemplate/AccountConfirmation.html")))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("{ConfirmationLink}", callbackUrl);
                    body = body.Replace("{UserName}", model.Email);
                    bool IsSendEmail = SendEmail.EmailSend(model.Email, "Confirm your account", body, true);
                    if (IsSendEmail)
                    {
                        return Ok("Please confirm your account. We sent an Email to confirm your account");
                    }
                    return BadRequest("An Error has occured");
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        if (error.Contains("Email"))
                            output = error;
                    }
                    if(output.IsNullOrWhiteSpace())
                    {
                        if(string.IsNullOrEmpty(result.Errors.FirstOrDefault()) == false)
                        output = result.Errors.FirstOrDefault();
                    }
                    return BadRequest(output);
                }
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("Something went wrong, please try again later");
        }


        // GET: Notification
        // POST: /Account/Register
        [HttpGet]
        [Route("Notifications")]
        public async Task<IHttpActionResult> Notifications(UserModel model)
        {
            var users = User.Identity.GetUserName();
            var seennotif = db.Notifications.Where(x => x.Receiver == users).ToList();
            foreach (var t in seennotif)
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
            return Ok(notif);
        }

        // GET: Address
        // POST: /Account/Address
        [HttpGet]
        [Route("Address")]
        public async Task<IHttpActionResult> Address()
        {
            var users = User.Identity.GetUserId();
            db.SaveChanges();
            var address = (from t in db.Locations
                         where t.UserId == users
                         select new
                         {
                             Loc_Address = t.Loc_Address,
                             Longitude = t.Geolocation.Longitude,
                             Latitude = t.Geolocation.Latitude,
                         })
                        .ToList().Select(p => new UserAddress()
                        {
                            Address = p.Loc_Address,
                            Longitude = p.Longitude,
                            Latitude = p.Latitude,
                        });
            return Ok(address);
        }
    }
}
