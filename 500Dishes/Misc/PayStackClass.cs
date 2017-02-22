using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _500Dishes.Misc
{
    public class PayStackClass
    {
        public string @event  { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public Dictionary<string, object> data { get; set; }
    }

    public class PayStackEvent_Plan
    {
        public string plan_code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int amount { get; set; }
        public string interval { get; set; }
        public bool send_invoices { get; set; }
        public string currency { get; set; }
        public bool send_sms { get; set; }
    }

    public class PayStackEvent_Customer
    {
        public string customer_code { get; set; }
        public string email { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
        public string phone { get; set; }
        public Dictionary<string, object> metadata { get; set; }
    }

    
    public class Plan
    {
        public string name { get; set; }
        public string plan_code { get; set; }
        public object description { get; set; }
        public int amount { get; set; }
        public string interval { get; set; }
        public bool send_invoices { get; set; }
        public bool send_sms { get; set; }
        public string currency { get; set; }
    }

    public class Authorization
    {
        public string authorization_code { get; set; }
        public string bin { get; set; }
        public string last4 { get; set; }
        public string exp_month { get; set; }
        public string exp_year { get; set; }
        public string card_type { get; set; }
        public string bank { get; set; }
        public string country_code { get; set; }
        public string brand { get; set; }
    }

    public class Metadata
    {
    }

    public class Customer
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string customer_code { get; set; }
        public string phone { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Data
    {
        public string domain { get; set; }
        public string status { get; set; }
        public string subscription_code { get; set; }
        public int amount { get; set; }
        public string cron_expression { get; set; }
        public string next_payment_date { get; set; }
        public object open_invoice { get; set; }
        public string createdAt { get; set; }
        public string email_token { get; set; }
        public Plan plan { get; set; }
        public Authorization authorization { get; set; }
        public Customer customer { get; set; }
        public string invoice_code { get; set; }
        public string period_start { get; set; }
        public string period_end { get; set; }
        public bool paid { get; set; }
        public string paid_at { get; set; }
        public object description { get; set; }
        public Subscription subscription { get; set; }
        public Transaction transaction { get; set; }
    }

    public class EventObject
    {
        public string @event { get; set; }
        public Data data { get; set; }
    }

    public class Transaction
    {
        public string reference { get; set; }
        public string status { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
    }


    public class Subscription
    {
        public string status { get; set; }
        public string subscription_code { get; set; }
        public int amount { get; set; }
        public string cron_expression { get; set; }
        public string next_payment_date { get; set; }
        public object open_invoice { get; set; }
    }



}