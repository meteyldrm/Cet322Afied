using Cet322Afied.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Cet322Afied.Controllers
{
    public class HomeController : Controller
    {
        #region Requests
        
        public IActionResult Index() {
            AfiedDB_322Context context = new AfiedDB_322Context();

            // var products = context.TblProduct.Where(i => i.ProductName != null);
            // ViewBag.products = new List<TblProduct>();
            //
            // foreach (var product in products) {
            //     ViewBag.products += product;
            // }

            var userLoginDetails = getUserFromSession();
            if (userLoginDetails.Count == 0) {
                ViewBag.activeUser = null;
            } else {
                ViewBag.activeUser = userLoginDetails;
                ViewBag.userAuthLevel = getUserAuthLevel(userLoginDetails);
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        [Route("/Login")]
        [HttpGet]
        public IActionResult Login() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginAction(IFormCollection fc) {
            string userEmail = fc["userEmail"];
            string userPassword = fc["userPassword"];
            AfiedDB_322Context context = new AfiedDB_322Context();
            
            Console.WriteLine(userEmail);
            Console.WriteLine(userPassword);

            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] _byteHash = Encoding.UTF8.GetBytes(userPassword);
            string userPasswordHash = Convert.ToBase64String(hashAlgorithm.ComputeHash(_byteHash));
            Console.WriteLine(userPasswordHash);
            
            TblUser user = context.TblUser.FirstOrDefault(user => user.UserEmail.Equals(userEmail));
            if(!HttpContext.Session.IsAvailable) return RedirectToAction("Index");
            
            HttpContext.Session.SetString("userEmail", "?");
            HttpContext.Session.SetString("userPasswordHash", "?");

            if (user != null) {
                if (Convert.FromBase64String(user.UserPasswordHash).SequenceEqual(Convert.FromBase64String(userPasswordHash))) {
                    HttpContext.Session.SetString("userEmail", userEmail);
                    HttpContext.Session.SetString("userPasswordHash", userPasswordHash);
                    HttpContext.Session.SetInt32("userAuthLevel", getUserAuthLevel(userEmail, userPasswordHash));
                }
            }
			
            return RedirectToAction("Index");
        }
        
        [Route("/Logout")]
        [HttpGet]
        public IActionResult Logout() {
            Console.WriteLine("LOGOUT");

            TempData["activeUser"] = null;
            TempData["userAuthLevel"] = 4;
            
            HttpContext.Session.SetString("userEmail", "?");
            HttpContext.Session.SetString("userPasswordHash", "?");
            HttpContext.Session.SetInt32("userAuthLevel", 4);
            
            return RedirectToAction("Index");
        }
        
        [Route("/Register")]
        [HttpGet]
        public IActionResult Register() {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterAction(IFormCollection fc) {
            AfiedDB_322Context context = new AfiedDB_322Context();
            
            string userEmail = fc["userEmail"];
            string userPassword = fc["userPassword"];
            string userPhone = fc["userPhone"];
            string userName = fc["userName"];
            
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] _byteHash = Encoding.UTF8.GetBytes(userPassword);
            string userPasswordHash = Convert.ToBase64String(hashAlgorithm.ComputeHash(_byteHash));

            TblUser user = new TblUser() {
                UserName = userName,
                UserEmail = userEmail,
                UserPhone = userPhone,
                UserPasswordHash = userPasswordHash
            };

            context.TblUser.Add(user);
            context.SaveChanges(); 
            return RedirectToAction("Login");
        }
        
        [Route("/Cart")]
        [HttpGet]
        public IActionResult Cart() {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cart(IFormCollection fc) {
            AfiedDB_322Context context = new AfiedDB_322Context();
            
            string userEmail = fc["userEmail"];
            string userPassword = fc["userPassword"];
            string userPhone = fc["userPhone"];
            string userName = fc["userName"];
            
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] _byteHash = Encoding.UTF8.GetBytes(userPassword);
            string userPasswordHash = Convert.ToBase64String(hashAlgorithm.ComputeHash(_byteHash));

            TblUser user = new TblUser() {
                UserName = userName,
                UserEmail = userEmail,
                UserPhone = userPhone,
                UserPasswordHash = userPasswordHash
            };

            context.TblUser.Add(user);
            context.SaveChanges(); 
            return View();
        }
        
        #endregion

        #region DBHelper

        bool validateLogin(string userEmail, string pwHash) {
            AfiedDB_322Context context = new AfiedDB_322Context();
            
            TblUser user = context.TblUser.FirstOrDefault(user => user.UserEmail.Equals(userEmail));
            if (user != null) {
                if (Convert.FromBase64String(user.UserPasswordHash).SequenceEqual(Convert.FromBase64String(pwHash))) {
                    //Login success
                    return true;
                }
            }

            return false;
        }

        int getUserAuthLevel(string userEmail, string pwHash) {
            if(validateLogin(userEmail, pwHash))
            {
                AfiedDB_322Context context = new AfiedDB_322Context();
                TblUser user = context.TblUser.FirstOrDefault(user => user.UserEmail.Equals(userEmail));
                TblManagerUser managerUser = context.TblManagerUser.FirstOrDefault(manager => manager.ManagerId == user.UserId);

                if (managerUser == null) {
                    return 3; //0 admin, 1 manager, 2 courier, 3 customer
                }
                return managerUser.ManagerAuthorizationLevel;
            }
            return 3;
        }

        int getUserAuthLevel(List<string> loginDetails) {
            return getUserAuthLevel(loginDetails[0], loginDetails[1]);
        }

        List<string> getUserFromSession() {
            return new List<string>() { HttpContext.Session.GetString("userEmail"), HttpContext.Session.GetString("userPasswordHash") };
        }

        #endregion
    }
}
