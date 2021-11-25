using iAssist.Models;
using iAssist.WebApiModels;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.IO;

namespace iAssist.WebApiControllers
{
    [Authorize]
    [RoutePrefix("api/Complaint")]
    public class ComplainController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private string _errorMessage = "An Error has occurred.";
        private string _errorMessageNotFound = "Corresponding Data not found.";
        private string _successMessage = "Success";

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
        // GET: Report
        [HttpGet]
        [Route("ReportWorker")]
        public async Task<IHttpActionResult> ReportWorker(int? id)
        {
            var user = User.Identity.GetUserId();
            var worker = db.RegistWork.Where(x => x.Id == id).FirstOrDefault();
            if(worker != null && worker.Userid == user)
            {
                return BadRequest(_errorMessageNotFound);
            }
            var complaints = new ComplaintModel();
            complaints.Workerid = (int)id;
            return Ok(complaints);
        }
        [HttpPost]
        [Route("ReportWorker")]
        public async Task<IHttpActionResult> ReportWorker(ComplaintModel model)
        {
            if(ModelState.IsValid)
            {
                var complaints = new Complaint();
                var user = User.Identity.GetUserId();
                //if (model.ImageFile != null && ValidateFile(model.ImageFile) == true)
                //{
                //    string filename = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                //    string extension = Path.GetExtension(model.ImageFile.FileName);
                //    filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                //    model.image = "~/Image/" + filename;
                //    filename = ConstantVariables.BaseURL + "image/" + filename;
                //    complaints.compimage = model.image;
                //    model.ImageFile.SaveAs(filename);
                //}
                complaints.compimage = model.image;
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
                return Ok(_successMessage);
            }
            return BadRequest(_errorMessage);
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