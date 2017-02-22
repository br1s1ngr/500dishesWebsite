namespace _500Dishes.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _500dishes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Banks",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Bids",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Keyword = c.String(),
                        CurrentBid = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VendorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Vendors", t => t.VendorID, cascadeDelete: true)
                .Index(t => t.VendorID);
            
            CreateTable(
                "dbo.Vendors",
                c => new
                    {
                        VendorID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        BrandName = c.String(),
                        Phone = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        AccountBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OverallBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Verified = c.Boolean(nullable: false),
                        ImagePath = c.String(),
                        DateVerified = c.DateTime(),
                        About = c.String(),
                        FullAddress = c.String(),
                        CustomerCode = c.String(),
                        AuthCode = c.String(),
                        ForgotPasswordCode = c.String(),
                        ConfirmMail = c.Boolean(nullable: false),
                        ConfirmMailCode = c.String(),
                        MealPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BankID = c.Int(),
                        AccountNumber = c.String(),
                        AccountName = c.String(),
                        RoleID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VendorID)
                .ForeignKey("dbo.Banks", t => t.BankID)
                .ForeignKey("dbo.Roles", t => t.RoleID, cascadeDelete: true)
                .Index(t => t.BankID)
                .Index(t => t.RoleID);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderID = c.Int(nullable: false, identity: true),
                        Address = c.String(),
                        Phone = c.String(),
                        AltPhone = c.String(),
                        Date = c.DateTime(nullable: false),
                        UserID = c.Int(nullable: false),
                        MealID = c.Int(nullable: false),
                        VendorID = c.Int(nullable: false),
                        DrinkID = c.Int(),
                    })
                .PrimaryKey(t => t.OrderID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .ForeignKey("dbo.Meals", t => t.MealID, cascadeDelete: true)
                .ForeignKey("dbo.Drinks", t => t.DrinkID)
                .ForeignKey("dbo.Vendors", t => t.VendorID, cascadeDelete: true)
                .Index(t => t.UserID)
                .Index(t => t.MealID)
                .Index(t => t.VendorID)
                .Index(t => t.DrinkID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Phone = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        ImagePath = c.String(),
                        Address = c.String(),
                        CustomerCode = c.String(),
                        ForgotPasswordCode = c.String(),
                        ConfirmMail = c.Boolean(nullable: false),
                        ConfirmMailCode = c.String(),
                        RoleID = c.Int(nullable: false),
                        SubscriptionPlanID = c.Int(),
                        AuthCode = c.String(),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.SubscriptionPlans", t => t.SubscriptionPlanID)
                .ForeignKey("dbo.Roles", t => t.RoleID, cascadeDelete: false)
                .Index(t => t.RoleID)
                .Index(t => t.SubscriptionPlanID);
            
            CreateTable(
                "dbo.SubscriptionPlans",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(nullable: false),
                        PlanCode = c.String(),
                        NoOfMeals = c.Int(nullable: false),
                        DrinksPerMeal = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Reviews",
                c => new
                    {
                        ReviewID = c.Int(nullable: false),
                        Text = c.String(),
                        Rating = c.Int(nullable: false),
                        UserID = c.Int(nullable: false),
                        VendorID = c.Int(nullable: false),
                        MealID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReviewID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .ForeignKey("dbo.Meals", t => t.MealID, cascadeDelete: true)
                .ForeignKey("dbo.Orders", t => t.ReviewID)
                .ForeignKey("dbo.Vendors", t => t.VendorID, cascadeDelete: true)
                .Index(t => t.ReviewID)
                .Index(t => t.UserID)
                .Index(t => t.VendorID)
                .Index(t => t.MealID);
            
            CreateTable(
                "dbo.Meals",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ImagePath = c.String(),
                        Description = c.String(),
                        Verified = c.Boolean(nullable: false),
                        DateVerified = c.DateTime(),
                        DateAdded = c.DateTime(nullable: false),
                        VendorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Vendors", t => t.VendorID, cascadeDelete: false)
                .Index(t => t.VendorID);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        RoleID = c.Int(nullable: false, identity: true),
                        RoleName = c.String(),
                    })
                .PrimaryKey(t => t.RoleID);
            
            CreateTable(
                "dbo.Drinks",
                c => new
                    {
                        DrinkID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        VendorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DrinkID)
                .ForeignKey("dbo.Vendors", t => t.VendorID, cascadeDelete: true)
                .Index(t => t.VendorID);
            
            CreateTable(
                "dbo.VendorLocations",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        LocationID = c.Int(nullable: false),
                        VendorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Locations", t => t.LocationID, cascadeDelete: true)
                .ForeignKey("dbo.Vendors", t => t.VendorID, cascadeDelete: true)
                .Index(t => t.LocationID)
                .Index(t => t.VendorID);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        LocationID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.LocationID);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExtraText = c.String(),
                        Date = c.DateTime(nullable: false),
                        VendorID = c.Int(nullable: false),
                        Withdrawal = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Vendors", t => t.VendorID, cascadeDelete: true)
                .Index(t => t.VendorID);
            
            CreateTable(
                "dbo.UserSubscriptions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SubID = c.Int(nullable: false),
                        UserID = c.Int(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        SubscriptionCode = c.String(),
                        Enabled = c.Boolean(nullable: false),
                        Token = c.String(),
                        TrxRef = c.String(),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .ForeignKey("dbo.SubscriptionPlans", t => t.SubID, cascadeDelete: true)
                .Index(t => t.SubID)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserSubscriptions", "SubID", "dbo.SubscriptionPlans");
            DropForeignKey("dbo.UserSubscriptions", "UserID", "dbo.Users");
            DropForeignKey("dbo.Vendors", "RoleID", "dbo.Roles");
            DropForeignKey("dbo.Transactions", "VendorID", "dbo.Vendors");
            DropForeignKey("dbo.VendorLocations", "VendorID", "dbo.Vendors");
            DropForeignKey("dbo.VendorLocations", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.Orders", "VendorID", "dbo.Vendors");
            DropForeignKey("dbo.Drinks", "VendorID", "dbo.Vendors");
            DropForeignKey("dbo.Orders", "DrinkID", "dbo.Drinks");
            DropForeignKey("dbo.Users", "RoleID", "dbo.Roles");
            DropForeignKey("dbo.Reviews", "VendorID", "dbo.Vendors");
            DropForeignKey("dbo.Reviews", "ReviewID", "dbo.Orders");
            DropForeignKey("dbo.Meals", "VendorID", "dbo.Vendors");
            DropForeignKey("dbo.Reviews", "MealID", "dbo.Meals");
            DropForeignKey("dbo.Orders", "MealID", "dbo.Meals");
            DropForeignKey("dbo.Reviews", "UserID", "dbo.Users");
            DropForeignKey("dbo.Orders", "UserID", "dbo.Users");
            DropForeignKey("dbo.Users", "SubscriptionPlanID", "dbo.SubscriptionPlans");
            DropForeignKey("dbo.Bids", "VendorID", "dbo.Vendors");
            DropForeignKey("dbo.Vendors", "BankID", "dbo.Banks");
            DropIndex("dbo.UserSubscriptions", new[] { "UserID" });
            DropIndex("dbo.UserSubscriptions", new[] { "SubID" });
            DropIndex("dbo.Transactions", new[] { "VendorID" });
            DropIndex("dbo.VendorLocations", new[] { "VendorID" });
            DropIndex("dbo.VendorLocations", new[] { "LocationID" });
            DropIndex("dbo.Drinks", new[] { "VendorID" });
            DropIndex("dbo.Meals", new[] { "VendorID" });
            DropIndex("dbo.Reviews", new[] { "MealID" });
            DropIndex("dbo.Reviews", new[] { "VendorID" });
            DropIndex("dbo.Reviews", new[] { "UserID" });
            DropIndex("dbo.Reviews", new[] { "ReviewID" });
            DropIndex("dbo.Users", new[] { "SubscriptionPlanID" });
            DropIndex("dbo.Users", new[] { "RoleID" });
            DropIndex("dbo.Orders", new[] { "DrinkID" });
            DropIndex("dbo.Orders", new[] { "VendorID" });
            DropIndex("dbo.Orders", new[] { "MealID" });
            DropIndex("dbo.Orders", new[] { "UserID" });
            DropIndex("dbo.Vendors", new[] { "RoleID" });
            DropIndex("dbo.Vendors", new[] { "BankID" });
            DropIndex("dbo.Bids", new[] { "VendorID" });
            DropTable("dbo.UserSubscriptions");
            DropTable("dbo.Transactions");
            DropTable("dbo.Locations");
            DropTable("dbo.VendorLocations");
            DropTable("dbo.Drinks");
            DropTable("dbo.Roles");
            DropTable("dbo.Meals");
            DropTable("dbo.Reviews");
            DropTable("dbo.SubscriptionPlans");
            DropTable("dbo.Users");
            DropTable("dbo.Orders");
            DropTable("dbo.Vendors");
            DropTable("dbo.Bids");
            DropTable("dbo.Banks");
        }
    }
}
