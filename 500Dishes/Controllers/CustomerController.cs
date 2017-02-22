using _500Dishes.Misc;
using _500Dishes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;

namespace _500Dishes.Controllers
{
    [Authorize(Roles = "U")]
    public class CustomerController : Controller
    {
        DataContext db = new DataContext();

        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddSubscription(bool? refresh)
        {
            ViewBag.Subscription = "active";
            if (refresh.GetValueOrDefault())
                //return Redirect(Request.UrlReferrer.ToString());
                return RedirectToAction("AddSubscription");

            var subscriptions = db.SubscriptionPlans.ToList();
            int customerID = getSignedInUserID();
            var date = DateTime.UtcNow.AddHours(1);
            var userSub = db.UserSubscriptions.Include("SubPlan").Where(x => x.UserID == customerID && x.StartDate != null && x.SubscriptionCode != null && x.Deleted == false).OrderByDescending(x => x.StartDate).FirstOrDefault();
            SubscriptionsViewModel subViewModel = new SubscriptionsViewModel() { AllPlans = subscriptions, UserSub = userSub };
            if (userSub == null)
                TempData["UserSubscriptionMessage"] = "<em>No subscription for user yet</em>";
            return View(subViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSubscription(int? subID)
        {
            ViewBag.Subscription = "active";
            if (subID == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var plan = db.SubscriptionPlans.Find(subID);

            var subscriptions = db.SubscriptionPlans.ToList();
            SubscriptionsViewModel subViewModel = new SubscriptionsViewModel() { AllPlans = subscriptions };

            int customerID = getSignedInUserID();

            var date = DateTime.UtcNow.AddHours(1);
            //find current sub and end it
            var currentUserSub = db.UserSubscriptions.Include("SubPlan").Where(x => x.UserID == customerID && x.StartDate != null && x.SubscriptionCode != null && x.Deleted == false).OrderByDescending(x => x.StartDate).FirstOrDefault(); //&& x.Token != null
            if (currentUserSub != null)
                subViewModel.UserSub = currentUserSub;

            try
            {
                if (currentUserSub != null && currentUserSub.SubID == subID.Value)
                {
                    TempData["ErrorMessage"] = "You are already subscribed to the plan.";
                    return View(subViewModel);
                }
                //var trxResult = await PayStackAPI.InitializeTransaction(Convert.ToInt32(plan.Amount), User.Identity.Name, "", Url.Action("EnableSubscription", "Customer", new { subID = subID }, Request.Url.Scheme));
                //var trxResult = await PayStackAPI.InitializeTransaction(Convert.ToInt32(plan.Amount), User.Identity.Name, "", Url.Action("AddSubscription", "Customer", null, Request.Url.Scheme));
                var trxResult = await PayStackAPI.InitializeTransaction(Convert.ToInt32(plan.Amount), User.Identity.Name, plan.PlanCode, Url.Action("SubWahala", "Customer", new { subID = subID, customerID = customerID }, Request.Url.Scheme));

                if (String.IsNullOrWhiteSpace(trxResult))
                {
                    TempData["ErrorMessage"] = "Error...an error occurred while trying to subscribe you to " + plan.Name + ", Please retry.";
                    return View(subViewModel);
                }

                //if (currentUserSub != null)
                //{
                //    if (await PayStackAPI.DisableSubscription(currentUserSub.SubscriptionCode, currentUserSub.Token))
                //    {
                //        currentUserSub.EndDate = DateTime.UtcNow;
                //        currentUserSub.Deleted = true;
                //        db.Entry(currentUserSub).State = System.Data.Entity.EntityState.Modified;
                //    }
                //    else
                //    {
                //        TempData["ErrorMessage"] = "Error...unable to deactivate your previous subscription, please retry.....";
                //        return View(subViewModel);
                //    }
                //}

                var tempString = trxResult.Split('%');
                string authorizeurl = tempString[0];
                string trxref = tempString[1];
                if (authorizeurl != null)
                {
                    //create new subscription
                    var newSub = new UserSubscription()
                    {
                        UserID = customerID,
                        StartDate = DateTime.UtcNow.AddHours(1),
                        SubID = subID.Value,
                        TrxRef = trxref
                    };
                    db.UserSubscriptions.Add(newSub);

                    //    var user = db.Users.Find(customerID);
                    //    user.SubscriptionPlanID = subID.Value;
                    //    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    //    subViewModel.UserSub = newSub;
                    //}
                    await db.SaveChangesAsync();
                    return Redirect(authorizeurl);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error...an error occurred while trying to subscribe you to " + plan.Name + ", Please retry.";
                db.SaveChanges();
            }
            return View(subViewModel);
        }

        public async Task<ActionResult> SubWahala (int? subID, int? customerId)
        {
            if (subID == null || customerId == null)
            {
                TempData["ErrorMessage"] = "Error...an error occurred during subscription. Please retry.";
                return RedirectToAction("AddSubscription");
            }

            var date = DateTime.Now;
            var userSub = db.UserSubscriptions.Include("SubPlan").Include("Customer").Where(x => x.SubID == subID.Value && x.UserID == customerId.Value).OrderByDescending(x => x.StartDate.Value).FirstOrDefault();

            if (userSub != null)
                if (await PayStackAPI.VerifyTransaction(userSub.TrxRef))
                {
                    var currentUserSub = db.UserSubscriptions.Include("SubPlan").Where(x => x.UserID == customerId && x.StartDate != null && x.SubscriptionCode != null && x.Deleted == false).OrderByDescending(x => x.StartDate).FirstOrDefault(); //&& x.Token != null
                    if (currentUserSub != null)
                    {
                        if (await PayStackAPI.DisableSubscription(currentUserSub.SubscriptionCode, currentUserSub.Token))
                        {
                            currentUserSub.EndDate = DateTime.UtcNow;
                            currentUserSub.Deleted = true;
                            db.Entry(currentUserSub).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Error...unable to deactivate your previous subscription, please retry.....";
                            return RedirectToAction("AddSubscription");
                        }
                    }

                    if (await PayStackAPI.CreateSubscription(userSub.Customer.Email, userSub.SubPlan.PlanCode) != null)
                        TempData["SuccessMessage"] = "<strong>Success</strong><br> You have been successfully subscribed to " + userSub.SubPlan.Name;
                }

            return RedirectToAction("AddSubscription", new { refresh = true });
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableSubscription(int? userSubID, int? subID)
        {
            ViewBag.Subscription = "active";
            if (userSubID == null || subID == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //var subscriptions = db.SubscriptionPlans.ToList();
            //SubscriptionsViewModel subViewModel = new SubscriptionsViewModel() { AllPlans = subscriptions };
            //try
            //{
                int customerID = getSignedInUserID();
                //var userSub = db.UserSubscriptions.Where(x => x.UserID == customerID && x.SubID == subID && x.StartDate != null && x.EndDate == null).OrderByDescending(x => x.StartDate).FirstOrDefault();
                //subViewModel.UserSub = userSub;
                var userSub = db.UserSubscriptions.Find(userSubID.Value);
                var plan = db.SubscriptionPlans.Find(subID.Value);

                if (userSub == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                else if (userSub.Enabled)
                    ViewBag.ErrorMessage = "Error...subscription is already active.";

                // latest user sub in neede of activating
                else
                {
                    //verify payment for subscription plan
                    string token = await PayStackAPI.FetchSubscription(userSub.SubscriptionCode);
                    //if (!String.IsNullOrEmpty(codeToken))
                    //{
                    //var code_token = await PayStackAPI.CreateSubscription(User.Identity.Name, plan.PlanCode);

                    if (String.IsNullOrEmpty(token))
                        TempData["ErrorMessage"] = "Error..there is no subscription for <em class=\"alert-link\">" + plan.Name + "</em>, Please retry.";

                    //sub created 0on paystack
                    else
                    {
                        //enable subscription -- don't know what this means yet
                        //but after enabling, i get the other details and set the enabled status of the plan
                        if (await PayStackAPI.EnableSubscription(userSub.SubscriptionCode, token))
                        {
                            //userSub.SubscriptionCode = code;
                            //userSub.Token = token;
                            //userSub.Enabled = true;
                            //userSub.StartDate = DateTime.UtcNow.AddHours(1);
                            //userSub.EndDate = DateTime.UtcNow.AddHours(1).AddDays(6);
                            //db.Entry(userSub).State = System.Data.Entity.EntityState.Modified;
                            //TempData["SuccessMessage"] = "Your subscription to <em class=\"alert-link\">" + plan.Name + "</em> has been activated. You may now place your order(s)";

                            //var user = db.Users.Find(customerID);
                            //user.SubscriptionPlanID = subID.Value;
                            //db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            //subViewModel.UserSub = userSub;
                            TempData["SuccessMessage"] = "<strong>Success</strong><br> You have been successfully subscribed to " + plan.Name;
                            var msg = "<strong>Success</strong><br> You have been successfully subscribed to " + plan.Name;
                            await Misc.MiscOperations.SendMail(User.Identity.Name, "Spoty Meals Subscription", msg);
                        }

                        else
                            TempData["ErrorMessage"] = "Error...an error occurred while trying to enable your subscription, please retry.";
                    }// end of sub created 0on paystack
                }// end of verify payment for subscription plan
            //}
            //catch (Exception ex)
            //{
            //    TempData["ErrorMessage"] = "Error...an error occurred while trying to activate your subscription, Please retry.";
            //}
            //return View("AddSubscription", subViewModel);
            return RedirectToAction("AddSubscription");
        }

        public ActionResult OrderMeal(int? id, string returnUrl)
        {
            ViewBag.Orders = "active";
            //check day of the week
            if ((DateTime.UtcNow).AddHours(1).DayOfWeek == DayOfWeek.Saturday || (DateTime.UtcNow).AddHours(1).DayOfWeek == DayOfWeek.Sunday)
            {
                TempData["SubscriptionError"] = "No order can be placed during the weekend. Please return on Monday";
                //if (Misc.MiscOperations.IsLocalUrl(returnUrl))
                //return Redirect(returnUrl); 
                return RedirectToAction("AvailableMenu", "Home");
            }

            if (id == null)
            {
                TempData["MealError"] = "Please pick a meal";
                return RedirectToAction("AvailableMenu", "Home");
            }
            var meal = db.Meals.Find(id);
            if (meal == null) return HttpNotFound();

            // get user
            int userId = getSignedInUserID();
            var user = db.Users.Find(userId);
            //check subscription
            if (user.SubscriptionPlanID == null)
            {
                TempData["SubscriptionError"] = "no subscription for customer, please add a subscription";
                return RedirectToAction("AvailableMenu", "Home");
            }
            //check when user subscribed
            var userSub = db.UserSubscriptions.SingleOrDefault(x => x.UserID == user.UserID && x.StartDate != null && x.EndDate == null && x.Enabled);
            if (userSub == null)
            {
                TempData["SubscriptionError"] = "no running subscription for user, please add a subscription";
                return RedirectToAction("AvailableMenu", "Home");
            }
            else if (((DateTime.Now - userSub.StartDate.Value).Days) > 7)
            {
                TempData["SubscriptionError"] = "subscription has expired, please renew your subscription now";
                return RedirectToAction("AvailableMenu", "Home");
            }
            //check for no of meals ordered today
            var sub = db.SubscriptionPlans.Find(user.SubscriptionPlanID);
            var date = (DateTime.UtcNow).AddHours(1);
            int noOfMealsOrdered = db.Orders.Where(x => x.UserID == user.UserID && x.Date.Day == date.Day).Count();
            if (noOfMealsOrdered == sub.NoOfMeals)
            {
                TempData["SubscriptionError"] = "Maximum number of Orders reached, change subscription to increase the number of orders per day";
                return RedirectToAction("AvailableMenu", "Home");
            }
            //confirm meal
            return RedirectToAction("ConfirmOrder", new { mealID = meal.ID, userID = user.UserID, venID = meal.Vendor.VendorID });
        }

        public ActionResult ConfirmOrder(int? mealID, int? userID, int? venID)
        {
            ViewBag.Orders = "active";
            if (mealID == null || userID == null || venID == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var customer = db.Users.Find(userID);
            if (customer == null) return HttpNotFound();

            OrderViewModel order = new OrderViewModel()
            {
                CustomerID = userID.Value,
                MealID = mealID.Value,
                VendorID = venID.Value,
                Address = customer.Address,
                Phone = customer.Phone
            };
            var meal = db.Meals.Find(mealID);
            ViewBag.MealName = meal.Name;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmOrder(OrderViewModel order)
        {
            ViewBag.Orders = "active";
            if (ModelState.IsValid)
            {
                Order newOrder = new Order()
                {
                    Address = order.Address,
                    AltPhone = order.AltPhone,
                    UserID = order.CustomerID,
                    Date = DateTime.UtcNow,
                    MealID = order.MealID,
                    Phone = order.Phone,
                    VendorID = order.VendorID,
                };
                db.Orders.Add(newOrder);

                var vendor = db.Vendors.Find(order.VendorID);
                vendor.AccountBalance += vendor.MealPrice;
                vendor.OverallBalance += vendor.MealPrice;
                db.Entry(vendor).State = System.Data.Entity.EntityState.Modified;

                _500Dishes.Models.Transaction tr = new _500Dishes.Models.Transaction()
                {
                    Withdrawal = false,
                    Amount = vendor.MealPrice,
                    Date = DateTime.UtcNow.AddHours(1),
                    ExtraText = "customer food order",
                    VendorID = vendor.VendorID
                };
                db.Transactions.Add(tr);

                //send text message here.

                await db.SaveChangesAsync();
                TempData["MealSuccess"] = "your order has been placed, please wait for your meal.";

                return RedirectToAction("Index", "Home");
                //return RedirectToAction("")
            }

            return View(order);
        }

        public ActionResult MyOrders(int? page)
        {
            ViewBag.Orders = "active";
            int pageSize = 12;
            int pageNumber = (page ?? 1);
            int id = getSignedInUserID();
            var orders = db.Orders.Include("Review").Where(x => x.UserID == id).OrderByDescending(x => x.Date).ToList();
            if (orders.Count == 0)
                TempData["OrderMessage"] = "<strong>No orders yet.<strong>";
            //var review = db.Reviews.Include("Order").ToList();
            //var xxx = orders.ElementAt(0).Review;

            ViewBag.Rating = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "5", Value = "5"},
                new SelectListItem() { Text = "4", Value = "4"},
                new SelectListItem() { Text = "3", Value = "3"},
                new SelectListItem() { Text = "2", Value = "2"},
                new SelectListItem() { Text = "1", Value = "1"}
            };

            return View(orders.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddReview(ReviewViewModel reviewModel)
        {
            ViewBag.Orders = "active";
            if (ModelState.IsValid)
            {
                var review = new Review()
                {
                    Rating = reviewModel.Rating,
                    Text = reviewModel.Text,
                    MealID = reviewModel.MealID,
                    ReviewID = reviewModel.OrderID,
                    UserID = reviewModel.UserID,
                    VendorID = reviewModel.VendorID
                };
                db.Reviews.Add(review);
                db.SaveChanges();
                TempData["ReviewMessageSuccess"] = "Success, your review has been added.<br/>Thanks for telling us what you think.";
            }
            else 
                TempData["ReviewMessageError"] = "Error occurred while trying to add your review, please retry";
            return RedirectToAction("MyOrders");
        }

        public ActionResult MyAccount()
        {
            ViewBag.Account = "active";
            int customerID = getSignedInUserID();
            var user = db.Users.Include("CurrentSub").SingleOrDefault(x => x.UserID == customerID);

            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UserViewModel uvm = new UserViewModel
            {
                 Address = user.Address, Email = user.Email, Name = user.Name, Phone = user.Phone, SubName = user.CurrentSub.Name
            };
            return View(uvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MyAccount(UserViewModel u)
        {
            ViewBag.Account = "active";
            if (ModelState.IsValid)
            {
                int userId = getSignedInUserID();
                var user = db.Users.Find(userId);
                user.Name = u.Name;
                user.Phone = u.Phone;
                user.Address = u.Address;

                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                var i = db.SaveChangesAsync();
                if (await i > 0)
                    TempData["MyAccountSuccess"] = "Account details have been saved.";
                else
                    TempData["MyAccountError"] = "Error occurred while trying to save Account details.";
            }
            return View(u);
        }

        public ActionResult EditPassword()
        {
            ViewBag.Password = "active";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditPassword(EditPasswordViewModel pwd)
        {
            ViewBag.Password = "active";
            if (ModelState.IsValid)
            {
                int id = getSignedInUserID();
                var user = await db.Users.FindAsync(id);
                if (user == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    user.Password = MiscOperations.SHA1HashStringForUTF8String(pwd.NewPassword);
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Password changed successfully";
                    return View();
            }
            return View(pwd);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        private int getSignedInUserID()
        {
            return Convert.ToInt32(Request.Cookies.Get("SpottyMeals_UserID").Value);
        }
    }
}