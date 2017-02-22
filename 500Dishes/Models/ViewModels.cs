using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _500Dishes.Models
{
    public class RegisterViewModel
    {
        [Required, MinLength(2), Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required, MinLength(2), Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required, DataType(DataType.EmailAddress), Display(Name = "Email Address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "please enter your phone number"), RegularExpression(@"\d{11}", ErrorMessage = "phone number should contain 11 digits")]
        public string Phone { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        [Required, DataType(DataType.Password), Display(Name = "Confirm Password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required, DataType(DataType.EmailAddress), Display(Name = "Email Address")]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VendorViewModel
    {
        public int ID { get; set; }
        [Required, MinLength(2)]
        public string FirstName { get; set; }
        [Required, MinLength(2)]
        public string LastName { get; set; }
        [Required, MinLength(2)]
        public string BrandName { get; set; }
        [Required(ErrorMessage = "please enter your phone number"), RegularExpression(@"\d{11}", ErrorMessage = "phone number should contain 11 digits")]
        public string Phone { get; set; }
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.MultilineText)]
        public string About { get; set; }
        [DataType(DataType.MultilineText)]
        public string FullAddress { get; set; }
        public string ImagePath { get; set; }
    }

    //public class BookingViewModel
    //{
    //    public Vendor vendor { get; set; }
    //    public List<Order> Bookings { get; set; }
    //}

    public class MealViewModel
    {
        [Required, MinLength(2)]
        public string Name { get; set; }
        [Required, DataType(DataType.MultilineText), MinLength(5)]
        public string Description { get; set; }
    }

    public class EditMealViewModel
    {
        public int ID { get; set; }
        [Required, MinLength(2)]
        public string Name { get; set; }
        [Required, DataType(DataType.MultilineText), MinLength(5)]
        public string Description { get; set; }
        public string ImagePath { get; set; }
    }

    public class WithdrawEarningsViewModel
    {
        public int VendorID { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        [DataType (DataType.Currency), Required]
        public decimal Amount { get; set; }
        [DataType(DataType.MultilineText)]
        public string ExtraText { get; set; }
    }

    public class WithdrawalSettingsViewModel
    {
        public int VendorID { get; set; }
        [Required]
        public int? BankName { get; set; }
        [Required, MinLength(2, ErrorMessage = "Please enter a valid account name")]
        public string AccountName { get; set; }
        [Required, StringLength(10, MinimumLength = 10, ErrorMessage = "Please enter a valid account number")]
        public string AccountNumber { get; set; }
    }

    public class TransactionsViewModel
    {
        public TransactionsViewModel()
        {   Withdrawals = new List<Transaction>();   }
        public int VendorID { get; set; }
        public decimal Balance { get; set; }
        public ICollection<Transaction> Withdrawals { get; set; }
    }

    public class OrderViewModel
    {
        [Required, DataType(DataType.MultilineText)]
        public string Address { get; set; }
        [Required(ErrorMessage = "please enter your phone number"), RegularExpression(@"\d{11}", ErrorMessage = "phone number should contain 11 digits")]
        public string Phone { get; set; }
        [RegularExpression(@"\d{11}", ErrorMessage = "phone number should contain 11 digits")]
        public string AltPhone { get; set; }
        
        public int CustomerID { get; set; }
        public int MealID { get; set; }
        public int VendorID { get; set; }
    }

    public class SubscriptionsViewModel
    {
        public ICollection<SubscriptionPlan> AllPlans { get; set; }
        public UserSubscription UserSub { get; set; }
    }

    public class EditPasswordViewModel
    {
        [Required(ErrorMessage = "Please enter password"), Display(Name = "New Password")]
        [DataType(DataType.Password), MinLength(6, ErrorMessage = "Passowrd should contain at least 6 characters")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Please enter password"), Compare("NewPassword", ErrorMessage = "password mismatch")]
        [DataType(DataType.Password), Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    public class ReviewViewModel
    {
        [Required, DataType(DataType.MultilineText)]
        public string Text { get; set; }
        [Required]
        public int OrderID { get; set; }
        [Required]
        public int Rating { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public int VendorID { get; set; }
        [Required]
        public int MealID { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        public string Code { get; set; }
        [Required, DataType(DataType.Password), Display(Name = "New Password")]
        public string Password { get; set; }
        [Required, DataType(DataType.Password), Display(Name = "Confirm Password"), Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class PreForgotPasswordViewModel
    {
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }

    public class UserViewModel
    {
        [Required, MinLength(6)]
        public string Name { get; set; }
        [Required(ErrorMessage = "please enter your phone number"), RegularExpression(@"\d{11}", ErrorMessage = "phone number should contain 11 digits")]
        public string Phone { get; set; }
        [DataType(DataType.EmailAddress, ErrorMessage = "invalid email address"), Required]
        public string Email { get; set; }
        //public string ImagePath { get; set; }
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }
        public string SubName { get; set; }   
    }

    public class OurClientsViewModel
    {
        public OurClientsViewModel()
        {
            Vendors = new List<Vendor>();
            Reviews = new List<Review>();
        }
        public List<Vendor> Vendors { get; set; }
        public List<Review> Reviews { get; set; }
    }
}