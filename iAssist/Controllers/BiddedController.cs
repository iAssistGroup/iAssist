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
    public class BiddedController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

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
        // GET: Bid
        public ActionResult CreateBidToTheTask(int id)
        {
            var bided = new BidViewModel();
            bided.TaskdetId = id;

            return View(bided);
        }
        [HttpPost]
        public ActionResult CreateBidToTheTask(BidViewModel model)
        {
            var userid = User.Identity.GetUserId();
            if(userid == null)
            {
                return View("Error");
            }
            var workerid = db.RegistWork.Where(x => x.Userid == userid).FirstOrDefault();
            if (workerid == null)
            {
                return View("Error");
            }

            if (ModelState.IsValid)
            {
                if(model.Bid_Amount == 0)
                {
                    ModelState.AddModelError("", "Please Specify Amount");
                    return View(model);
                }
                if (model.Bid_Amount < 100)
                {
                    ModelState.AddModelError("", "Amount must be greater than or equal to 100");
                    return View(model);
                }
                var bid = new Bid();
                bid.Bid_Amount = model.Bid_Amount;
                bid.Bid_Description = model.Bid_Description;
                bid.TaskDetId = model.TaskdetId;
                bid.BidTimeExp = model.BidTimeExp;
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
                    DetailsURL = $"/Bidded/ViewBidding/{model.TaskdetId}?user=1",
                    Date = DateTime.Now,
                    IsRead = false
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                return RedirectToAction("ViewBiddedRequestTask", "Task");
            }
            ModelState.AddModelError("", "Something Went Wrong");
            return View(model);
        }
        public ActionResult ViewBidding(int? id, int? user,string category, decimal?minimum, decimal? maximum)
        {
            if (user == 1) //meaning User request to view bidding of workers or worker
            {
                var users = User.Identity.GetUserId();
                var workerids = db.RegistWork.Where(x => x.Userid == users).FirstOrDefault();
                if (id == null)
                {
                    return View("Error");
                }
                var bi = (from b in db.Bids
                          where b.TaskDetId == id && b.bid_status != 1 && b.bid_status != -1 && b.bid_status != -1
                          join taskdet in db.TaskDetails on b.TaskDetId equals taskdet.Id
                          join task in db.TaskBook on taskdet.Id equals task.TaskDetId
                          join worker in db.RegistWork on b.WorkerId equals worker.Id
                          join userworker in db.UsersIdentities on worker.Userid equals userworker.Userid
                          join username in db.Users on worker.Userid equals username.Id
                          select new
                          {
                              bidId = b.Id,
                              bidstat = b.bid_status,
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
                              bidtimeexp = b.BidTimeExp,
                          }).ToList();
                foreach(var exbid in bi)
                {
                    if(exbid.bidstat != -1 && DateTime.Compare(DateTime.Now, exbid.bidtimeexp)>0)
                    {
                        var ebid = db.Bids.Where(x=>x.Id == exbid.bidId).FirstOrDefault();
                        ebid.bid_status = -1;
                        db.SaveChanges();
                        var notification = new NotificationModel
                        {
                            Receiver = exbid.username,
                            Title = $"The bid {exbid.biddesc} has expired",
                            Details = $"The bid has already expired.",
                            DetailsURL = $"",
                            Date = DateTime.Now,
                            IsRead = false
                        };
                        db.Notifications.Add(notification);
                        db.SaveChanges();
                    }
                }
                var averate = 0;
                ViewBag.Id = id;
                ViewBag.user = user;
                ViewBag.checkuser = user;
                List<BidViewModel> bidding = new List<BidViewModel>();
                foreach(var b in bi)
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
                    bidds.BidTimeExp = b.bidtimeexp;
                    bidds.Username = b.username;
                    bidds.TaskdetId = b.taskdetid;
                    bidds.Rate = averate;
                    bidds.user = user;
                    bidding.Add(bidds);
                }
                if(minimum > 0 && maximum > 0)
                {
                    int rate = 0;
                    List<ShowposttaskcategoryViewModel> categors = new List<ShowposttaskcategoryViewModel>();
                    var cats = new ShowposttaskcategoryViewModel();
                    if (category != "" && category != null)
                    {
                        if (category == "5 Stars")
                        {
                            rate = 5;
                        }
                        if (category == "4 Stars")
                        {
                            rate = 4;
                        }
                        if (category == "3 Stars")
                        {
                            rate = 3;
                        }
                        if (category == "2 Stars")
                        {
                            rate = 2;
                        }
                        if (category == "1 Star")
                        {
                            rate = 1;
                        }
                        cats.CategoryName = "5 Stars";
                        cats.Id = 0;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "4 Stars";
                        cats.Id = 1;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "3 Stars";
                        cats.Id = 2;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "2 Stars";
                        cats.Id = 3;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "1 Star";
                        cats.Id = 4;
                        categors.Add(cats);
                        ViewBag.Category = new SelectList(categors.Select(p => p.CategoryName).ToList().Distinct());
                        var bidmax = bidding.Where(x => x.Bid_Amount <= maximum && x.Bid_Amount >= minimum && x.Rate == rate).ToList();
                        return View(bidmax);
                    }
                    if (category == "5 Stars")
                    {
                        rate = 5;
                    }
                    if (category == "4 Stars")
                    {
                        rate = 4;
                    }
                    if (category == "3 Stars")
                    {
                        rate = 3;
                    }
                    if (category == "2 Stars")
                    {
                        rate = 2;
                    }
                    if (category == "1 Star")
                    {
                        rate = 1;
                    }
                    cats.CategoryName = "5 Stars";
                    cats.Id = 0;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "4 Stars";
                    cats.Id = 1;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "3 Stars";
                    cats.Id = 2;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "2 Stars";
                    cats.Id = 3;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "1 Star";
                    cats.Id = 4;
                    categors.Add(cats);
                    ViewBag.Category = new SelectList(categors.Select(p => p.CategoryName).ToList().Distinct());
                    var bidmaxs = bidding.Where(x => x.Bid_Amount <= maximum && x.Bid_Amount >= minimum).ToList();
                    return View(bidmaxs);
                }
                if(minimum > 0 && maximum == 0 || minimum > 0 && maximum == null)
                {
                    int rate = 0;
                    List<ShowposttaskcategoryViewModel> categors = new List<ShowposttaskcategoryViewModel>();
                    var cats = new ShowposttaskcategoryViewModel();
                    if (category != "" && category != null)
                    {
                        if (category == "5 Stars")
                        {
                            rate = 5;
                        }
                        if (category == "4 Stars")
                        {
                            rate = 4;
                        }
                        if (category == "3 Stars")
                        {
                            rate = 3;
                        }
                        if (category == "2 Stars")
                        {
                            rate = 2;
                        }
                        if (category == "1 Star")
                        {
                            rate = 1;
                        }
                        cats.CategoryName = "5 Stars";
                        cats.Id = 0;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "4 Stars";
                        cats.Id = 1;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "3 Stars";
                        cats.Id = 2;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "2 Stars";
                        cats.Id = 3;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "1 Star";
                        cats.Id = 4;
                        categors.Add(cats);
                        ViewBag.Category = new SelectList(categors.Select(p => p.CategoryName).ToList().Distinct());
                        var bidmax = bidding.Where(x => x.Bid_Amount >= minimum && x.Rate == rate).ToList();
                        return View(bidmax);
                    }
                    if (category == "5 Stars")
                    {
                        rate = 5;
                    }
                    if (category == "4 Stars")
                    {
                        rate = 4;
                    }
                    if (category == "3 Stars")
                    {
                        rate = 3;
                    }
                    if (category == "2 Stars")
                    {
                        rate = 2;
                    }
                    if (category == "1 Star")
                    {
                        rate = 1;
                    }
                    cats.CategoryName = "5 Stars";
                    cats.Id = 0;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "4 Stars";
                    cats.Id = 1;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "3 Stars";
                    cats.Id = 2;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "2 Stars";
                    cats.Id = 3;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "1 Star";
                    cats.Id = 4;
                    categors.Add(cats);
                    ViewBag.Category = new SelectList(categors.Select(p => p.CategoryName).ToList().Distinct());
                    var bidmaxs = bidding.Where(x => x.Bid_Amount >= minimum).ToList();
                    return View(bidmaxs);
                }
                if(minimum == 0 && maximum > 0 || minimum == null && maximum > 0)
                {
                    int rate = 0;
                    List<ShowposttaskcategoryViewModel> categors = new List<ShowposttaskcategoryViewModel>();
                    var cats = new ShowposttaskcategoryViewModel();
                    if (category != "" && category != null)
                    {
                        if (category == "5 Stars")
                        {
                            rate = 5;
                        }
                        if (category == "4 Stars")
                        {
                            rate = 4;
                        }
                        if (category == "3 Stars")
                        {
                            rate = 3;
                        }
                        if (category == "2 Stars")
                        {
                            rate = 2;
                        }
                        if (category == "1 Star")
                        {
                            rate = 1;
                        }
                        cats.CategoryName = "5 Stars";
                        cats.Id = 0;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "4 Stars";
                        cats.Id = 1;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "3 Stars";
                        cats.Id = 2;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "2 Stars";
                        cats.Id = 3;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "1 Star";
                        cats.Id = 4;
                        categors.Add(cats);
                        ViewBag.Category = new SelectList(categors.Select(p => p.CategoryName).ToList().Distinct());
                        var bidmax = bidding.Where(x => x.Bid_Amount <= maximum&& x.Rate == rate).ToList();
                        return View(bidmax);
                    }
                    if (category == "5 Stars")
                    {
                        rate = 5;
                    }
                    if (category == "4 Stars")
                    {
                        rate = 4;
                    }
                    if (category == "3 Stars")
                    {
                        rate = 3;
                    }
                    if (category == "2 Stars")
                    {
                        rate = 2;
                    }
                    if (category == "1 Star")
                    {
                        rate = 1;
                    }
                    cats.CategoryName = "5 Stars";
                    cats.Id = 0;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "4 Stars";
                    cats.Id = 1;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "3 Stars";
                    cats.Id = 2;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "2 Stars";
                    cats.Id = 3;
                    categors.Add(cats);
                    cats = new ShowposttaskcategoryViewModel();
                    cats.CategoryName = "1 Star";
                    cats.Id = 4;
                    categors.Add(cats);
                    ViewBag.Category = new SelectList(categors.Select(p => p.CategoryName).ToList().Distinct());
                    var bidmaxs = bidding.Where(x => x.Bid_Amount <= maximum).ToList();
                    return View(bidmaxs);
                }
                if (minimum == 0 && maximum == 0 || minimum == null && maximum == null)
                {
                    int rate = 0;
                    List<ShowposttaskcategoryViewModel> categors = new List<ShowposttaskcategoryViewModel>();
                    var cats = new ShowposttaskcategoryViewModel();
                    if (category != "" && category != null)
                    {
                        if (category == "5 Stars")
                        {
                            rate = 5;
                        }
                        if (category == "4 Stars")
                        {
                            rate = 4;
                        }
                        if (category == "3 Stars")
                        {
                            rate = 3;
                        }
                        if (category == "2 Stars")
                        {
                            rate = 2;
                        }
                        if (category == "1 Star")
                        {
                            rate = 1;
                        }
                        cats.CategoryName = "5 Stars";
                        cats.Id = 0;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "4 Stars";
                        cats.Id = 1;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "3 Stars";
                        cats.Id = 2;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "2 Stars";
                        cats.Id = 3;
                        categors.Add(cats);
                        cats = new ShowposttaskcategoryViewModel();
                        cats.CategoryName = "1 Star";
                        cats.Id = 4;
                        categors.Add(cats);
                        ViewBag.Category = new SelectList(categors.Select(p => p.CategoryName).ToList().Distinct());
                        var bidmax = bidding.Where(x => x.Rate == rate).ToList();
                        return View(bidmax);
                    }
                }
                List<ShowposttaskcategoryViewModel> categor = new List<ShowposttaskcategoryViewModel>();
                var cat = new ShowposttaskcategoryViewModel();
                cat.CategoryName = "5 Stars";
                cat.Id = 0;
                categor.Add(cat);
                cat = new ShowposttaskcategoryViewModel();
                cat.CategoryName = "4 Stars";
                cat.Id = 1;
                categor.Add(cat);
                cat = new ShowposttaskcategoryViewModel();
                cat.CategoryName = "3 Stars";
                cat.Id = 2;
                categor.Add(cat);
                cat = new ShowposttaskcategoryViewModel();
                cat.CategoryName = "2 Stars";
                cat.Id = 3;
                categor.Add(cat);
                cat = new ShowposttaskcategoryViewModel();
                cat.CategoryName = "1 Star";
                cat.Id = 4;
                categor.Add(cat);
                ViewBag.Category = new SelectList(categor.Select(p => p.CategoryName).ToList().Distinct());
                return View(bidding);
            }
            if (user == 2)//meaning Worker want to see his bidding on the task
            {
                var users = User.Identity.GetUserId();
                var workerids = db.RegistWork.Where(x => x.Userid == users).FirstOrDefault();
                if (id == null)
                {
                    return View("Error");
                }
                var bi = (from b in db.Bids
                          where b.TaskDetId == id && b.WorkerId == workerids.Id && b.bid_status != 1 && b.bid_status != -1
                          join taskdet in db.TaskDetails on b.TaskDetId equals taskdet.Id
                          join task in db.TaskBook on taskdet.Id equals task.TaskDetId
                          join worker in db.RegistWork on b.WorkerId equals worker.Id
                          join userworker in db.UsersIdentities on worker.Userid equals userworker.Userid
                          join username in db.Users on worker.Userid equals username.Id
                          select new
                          {
                              bidId = b.Id,
                              amount = b.Bid_Amount,
                              bidstat = b.bid_status,
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
                              bidtimeexp = b.BidTimeExp,
                          }).ToList();
                foreach (var exbid in bi)
                {
                    if (exbid.bidstat != -1 && DateTime.Compare(DateTime.Now, exbid.bidtimeexp) > 0)
                    {
                        var ebid = db.Bids.Where(x => x.Id == exbid.bidId).FirstOrDefault();
                        ebid.bid_status = -1;
                        db.SaveChanges();
                        var notification = new NotificationModel
                        {
                            Receiver = exbid.username,
                            Title = $"The bid {exbid.biddesc} has expired",
                            Details = $"The bid has already expired.",
                            DetailsURL = $"",
                            Date = DateTime.Now,
                            IsRead = false
                        };
                        db.Notifications.Add(notification);
                        db.SaveChanges();
                    }
                }
                var averate = 0;
                ViewBag.Id = id;
                ViewBag.user = user;
                ViewBag.checkuser = user;
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
                    bidds.BidTimeExp = b.bidtimeexp;
                    bidds.TaskdetId = b.taskdetid;
                    bidds.Rate = averate;
                    bidds.user = user;
                    bidding.Add(bidds);
                }
                return View(bidding);
            }
            return View("Error");
        }
        public ActionResult EditBidding(int id)
        {
            var bid = db.Bids.Where(x => x.Id == id).FirstOrDefault();
            var bidmodel = new BidViewModel();
            ViewBag.bidid = id;
            bidmodel.Bid_Amount = bid.Bid_Amount;
            bidmodel.Bid_Description = bid.Bid_Description;
            bidmodel.BidTimeExp = bid.BidTimeExp;
            return View(bidmodel);
            
        }
        [HttpPost]
        public ActionResult EditBidding(BidViewModel model)
        {
            if(ModelState.IsValid)
            {
                if (model.Bid_Amount < 100)
                {
                    ViewBag.bidid = model.Bidid;
                    ModelState.AddModelError("", "Amount must be greater than or equal to 100 ");
                    return View(model);
                }

                var bidmodel = db.Bids.Where(x => x.Id == model.Bidid).FirstOrDefault();
                bidmodel.Bid_Amount = model.Bid_Amount;
                bidmodel.Bid_Description = model.Bid_Description;
                bidmodel.BidTimeExp = model.BidTimeExp;
                bidmodel.Updated_at = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("ViewBiddedRequestTask", "Task");
            }
            ViewBag.bidid = model.Bidid;
            return View(model);

        }
        public ActionResult CancelBidding(int? id,int? taskid)
        {
            var users = User.Identity.GetUserId();
            var workerids = db.RegistWork.Where(x => x.Userid == users).FirstOrDefault();
                if (id == null)
                {
                    return View("Error");
                }
                var bid = db.Bids.Where(x => x.Id == id).FirstOrDefault();
                if (bid == null)
                {
                    return View("Error");
                }
                if(bid.WorkerId == workerids.Id)
                {
                    bid.bid_status = 1;
                    db.SaveChanges();
                    return RedirectToAction("ViewBiddedRequestTask","Task");
                }
                else
                {
                    bid.bid_status = 1;
                    var taskbook = db.TaskBook.Where(x => x.TaskDetId == taskid).FirstOrDefault();
                    taskbook.Taskbook_Status = 0;
                    taskbook.workerId = 0;
                    db.SaveChanges();
                    return RedirectToAction("ViewBidding", new { @id = taskid, @user = 1 });
                }
        }

        public ActionResult AcceptBid(int id, int? taskid)// User
        {
            var userid = User.Identity.GetUserId();
            var usersend = db.Users.Where(x => x.Id == userid).FirstOrDefault();
            var balance = db.Balance.Where(x => x.UserId == userid).FirstOrDefault();
            var bidid = db.Bids.Where(x => x.Id == id).FirstOrDefault();
            if (balance.Money <= 0 || balance.Money < bidid.Bid_Amount)
            {
                return View("ErrorAcceptBid");
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
                Details = $"{usersend.UserName} Accepted your bid and the task Added to your Contract Task",
                DetailsURL = "/Task/ViewContractTask",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
            return RedirectToAction("ShowMyTaskPost","Task");
        }
    }
}