using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Aplikacija.Models;

namespace Aplikacija.Controllers
{
    public class AuthController : Controller
    {
        private readonly AlgebraDatabaseEntities context = new AlgebraDatabaseEntities();

        [HttpGet]
        [NoCookie]
        public ActionResult Login()
        {
            return View();
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult Register()
        {
            return View();
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult ShowEmployees()
        {
            List<Employees> employeeListDb = context.Employees.ToList();

            return View(employeeListDb);
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult DeleteEmployee(int id)
        {
            Employees employeeDb = context.Employees
                .Where(c => c.Id == id)
                .Single();

            return View(employeeDb);
        }

        [HttpPost]
        [EmployeeAuth]
        public ActionResult DeleteEmployee(Employees employee)
        {
            HttpCookie cookie = Request.Cookies["authCookie"];

            if (employee.Username == "Admin")
            {
                SetFlash("danger", "You can't delete administrator account!");

                return RedirectToAction("ShowEmployees");
            }
            else if (cookie["username"] != "Admin")
            {
                SetFlash("danger", "Permission denied! Only administrator can delete accounts!");

                return RedirectToAction("ShowEmployees");
            }

            Employees employeeDb = context.Employees.Where(c => c.Id == employee.Id).Single();
            context.Employees.Remove(employeeDb);
            context.SaveChanges();

            SetFlash("success", "You have removed employee from database!");

            return RedirectToAction("ShowEmployees");
        }

        [HttpPost]
        public ActionResult Login(Employees user)
        {
            Employees employee = context.Employees
                .Where(c => c.Username == user.Username)
                .SingleOrDefault();

            if (employee == null)
            {
                ModelState.AddModelError(nameof(Employees.Username), "Username does not exist");
            }
            else
            {
                ModelState["Id"].Errors.Clear();
                string hashedPassword = HashPassword(user.Password, employee.Salt);

                if (hashedPassword != employee.Password)
                {
                    ModelState.AddModelError(nameof(Employees.Password), "Wrong password!");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Login");
            }

            string newToken = GetRandom(32);

            ReplaceEmployeeToken(employee, newToken);
            DeleteCookie();
            CreateCookie(employee);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Register(Employees user, string confirmPassword)
        {
            Employees userDb = context.Employees.Where(c => c.Username == user.Username).SingleOrDefault();

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                ModelState.AddModelError(nameof(Employees.Username), "This field can't be empty");
            }
            else if (userDb != null)
            {
                ModelState.AddModelError(nameof(Employees.Username), "That username already exists");
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.AddModelError(nameof(Employees.Password), "This field can't be empty");
            }

            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError(nameof(Employees.Password), "Passwords do not match");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            string generatedSalt = GetRandom(8);
            string hashedPassword = HashPassword(user.Password, generatedSalt);

            Employees employee = new Employees();
            employee.Id = 3;
            employee.Username = user.Username;
            employee.Password = hashedPassword;
            employee.Salt = generatedSalt;
            employee.Token = GetRandom(32);

            context.Employees.Add(employee);
            context.SaveChanges();

            SetFlash("success", "You have successfully registered new employee!");

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            DeleteCookie();

            return RedirectToAction("Index", "Home");
        }

        private string GetRandom(int stringSize)
        {
            StringBuilder strB = new StringBuilder("");
            Random r = new Random();
            const string alphanumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

            while ((stringSize--) > 0) strB.Append(alphanumeric[(int)(r.NextDouble() * alphanumeric.Length)]);

            return strB.ToString();
        }

        private string HashPassword(string password, string salt)
        {
            string mergedPass = string.Concat(password, salt);

            return EncryptUsingMD5(mergedPass);
        }

        private string EncryptUsingMD5(string inputStr)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                StringBuilder sBuilder = new StringBuilder();
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(inputStr));

                for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));

                return sBuilder.ToString();
            }
        }

        private void CreateCookie(Employees employee)
        {
            HttpCookie authCookie = new HttpCookie("authCookie");

            authCookie.Values.Add("username", employee.Username.ToString());
            authCookie.Values.Add("token", employee.Token.ToString());

            authCookie.Expires = DateTime.Now.AddHours(12);

            Response.Cookies.Add(authCookie);
        }

        private void DeleteCookie()
        {
            HttpCookie cookie = Response.Cookies["authCookie"];

            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
            }
        }

        private void ReplaceEmployeeToken(Employees employee, string token)
        {
            employee.Token = token;
            context.SaveChanges();
        }

        public void SetFlash(string type, string text)
        {
            TempData["FlashMessage.Type"] = type;
            TempData["FlashMessage.Text"] = text;
        }
    }
}