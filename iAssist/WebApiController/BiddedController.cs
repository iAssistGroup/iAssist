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

namespace iAssist.WebApiControllers
{
    [Authorize]
    [RoutePrefix("api/Bid")]
    public class BiddedController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private string _errorMessage = "An Error has occurred.";
        private string _errorMessageNotFound = "Corresponding Data not found.";
        private string _successMessage = "Success";

        public BiddedController()
        {
        }

        public BiddedController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: Bid
        [HttpGet]
        [Route("CreateBidToTheTask")]
        public async Task<IHttpActionResult> CreateBidToTheTask(int id)
        {
            var bided = new BidViewModel();
            bided.TaskdetId = id;

            return Ok(bided);
        }
        [HttpPost]
        [Route("CreateBidToTheTask")]
        public async Task<IHttpActionResult> CreateBidToTheTask(BidViewModel model)
        {
            var userid = User.Identity.GetUserId();
            if(userid == null)
            {
                return BadRequest(_errorMessage);
            }
            var workerid = db.RegistWork.Where(x => x.Userid == userid).FirstOrDefault();
            if (workerid == null)
            {
                return BadRequest(_errorMessageNotFound);
            }

            if (ModelState.IsValid)
            {
                if(model.Bid_Amount == 0)
                {
                    return BadRequest("Please Specify Amount");
                }
                if (model.Bid_Amount < 100)
                {
                    return BadRequest("Amount must be greater than or equal to 100");
                }
                var bid = new Bid();
                bid.Bid_Amount = model.Bid_Amount;
                bid.Bid_Description = model.Bid_Description;
                bid.TaskDetId = model.TaskdetId;
                bid.Created_at = DateTime.Now;
                bid.Updated_at = bid.Created_at;
                bid.WorkerId = workerid.Id;
                db.Bids.Add(bid);
                db.SaveChanges();
                var taskdet = db.TaskDetails.Where(x => x.Id == model.TaskdetId).FirstOrDefault();
                var username = db.Users.Where(x => x.Id == taskdet.UserId).FirstOrDefault();
                var notification = new NotificationModel
                {
                    Receiver = username.UserName,
                    Title = $"Worker bid the task you post/requested",
                    Details = $"A worker bidded your task post/requested",
                    DetailsURL = "",
                    Date = DateTime.Now,
                    IsRead = false
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                return Ok("Success");
            }
            return BadRequest("Something Went Wrong");
        }
        [HttpGet]
        [Route("ViewBidding")]
        public async Task<IHttpActionResult> ViewBidding(int? id, int? user)
        {
            if (user == 1) //meaning User request to view bidding of workers or worker
            {
                var users = User.Identity.GetUserId();
                var workerids = db.RegistWork.Where(x => x.Userid == users).FirstOrDefault();
                if (id == null)
                {
                    return BadRequest(_errorMessage);
                }
                var bi = (from b in db.Bids
                          where b.TaskDetId == id && b.bid_status != 1
                          join taskdet in db.TaskDetails on b.TaskDetId equals taskdet.Id
                          join task in db.TaskBook on taskdet.Id equals task.TaskDetId
                          join worker in db.RegistWork on b.WorkerId equals worker.Id
                          join userworker in db.UsersIdentities on worker.Userid equals userworker.Userid
                          join username in db.Users on worker.Userid equals username.Id
                          select new
                          {
                              bidId = b.Id,
                              amount = b.Bid_Amount,
                              biddesc = b.Bid_Description,
                              tasktitle = taskdet.taskdet_name,
                              workerid = worker.Id,
                              workerfname = userworker.Firstname,
                              workerlname = userworker.Lastname,
                              workerpic = userworker.ProfilePicture,
                              taskworkerid = task.workerId,
                              bookstatus = b.bid_status,
                              username = username.UserName,
                              taskdetid = b.TaskDetId,
                          }).ToList();
                var averate = 0;
                List<BidViewModel> bidding = new List<BidViewModel>();
                foreach (var b in bi)
                {
                    int a = 0;
                    averate = 0;
                    var rat = db.Ratings.Where(x => x.WorkerID == b.workerid).ToList();
                    if (rat.Any() == true)
                    {
                        foreach (var i in rat)
                        {
                            a = a + i.Rate;
                        }
                        averate = a / db.Ratings.Where(x => x.WorkerID == b.workerid).Count();
                    }
                    var bidds = new BidViewModel();
                    bidds.Bidid = b.bidId;
                    bidds.Bid_Amount = b.amount;
                    bidds.Bid_Description = b.biddesc;
                    bidds.Tasktitle = b.tasktitle;
                    bidds.Firstname = b.workerfname;
                    bidds.Lastname = b.workerlname;
                    bidds.ProfilePicture = b.workerpic;
                    bidds.workerid = b.taskworkerid;
                    bidds.bookstatus = b.bookstatus;
                    bidds.Username = b.username;
                    bidds.TaskdetId = b.taskdetid;
                    bidds.Rate = averate;
                    bidds.user = user;
                    bidding.Add(bidds);
                }
                return Ok(bidding);
            }
            if (user == 2)//meaning Worker want to see his bidding on the task
            {
                var users = User.Identity.GetUserId();
                var workerids = db.RegistWork.Where(x => x.Userid == users).FirstOrDefault();
                if (id == null)
                {
                    return BadRequest(_errorMessageNotFound);
                }
                var bi = (from b in db.Bids
                          where b.TaskDetId == id && b.WorkerId == workerids.Id && b.bid_status != 1
                          join taskdet in db.TaskDetails on b.TaskDetId equals taskdet.Id
                          join task in db.TaskBook on taskdet.Id equals task.TaskDetId
                          join worker in db.RegistWork on b.WorkerId equals worker.Id
                          join userworker in db.UsersIdentities on worker.Userid equals userworker.Userid
                          join username in db.Users on worker.Userid equals username.Id
                          select new
                          {
                              bidId = b.Id,
                              amount = b.Bid_Amount,
                              biddesc = b.Bid_Description,
                              tasktitle = taskdet.taskdet_name,
                              workerid = worker.Id,
                              workerfname = userworker.Firstname,
                              workerlname = userworker.Lastname,
                              workerpic = userworker.ProfilePicture,
                              taskworkerid = task.workerId,
                              bookstatus = b.bid_status,
                              username = username.UserName,
                              taskdetid = b.TaskDetId,
                          }).ToList();
                var averate = 0;
                List<BidViewModel> bidding = new List<BidViewModel>();
                foreach (var b in bi)
                {
                    int a = 0;
                    averate = 0;
                    var rat = db.Ratings.Where(x => x.WorkerID == b.workerid).ToList();
                    if (rat.Any() == true)
                    {
                        foreach (var i in rat)
                        {
                            a = a + i.Rate;
                        }
                        averate = a / db.Ratings.Where(x => x.WorkerID == b.workerid).Count();
                    }
                    var bidds = new BidViewModel();
                    bidds.Bidid = b.bidId;
                    bidds.Bid_Amount = b.amount;
                    bidds.Bid_Description = b.biddesc;
                    bidds.Tasktitle = b.tasktitle;
                    bidds.Firstname = b.workerfname;
                    bidds.Lastname = b.workerlname;
                    bidds.ProfilePicture = b.workerpic;
                    bidds.workerid = b.taskworkerid;
                    bidds.bookstatus = b.bookstatus;
                    bidds.Username = b.username;
                    bidds.TaskdetId = b.taskdetid;
                    bidds.Rate = averate;
                    bidds.user = user;
                    bidding.Add(bidds);
                }
                return Ok(bidding);
            }
            return BadRequest(_errorMessage);
        }
        [HttpGet]
        [Route("EditBidding")]
        public async Task<IHttpActionResult> EditBidding(int id)
        {
            var bid = db.Bids.Where(x => x.Id == id).FirstOrDefault();
            var bidmodel = new BidViewModel();
            bidmodel.Bidid = id;
            bidmodel.Bid_Amount = bid.Bid_Amount;
            bidmodel.Bid_Description = bid.Bid_Description;
            return Ok(bidmodel);
        }
        [HttpPost]
        [Route("EditBidding")]
        public async Task<IHttpActionResult> EditBidding(BidViewModel model)
        {
            if(ModelState.IsValid)
            {
                if (model.Bid_Amount < 100)
                {
                    return BadRequest("Amount must be greater than or equal to 100 ");
                }

                var bidmodel = db.Bids.Where(x => x.Id == model.Bidid).FirstOrDefault();
                bidmodel.Bid_Amount = model.Bid_Amount;
                bidmodel.Bid_Description = model.Bid_Description;
                bidmodel.Updated_at = DateTime.Now;
                db.SaveChanges();
                return Ok(_successMessage);
            }
            return BadRequest("Please fill up the form correctly.");

        }
        [HttpGet]
        [Route("CancelBidding")]
        public async Task<IHttpActionResult> CancelBidding(int? id,int? taskid)
        {
            var users = User.Identity.GetUserId();
            var workerids = db.RegistWork.Where(x => x.Userid == users).FirstOrDefault();
                if (id == null)
                {
                    return BadRequest(_errorMessage);
                }
                var bid = db.Bids.Where(x => x.Id == id).FirstOrDefault();
                if (bid == null)
                {
                    return BadRequest(_errorMessageNotFound);
                }
                if(bid.WorkerId == workerids.Id)
                {
                    bid.bid_status = 1;
                    db.SaveChanges();
                    return Ok(_successMessage);
                }
                else
                {
                    bid.bid_status = 1;
                    var taskbook = db.TaskBook.Where(x => x.TaskDetId == taskid).FirstOrDefault();
                    taskbook.Taskbook_Status = 0;
                    taskbook.workerId = 0;
                    db.SaveChanges();
                    return Ok(_successMessage);
                }
        }

        [HttpGet]
        [Route("AcceptBid")]
        public async Task<IHttpActionResult> AcceptBid(int id, int? taskid)// User
        {
            var userid = User.Identity.GetUserId();
            var usersend = db.Users.Where(x => x.Id == userid).FirstOrDefault();
            var balance = db.Balance.Where(x => x.UserId == userid).FirstOrDefault();
            var bidid = db.Bids.Where(x => x.Id == id).FirstOrDefault();
            if (balance.Money <= 0 || balance.Money < bidid.Bid_Amount)
            {
                return BadRequest("You do not have enough balance to proceed with the transaction");
            }
            bidid.bid_status = 2;
            db.SaveChanges();
            var taskids = db.TaskDetails.Where(x => x.Id == taskid).FirstOrDefault();
            var taskbook = db.TaskBook.Where(x => x.TaskDetId == taskid).FirstOrDefault();
            taskbook.Taskbook_Status = 2;
            db.SaveChanges();
            var tasked = new Tasked();
            tasked.TaskCreated_at = DateTime.Now;
            tasked.TaskCompletionTime = DateTime.Now;
            tasked.TaskUpdated_at = DateTime.Now;
            tasked.TaskDetId = taskids.Id;
            tasked.TaskPayable = bidid.Bid_Amount;
            tasked.TaskStatus = 1; // going palang
            tasked.WorkerId = bidid.WorkerId;
            db.Taskeds.Add(tasked);
            db.SaveChanges();
            var workerid = db.RegistWork.Where(x => x.Id == bidid.WorkerId).FirstOrDefault();
            var username = db.Users.Where(x => x.Id == workerid.Userid).FirstOrDefault();
            var notification = new NotificationModel
            {
                Receiver = username.UserName,
                Title = $"{usersend.UserName} accepted your bid",
                Details = $"{usersend.UserName} accepted your bid and the task Added to your Contract Task",
                DetailsURL = "/Task/ViewContractTask",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
            return Ok(_successMessage);
        }
    }
}