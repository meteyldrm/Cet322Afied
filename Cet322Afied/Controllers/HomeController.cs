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

            var userLoginDetails = getUserFromSession();
            if (userLoginDetails.Count == 0) {
                ViewBag.activeUser = null;
            } else {
                ViewBag.activeUser = userLoginDetails;
                ViewBag.userAuthLevel = getUserAuthLevel(userLoginDetails);
            }
            
            var products = context.TblProduct.Where(i => i.ProductName != null);
            ViewBag.products = products;

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
            
            HttpContext.Session.SetString("userEmail", "");
            HttpContext.Session.SetString("userPasswordHash", "");

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

            ViewBag.activeUser = null;
            
            HttpContext.Session.SetString("userEmail", "");
            HttpContext.Session.SetString("userPasswordHash", "");
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
            
            TblUser login_user = context.TblUser.FirstOrDefault(login_user => user.UserEmail.Equals(userEmail));
            if(!HttpContext.Session.IsAvailable) return RedirectToAction("Index");
            
            HttpContext.Session.SetString("userEmail", "");
            HttpContext.Session.SetString("userPasswordHash", "");

            if (login_user != null) {
                if (Convert.FromBase64String(login_user.UserPasswordHash).SequenceEqual(Convert.FromBase64String(userPasswordHash))) {
                    HttpContext.Session.SetString("userEmail", userEmail);
                    HttpContext.Session.SetString("userPasswordHash", userPasswordHash);
                    HttpContext.Session.SetInt32("userAuthLevel", getUserAuthLevel(userEmail, userPasswordHash));
                }
            } else {
                return RedirectToAction("Login");
            }
            return RedirectToAction("Index");
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
                            }).ToList();

                        ViewBag.ProductName = new List<string>() { };
                        ViewBag.ProductMeasurementUnit = new List<string>() { };
                        ViewBag.ProductQuantity = new List<decimal>() { };
                        ViewBag.ProductPrice = new List<decimal>() { };

                        foreach (var q in products) {
                            ViewBag.ProductName.Add(q.ProductName);
                            ViewBag.ProductMeasurementUnit.Add(q.ProductMeasurementUnit);
                            ViewBag.ProductQuantity.Add(q.ProductQuantity);
                            ViewBag.ProductPrice.Add(q.ProductPrice);
                        }

                        if (products.Count > 0) {
                            ViewBag.hasElements = true;
                            ViewBag.elementCount = products.Count;
                        } else {
                            ViewBag.hasElements = false;
                            ViewBag.elementCount = 0;
                        }
                        
                        return View();

                        
                    }
                }
            }
            
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CartBuyAction(IFormCollection fc) {
            AfiedDB_322Context context = new AfiedDB_322Context();
            
            if (validateLogin(getUserFromSession())) {
                var user = context.TblUser.FirstOrDefault(u => u.UserEmail.Equals(getUserFromSession()[0]));
                if (user != null) {
                    var cart = context.TblOrder.Where(i => i.OrderCustomerId.Equals(user.UserId)).FirstOrDefault(i => !i.OrderDate.HasValue);
                    if(cart != null) {
                        cart.OrderDate = DateTime.Now;
                        context.SaveChanges();
                    }
                }
            }
            
            return View("Cart");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CartAddAction(IFormCollection fc) {
            AfiedDB_322Context context = new AfiedDB_322Context();
            
            if (validateLogin(getUserFromSession())) {
                var user = context.TblUser.FirstOrDefault(u => u.UserEmail.Equals(getUserFromSession()[0]));
                if (user != null) {
                    var product = context.TblProduct.First(i => i.ProductName.Equals(fc["productName"].ToString()));
                    
                    var cart = context.TblOrder.Where(i => i.OrderCustomerId.Equals(user.UserId)).FirstOrDefault(i => !i.OrderDate.HasValue);
                    if(cart == null) {
                        context.TblOrder.Add(new TblOrder() {
                            OrderCustomerId = user.UserId,
                            OrderDate = null
                        });
                        context.SaveChanges();
                    }
                    
                    cart = context.TblOrder.Where(i => i.OrderCustomerId.Equals(user.UserId)).First(i => !i.OrderDate.HasValue);

                    if (context.TblProductOrder.FirstOrDefault(i => i.OrderId.Equals(cart.OrderId)) != null) {
                        if (context.TblProductOrder.FirstOrDefault(i => i.ProductId.Equals(product.ProductId)) != null) {
                            var _p = context.TblProductOrder
                                .Where(i => i.OrderId.Equals(cart.OrderId))
                                .FirstOrDefault(i => i.ProductId.Equals(product.ProductId));

                            if (_p != null && int.Parse(fc["productQuantity"].ToString())>0) {
                                _p.Quantity += int.Parse(fc["productQuantity"].ToString());
                                context.SaveChanges();
                                return RedirectToAction("Index");
                            }
                        }
                    }
                    
                    context.TblProductOrder.Add(new TblProductOrder() {
                        OrderId = cart.OrderId,
                        ProductId = product.ProductId,
                        Quantity = int.Parse(fc["productQuantity"].ToString()),
                        Price = product.ProductPrice
                    });
                    context.SaveChanges();
                }
            }
            
            return RedirectToAction("Index");
        }
        
        [Route("/Admin")]
        [HttpGet]
        public IActionResult Admin() {
            if (validateLogin(getUserFromSession())) {
                if (getUserAuthLevel(getUserFromSession()) <= 0) {
                    return View();
                }
            }

            return View("PageNotFound");
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
        
        [Route("/Management")]
        [HttpGet]
        public IActionResult Management() {
            
            if (validateLogin(getUserFromSession())) {
                if (getUserAuthLevel(getUserFromSession()) <= 1) {
                    return View();
                }
            }

            return View("PageNotFound");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ManagerCourierManageAction(IFormCollection fc) {
            AfiedDB_322Context context = new AfiedDB_322Context();

            if (fc["manageActionValue"] == "UpdateCourierState") {
                if (checkUserExists(fc["userEmail"])) {
                    var user = context.TblUser.FirstOrDefault(u => u.UserEmail.Equals(fc["userEmail"]));
                    if (user != null) {
                        var managerUser = context.TblManagerUser.FirstOrDefault(i => i.ManagerId == user.UserId);
                        if (managerUser != null) {
                            if (fc["userCourierState"] == "false" && managerUser.ManagerAuthorizationLevel == 2) {
                                context.TblManagerUser.Remove(managerUser); //Unset courier = regular customer
                                context.SaveChanges();
                            }
                        } else if (fc["userCourierState"] == "true") {
                            TblManagerUser manager = new TblManagerUser() {
                                ManagerId = user.UserId,
                                ManagerAuthorizationLevel = 2 //Set courier
                            };
                            context.TblManagerUser.Add(manager);
                            context.SaveChanges();
                        }
                    }
                }
            }
            return RedirectToAction("Management");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ManagerCategoryManageAction(IFormCollection fc) {
            AfiedDB_322Context context = new AfiedDB_322Context();

            if (fc["manageActionValue"] == "AddCategory" && context.TblProductCategory.FirstOrDefault(i => i.CategoryName.Equals(fc["productCategory"])) == null) {
                TblProductCategory category = new TblProductCategory {
                    CategoryName = fc["productCategory"].ToString()
                };
                context.TblProductCategory.Add(category);
                context.SaveChanges();
            } else if (fc["manageActionValue"] == "UpdateCategory" && fc["productCategoryReplace"].ToString().Length > 0) {
                var category = context.TblProductCategory.FirstOrDefault(i => i.CategoryName.Equals(fc["productCategory"]));
                if (category != null) {
                    category.CategoryName = fc["productCategoryReplace"].ToString();
                    context.SaveChanges();
                }
            } else if (fc["manageActionValue"] == "DeleteCategory" && context.TblProductCategory.FirstOrDefault(i => i.CategoryName.Equals(fc["productCategory"])) != null) {
                var category = context.TblProductCategory.FirstOrDefault(i => i.CategoryName.Equals(fc["productCategory"]));
                if (category != null) {
                    context.TblProductCategory.Remove(category);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Management");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ManagerProductManageAction(IFormCollection fc) {
            AfiedDB_322Context context = new AfiedDB_322Context();

            if (fc["manageActionValue"] == "AddProduct") {
                var category = context.TblProductCategory.FirstOrDefault(i => i.CategoryName.Equals(fc["productCategory"].ToString()));
                if (category != null) {
                    TblProduct product = new TblProduct() {
                        ProductName = fc["productName"],
                        ProductCategory = category.CategoryId,
                        ProductPrice = int.Parse(fc["productPrice"].ToString()),
                        ProductMeasurementUnit = fc["productMeasurement"].ToString()
                    };
                    context.TblProduct.Add(product);
                    context.SaveChanges();
                } else {
                    TblProductCategory _category = new TblProductCategory {
                        CategoryName = fc["productCategory"].ToString()
                    };
                    context.TblProductCategory.Add(_category);
                    context.SaveChanges();
                    
                    category = context.TblProductCategory.First(i => i.CategoryName.Equals(fc["productCategory"].ToString()));
                    TblProduct product = new TblProduct() {
                        ProductName = fc["productName"],
                        ProductCategory = category.CategoryId,
                        ProductPrice = int.Parse(fc["productPrice"].ToString()),
                        ProductMeasurementUnit = fc["productMeasurement"].ToString()
                    };
                    context.TblProduct.Add(product);
                }
                
                var _product = context.TblProduct.FirstOrDefault(i => i.ProductName.Equals(fc["productName"]));
                if (_product != null) {
                    _product.ProductMeasurementUnit = fc["productMeasurement"].ToString();
                }

                context.SaveChanges();
            }
            
            else if (fc["manageActionValue"] == "UpdateProduct") {
                var category = context.TblProductCategory.FirstOrDefault(i => i.CategoryName.Equals(fc["productCategory"]));
                if (category != null) {
                    category.CategoryName = fc["productCategoryReplace"].ToString();
                    context.SaveChanges();
                } else {
                    TblProductCategory _category = new TblProductCategory {
                        CategoryName = fc["productCategory"].ToString()
                    };
                    context.TblProductCategory.Add(_category);
                    context.SaveChanges();
                }
                category = context.TblProductCategory.First(i => i.CategoryName.Equals(fc["productCategory"].ToString()));
                var product = context.TblProduct.FirstOrDefault(i => i.ProductName.Equals(fc["productName"]));
                if (product != null && int.TryParse(fc["productPrice"].ToString(), out var _)) {
                    product.ProductName = fc["productName"];
                    product.ProductCategory = category.CategoryId;
                    product.ProductPrice = int.Parse(fc["productPrice"]);
                    product.ProductMeasurementUnit = fc["productMeasurementUnit"];
                } else {
                    TblProduct _product = new TblProduct() {
                        ProductName = fc["productName"],
                        ProductCategory = category.CategoryId,
                        ProductPrice = int.Parse(fc["productPrice"].ToString()),
                        ProductMeasurementUnit = fc["productMeasurement"].ToString()
                    };
                    context.TblProduct.Add(_product);
                }

                context.SaveChanges();
            }
            
            else if (fc["manageActionValue"] == "DeleteProduct") {
                var product = context.TblProduct.FirstOrDefault(i => i.ProductName.Equals(fc["productName"]));
                if (product != null) {
                    context.TblProduct.Remove(product);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Management");
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
