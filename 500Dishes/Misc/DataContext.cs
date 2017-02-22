using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
using _500Dishes.Models;

namespace _500Dishes.Misc
{ 
    public class DataContext : DbContext
    {
         public DataContext()
            : base("name=ConnectionString")
         { }
         
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Drink> Drinks { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<VendorLocation> VendorsLocations { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Models.Transaction> Transactions { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Review>().HasRequired(x => x.Order).WithOptional(x => x.Review);
        }
    }
}