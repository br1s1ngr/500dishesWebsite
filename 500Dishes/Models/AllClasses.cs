using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _500Dishes.Models
{
    public class Role
    {
        public Role()
        {
            Users = new List<User>();
        }

        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }

    public class User
    {
        public User()
        {
            Orders = new List<Order>();
            Reviews = new List<Review>();
        }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ImagePath { get; set; }
        public string Address { get; set; }
        public string CustomerCode { get; set; }
        public string ForgotPasswordCode { get; set; }
        public bool ConfirmMail { get; set; }
        public string ConfirmMailCode { get; set; }
        
        /// below i have the foreign keys
        public int RoleID { get; set; }
        public int? SubscriptionPlanID { get; set; }
        public string AuthCode { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual SubscriptionPlan CurrentSub { get; set; }
        public virtual Role UserRole { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }

    public class Vendor {
        public Vendor()
        {
            Drinks = new List<Drink>();
            DeliveryLocations = new List<VendorLocation>();
            Meals = new List<Meal>();
            Bids = new List<Bid>();
            Bookings = new List<Order>();
            Transactions = new List<Transaction>();
            Reviews = new List<Review>();
        }
        
        public int VendorID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BrandName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal OverallBalance { get; set; }
        public bool Verified { get; set; }
        public string ImagePath { get; set; }
        public DateTime? DateVerified { get; set; }
        public string About { get; set; }
        public string FullAddress { get; set; }
        public string CustomerCode { get; set; }
        public string AuthCode { get; set; }

        public string ForgotPasswordCode { get; set; }
        public bool ConfirmMail { get; set; }
        public string ConfirmMailCode { get; set; }

        public decimal MealPrice { get; set; }
        //bank details
        public int?  BankID { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }

        /// below i have the foreign keys
        public int RoleID { get; set; }

        public virtual ICollection<Drink> Drinks { get; set; }
        public virtual ICollection<VendorLocation> DeliveryLocations { get; set; }
        public virtual ICollection<Meal> Meals { get; set; }
        public virtual ICollection<Bid> Bids { get; set; }
        public virtual ICollection<Order> Bookings { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual Role UserRole { get; set; }
        public virtual  Bank Bank { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }

    public class Bank
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class Meal
    {
        public Meal()
        {
            Orders = new List<Order>();
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public bool Verified { get; set; }
        public DateTime? DateVerified { get; set; }
        public DateTime DateAdded { get; set; }


        /// foreign keys
        public int VendorID { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        public int GetRating()
        {
            int rating = (this.Reviews.Sum(x => x.Rating) / this.Reviews.Count);
            return rating < 1 ? 1 : rating;
        }
    }

    public class Booking
    {
        public int ID { get; set; }
        public int MyProperty { get; set; }
    }

    public class Order
    {
        public int OrderID { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string AltPhone { get; set; }
        public DateTime Date { get; set; }

        ///foreign keys
        public int UserID { get; set; }
        public int MealID { get; set; }
        public int VendorID { get; set; }
        public int? DrinkID { get; set; }
        //public int? ReviewID { get; set; }

        public virtual Drink Drink { get; set; }
        public virtual User Customer { get; set; }
        public virtual Meal Meal { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual Review Review { get; set; }
    }

    public class Drink
    {
        public int DrinkID { get; set; }
        public string Name { get; set; }
        
        ///foreign key
        public int VendorID { get; set; }
        public virtual Vendor Vendor { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

    public class Location
    {
        public int LocationID { get; set; }
        public string Name { get; set; }
    }

    public class VendorLocation
    {
        public int ID { get; set; }
        
        //foreign keys
        public int LocationID { get; set; }
        public int VendorID { get; set; }

        public virtual Location Location { get; set; }
        public virtual Vendor Vendor { get; set; }
    }

    public class SubscriptionPlan 
    {
        public int ID { get; set; }
        [Required, MinLength(2)]
        public string Name { get; set; }
        [Required, DataType(DataType.Currency)]
        public decimal Amount { get; set; }
        [Required, DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public string PlanCode { get; set; }
        [Required, Range(0, 20)]
        public int NoOfMeals { get; set; }
        [Required, Range(0, 20)]
        public int DrinksPerMeal { get; set; }

        public ICollection<User> Subscribers { get; set; }
    }

    public class UserSubscription {
        public int ID { get; set; }
        public int SubID { get; set; }
        public int UserID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SubscriptionCode { get; set; }
        public bool Enabled { get; set; }
        public string Token { get; set; }
        public string TrxRef { get; set; }
        public bool Deleted { get; set; }

        public virtual User Customer { get; set; }
        [ForeignKey("SubID")]
        public virtual SubscriptionPlan SubPlan { get; set; }
    }

    public class Bid
    {
        public int ID { get; set; }
        public string Keyword { get; set; }
        public decimal CurrentBid { get; set; }

        public int VendorID { get; set; }
        public virtual Vendor Vendor { get; set; }
    }

    public class Transaction
    {
        public int ID { get; set; }
        public decimal Amount { get; set; }
        public string ExtraText { get; set; }
        public DateTime Date { get; set; }
        public int VendorID { get; set; }
        public bool Withdrawal { get; set; }
        public virtual Vendor Vendor { get; set; }
    }

    public class Review
    {
        [Key]
        public int ReviewID { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; }
        //public int OrderID { get; set; }
        public int UserID { get; set; }
        public int VendorID { get; set; }
        public int MealID { get; set; }

        public virtual User Customer { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual Meal Meal { get; set; }
        public virtual Order Order { get; set; }
    }
}