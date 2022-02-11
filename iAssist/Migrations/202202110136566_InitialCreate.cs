namespace iAssist.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Wallets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Money = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Bids",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Bid_Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Bid_Description = c.String(),
                        BidTimeExp = c.DateTime(nullable: false),
                        Created_at = c.DateTime(nullable: false),
                        Updated_at = c.DateTime(nullable: false),
                        TaskDetId = c.Int(nullable: false),
                        WorkerId = c.Int(nullable: false),
                        bid_status = c.Int(nullable: false),
                        TaskDetails_Id = c.Int(),
                        Work_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaskDetails", t => t.TaskDetails_Id)
                .ForeignKey("dbo.Works", t => t.Work_Id)
                .Index(t => t.TaskDetails_Id)
                .Index(t => t.Work_Id);
            
            CreateTable(
                "dbo.TaskDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        taskdet_name = c.String(nullable: false),
                        taskdet_desc = c.String(nullable: false),
                        taskdet_sched = c.DateTime(nullable: false),
                        taskdet_time = c.DateTime(nullable: false),
                        Budget = c.Decimal(nullable: false, precision: 18, scale: 2),
                        taskdet_Created_at = c.DateTime(nullable: false),
                        taskdet_Updated_at = c.DateTime(nullable: false),
                        TaskImage = c.String(),
                        Loc_Address = c.String(nullable: false),
                        Geolocation = c.Geography(),
                        JobId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.JobId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Jobs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobName = c.String(nullable: false),
                        JobDescription = c.String(nullable: false),
                        Created_At = c.DateTime(nullable: false),
                        Updated_At = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Works",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        worker_overview = c.String(nullable: false),
                        worker_status = c.Int(nullable: false),
                        Created_At = c.DateTime(nullable: false),
                        Updated_At = c.DateTime(nullable: false),
                        Verified_At = c.String(),
                        Userid = c.String(maxLength: 128),
                        JobId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.Userid)
                .Index(t => t.Userid)
                .Index(t => t.JobId);
            
            CreateTable(
                "dbo.Complaints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ComplaintTitle = c.String(),
                        Desc = c.String(),
                        Created_at = c.DateTime(nullable: false),
                        Updated_at = c.DateTime(nullable: false),
                        compimage = c.String(),
                        UserId = c.String(maxLength: 128),
                        WorkerId = c.Int(nullable: false),
                        works_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.Works", t => t.works_Id)
                .Index(t => t.UserId)
                .Index(t => t.works_Id);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobId = c.Int(),
                        Loc_Address = c.String(),
                        Created_At = c.DateTime(nullable: false),
                        Updated_At = c.DateTime(nullable: false),
                        Geolocation = c.Geography(),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.NotificationModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Details = c.String(),
                        Title = c.String(),
                        DetailsURL = c.String(),
                        Receiver = c.String(),
                        Date = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Ratings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Rate = c.Int(nullable: false),
                        Feedback = c.String(),
                        UsernameFeedback = c.String(),
                        WorkerID = c.Int(nullable: false),
                        Jobid = c.Int(nullable: false),
                        Works_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Works", t => t.Works_Id)
                .Index(t => t.Works_Id);
            
            CreateTable(
                "dbo.WorkerRegImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(nullable: false),
                        Userid = c.String(nullable: false, maxLength: 128),
                        Jobid = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jobs", t => t.Jobid, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.Userid, cascadeDelete: true)
                .Index(t => t.Userid)
                .Index(t => t.Jobid);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.Skills",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Skillname = c.String(),
                        Jobid = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jobs", t => t.Jobid, cascadeDelete: true)
                .Index(t => t.Jobid);
            
            CreateTable(
                "dbo.SkillServiceTasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Skillname = c.String(),
                        Taskdet = c.Int(nullable: false),
                        UserId = c.String(),
                        Jobid = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SkillsOfWorkers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Jobid = c.Int(nullable: false),
                        Skillname = c.String(),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jobs", t => t.Jobid, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.Jobid)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Task_Book",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Taskbook_Type = c.String(),
                        Taskbook_Status = c.Int(nullable: false),
                        Taskbook_Created_at = c.DateTime(nullable: false),
                        Taskbook_Updated_at = c.DateTime(nullable: false),
                        workerId = c.Int(),
                        TaskDetId = c.Int(nullable: false),
                        TaskDetails_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaskDetails", t => t.TaskDetails_Id)
                .Index(t => t.TaskDetails_Id);
            
            CreateTable(
                "dbo.Taskeds",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TaskType = c.String(),
                        TaskPayable = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaskStatus = c.Int(nullable: false),
                        TaskCompletionTime = c.DateTime(nullable: false),
                        TaskCreated_at = c.DateTime(nullable: false),
                        TaskUpdated_at = c.DateTime(nullable: false),
                        WorkerId = c.Int(nullable: false),
                        TaskDetId = c.Int(nullable: false),
                        TaskDetails_Id = c.Int(),
                        Work_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaskDetails", t => t.TaskDetails_Id)
                .ForeignKey("dbo.Works", t => t.Work_Id)
                .Index(t => t.TaskDetails_Id)
                .Index(t => t.Work_Id);
            
            CreateTable(
                "dbo.TaskFiles",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TaskFileName = c.String(),
                        TaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.TransactionHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        tasktitle = c.String(),
                        BidAmount = c.String(),
                        TotalAmount = c.String(),
                        Commission = c.String(),
                        Payer = c.String(),
                        Reciever = c.String(),
                        Created_At = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Firstname = c.String(nullable: false),
                        Lastname = c.String(nullable: false),
                        ProfilePicture = c.String(),
                        Created_At = c.DateTime(nullable: false),
                        Updated_At = c.DateTime(nullable: false),
                        Userid = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Userid, cascadeDelete: true)
                .Index(t => t.Userid);
            
            CreateTable(
                "dbo.WithDrawRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Money = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Email = c.String(nullable: false),
                        Username = c.String(nullable: false),
                        status = c.Boolean(nullable: false),
                        UserId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.users", "Userid", "dbo.AspNetUsers");
            DropForeignKey("dbo.Taskeds", "Work_Id", "dbo.Works");
            DropForeignKey("dbo.Taskeds", "TaskDetails_Id", "dbo.TaskDetails");
            DropForeignKey("dbo.Task_Book", "TaskDetails_Id", "dbo.TaskDetails");
            DropForeignKey("dbo.SkillsOfWorkers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.SkillsOfWorkers", "Jobid", "dbo.Jobs");
            DropForeignKey("dbo.Skills", "Jobid", "dbo.Jobs");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.WorkerRegImages", "Userid", "dbo.AspNetUsers");
            DropForeignKey("dbo.WorkerRegImages", "Jobid", "dbo.Jobs");
            DropForeignKey("dbo.Ratings", "Works_Id", "dbo.Works");
            DropForeignKey("dbo.NotificationModels", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Locations", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Complaints", "works_Id", "dbo.Works");
            DropForeignKey("dbo.Complaints", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Bids", "Work_Id", "dbo.Works");
            DropForeignKey("dbo.Works", "Userid", "dbo.AspNetUsers");
            DropForeignKey("dbo.Works", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.Bids", "TaskDetails_Id", "dbo.TaskDetails");
            DropForeignKey("dbo.TaskDetails", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TaskDetails", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.Wallets", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.users", new[] { "Userid" });
            DropIndex("dbo.Taskeds", new[] { "Work_Id" });
            DropIndex("dbo.Taskeds", new[] { "TaskDetails_Id" });
            DropIndex("dbo.Task_Book", new[] { "TaskDetails_Id" });
            DropIndex("dbo.SkillsOfWorkers", new[] { "UserId" });
            DropIndex("dbo.SkillsOfWorkers", new[] { "Jobid" });
            DropIndex("dbo.Skills", new[] { "Jobid" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.WorkerRegImages", new[] { "Jobid" });
            DropIndex("dbo.WorkerRegImages", new[] { "Userid" });
            DropIndex("dbo.Ratings", new[] { "Works_Id" });
            DropIndex("dbo.NotificationModels", new[] { "User_Id" });
            DropIndex("dbo.Locations", new[] { "UserId" });
            DropIndex("dbo.Complaints", new[] { "works_Id" });
            DropIndex("dbo.Complaints", new[] { "UserId" });
            DropIndex("dbo.Works", new[] { "JobId" });
            DropIndex("dbo.Works", new[] { "Userid" });
            DropIndex("dbo.TaskDetails", new[] { "UserId" });
            DropIndex("dbo.TaskDetails", new[] { "JobId" });
            DropIndex("dbo.Bids", new[] { "Work_Id" });
            DropIndex("dbo.Bids", new[] { "TaskDetails_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Wallets", new[] { "UserId" });
            DropTable("dbo.WithDrawRequests");
            DropTable("dbo.users");
            DropTable("dbo.TransactionHistories");
            DropTable("dbo.TaskFiles");
            DropTable("dbo.Taskeds");
            DropTable("dbo.Task_Book");
            DropTable("dbo.SkillsOfWorkers");
            DropTable("dbo.SkillServiceTasks");
            DropTable("dbo.Skills");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.WorkerRegImages");
            DropTable("dbo.Ratings");
            DropTable("dbo.NotificationModels");
            DropTable("dbo.Locations");
            DropTable("dbo.Complaints");
            DropTable("dbo.Works");
            DropTable("dbo.Jobs");
            DropTable("dbo.TaskDetails");
            DropTable("dbo.Bids");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Wallets");
        }
    }
}
