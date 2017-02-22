using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _500Dishes.Models
{
    public class Data
    {
        public List<object> invoices { get; set; }
        //public Customer customer { get; set; }
        //public Plan plan { get; set; }
        public int integration { get; set; }
        //public Authorization authorization { get; set; }
        public string domain { get; set; }
        public int start { get; set; }
        public string status { get; set; }
        public int quantity { get; set; }
        public int amount { get; set; }
        public string subscription_code { get; set; }
        public string email_token { get; set; }
        public object easy_cron_id { get; set; }
        public string cron_expression { get; set; }
        public string next_payment_date { get; set; }
        public object open_invoice { get; set; }
        public int id { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class RootObject
    {
        public bool status { get; set; }
        public string message { get; set; }
        public Models.Data data { get; set; }
    }
}