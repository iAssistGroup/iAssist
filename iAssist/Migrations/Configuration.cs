namespace iAssist.Migrations
{
    using Bogus;
    using iAssist.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.SqlServer.Types;
    using System;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<iAssist.Models.ApplicationDbContext>
    {
        private bool _debug = false;

        private string _customExceptionMessage = "";
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "iAssist.Models.ApplicationDbContext";
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            SqlProviderServices.SqlServerTypesAssemblyName = typeof(SqlGeography).Assembly.FullName;
        }

        protected override void Seed(iAssist.Models.ApplicationDbContext context)
        {
            try
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = new RoleManager<IdentityRole>(roleStore);
                DateTime created = DateTime.Now;
                DateTime updated = DateTime.Now;
                string name = ConfigurationManager.AppSettings["adminmail"].ToString();
                string password = ConfigurationManager.AppSettings["adminpass"].ToString();
                const string roleName = "admin";
                string Firstname = ConfigurationManager.AppSettings["adminname"].ToString();
                string Lastname = ConfigurationManager.AppSettings["adminname"].ToString();
                //Create Role Admin
                var role = roleManager.FindByName(roleName);
                if (role == null)
                {
                    role = new IdentityRole(roleName);
                    var roleresult = roleManager.Create(role);
                }
                var user = userManager.FindByName(name);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = name,
                        Email = name,
                        EmailConfirmed = true
                    };
                    var result = userManager.Create(user, password);
                    var useridentity = new users { Firstname = Firstname, Lastname = Lastname, Created_At = created, Updated_At = updated, Userid = user.Id };
                    if (result.Succeeded)
                    {
                        context.UsersIdentities.Add(useridentity);
                        context.SaveChanges();
                        var adminwallet = new Wallet { Money = 0, UserId = user.Id };
                        context.Balance.Add(adminwallet);
                        context.SaveChanges();
                        result = userManager.SetLockoutEnabled(user.Id, false);
                    }
                }
                // Add user admin to Role Admin
                var rolesForuser = userManager.GetRoles(user.Id);
                if (!rolesForuser.Contains(role.Name))
                {
                    var result = userManager.AddToRole(user.Id, role.Name);
                }
                //==============================================Adding 50 Seeker===============================================
                for (int i = 0; i < 50; i++)
                {
                    var faker = new Faker("en_US");
                    var pass = "Password099`";
                    const string seekrole = "User";
                    var seekerrole = roleManager.FindByName(seekrole);
                    if (seekerrole == null)
                    {
                        seekerrole = new IdentityRole(seekrole);
                        var roleresult = roleManager.Create(seekerrole);
                    }
                    var addseeker = new Faker<ApplicationUser>()
                        .RuleFor(x => x.UserName, x => x.Person.Email)
                        .RuleFor(x => x.Email, x => x.Person.Email)
                        .RuleFor(x => x.EmailConfirmed, true)
                        .RuleFor(x => x.PhoneNumber, x => x.Person.Phone);
                    var valuesaddseeker = addseeker.Generate();
                    var findseek = userManager.FindByName(valuesaddseeker.UserName);
                    if (findseek == null)
                    {
                        var res = userManager.Create(valuesaddseeker, pass);
                        var seekeridentity = new Faker<users>()
                            .RuleFor(x => x.Firstname, x => x.Person.FirstName)
                            .RuleFor(x => x.Lastname, x => x.Person.LastName)
                            .RuleFor(x => x.Created_At, DateTime.Now)
                            .RuleFor(x => x.Updated_At, DateTime.Now)
                            .RuleFor(x => x.Userid, valuesaddseeker.Id);
                        var seekergenerateidentity = seekeridentity.Generate();
                        if (res.Succeeded)
                        {
                            context.UsersIdentities.Add(seekergenerateidentity);
                            context.SaveChanges();
                            var locationidentity = new Faker<Location>()
                            .RuleFor(x => x.Loc_Address, x => x.Person.Address.City)
                            .RuleFor(x => x.Geolocation, x => DbGeography.FromText("POINT(" + x.Person.Address.Geo.Lng + " " + x.Person.Address.Geo.Lat + ")"))
                            .RuleFor(x => x.Created_At, DateTime.Now)
                            .RuleFor(x => x.Updated_At, DateTime.Now)
                            .RuleFor(x => x.UserId, valuesaddseeker.Id);
                            var locationidentitygenerate = locationidentity.Generate();
                            context.Locations.Add(locationidentitygenerate);
                            context.SaveChanges();
                            var seekerwallet = new Wallet { Money = 0, UserId = valuesaddseeker.Id };
                            context.Balance.Add(seekerwallet);
                            context.SaveChanges();
                            res = userManager.SetLockoutEnabled(valuesaddseeker.Id, false);
                        }
                    }
                    var seekerroles = userManager.GetRoles(valuesaddseeker.Id);
                    if (!seekerroles.Contains(seekerrole.Id))
                    {
                        var result = userManager.AddToRole(valuesaddseeker.Id, seekerrole.Name);
                    }
                }
                //======================================Adding Job Category=========================================
                var job = context.JobCategories.Where(x => x.JobName == "Computer Repair").FirstOrDefault();
                _customExceptionMessage = "OK";
                if (job == null)
                {
                    _customExceptionMessage = "OK";
                    var jobs = new Job();
                    jobs.JobDescription = "Fixing Computer, Cleaning and Etc";
                    jobs.JobName = "Computer Repair";
                    jobs.Created_At = DateTime.Now;
                    jobs.Updated_At = DateTime.Now;
                    context.JobCategories.Add(jobs);
                    context.SaveChanges();

                }
                var jobes = context.JobCategories.Where(x => x.JobName == "Aircon Repair").FirstOrDefault();
                _customExceptionMessage = "OK";
                if (jobes == null)
                {
                    _customExceptionMessage = "OK";
                    var jobses = new Job();
                    jobses.JobDescription = "Fixing Aircon, Cleaning and Etc";
                    jobses.JobName = "Aircon Repair";
                    jobses.Created_At = DateTime.Now;
                    jobses.Updated_At = DateTime.Now;
                    context.JobCategories.Add(jobses);
                    context.SaveChanges();

                }
                //======================================Adding Type of Service=====================================
                var seekjob = context.JobCategories.ToList();
                if (seekjob != null)
                {
                    foreach (var item in seekjob)
                    {
                        if (item.JobName == "Computer Repair")
                        {
                            var skill = context.Skills.Where(x => x.Skillname == "Virus Removal").FirstOrDefault();
                            if (skill == null)
                            {
                                var skills = new Skill();
                                skills.Jobid = item.Id;
                                skills.Skillname = "Virus Removal";
                                context.Skills.Add(skills);
                                context.SaveChanges();
                            }

                            var skilles = context.Skills.Where(x => x.Skillname == "Computer Cleaning").FirstOrDefault();
                            if (skilles == null)
                            {
                                var skillses = new Skill();
                                skillses.Jobid = item.Id;
                                skillses.Skillname = "Computer Cleaning";
                                context.Skills.Add(skillses);
                                context.SaveChanges();
                            }
                        }
                        else if (item.JobName == "Aircon Repair")
                        {
                            var skilleses = context.Skills.Where(x => x.Skillname == "Aircon Troubleshoot").FirstOrDefault();
                            if (skilleses == null)
                            {
                                var skills = new Skill();
                                skills.Jobid = 2;
                                skills.Skillname = "Aircon Troubleshoot";
                                context.Skills.Add(skills);
                                context.SaveChanges();
                            }
                            var skilles = context.Skills.Where(x => x.Skillname == "Aircon Cleaning").FirstOrDefault();
                            if (skilles == null)
                            {
                                var skillses = new Skill();
                                skillses.Jobid = 2;
                                skillses.Skillname = "Aircon Cleaning";
                                context.Skills.Add(skillses);
                                context.SaveChanges();
                            }
                        }
                    }
                }
                //=======================================Adding 50 Worker============================================
                var seekingjob = context.JobCategories.ToList();
                foreach (var item in seekingjob)
                {
                    for (var j = 1; j <= 25; j++)
                    {
                        var faker = new Faker("en_US");
                        var pass = "Password099`";
                        const string seekrole = "Worker";
                        var seekerrole = roleManager.FindByName(seekrole);
                        if (seekerrole == null)
                        {
                            seekerrole = new IdentityRole(seekrole);
                            var roleresult = roleManager.Create(seekerrole);
                        }
                        var addseeker = new Faker<ApplicationUser>()
                            .RuleFor(x => x.UserName, x => x.Person.Email)
                            .RuleFor(x => x.Email, x => x.Person.Email)
                            .RuleFor(x => x.EmailConfirmed, true)
                            .RuleFor(x => x.PhoneNumber, x => x.Person.Phone);
                        var valuesaddseeker = addseeker.Generate();
                        var findseek = userManager.FindByName(valuesaddseeker.UserName);
                        if (findseek == null)
                        {
                            var res = userManager.Create(valuesaddseeker, pass);
                            var seekeridentity = new Faker<users>()
                                .RuleFor(x => x.Firstname, x => x.Person.FirstName)
                                .RuleFor(x => x.Lastname, x => x.Person.LastName)
                                .RuleFor(x => x.Created_At, DateTime.Now)
                                .RuleFor(x => x.Updated_At, DateTime.Now)
                                .RuleFor(x => x.Userid, valuesaddseeker.Id);
                            var seekergenerateidentity = seekeridentity.Generate();
                            if (res.Succeeded)
                            {
                                context.UsersIdentities.Add(seekergenerateidentity);
                                context.SaveChanges();
                                var locationidentity = new Faker<Location>()
                                .RuleFor(x => x.Loc_Address, x => x.Person.Address.City)
                                .RuleFor(x => x.Geolocation, x => DbGeography.FromText("POINT(" + x.Person.Address.Geo.Lng + " " + x.Person.Address.Geo.Lat + ")"))
                                .RuleFor(x => x.Created_At, DateTime.Now)
                                .RuleFor(x => x.Updated_At, DateTime.Now)
                                .RuleFor(x => x.JobId, item.Id)
                                .RuleFor(x => x.UserId, valuesaddseeker.Id);
                                var locationidentitygenerate = locationidentity.Generate();
                                context.Locations.Add(locationidentitygenerate);
                                context.SaveChanges();
                                var seekerwallet = new Wallet { Money = 0, UserId = valuesaddseeker.Id };
                                context.Balance.Add(seekerwallet);
                                context.SaveChanges();
                                res = userManager.SetLockoutEnabled(valuesaddseeker.Id, false);
                            }
                        }
                        var seekerroles = userManager.GetRoles(valuesaddseeker.Id);
                        if (!seekerroles.Contains(seekerrole.Id))
                        {
                            var result = userManager.AddToRole(valuesaddseeker.Id, seekerrole.Name);
                        }
                        var workerinfo = new Work();
                        workerinfo.Userid = valuesaddseeker.Id;
                        workerinfo.JobId = item.Id;
                        workerinfo.Updated_At = DateTime.Now;
                        workerinfo.worker_overview = "I am a Robot";
                        workerinfo.Verified_At = DateTime.Now.ToString();
                        workerinfo.worker_status = 0;
                        workerinfo.Created_At = DateTime.Now;
                        context.RegistWork.Add(workerinfo);
                        context.SaveChanges();
                        var chooseskill = context.Skills.Where(x => x.Jobid == item.Id).ToList();
                        foreach (var items in chooseskill)
                        {
                            var skillsofworker = new SkillsOfWorker();
                            skillsofworker.Jobid = item.Id;
                            skillsofworker.Skillname = items.Skillname;
                            skillsofworker.UserId = valuesaddseeker.Id;
                            context.SkillsOfWorkers.Add(skillsofworker);
                            context.SaveChanges();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception(_customExceptionMessage, ex);

                throw ex2;
            }
        }
    }
}
