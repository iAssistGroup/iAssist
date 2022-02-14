using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace iAssist.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync2(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDBInitializer());
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<users> UsersIdentities { get; set; }
        public DbSet<Job> JobCategories { get; set; }
        public DbSet<Work> RegistWork { get; set; }
        public DbSet<WorkerRegImages> RegistWorkFile { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Task_Book> TaskBook { get; set; }
        public DbSet<TaskDetails> TaskDetails { get; set; }
        public DbSet<Tasked> Taskeds { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Wallet> Balance { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<SkillsOfWorker> SkillsOfWorkers { get; set; }
        public DbSet<SkillServiceTask> SkillServiceTasks { get; set; }
        public DbSet<TransactionHistory> TransactionHistories { get; set; }
        public DbSet<WithDrawRequest> Withdraw { get; set; }
        public DbSet<TaskFiles> Taskfileses { get; set; }
        public DbSet<Workerstat> WorkerReportTask { get; set; }
    }
}