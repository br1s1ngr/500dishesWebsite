using _500Dishes.Misc;
using _500Dishes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _500Dishes.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel user)
        {
            if (ModelState.IsValid)
            {
                using (DataContext db = new DataContext())
                {
                    HttpCookie cookieUserID;
                    var pwd = MiscOperations.SHA1HashStringForUTF8String(user.Password);
                    User u = db.Users.SingleOrDefault(x => x.Email == user.Email && x.Password == pwd);
                    if (u != null)
                    {
                        if (u.ConfirmMail)
                        {
                            cookieUserID = new HttpCookie("SpottyMeals_UserID", u.UserID.ToString());
                            cookieUserID.Expires = DateTime.Now.AddDays(1);
                            if (user.RememberMe)
                                cookieUserID.Expires = DateTime.Now.AddDays(7);
                            Response.Cookies.Add(cookieUserID);
                            try
                            {
                                FormsAuthentication.RedirectFromLoginPage(u.Email, user.RememberMe);
                            }
                            catch
                            {
                                //Membership.ValidateUser()
                                //if (Membership.ValidateUser(u.Email, user.Password))
                                //    return RedirectToAction("Index", "Home");
                                TempData["EmailMessage"] = "invalid email or password";
                                return View(user);
                            }
                        }
                        TempData["EmailMessage"] = "please confirm account with link sent to your email";
                        return View(user);
                    }
                    // return View("Index");
                    Vendor v = db.Vendors.SingleOrDefault(x => x.Email == user.Email && x.Password == pwd);
                    if (v != null)
                    {
                        if (v.ConfirmMail)
                        {
                            cookieUserID = new HttpCookie("SpottyMeals_UserID", v.VendorID.ToString());
                            cookieUserID.Expires = DateTime.Now.AddDays(1);
                            if (user.RememberMe)
                                cookieUserID.Expires = DateTime.Now.AddDays(7);
                            Response.Cookies.Add(cookieUserID);
                            FormsAuthentication.RedirectFromLoginPage(v.Email, user.RememberMe);
                        }
                        TempData["EmailMessage"] = "please confirm account with link sent to your email";
                        return View(user);
                    }
                    else
                        TempData["EmailMessage"] = "invalid email or password";
                }
                
            }
            return View(user);

        }

        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignUp(RegisterViewModel user)
        {
            if (ModelState.IsValid)
            {
                using (DataContext db = new DataContext())
                {
                    var regUser = db.Users.SingleOrDefault(x => x.Email == user.Email);
                    if (regUser != null)
                    {
                        ModelState.AddModelError("", "email already registered");
                        return View(user);
                    }
                    else
                    {
                        var tempUser = db.Vendors.SingleOrDefault(x => x.Email == user.Email);
                        if (tempUser != null)
                        {
                            ModelState.AddModelError("", "email already registered");
                            return View(user);
                        }
                    }
                    var customer_code = PayStackAPI.CreateCustomer(user.Email, user.FirstName, user.LastName, user.Phone);
                    if (await customer_code == null)
                    {
                        ModelState.AddModelError("", "error occured while creating account. Please retry.");
                        return View(user);
                    }
                    User u = new User
                    {
                        Email = user.Email,
                        Name = user.FirstName + " " + user.LastName,
                        Password = MiscOperations.SHA1HashStringForUTF8String(user.Password),
                        Phone = user.Phone,
                        RoleID = 3,
                        CustomerCode = await customer_code, ConfirmMailCode = Guid.NewGuid().ToString("N")
                        //
                        ,ConfirmMail = true
                    };
                    var msg = "follow this link to register" + Url.Action("ConfirmAccount", "Account", new { confirmationCode = u.ConfirmMailCode, email = u.Email }, Request.Url.Scheme);

                    var result = Misc.MiscOperations.SendMail(u.Email, "spoty meals registration", msg);
                    TempData["EmailMessage"] = "please confirm account with link sent to your email";
                    
                    db.Users.Add(u);
                    db.SaveChanges();

                    if (await result == "error")
                    {
                        TempData["MailError"] = "unable to send confirmation mail";
                        ViewBag.ConfirmationCode = u.ConfirmMailCode;
                    }
                    return RedirectToAction("Login");
                }
            }
            return View(user);
        }

        public ActionResult VendorSignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VendorSignUp(RegisterViewModel user)
        {
            if (ModelState.IsValid)
            {
                using (DataContext db = new DataContext())
                {
                    var regUser = db.Vendors.SingleOrDefault(x => x.Email == user.Email);
                    if (regUser != null)
                    {
                        ModelState.AddModelError("", "email already registered");
                        return View(user);
                    }
                    else
                    {
                        var tempUser = db.Users.SingleOrDefault(x => x.Email == user.Email);
                        if (tempUser != null)
                        {
                            ModelState.AddModelError("", "email already registered");
                            return View(user);
                        }
                    }
                    var customer_code = await PayStackAPI.CreateCustomer(user.Email, user.FirstName, user.LastName, user.Phone);
                    if (String.IsNullOrWhiteSpace(customer_code))
                    {
                        ModelState.AddModelError("", "error occured while creating account. Please retry.");
                        return View(user);
                    }
                    Vendor v = new Vendor
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Password = MiscOperations.SHA1HashStringForUTF8String(user.Password),
                        Phone =  user.Phone,
                        RoleID = 2,
                        CustomerCode = customer_code,
                        ConfirmMailCode = Guid.NewGuid().ToString("N")
                        //
                        ,ConfirmMail = true
                    };
                    db.Vendors.Add(v);
                    db.SaveChanges();

                    var msg = "use this link to confirm your account  " + Url.Action("ConfirmAccount", "Account", new { confirmationCode = v.ConfirmMailCode, email = v.Email }, Request.Url.Scheme);

                    var result = Misc.MiscOperations.SendMail(v.Email, "spoty meals registration", msg);
                    TempData["EmailMessage"] = "please confirm account with link sent to your email";

                    if (await result == "error")
                    {
                        TempData["MailError"] = "unable to send confirmation mail";
                        ViewBag.ConfirmationCode = v.ConfirmMailCode;
                    }
                    return RedirectToAction("Login");
                }
            }
            return View(user);
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(PreForgotPasswordViewModel pfpv)
        {
            if (ModelState.IsValid)
            {
                using (DataContext db = new DataContext())
                {
                    string forgotPasswordCode = Guid.NewGuid().ToString("N");
                    Vendor vendor = new Models.Vendor();
                    var client = db.Users.Single(x => x.Email == pfpv.Email);
                    if (client == null)
                    {
                        vendor = db.Vendors.Single(x => x.Email == pfpv.Email);
                        if (vendor == null)
                        {
                            ModelState.AddModelError("", "invalid details");
                            return View(pfpv);
                        }
                        vendor.ForgotPasswordCode = forgotPasswordCode;
                        db.Entry(vendor).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        client.ForgotPasswordCode = forgotPasswordCode;
                        db.Entry(client).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                    
                    var msg = "follow this link to change password " + Url.Action("ChangePassword", "Account", new { fpc = forgotPasswordCode }, Request.Url.Scheme);

                    await Misc.MiscOperations.SendMail(pfpv.Email, "spoty meals forgot password", msg);
                    return View();
                }
            }
            return View(pfpv);
        }

        public ActionResult ChangePassword(string fpc)
        {
            if (String.IsNullOrWhiteSpace(fpc))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var fpvm = new ForgotPasswordViewModel
            {
                Code = fpc
            };
            return View(fpvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ForgotPasswordViewModel fpvm)
        {
            if (ModelState.IsValid)
            {
                using (DataContext db = new DataContext())
                {
                    Vendor vendor = new Models.Vendor();
                    var client = db.Users.SingleOrDefault(x => x.ForgotPasswordCode == fpvm.Code);
                    if (client == null)
                    {
                        vendor = db.Vendors.Single(x => x.ForgotPasswordCode == fpvm.Code);
                        if (vendor == null)
                        {
                            ModelState.AddModelError("", "invalid details");
                            return View(fpvm);
                        }
                        vendor.Password = Misc.MiscOperations.SHA1HashStringForUTF8String(fpvm.Password);
                        db.Entry(vendor).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        client.Password = Misc.MiscOperations.SHA1HashStringForUTF8String(fpvm.Password);
                        db.Entry(client).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                    //add message here
                    TempData["ChangePasswordMessage"] = "Password changed successfully. Retry login";
                    return RedirectToAction("Login");
                }
            }
            return View(fpvm);
        }

        //[HttpPost]
        public ActionResult ConfirmAccount(string confirmationCode, string email)
        {
            HttpCookie cookieUserID;
            Vendor vendor = new Models.Vendor();
            using (DataContext db = new DataContext())
            {
                var client = db.Users.Single(x => x.ConfirmMailCode == confirmationCode && x.Email == email);
                if (client == null)
                {
                    vendor = db.Vendors.Single(x => x.ConfirmMailCode == confirmationCode && x.Email == email);
                    if (vendor == null)
                    {
                        ModelState.AddModelError("", "invalid details");
                        return null;
                    }
                    cookieUserID = new HttpCookie("SpottyMeals_UserID", vendor.VendorID.ToString());
                    vendor.ConfirmMail = true;
                    db.Entry(vendor).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    client.ConfirmMail = true;
                    cookieUserID = new HttpCookie("SpottyMeals_UserID", client.UserID.ToString());
                    db.Entry(client).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
                
                cookieUserID.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(cookieUserID);
                //add message
                TempData["ConfirmationMessage"] = "your account has been confirmed";
                FormsAuthentication.RedirectFromLoginPage(email, false);
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult SignOut()
        {
            var cookieUserID = new HttpCookie("SpottyMeals_UserID", "");
            cookieUserID.Expires = DateTime.Now.AddDays(-1);
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");            
        }
    }
}