using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iAssist.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Net;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Dynamic;
using iAssist.Hubs;

namespace iAssist.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin
        public ActionResult AdminDashboard()
        {
            var admindashrep = new AdminReportModelView();
            admindashrep.Totaluser = db.Users.ToList().Count();
            admindashrep.TotalListOfTask = db.TaskBook.Where(x => x.Taskbook_Status != 0).ToList().Count();
            admindashrep.TotalComplaint = db.Complaints.ToList().Count();
            admindashrep.TotalWorker = db.RegistWork.Where(x => x.worker_status == 0).ToList().Count();
            admindashrep.TotalJobCategory = db.JobCategories.ToList().Count();
            return View(admindashrep);
        }
        //Start of Job Category
        public async Task<ActionResult> ListJobCategory(string search)
        {
            var Joblist = await db.JobCategories.ToListAsync();
            if (!String.IsNullOrEmpty(search))
            {
                Joblist = await db.JobCategories.Where(p => p.JobName.Contains(search) || p.JobDescription.Contains(search)).ToListAsync();
            }
            return View(Joblist);
        }
        public ActionResult DeleteJobCategory(int? id)
        {
            if(id == null)
            {
                return View("Error");
            }
            var deljob = db.JobCategories.Where(x => x.Id == id).FirstOrDefault();
            db.JobCategories.Remove(deljob);
            db.SaveChanges();
            return RedirectToAction("ListJobCategory");
        }
        //httpget details
        public async Task<ActionResult> DetailsJobCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job jobcateg = await db.JobCategories.FindAsync(id);
            if (jobcateg == null)
            {
                return HttpNotFound();
            }
            return View(jobcateg);
        }
        //httpget create job category
        public ActionResult CreateJobCategory()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateJobCategory(JobCategory model)
        {
            if (ModelState.IsValid)
            {
                var jobcategory = new Job { JobName = model.Jobname, JobDescription = model.JobDescription, Created_At = DateTime.Now, Updated_At = DateTime.Now };
                db.JobCategories.Add(jobcategory);
                await db.SaveChangesAsync();
                return RedirectToAction("ListJobCategory", "Admin");
            }
            return View(model);
        }
        //httpget edit job category
        public async Task<ActionResult> EditJobCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job jobcateg = await db.JobCategories.FindAsync(id);
            if (jobcateg == null)
            {
                return HttpNotFound();
            }
            return View(jobcateg);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditJobCategory(Job model)
        {
            if (ModelState.IsValid)
            {
                var data = db.JobCategories.Where(x => x.Id == model.Id).FirstOrDefault();
                if (data != null)
                {
                    data.JobName = model.JobName;
                    data.JobDescription = model.JobDescription;
                    data.Updated_At = DateTime.Now;
                    await db.SaveChangesAsync();
                    return RedirectToAction("ListJobCategory", "Admin");
                }
            }
            return View(model);
        }
        //Type of Service
        public ActionResult TypeofService(int? id)
        {
            if(id == null)
            {
                return View("Error");
            }
            ViewBag.Job = (from j in db.JobCategories where j.Id == id select j.JobName).FirstOrDefault();
            ViewBag.JobId = id;
            var typeofservicemodel = (from s in db.Skills where s.Jobid == id select new { skillname = s.Skillname, id = s.Id, jobid = s.Jobid }).ToList().Select(p => new ServiceViewModel() { Skillname = p.skillname, Id = p.id });
            return View(typeofservicemodel);
        }
        public ActionResult CreateService(int? jobid)
        {
            if(jobid == null)
            {
                return View("Error");
            }
            ViewBag.J = jobid;
            var skill = new ServiceViewModel();
            return View(skill);
        }
        [HttpPost]
        public ActionResult CreateService(ServiceViewModel model)
        {
            if(ModelState.IsValid)
            {
                var skill = new Skill();
                skill.Jobid = model.Jobid;
                skill.Skillname = model.Skillname;
                db.Skills.Add(skill);
                db.SaveChanges();
                return RedirectToAction("TypeofService", new { id = model.Jobid });
            }
            return View(model);
        }
        public ActionResult EditService(int? id, int? job)
        {
            if(id == null || job == null)
            {
                return View("Error");
            }
            var skill = new ServiceViewModel();
            skill.Skillname = (from u in db.Skills where u.Id == id select u.Skillname).FirstOrDefault();
            skill.Id = (from u in db.Skills where u.Id == id select u.Id).FirstOrDefault();
            skill.Jobid = (from u in db.Skills where u.Id == id select u.Jobid).FirstOrDefault();
            ViewBag.Jobid = job;
            return View(skill);
        }
        [HttpPost]
        public ActionResult EditService(ServiceViewModel model)
        {
            if(ModelState.IsValid)
            {
                var skill = db.Skills.Where(x => x.Id == model.Id).FirstOrDefault();
                skill.Skillname = model.Skillname;
                db.SaveChanges();
                return RedirectToAction("TypeofService", new { id = model.Jobid });
            }
            ViewBag.Jobid = model.Jobid;
            return View(model);
        }
        public ActionResult DeleteService(int? id, int? job)
        {
            if(id == null)
            {
                return View("Error");
            }
            var skill = db.Skills.Where(x => x.Id == id).FirstOrDefault();
            db.Skills.Remove(skill);
            db.SaveChanges();
            return RedirectToAction("TypeofService", new { id = job });
        }
        //Complaints
        public ActionResult ManageUserComplaints()
        {
            var userscomplaints = (from u in db.Complaints
                                   select new
                                   {
                                       complainttitle = u.ComplaintTitle,
                                       desc = u.Desc,
                                       compimage = u.compimage,
                                       username = (from us in db.Users where us.Id == u.UserId select us.UserName).FirstOrDefault(),
                                       workername = (from wu in db.RegistWork where wu.Id == u.WorkerId join uw in db.Users on wu.Userid equals uw.Id select uw.UserName).FirstOrDefault(),
                                   }).ToList().Select(p => new complaintViewModel { 
                                        ComplaintTitle = p.complainttitle,
                                        compimage = p.compimage,
                                        Desc = p.desc,
                                        Username = p.username,
                                        Workerusername = p.workername,
                                   });
            return View(userscomplaints);
        }
        //SkilledWorker
        public ActionResult ManageSkilledWorker()
        {
            var data = (from worker in db.RegistWork
                        where worker.worker_status == 1
                        join workuser in db.Users on worker.Userid equals workuser.Id
                        join userident in db.UsersIdentities on worker.Userid equals userident.Userid
                        join namejob in db.JobCategories on worker.JobId equals namejob.Id
                        select new
                        {
                            id = workuser.Id,
                            firstname = userident.Firstname,
                            lastname = userident.Lastname,
                            jobname = namejob.JobName,
                            workerstatus = worker.worker_status,
                            profile = userident.ProfilePicture,
                            jobid = namejob.Id
                        }).ToList().Select(p => new ManageSkilledWorker()
                        {
                            Id = p.id,
                            Jobid = p.jobid,
                            Firstname = p.firstname,
                            Profile = p.profile,
                            Lastname = p.lastname,
                            JobName = p.jobname,
                            Worker_status = p.workerstatus,
                        });

            return View(data);
        }
        public ActionResult ViewDetailsSkilledWorker(string Id)
        {
            var inforegit = db.RegistWork.Where(x => x.Userid == Id).FirstOrDefault();
            var infoR = db.JobCategories.Where(x => x.Id == inforegit.JobId).FirstOrDefault();
            var workerin = db.UsersIdentities.Where(x => x.Userid == Id).FirstOrDefault();
            var workerinfor = db.Users.Where(x => x.Id == Id).FirstOrDefault();
            var inforegist = new ViewDetailsRegSkilledWorker();
            inforegist.workoverview = inforegit.worker_overview;
            if (inforegit.worker_status == 1)
            {
                inforegist.worker_status = "Pending";
            }
            if (inforegit.worker_status != 1)
            {
                return RedirectToAction("ManageSkilledWorker");
            }
            inforegist.JobName = infoR.JobName;
            inforegist.JobId = infoR.Id;
            inforegist.Id = workerinfor.Id;
            inforegist.Firstname = workerin.Firstname;
            inforegist.Lastname = workerin.Lastname;
            inforegist.Phonenumber = workerinfor.PhoneNumber;
            inforegist.EmailAdd = workerinfor.Email;
            inforegist.ProfilePicture = workerin.ProfilePicture;
            var inforegfile = (from db in db.RegistWorkFile where db.Userid == Id select new { filename = db.FileName }).ToList().Select(p => new ViewFileRegSkilledWorker() { FileName = p.filename });
            dynamic model = new ExpandoObject();
            model.ViewDetailsRegSkilledWorker = inforegist;
            model.ViewFileRegSkilledWorker = inforegfile;
            return View(model);
        }
        public ActionResult AcceptRegistWorker(string id,int? jbid)
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roleManager = HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            var roleName = "Worker";
            var role = roleManager.FindByName(roleName);
            if (role == null)
            {
                role = new IdentityRole(roleName);
                var roleresult = roleManager.Create(role);
            }
            var regworker = db.RegistWork.Where(x => x.Userid == id && x.JobId == jbid).FirstOrDefault();
            regworker.worker_status = 0;
            regworker.Verified_At = DateTime.Now.ToString();
            db.SaveChanges();
            var user = db.Users.Where(x => x.Id == id).FirstOrDefault();
            var oldrole = roleManager.FindById(user.Roles.FirstOrDefault().RoleId);
            var newrole = db.Roles.Where(x => x.Name == roleName).FirstOrDefault();
            userManager.RemoveFromRole(user.Id, oldrole.Name);
            var result = userManager.AddToRole(user.Id, newrole.Name);
            var notification = new NotificationModel
            {
                Receiver = user.UserName,
                Title = $"Admin Approve your application",
                Details = $"You are now a worker in iAssist but First you need to logout to take changes",
                DetailsURL = "./image/terms-and-conditions.pdf",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
            NotificationHub objNotifHub = new NotificationHub();
            objNotifHub.SendNotification(notification.Receiver);
            return RedirectToAction("ManageSkilledWorker");
        }
        public ActionResult DenyRegistWorker(string id)
        {
            ViewBag.Id = id;
            var denyreg = new DenyRegistWorkerViewModel();
            denyreg.UserId = id;
            return View(denyreg);

        }
        [HttpPost]
        public ActionResult DenyRegistWorker(DenyRegistWorkerViewModel model)
        {
            if(ModelState.IsValid)
            {
                var workerfile = db.RegistWork.Where(x => x.Userid == model.UserId).FirstOrDefault();
                db.RegistWork.Remove(workerfile);
                var workerimg = db.RegistWorkFile.Where(x => x.Userid == model.UserId).ToList();
                db.RegistWorkFile.RemoveRange(workerimg);
                db.SaveChanges();
                var users = db.Users.Where(x => x.Id == model.UserId).FirstOrDefault();
                var notification = new NotificationModel
                {
                    Receiver = users.UserName,
                    Title = $"Admin denied your application",
                    Details = $"Admin Message: {model.Feedback}",
                    DetailsURL = $"",
                    Date = DateTime.Now,
                    IsRead = false
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
                return RedirectToAction("ManageSkilledWorker");
            }
            ViewBag.Id = model.UserId;
            return View(model);
        }
        public ActionResult ViewSkilledWorker()
        {
            var roleuser = (from rolename in db.Roles where rolename.Name.Contains("Worker") select rolename).FirstOrDefault();
            if(roleuser != null)
            {
                var checkuser = (from user in db.Users where user.Roles.Any(r => r.RoleId == roleuser.Id) select user).ToList();
                if (checkuser != null)
                {
                   var usersWithRoles = (from user in db.Users
                                          where user.Roles.Any(r => r.RoleId == roleuser.Id)
                                          join userident in db.UsersIdentities on user.Id equals userident.Userid
                                          join u in db.RegistWork on userident.Userid equals u.Userid where u.worker_status == 0
                                          join j in db.JobCategories on u.JobId equals j.Id
                                          select new
                                          {
                                              userId = user.Id,
                                              Email = user.Email,
                                              Firstname = userident.Firstname,
                                              profilepic = userident.ProfilePicture,
                                              Lastname = userident.Lastname,
                                              created = userident.Created_At,
                                              jobname = j.JobName,
                                          }).ToList().Select(p => new Users()
                                          {
                                              UserId = p.userId,
                                              Email = p.Email,
                                              ProfilePicture = p.profilepic,
                                              Firstname = p.Firstname,
                                              Lastname = p.Lastname,
                                              Jobname = p.jobname,
                                              Created = p.created.ToString()
                                          });
                    return View(usersWithRoles);
                }
            }
            return View();
        }
        public ActionResult ViewAcceptedSkilledWorker(string Id)
        {
            var inforegit = db.RegistWork.Where(x => x.Userid == Id).FirstOrDefault();
            var infoR = db.JobCategories.Where(x => x.Id == inforegit.JobId).FirstOrDefault();
            var workerin = db.UsersIdentities.Where(x => x.Userid == Id).FirstOrDefault();
            var workerinfor = db.Users.Where(x => x.Id == Id).FirstOrDefault();
            var inforegist = new ViewDetailsRegSkilledWorker();
            inforegist.workoverview = inforegit.worker_overview;
            inforegist.JobName = infoR.JobName;
            inforegist.Id = workerinfor.Id;
            inforegist.Firstname = workerin.Firstname;
            inforegist.Lastname = workerin.Lastname;
            inforegist.Phonenumber = workerinfor.PhoneNumber;
            inforegist.EmailAdd = workerinfor.Email;
            inforegist.ProfilePicture = workerin.ProfilePicture;
            var inforegfile = (from db in db.RegistWorkFile where db.Userid == Id select new { filename = db.FileName }).ToList().Select(p => new ViewFileRegSkilledWorker() { FileName = p.filename });
            dynamic model = new ExpandoObject();
            model.ViewDetailsRegSkilledWorker = inforegist;
            model.ViewFileRegSkilledWorker = inforegfile;
            return View(model);
        }
        //Transaction
        public ActionResult ManageTransactions()
        {
            return View();
        }
        //View Users Report
        public ActionResult ViewUsers()
        {
            var users = (from user in db.Users
                         select new
                         {
                             userId = user.Id,
                             lockoutdate = user.LockoutEndDateUtc,
                             Email = user.Email,
                             Firstname = (from userident in db.UsersIdentities where userident.Userid == user.Id select userident.Firstname).FirstOrDefault(),
                             profilepic = (from userident in db.UsersIdentities where userident.Userid == user.Id select userident.ProfilePicture).FirstOrDefault(),
                             Lastname = (from userident in db.UsersIdentities where userident.Userid == user.Id select userident.Lastname).FirstOrDefault(),
                             created = (from userident in db.UsersIdentities where userident.Userid == user.Id select userident.Created_At).FirstOrDefault(),
                             role = (from userroles in user.Roles join roles in db.Roles on userroles.RoleId equals roles.Id select roles.Name).ToList(),
                             balance = (from balance in db.Balance where user.Id == balance.UserId select balance.Money).FirstOrDefault(),
                         }).ToList().Select(p => new Users()
                         {
                             UserId = p.userId,
                             locoutdatetime = p.lockoutdate,
                             Email = p.Email,
                             ProfilePicture = p.profilepic,
                             Firstname = p.Firstname,
                             Lastname = p.Lastname,
                             Created = p.created.ToString(),
                             Rolename = string.Join(",", p.role),
                             balance = p.balance,
                         });

            return View(users);
        }
        public ActionResult Replenish(string id)
        {
            if(id == null)
            {
                return View("Error");
            }
            ViewBag.Uid = id;
            var a = new ReplenishBalance();
            a.Userid = id;
            return View(a);
        }
        [HttpPost]
        public ActionResult Replenish(ReplenishBalance model)
        {
            var user = User.Identity.GetUserId();
            var userinfo = db.Users.Where(x => x.Id == user).FirstOrDefault();
            var rec = db.Users.Where(x => x.Id == model.Userid).FirstOrDefault();
            if (model.Balance <= 0)
            {
                return View("Error");
            }
            var balance = db.Balance.Where(x => x.UserId == model.Userid).FirstOrDefault();
            balance.Money = balance.Money + model.Balance;
            db.SaveChanges();
            var transaction = new TransactionHistory();
            transaction.Payer = userinfo.UserName;
            transaction.Reciever = rec.UserName;
            transaction.BidAmount = model.Balance.ToString();
            transaction.TotalAmount = model.Balance.ToString();
            transaction.Commission = "0";
            transaction.Created_At = DateTime.Now;
            transaction.tasktitle = "Replenish Balance";
            db.TransactionHistories.Add(transaction);
            db.SaveChanges();

            return RedirectToAction("ViewUsers");
        }
        public ActionResult Decrease(string id, decimal bal)
        {
            if (id == null)
            {
                return View("Error");
            }
            if(bal < 0 || bal == 0)
            {
                return View("Error");
            }
            ViewBag.Uid = id;
            var a = new ReplenishBalance();
            a.Userid = id;
            return View(a);
        }
        [HttpPost]
        public ActionResult Decrease(ReplenishBalance model)
        {
            if(model.Balance <= 0)
            {
                return View("Error");
            }
            var user = User.Identity.GetUserId();
            var userinfo = db.Users.Where(x => x.Id == user).FirstOrDefault();
            var rec = db.Users.Where(x => x.Id == model.Userid).FirstOrDefault();
            var balance = db.Balance.Where(x => x.UserId == model.Userid).FirstOrDefault();
            balance.Money = balance.Money - model.Balance;
            db.SaveChanges();
            var transaction = new TransactionHistory();
            transaction.Payer = userinfo.UserName;
            transaction.Reciever = rec.UserName;
            transaction.BidAmount = model.Balance.ToString();
            transaction.TotalAmount = model.Balance.ToString();
            transaction.Commission = "0";
            transaction.Created_At = DateTime.Now;
            transaction.tasktitle = "Decrease Balance";
            db.TransactionHistories.Add(transaction);
            db.SaveChanges();
            return RedirectToAction("ViewUsers");
        }
        public ActionResult BlockUser(string id)
        {
            var user = db.Users.Where(x => x.Id == id).FirstOrDefault();
            if (user == null)
            {
                return View("Error");
            }
            if(user.LockoutEndDateUtc == null)
            {
                user.LockoutEndDateUtc = DateTime.MaxValue;
                db.SaveChanges();
                return RedirectToAction("ViewUsers");
            }
            user.LockoutEndDateUtc = null;
            db.SaveChanges();
            return RedirectToAction("ViewUsers");
        }
        public ActionResult RequestWithdraw()
        {
            var requestwith = db.Withdraw.ToList();
            return View(requestwith);
        }
        public ActionResult MarkasDoneReq(int? id)
        {
            if(id == null)
            {
                return View("Error");
            }
            var reqlist = db.Withdraw.Where(x => x.Id == id).FirstOrDefault();
            var user = db.Users.Where(x => x.Id == reqlist.UserId).FirstOrDefault();
            var balance = db.Balance.Where(x => x.UserId == user.Id).FirstOrDefault();
            var money = balance.Money;
            if(money == 0)
            {
                return View("Error");
            }
            money = money - reqlist.Money;
            balance.Money = money;
            db.SaveChanges();
            reqlist.status = true;
            db.SaveChanges();
            var notification = new NotificationModel
            {
                Receiver = reqlist.Username,
                Title = $"Admin has accepted your Request",
                Details = $"The admin accepted your request you have now a balance of {balance.Money}",
                DetailsURL = $"/Transactions/TransIndex",
                Date = DateTime.Now,
                IsRead = false
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
            var trans = new TransactionHistory
            {
                tasktitle = "Request Withdraw",
                BidAmount = reqlist.Money.ToString(),
                TotalAmount = reqlist.Money.ToString(),
                Commission = "0",
                Payer = "iAssist.mvc@gmail.com",
                Reciever = user.Email,
                Created_At = DateTime.Now
            };
            db.TransactionHistories.Add(trans);
            db.SaveChanges();
            return RedirectToAction("RequestWithdraw","Admin");
        }
    }
}