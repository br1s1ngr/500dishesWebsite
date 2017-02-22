





using _500Dishes.Misc;
using _500Dishes.Models;
using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace _500Dishes.WebHookHandlers
{
    public class GitHubWebHookHandler : WebHookHandler
    {
        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            // make sure we're only processing the intended type of hook
            //if("GitHub".Equals(receiver, System.StringComparison.CurrentCultureIgnoreCase))
            //{
            //	// todo: replace this placeholder functionality with your own code
            //	string action = context.Actions.First();
            //	JObject incoming = context.GetDataOrDefault<JObject>();
            //}
            EventObject eventObject = context.GetDataOrDefault<EventObject>();
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
            return Task.FromResult(true);
        }

        private async void createSubscriptionAync(Data responseData)
        {
            string customerCode = responseData.customer.customer_code;
            string subCode = responseData.subscription_code;
            string planCode = responseData.plan.plan_code;

            using (DataContext db = new DataContext())
            {
                var customer = db.Users.SingleOrDefault(x => x.CustomerCode == customerCode);
                if (customer != null)
                {
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
                        db.UserSubscriptions.Add(newSub);
                        await db.SaveChangesAsync();
                    }
                }
            }
            enableSubscriptionAync(responseData);

        }

        private async void disableSubscriptionAync(Data responseData)
        {
            string customerCode = responseData.customer.customer_code;
            string subCode = responseData.subscription_code;
            string planCode = responseData.plan.plan_code;

            using (DataContext db = new DataContext())
            {
                var customer = db.Users.SingleOrDefault(x => x.CustomerCode == customerCode);
                if (customer != null)
                {
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
                        db.UserSubscriptions.Add(newSub);
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        private async void enableSubscriptionAync(Data responseData)
        {
            string customerCode = responseData.customer.customer_code;
            string subCode = responseData.subscription_code;
            string planCode = responseData.plan.plan_code;

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
