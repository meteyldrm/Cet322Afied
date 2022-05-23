using Cet322Afied.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Cet322Afied.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
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
            
            Console.WriteLine("TEST LOGIN");
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
                    Console.WriteLine("USER LOGGED IN");
                }
            }
			
            return RedirectToAction("Index");
        }
        
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
    }
}
