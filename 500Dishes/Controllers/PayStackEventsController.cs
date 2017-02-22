using _500Dishes.Misc;
using _500Dishes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
//using System.Web.Http; 

namespace _500Dishes.Controllers
{
    public class PayStackEventsController : Controller
    {
        // GET: PayStackEvents
        [HttpPost]
        public string Index()
        {
            System.IO.Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new System.IO.StreamReader(req).ReadToEnd();
            EventObject eventObject = Newtonsoft.Json.JsonConvert.DeserializeObject<EventObject>(json);

            if (eventObject == null)
                return "null ish";

            switch (eventObject.@event)
            {
                case "subscription.create":
                    new Task(() => { createSubscriptionAync(eventObject.data); }).Start();
                    //createSubscriptionAync(eventObject);
                    break;

                case "subscription.disable":
                    new Task(() => { disableSubscriptionAync(eventObject.data); }).Start();
                    break;

                case "subscription.enable":
                    new Task(() => { enableSubscriptionAync(eventObject.data); }).Start();
                    break;
            }
            //return new HttpStatusCodeResult(HttpStatusCode.OK);
            return "event => " + eventObject.@event + " event amount => " + eventObject.data.amount;
        }

        [HttpPost]
        public async Task<ActionResult> EventManager(/*RootObject eventObject*/)
        {

            System.IO.Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);

            string json = new System.IO.StreamReader(req).ReadToEnd();
            //StripeEvent stripeEvent = null;

            EventObject eventObject = Newtonsoft.Json.JsonConvert.DeserializeObject<EventObject>(json);

