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
        
        [Route("/NotFound")]
        public IActionResult PageNotFound()
        {
            return View();
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
            string userAddress = fc["userAddress"];
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

            TblCustomerUser customerUser = new TblCustomerUser() {
                CustomerId = context.TblUser.FirstOrDefault(i => i.UserEmail.Equals(userEmail))!.UserId,
                CustomerAddress = userAddress
            };
            context.TblCustomerUser.Add(customerUser);
            context.SaveChanges();
            return RedirectToAction("Login");
        }
        
        [Route("/Cart")]
        [HttpGet]
        public IActionResult Cart() {
            AfiedDB_322Context context = new AfiedDB_322Context();

            if (validateLogin(getUserFromSession())) {
                var user = context.TblUser.FirstOrDefault(u => u.UserEmail.Equals(getUserFromSession()[0]));
                if (user != null) {
                    var cart = context.TblOrder.Where(i => i.OrderCustomerId.Equals(user.UserId)).FirstOrDefault(i => !i.OrderDate.HasValue);
                    if(cart != null) {
                        var productOrders = context.TblProductOrder.Where(i => i.OrderId.Equals(cart.OrderId));
                        var products = context.TblProduct.Join(productOrders,
                            productOrder => productOrder.ProductId,
                            product => product.ProductId,
                            (product, order) => new {
                                ProductName = product.ProductName,
                                ProductMeasurementUnit = product.ProductMeasurementUnit,
                                ProductQuantity = order.Quantity,
                                ProductPrice = order.Price
                            });
                        ViewBag.products = products;
                    }
                }
            }
            
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cart(IFormCollection fc) {
            return View();
        }
        
        [Route("/Admin")]
        [HttpGet]
        public IActionResult Admin() {
            if (validateLogin(getUserFromSession())) {
                if (getUserAuthLevel(getUserFromSession()) == 0) {
                    return View();
                }
            }

            return RedirectToAction("PageNotFound");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminManageAction(IFormCollection fc) {
            AfiedDB_322Context context = new AfiedDB_322Context();

            if (fc["manageActionValue"] == "UpdateAuth") {
                if (checkUserExists(fc["userEmail"])) {
                    var user = context.TblUser.FirstOrDefault(u => u.UserEmail.Equals(fc["userEmail"]));
                    if (user != null) {
                        var managerUser = context.TblManagerUser.FirstOrDefault(i => i.ManagerId == user.UserId);
                        if (managerUser != null) {
                            if (int.TryParse(fc["userAuthLevel"].ToString(), out var level)) {
                                managerUser.ManagerAuthorizationLevel = level;
                                context.SaveChanges();
                            }
                        } else {
                            if (int.TryParse(fc["userAuthLevel"].ToString(), out var level)) {
                                TblManagerUser manager = new TblManagerUser() {
                                    ManagerId = user.UserId,
                                    ManagerAuthorizationLevel = level
                                };
                                context.TblManagerUser.Add(manager);
                                context.SaveChanges();
                            }
                        }
                    }
                }
            } else if (fc["manageActionValue"] == "DeleteUser") {
                if (checkUserExists(fc["userEmail"])) {
                    var user = context.TblUser.First(u => u.UserEmail.Equals(fc["userEmail"]));
                    var cu = context.TblCustomerUser.FirstOrDefault(i => i.CustomerId == user.UserId);
                    if (cu != null) {
                        context.TblCustomerUser.Remove(cu);
                        context.SaveChanges();
                    }
                    var mu = context.TblManagerUser.FirstOrDefault(i => i.ManagerId == user.UserId);
                    if (mu != null) {
                        context.TblManagerUser.Remove(mu);
                        context.SaveChanges();
                    }
                    context.TblUser.Remove(user);
                    context.SaveChanges();
                }
            }
            
            return RedirectToAction("Admin");
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

        bool checkUserExists(string email) {
            AfiedDB_322Context context = new AfiedDB_322Context();
            
            TblUser user = context.TblUser.FirstOrDefault(user => user.UserEmail.Equals(email));
            if (user != null) {
                return true;
            }

            return false;
        }

        bool validateLogin(List<string> person) {
            return validateLogin(person[0], person[1]);
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
