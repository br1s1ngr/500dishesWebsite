using _500Dishes.Misc;
using _500Dishes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PagedList;
using System.Threading.Tasks;

namespace _500Dishes.Controllers
{
    public class HomeController : Controller
    {
        DataContext db = new DataContext();

        public ActionResult Index()
        {
            //if (Request.IsAuthenticated)
            //{
            //    if (User.IsInRole("A"))
            //        return RedirectToAction("Index", "Admin");
            //    if (User.IsInRole("V"))
            //        return RedirectToAction("Index", "Vendor");
            //}

            //var meals = db.Meals.Include("Reviews").ToList();
            //return View(meals);
            ViewBag.Locations = new SelectList(db.Locations.ToList(), "LocationID", "Name");
            return View();
        }

        public ActionResult OurClients()
        {
            var reviews = db.Reviews.Include("Customer").Include("Meal").Where(x => x.Rating >= 4).ToList();
            OurClientsViewModel viewModel = new OurClientsViewModel();
            Random random = new Random();
            if (reviews.Count >= 5)
                for (int i = 0; i < 5; i++)
                {
                    int x = random.Next(reviews.Count);
                    viewModel.Reviews.Add(reviews.ElementAt(x));
                    reviews.RemoveAt(x);
                }
            else if (reviews.Count >= 4)
                for (int i = 0; i < 4; i++)
                {
                    int x = random.Next(reviews.Count);
                    viewModel.Reviews.Add(reviews.ElementAt(x));
                    reviews.RemoveAt(x);
                }
            else if (reviews.Count >= 1)
                    viewModel.Reviews = reviews;

            var vendors = db.Vendors.OrderByDescending(x => x.Bookings.Count).ToList();
            if (vendors.Count >= 4)
                viewModel.Vendors = vendors.Take(4).ToList();
            else
                viewModel.Vendors = vendors;
            
            return View(viewModel);
        }

        public async Task<ActionResult> AvailableMenu(int? location, int? page, string searchString)
        {
            List<Meal> allMeals = db.Meals.ToList();
            List<Meal> meals = new List<Meal>();
            ViewBag.Locations = new SelectList(db.Locations.ToList(), "LocationID", "Name");

            if (location.HasValue)
            {
                var locationName = await db.Locations.FindAsync(location);
                Session["LocationName"] = locationName.Name;
                Session["LocationID"] = location.Value;
            }
            else if (Session["LocationID"] != null)
                location = (int)Session["LocationID"];
            
            if (location != null)
            {
                allMeals = (from vendorLoc in db.VendorsLocations
                             where vendorLoc.LocationID == location
                             join vendor in db.Vendors on vendorLoc.VendorID equals vendor.VendorID
                             from meal in vendor.Meals
                             where meal.Verified == true
                             select meal).ToList();
            }
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                page = 1;
                foreach (var item in allMeals)
                    if (item.Name.ContainsForSearch(searchString))
                        meals.Add(item);
                ViewBag.CurrentFilter = searchString;
            }
            else
                meals = allMeals;

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            ViewBag.Locations = new SelectList(db.Locations.ToList(), "LocationID", "Name");

            if (meals.Count == 0)
                TempData["MealError"] = "no meal for selected location";
            return View(meals.ToPagedList(pageNumber, pageSize));
        }

        //[HttpPost]
        //public ActionResult AvailableMenu(int? location)
        //{
        //    ViewBag.Locations = new SelectList(db.Locations.ToList(), "LocationID", "Name");
        //    if (location == null)
        //        return RedirectToAction("Index");

        //    Session["LocationID"] = location.Value;
        //    return RedirectToAction("Index");
        //}

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}