            if (eventObject == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            switch (eventObject.@event)
            {
                case "subscription.create":
                    //new Task(() => { createSubscriptionAync(eventObject.data); }).Start();
                    await createSubscriptionAync(eventObject.data);
                    break;

                case "subscription.disable":
                    //new Task(() => { disableSubscriptionAync(eventObject.data); }).Start();
                    await disableSubscriptionAync(eventObject.data);
                    break;

                case "subscription.enable":
                    //new Task(() => { enableSubscriptionAync(eventObject.data); }).Start();
                    await enableSubscriptionAync(eventObject.data);
                    break;
                case "invoice.create":
                    await createInvoiceForSubscriptionAync(eventObject.data);
                    break;
                case "invoice.update":
                    await enableInvoiceForSubscriptionAync(eventObject.data);
                    break;
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private async Task enableInvoiceForSubscriptionAync(Data data)
        {
            if (data.paid && data.status == "success")
            {
                using (DataContext db = new DataContext())
                {
                    var customer = db.Users.SingleOrDefault(x => x.CustomerCode == data.customer.customer_code);
                    if (customer != null)
                    {
                        var prevSub = await db.UserSubscriptions.Where(x => x.UserID == customer.UserID && x.Enabled && !x.Deleted).OrderByDescending(x => x.EndDate).FirstAsync();
                        if (prevSub != null)
                            await disableSubscriptionAyncMethod(customer.CustomerCode, prevSub.SubscriptionCode, prevSub.SubPlan.PlanCode);

                        var userSub = db.UserSubscriptions.SingleOrDefault(x => x.SubscriptionCode == data.subscription.subscription_code);
                        if (userSub != null && !userSub.Enabled)
                        {
                            userSub.Enabled = true;
                            userSub.Token = await PayStackAPI.FetchSubscription(data.subscription.subscription_code);
                            userSub.StartDate = DateTime.UtcNow.AddHours(1);
                            userSub.EndDate = DateTime.UtcNow.AddHours(1).AddDays(6);
                            db.Entry(userSub).State = System.Data.Entity.EntityState.Modified;
                            customer.SubscriptionPlanID = userSub.SubPlan.ID;
                            db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        private async Task createInvoiceForSubscriptionAync(Data data)
        {
            using (DataContext db = new DataContext())
            {
                var customer = db.Users.SingleOrDefault(x => x.CustomerCode == data.customer.customer_code);
                if (customer != null)
                {
                    var userSub = await db.UserSubscriptions.Include(x => x.SubPlan).Where(x => x.UserID == customer.UserID && x.Enabled && !x.Deleted).OrderByDescending(x => x.EndDate).FirstOrDefaultAsync();
                    if (userSub != null)
                    {
                        var newSub = new UserSubscription()
                        {
                            UserID = customer.UserID,
                            StartDate = DateTime.UtcNow.AddHours(1),
                            SubID = userSub.SubPlan.ID,
                            SubscriptionCode = data.subscription.subscription_code
                        };

                        customer.SubscriptionPlanID = userSub.SubPlan.ID;
                        db.UserSubscriptions.Add(newSub);
                        db.Entry(customer).State = System.Data.Entity.EntityState.Modified;                        
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task createSubscriptionAync(Data responseData)
        {
            string customerCode = responseData.customer.customer_code;
            string subCode = responseData.subscription_code;
            string planCode = responseData.plan.plan_code;

            await createSubAsyncMethod(customerCode, subCode, planCode);
            await enableSubscriptionAync(responseData);
        }

        private async Task createSubAsyncMethod(string customerCode, string subCode, string planCode)
        {
            using (DataContext db = new DataContext())
            {
                var customer = db.Users.SingleOrDefault(x => x.CustomerCode == customerCode);
                if (customer != null)
                {
                    var oldPlan = await db.UserSubscriptions.Include(x => x.SubPlan).Where(x => x.UserID == customer.UserID && x.Enabled ==  true && x.Deleted == false).OrderByDescending(x => x.EndDate).FirstOrDefaultAsync();
                    if (oldPlan != null)
                        await disableSubscriptionAyncMethod(customerCode, oldPlan.SubscriptionCode, oldPlan.SubPlan.PlanCode);

                    var subPlan = db.SubscriptionPlans.SingleOrDefault(x => x.PlanCode == planCode);
                    if (subPlan != null)
                    {
                        var newSub = new UserSubscription()
                        {
                            UserID = customer.UserID,
                            StartDate = DateTime.UtcNow.AddHours(1),
                            SubID = subPlan.ID,
                            SubscriptionCode = subCode
                        };

                        customer.SubscriptionPlanID = subPlan.ID;
                        db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                        db.UserSubscriptions.Add(newSub);
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task disableSubscriptionAync(Data responseData)
        {
            string customerCode = responseData.customer.customer_code;
            string subCode = responseData.subscription_code;
            string planCode = responseData.plan.plan_code;

            await disableSubscriptionAyncMethod(customerCode, subCode, planCode);
        }

        private async Task disableSubscriptionAyncMethod(string customerCode, string subCode, string planCode)
        {
            using (DataContext db = new DataContext())
            {
                var customer = db.Users.SingleOrDefault(x => x.CustomerCode == customerCode);
                if (customer != null)
                {
                    var subPlan = db.SubscriptionPlans.SingleOrDefault(x => x.PlanCode == planCode);
                    if (subPlan != null)
                    {
                        var userSub = db.UserSubscriptions.SingleOrDefault(x => x.SubscriptionCode == subCode && x.SubID == subPlan.ID && x.UserID == customer.UserID);
                        userSub.Deleted = true;
                        db.Entry(userSub).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task enableSubscriptionAync(Data responseData)
        {
            string customerCode = responseData.customer.customer_code;
            string subCode = responseData.subscription_code;
            string planCode = responseData.plan.plan_code;

            await enableSubAsyncMethod(customerCode, subCode, planCode);
        }

        private async Task enableSubAsyncMethod(string customerCode, string subCode, string planCode)
        {
            using (DataContext db = new DataContext())
            {
                var customer = db.Users.SingleOrDefault(x => x.CustomerCode == customerCode);
                if (customer != null)
                {
                    var subPlan = db.SubscriptionPlans.SingleOrDefault(x => x.PlanCode == planCode);
                    if (subPlan != null)
                    {
                        var userSub = db.UserSubscriptions.SingleOrDefault(x => x.SubscriptionCode == subCode && x.SubID == subPlan.ID);
                        if (!userSub.Enabled)
                        {
                            userSub.Enabled = true;
                            userSub.Token = await PayStackAPI.FetchSubscription(subCode);
                            userSub.StartDate = DateTime.UtcNow.AddHours(1);
                            userSub.EndDate = DateTime.UtcNow.AddHours(1).AddDays(6);
                            db.Entry(userSub).State = System.Data.Entity.EntityState.Modified;

                            customer.SubscriptionPlanID = subPlan.ID;
                            db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
        }


    }
}
