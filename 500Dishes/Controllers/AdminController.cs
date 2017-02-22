using _500Dishes.Misc;
using _500Dishes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace _500Dishes.Controllers
{
    [Authorize (Roles = "A")]
    public class AdminController : Controller
    {
        DataContext db = new DataContext();

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Orders(int? page)
        {
            ViewBag.Orders = "active";
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            var orders = db.Orders.OrderByDescending(x => x.Date).ToPagedList(pageNumber, pageSize);
            return View(orders);
        }

        public ActionResult Order(int? id)
        {
            ViewBag.Orders = "active";
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var order = db.Orders.Include("Review").SingleOrDefault(x => x.OrderID == id);
            return View(order);
        }

        public ActionResult Subscriptions()
        {
            ViewBag.Subscriptions = "active";
            var subPlans = db.SubscriptionPlans.Include("Subscribers").ToList();
            return View(subPlans);
        }

        public ActionResult CreateSubscription()
        {
            ViewBag.Subscriptions = "active";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateSubscription([Bind (Exclude = "ID, PlanCode")]SubscriptionPlan sub)
        {
            ViewBag.Subscriptions = "active";
            if (ModelState.IsValid)
            {
                var oldSub = db.SubscriptionPlans.SingleOrDefault(x => x.Name == sub.Name);
                if (oldSub != null)
                    ModelState.AddModelError("", "Subscription plan with name already exists");
                else if (oldSub == null)
                {
                    string plan_code = await PayStackAPI.CreatePlan(sub.Name, sub.Description, Convert.ToInt32(sub.Amount));
                    if (plan_code != null)
                    {
                        sub.PlanCode = plan_code;
                        db.SubscriptionPlans.Add(sub);
                        db.SaveChanges();
                        TempData["SuccessMsg"] = "subscription added successfully";
                        return RedirectToAction("Subscriptions");
                    }
                    ModelState.AddModelError("", "An error occurred while creating the subscription plan, please retry");
                }
            }
            return View(sub);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteSubscription(int? id)
        {
            ViewBag.Subscriptions = "active";
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var subscription = db.SubscriptionPlans.Find(id);
            db.Entry(subscription).State = System.Data.Entity.EntityState.Deleted;
            await db.SaveChangesAsync();
            return RedirectToAction("Subscriptions");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSubscription(SubscriptionPlan plan)
        {
            ViewBag.Subscriptions = "active";
            if (ModelState.IsValid)
            {
                db.Entry(plan).State = System.Data.Entity.EntityState.Modified;
                Task<int> i = db.SaveChangesAsync();
                var status = PayStackAPI.UpdatePlan(plan.PlanCode, plan.Name, plan.Description, Convert.ToInt32(plan.Amount), "weekly", true, true, "NGN");
                await i;
                await status;
                return RedirectToAction("Subscriptions");
            }
            return View(plan);
        }

        public ActionResult AllMeals(int? page)
        {
            ViewBag.Meals = "active";
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            var meals = db.Meals.Include("Orders").Where(x => x.Vendor.Verified).OrderByDescending(x => x.DateAdded).ToList();
            return View(meals.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Meal(int? id)
        {
            ViewBag.Meals = "active";
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var meal = db.Meals.Find(id);
            return View(meal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyMeal(int? id)
        {
            ViewBag.Meals = "active";
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var meal = db.Meals.Find(id);
            meal.Verified = true;
            db.Entry(meal).State = System.Data.Entity.EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("AllMeals");
        }


        public ActionResult AllVendors(int? page)
        {
            ViewBag.Vendors = "active";
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            var vendors = db.Vendors.ToList();
            if (vendors.Count == 0)
                TempData["VendorMessage"] = "No vendors yet";
            return View(vendors.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Vendor(int? id)
        {
            ViewBag.Vendors = "active";
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var vendor = db.Vendors.Include("DeliveryLocations").Include("Meals").Include("Bookings").Include("Transactions").Include("Reviews").SingleOrDefault( x => x.VendorID == id);
            return View(vendor);
        }

        public ActionResult AllCustomers(int? page)
        {
            ViewBag.Customers = "active";
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            var customers = db.Users.ToList();
            if (customers.Count == 0)
                TempData["CustomerMessage"] = "No customers yet";
            return View(customers.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Customer(int? id)
        {
            ViewBag.Customers = "active";
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var client = db.Users.Include("Orders").Include("Reviews").SingleOrDefault(x => x.UserID == id);
            return View(client);
        }
               
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyVendor(int? id, decimal? price)
        {
            ViewBag.Vendors = "active";
            if (id == null || price == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (price < 10)
                TempData["ErrorMessage"] = "Error, price cannot be less than 100";
            else
            {
                var vendor = db.Vendors.Find(id);
                vendor.MealPrice = price.Value;
                vendor.Verified = true;
                db.Entry(vendor).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("AllVendors");
        }

        public ActionResult AllWithdrawals(int? page)
        {
            ViewBag.Withdrawals = "active";
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            var withdrawals = db.Transactions.Where(x => x.Withdrawal == true).OrderByDescending(x => x.Date).ToList();
            if (withdrawals.Count == 0)
                TempData["WithdrawalMessage"] = "No withdrawals yet";
            return View(withdrawals.ToPagedList(pageNumber, pageSize));
        }

        
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}