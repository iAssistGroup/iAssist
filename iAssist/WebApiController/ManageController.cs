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

namespace iAssist.WebApiControllers
{
    [Authorize]
    [RoutePrefix("api/Manage")]
    public class ManageController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: /Manage/Index
        public async Task<IHttpActionResult> Index(ManageMessageId? message)
        {

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return Ok(model);
        }


        /*
        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        public async Task<IHttpActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }
        */

        // POST: /Manage/ChangePassword
        [Authorize]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("The password must be at least 6 characters long");
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return Ok("Password Change Successful.");
            }
            AddErrors(result);
            return BadRequest("Incorrect Password");
        }

        [Authorize]
        [HttpGet]
        [Route("UserProfile")]
        public async Task<IHttpActionResult> UserProfile()
        {
            var user = User.Identity.GetUserId();
            if (user == null)
            {
                return BadRequest();
            }
            var Useridentity = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
            var j = db.RegistWork.Where(x => x.Userid == user).FirstOrDefault();
            if (j == null)
            {
                var userprofile = new profile
                {
                    Firstname = Useridentity.Firstname,
                    Lastname = Useridentity.Lastname,
                    Created_At = Useridentity.Created_At,
                    Updated_At = Useridentity.Updated_At,
                    userid = user,
                    ProfilePicture = Useridentity.ProfilePicture,
                    Phonenumber = await UserManager.GetPhoneNumberAsync(user),
                    Email = await UserManager.GetEmailAsync(user),
                    Address = db.Locations.Where(x => x.UserId == user && x.JobId == null).Select(p => p.Loc_Address).FirstOrDefault(),
                    check = db.RegistWork.Where(x => x.Userid == user && x.worker_status == 1).Count(),
                };
                //return Ok(userprofile);

                var userprofileConvert = new
                {
                    Firstname = userprofile.Firstname,
                    Lastname = userprofile.Lastname,
                    Created_At = userprofile.Created_At,
                    Updated_At = userprofile.Updated_At,
                    Address = userprofile.Address,
                    ProfilePicture = userprofile.ProfilePicture,
                    Phonenumber = userprofile.Phonenumber,
                    Email = userprofile.Email,
                };
                return Ok(userprofileConvert);

            }
            var userprofiles = new profile
            {
                jobid = j.Id,
                Firstname = Useridentity.Firstname,
                Lastname = Useridentity.Lastname,
                Created_At = Useridentity.Created_At,
                Updated_At = Useridentity.Updated_At,
                userid = user,
                ProfilePicture = Useridentity.ProfilePicture,
                Phonenumber = await UserManager.GetPhoneNumberAsync(user),
                Address = db.Locations.Where(x => x.UserId == user && x.JobId == null).Select(p => p.Loc_Address).FirstOrDefault(),
                Email = await UserManager.GetEmailAsync(user),
                userworkdet = (from jr in db.RegistWork where jr.Userid == user && jr.worker_status == 0 join jjr in db.JobCategories on jr.JobId equals jjr.Id join ll in db.Locations on jr.Userid equals ll.UserId where jr.JobId == ll.JobId select new UsersWorkdet { Address = ll.Loc_Address, Overview = jr.worker_overview, JobId = jjr.Id, jobname = jjr.JobName, workid = jr.Id }).ToList(),
                workerskills = (from s in db.SkillsOfWorkers where s.UserId == j.Userid select new worskills { Skillname = s.Skillname, Jobid = s.Jobid }).ToList(),
                rateandFeedbacks = (from r in db.Ratings where r.WorkerID == j.Id select new RateandFeedback { Rate = r.Rate, jobid = r.Jobid, Feedback = r.Feedback, Username = r.UsernameFeedback, WorkerId = r.WorkerID }).ToList(),
                check = db.RegistWork.Where(x => x.Userid == user && x.worker_status == 1).Count(),
            };
            var userprofileConvert1 = new
            {
                Firstname = userprofiles.Firstname,
                Lastname = userprofiles.Lastname,
                Created_At = userprofiles.Created_At,
                Updated_At = userprofiles.Updated_At,
                Address = userprofiles.Address,
                ProfilePicture = userprofiles.ProfilePicture,
                Phonenumber = userprofiles.Phonenumber,
                Email = userprofiles.Email,
            };
            return Ok(userprofileConvert1);

        }

        [Authorize]
        [HttpGet]
        [Route("EditUserProfile")]
        public async Task<IHttpActionResult> EditUserProfile()
        {
            var user = User.Identity.GetUserId();
            if (user == null)
            {
                return BadRequest();
            }
            var Useridentity = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
            var userprofile = new profileWebApi
            {
                Firstname = Useridentity.Firstname,
                Lastname = Useridentity.Lastname,
                Created_At = Useridentity.Created_At,
                Updated_At = Useridentity.Updated_At,
                userid = user,
                ProfilePicture = Useridentity.ProfilePicture,
                Phonenumber = await UserManager.GetPhoneNumberAsync(user),
                Email = await UserManager.GetEmailAsync(user)
            };
            var userprofileConvert = new
            {
                Firstname = userprofile.Firstname,
                Lastname = userprofile.Lastname,
                Email = userprofile.Email,
                userid = user,
                Phonenumber = userprofile.Phonenumber,
            };
            return Ok(userprofileConvert);
        }

        [Authorize]
        [HttpPost]
        [Route("EditUserProfile")]
        public async Task<IHttpActionResult> EditUserProfile(profileWebApi profile)
        {
            if (ModelState.IsValid)
            {
                var user = User.Identity.GetUserId();
                var userident = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
                if (profile.Firstname != null)
                {
                    userident.Firstname = profile.Firstname;
                }
                if (profile.Lastname != null)
                {
                    userident.Lastname = profile.Lastname;
                }
                if (profile.Phonenumber != null)
                {
                    var changePhoneNumberToken = await UserManager.GenerateChangePhoneNumberTokenAsync(user, profile.Phonenumber);
                    var result = await UserManager.ChangePhoneNumberAsync(user, profile.Phonenumber, changePhoneNumberToken);
                }
                userident.Updated_At = DateTime.Now;
                /*if (profile.ImageFile != null && ValidateFile(profile.ImageFile) == true)
                {
                    string filename = Path.GetFileNameWithoutExtension(profile.ImageFile.FileName);
                    string extension = Path.GetExtension(profile.ImageFile.FileName);
                    filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                    profile.ProfilePicture = "~/Image/" + filename;
                    filename = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/image/"), filename);
                    userident.ProfilePicture = profile.ProfilePicture;
                    profile.ImageFile.SaveAs(filename);
                }*/
                db.SaveChanges();
                return Ok();
            }
            return BadRequest("Please Fill up the form correctly");
        }

        [Authorize]
        [HttpPost]
        [Route("UploadProfilePicture")]
        public async Task<string> UploadProfilePicture()
        {
            var user = User.Identity.GetUserId();
            var userident = db.UsersIdentities.Where(x => x.Userid == user).FirstOrDefault();
            userident.Updated_At = DateTime.Now;

            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        var fileName = postedFile.FileName.Split('\\').LastOrDefault().Split('/').LastOrDefault();

                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string extension = Path.GetExtension(fileName);

                        fileName = DateTime.Now.ToString("yymmssfff") + fileName;

                        var filePath = HttpContext.Current.Server.MapPath("~/image/" + fileName);

                        userident.ProfilePicture = "" + fileName;
                        //userident.ImageFile.SaveAs(filePath);
                        postedFile.SaveAs(filePath);
                        db.SaveChanges();
                        string temp = "image/" + fileName;
                        return temp;
                    }
                }
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
            return "no files";
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
        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return Request.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}