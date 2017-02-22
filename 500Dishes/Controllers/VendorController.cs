using _500Dishes.Misc;
using _500Dishes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.IO;
using System.Threading.Tasks;

namespace _500Dishes.Controllers
{
    [Authorize(Roles = "V")]
    public class VendorController : Controller
    {
        private DataContext db = new DataContext();
        private void setVendorDetails()
        {
            int userID = getSignedInUserID();
            var vendor = db.Vendors.Find(userID);
            ViewBag.VendorName = vendor.FirstName.ToLower() + " " + vendor.LastName.ToUpper();
            ViewBag.VendorBrandName = vendor.BrandName;
            ViewBag.VendorReviews = vendor.Reviews.Count;
            ViewBag.VendorImage = vendor.ImagePath;
        }
        // GET: Vendor
        public ActionResult Index()
        {
            setVendorDetails();
            return View();
        }

        public ActionResult Locations(int? page)
        {
            setVendorDetails();
            ViewBag.Locations = "active";
            int pageSize = 12;
            int pageNumber = (page ?? 1);
            string email = User.Identity.Name;
            Vendor vendor = db.Vendors.Include("DeliveryLocations").SingleOrDefault(x => x.Email == email);
            if (vendor != null)
            {
                return View(vendor.DeliveryLocations.ToPagedList(pageNumber, pageSize));
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        public ActionResult AddLocation()
        {
            setVendorDetails();
            ViewBag.Locations = "active";
            ViewBag.Location = new SelectList(db.Locations.ToList(), "LocationID", "Name");
            if (Request.IsAjaxRequest())
                return PartialView();
            else
                return View();
        }

        [HttpPost]
        public ActionResult AddLocation(int? Location)
        {
            setVendorDetails();
            ViewBag.Locations = "active";
            if (Location != null)
            {
                int vendorID = getSignedInUserID();
                VendorLocation vloc = new VendorLocation
                {
                    LocationID = Location.Value,
                    VendorID = vendorID
                };
                db.VendorsLocations.Add(vloc);
                db.SaveChanges();
                return RedirectToAction("Locations");
            }
            return RedirectToAction("Locations");
        }

        public ActionResult Meals(int? page)
        {
            setVendorDetails();
            ViewBag.Meals = "active";
            int pageSize = 12;
            int pageNumber = (page ?? 1);
            int id = getSignedInUserID();
            Vendor v = db.Vendors.Include("Meals").SingleOrDefault(x => x.VendorID == id);
            if (v != null)
                return View(v.Meals.ToPagedList(pageNumber, pageSize));

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult BankDetails()
        {
            setVendorDetails();
            ViewBag.BankDetails = "active";
            int id = getSignedInUserID();
            ViewBag.BankName = new SelectList(db.Banks, "ID", "Name");
            Vendor v = db.Vendors.Find(id);
            if (v != null)
                return View(new WithdrawalSettingsViewModel
                {
                    VendorID = v.VendorID,
                    AccountName = v.AccountName,
                    AccountNumber = v.AccountNumber,
                    BankName = v.BankID
                });
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        public ActionResult BankDetails(WithdrawalSettingsViewModel settings)
        {
            setVendorDetails();
            ViewBag.BankDetails = "active";
            if (ModelState.IsValid)
                if (settings.VendorID == getSignedInUserID())
                {
                    Vendor v = db.Vendors.Find(settings.VendorID);
                    v.AccountName = settings.AccountName;
                    v.AccountNumber = settings.AccountNumber;
                    v.BankID = settings.BankName;
                    db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            ViewBag.BankName = new SelectList(db.Banks, "ID", "Name");
            return View(settings);
        }

        public ActionResult Transactions()
        {
            setVendorDetails();
            ViewBag.Transactions = "active";
            int id = getSignedInUserID();
            Vendor v = db.Vendors.Include("Transactions").SingleOrDefault(x => x.VendorID == id);
            TransactionsViewModel tr = new TransactionsViewModel()
            {
                VendorID = v.VendorID,
                Balance = v.AccountBalance,
                Withdrawals = (v.Transactions != null ? v.Transactions.OrderByDescending(x => x.Date).ToList() : new List<_500Dishes.Models.Transaction>())
            };
            return View(tr);
        }

        public ActionResult WithdrawEarnings()
        {
            setVendorDetails();
            ViewBag.Withdraw = "active";
            int id = getSignedInUserID();
            Vendor v = db.Vendors.Include("Bank").SingleOrDefault(x => x.VendorID == id);
            if (v != null)
                return View(new WithdrawEarningsViewModel
                {
                    VendorID = v.VendorID,
                    AccountName = v.AccountName,
                    AccountNumber = v.AccountNumber,
                    BankName = (v.Bank != null ? v.Bank.Name : "")
                });
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        public ActionResult WithdrawEarnings(WithdrawEarningsViewModel withdrawal)
        {
            setVendorDetails();
            ViewBag.Withdraw = "active";
            if (ModelState.IsValid)
            {
                Vendor v = db.Vendors.Find(withdrawal.VendorID);
                if (v.AccountBalance >= withdrawal.Amount)
                {
                    _500Dishes.Models.Transaction w = new _500Dishes.Models.Transaction
                    {
                        Amount = withdrawal.Amount,
                        VendorID = withdrawal.VendorID,
                        ExtraText = (withdrawal.ExtraText == "") ? "--" : withdrawal.ExtraText,
                        Date = DateTime.UtcNow.AddHours(1),
                        Withdrawal = true,
                    };
                    db.Transactions.Add(w);
                    v.AccountBalance -= withdrawal.Amount;
                    db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return View();
                }
                ModelState.AddModelError("", "invalid transaction. \r\namount to be withdrawn exceeds vendor balance");
            }
            return View(withdrawal);
        }

        public ActionResult AddMeal()
        {
            setVendorDetails();
            ViewBag.Meals = "active";
            return View();
        }

        [HttpPost]
        public ActionResult AddMeal(MealViewModel mvm, HttpPostedFileBase image)
        {
            setVendorDetails();
            ViewBag.Meals = "active";
            if (ModelState.IsValid)
            {
                Meal meal = new Meal()
                {
                    Name = mvm.Name,
                    Description = mvm.Description,
                    VendorID = getSignedInUserID(),
                    DateAdded = DateTime.UtcNow.AddHours(1)
                };
                if (image != null && image.ContentLength > 0 && image.ContentType.Contains("image"))
                {
                    var filePath = Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(image.FileName);
                    string path = Path.Combine(HttpContext.Server.MapPath("~/Images/Meals"), filePath);
                    image.SaveAs(path);
                    meal.ImagePath = filePath;
                }
                db.Meals.Add(meal);
                db.SaveChanges();
                return RedirectToAction("Meals");
            }
            return View(mvm);
        }

        public ActionResult DeleteMeal(int? id)
        {
            setVendorDetails();
            if (id != null)
            {
                Meal meal = db.Meals.Find(id.Value);
                if (meal != null && meal.VendorID == getSignedInUserID())
                {
                    db.Entry(meal).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Meals");
        }

        public ActionResult EditMeal(int? id)
        {
            setVendorDetails();
            ViewBag.Meals = "active";
            if (id != null)
            {
                Meal meal = db.Meals.Find(id.Value);
                if (meal != null)
                {
                    EditMealViewModel emvm = new EditMealViewModel
                    {
                        ID = meal.ID,
                        Name = meal.Name,
                        Description = meal.Description,
                        ImagePath = meal.ImagePath
                    };
                    if (Request.IsAjaxRequest())
                        return PartialView(emvm);
                    return View(emvm);
                }
            }
            return RedirectToAction("Meals");
        }

        [HttpPost]
        public ActionResult EditMeal(EditMealViewModel emvm, HttpPostedFileBase image)
        {
            setVendorDetails();
            ViewBag.Meals = "active";
            if (ModelState.IsValid)
            {
                Meal meal = db.Meals.Find(emvm.ID);
                if (meal != null)
                {
                    meal.Name = emvm.Name;
                    meal.Description = emvm.Description;
                }
                if (image != null && image.ContentLength > 0 && image.ContentType.Contains("image"))
                {
                    var filePath = Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(image.FileName);
                    string path = Path.Combine(HttpContext.Server.MapPath("~/Images/Meals"), filePath);
                    image.SaveAs(path);
                    meal.ImagePath = filePath;
                }
                db.Entry(meal).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Meals");
        }

        public ActionResult EditProfile()
        {
            setVendorDetails();
            ViewBag.Profile = "active";
            int vendorId = getSignedInUserID();
            Vendor vendor = db.Vendors.Find(vendorId);
            VendorViewModel vvm = new VendorViewModel
            {
                ID = vendor.VendorID,
                FirstName = vendor.FirstName,
                LastName = vendor.LastName,
                BrandName = vendor.BrandName,
                Phone = vendor.Phone,
                Email = vendor.Email,
                About = vendor.About,
                FullAddress = vendor.FullAddress,
                ImagePath = vendor.ImagePath
            };
            return View(vvm);
        }

        //confirm if the excluded propertiers change or go to null
        [HttpPost]
        public ActionResult EditProfile(VendorViewModel vendor, HttpPostedFileBase image)
        {
            setVendorDetails();
            ViewBag.Profile = "active";
            if (ModelState.IsValid)
            {
                Vendor v = db.Vendors.Find(vendor.ID);
                //if (image != null && image.ContentLength > 0 && image.ContentType.Contains("image"))
                //{
                //    var filePath = Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(image.FileName);
                //    string path = Path.Combine(HttpContext.Server.MapPath("~/Images/Vendors"), filePath);
                //    if (!Directory.Exists(path))
                //        Directory.CreateDirectory(path);
                //    image.SaveAs(path);
                //    v.ImagePath = filePath;
                //    vendor.ImagePath = filePath;
                //}
                v.FirstName = vendor.FirstName;
                v.LastName = vendor.LastName;
                v.BrandName = vendor.BrandName;
                v.Phone = vendor.Phone;
                v.Email = vendor.Email;
                v.About = vendor.About;
                v.FullAddress = vendor.FullAddress;

                db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return View(vendor);
        }

        [HttpPost]
        public ActionResult DeleteLocation(int? id)
        {
            setVendorDetails();
            ViewBag.Locations = "active";
            if (id != null)
            {
                int vendorID = getSignedInUserID();
                VendorLocation vloc = db.VendorsLocations.Find(id);
                db.Entry(vloc).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                return RedirectToAction("Locations");
            }
            return RedirectToAction("Locations");
        }

        public ActionResult MyOrders(int? page)
        {
            setVendorDetails();
            ViewBag.Orders = "active";
            int pageSize = 12;
            int pageNumber = (page ?? 1);
            int id = getSignedInUserID();
            var orders = db.Orders.Where(x => x.VendorID == id).OrderByDescending(x => x.Date);
            if (orders == null)
                TempData["OrderMessage"] = "<strong>No orders yet.<strong>";

            return View(orders.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult EditPassword()
        {
            setVendorDetails();
            ViewBag.Password = "active";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditPassword(EditPasswordViewModel pwd)
        {
            setVendorDetails();
            ViewBag.Password = "active";
            if (ModelState.IsValid)
            {
                int id = getSignedInUserID();
                var vendor = await db.Vendors.FindAsync(id);
                if (vendor == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                               
                    vendor.Password = MiscOperations.SHA1HashStringForUTF8String(pwd.NewPassword);
                    db.Entry(vendor).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Password changed successfully";
                    return View();
            }
            return View(pwd);
        }

        [HttpPost]
        public async Task<ActionResult> UploadImage()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpPostedFileBase image = Request.Files[0];
                    if (image != null && image.ContentLength > 0 && image.ContentType.Contains("image"))
                    {
                        int vendorId = getSignedInUserID();
                        Vendor v = db.Vendors.Find(vendorId);
                        var filePath = Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(image.FileName);
                        string path = Path.Combine(HttpContext.Server.MapPath("~/Images/Vendors"), filePath);
                        if (!Directory.Exists(HttpContext.Server.MapPath("~/Images/Vendors")))
                            Directory.CreateDirectory(HttpContext.Server.MapPath("~/Images/Vendors"));
                        image.SaveAs(path);
                        v.ImagePath = filePath;
                        db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();

                        return Json("File Uploaded Successfully!");
                    }
                }
                catch (Exception e)
                {
                    return Json("Error ocurred. Error details; " + e.Message);
                }
            }
            return Json("Error, no files selected");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        private int getSignedInUserID()
        {
            return Convert.ToInt32(Request.Cookies.Get("SpottyMeals_UserID").Value);
        }
    }
}