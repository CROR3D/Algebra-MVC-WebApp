using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Aplikacija.Models;

namespace Aplikacija.Controllers
{
    public class HomeController : Controller
    {
        private readonly AlgebraDatabaseEntities context = new AlgebraDatabaseEntities();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Courses()
        {
            List<Courses> courses = GetAvailableCourses();
            ViewBag.Generation = GetGeneration();

            return View(courses);
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult ManageCourses()
        {
            List<Courses> allCourses = context.Courses.ToList();

            return View(allCourses);
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult CourseCreate()
        {
            return View();
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult Preorders()
        {
            ViewBag.Generation = GetGeneration();

            List<Preorders> allPreorders = context.Preorders.OrderBy(c => c.CourseId).ToList();
            List<int?> courseIDs = context.Preorders.Select(c => c.CourseId).Distinct().ToList();
            List<Courses> preorderCourses = new List<Courses>();

            foreach (int id in courseIDs)
            {
                Courses course = GetCourse(id);
                preorderCourses.Add(course);
            }

            ViewBag.PreorderCourses = preorderCourses;

            return View(allPreorders);
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult ApprovePreorder(int id)
        {
            Preorders preorder = GetPreorder(id);

            return View(preorder);
        }

        [HttpPost]
        [EmployeeAuth]
        public ActionResult ApprovePreorder(Preorders preorder)
        {
            Preorders preorderDb = context.Preorders.Where(c => c.Id == preorder.Id).Single();
            Courses courseDb = context.Courses.Where(c => c.Id == preorderDb.CourseId).Single();

            preorderDb.IsApproved = !preorderDb.IsApproved;

            if ((bool)preorderDb.IsApproved)
            {
                courseDb.CurrentAttendants++;
            } else
            {
                courseDb.CurrentAttendants--;
            }

            context.SaveChanges();

            SetFlash("success", "You have successfully modified preorder!");

            return RedirectToAction("Preorders");
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult PreorderDelete(int id)
        {
            Preorders preorder = GetPreorder(id);

            return View(preorder);
        }

        [HttpPost]
        [EmployeeAuth]
        public ActionResult PreorderDelete(Preorders preorder)
        {
            Preorders preorderDb = context.Preorders.Where(c => c.Id == preorder.Id).Single();
            Courses courseDb = context.Courses.Where(c => c.Id == preorderDb.CourseId).Single();
            courseDb.CurrentAttendants--;
            context.Preorders.Remove(preorderDb);
            context.SaveChanges();

            SetFlash("success", "You have successfully deleted preorder!");

            return RedirectToAction("Preorders");
        }

        [HttpGet]
        public ActionResult SignUp(int id)
        {
            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            Courses course = GetCourse(id);
            ViewBag.Course = course;

            return View();
        }

        [HttpPost]
        public ActionResult SignUp(Preorders preorder, int courseId)
        {
            ValidatePreorder(preorder);

            if (!ModelState.IsValid)
            {
                TempData["ViewData"] = ViewData;
                return RedirectToAction("SignUp", new { id = courseId });
            }

            preorder.SignUpDate = DateTime.Today;
            preorder.CourseId = courseId;
            preorder.IsApproved = false;

            context.Preorders.Add(preorder);
            context.SaveChanges();

            SetFlash("success", "You have signed up for course! Your data is stored. We will contact you soon.");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult CourseDetails(int id)
        {
            Courses course = GetCourse(id);

            return View(course);
        }

        [HttpPost]
        [EmployeeAuth]
        public ActionResult CourseCreate(Courses course)
        {
            ValidateCourse(course);

            if (!ModelState.IsValid)
            {
                return View();
            }

            context.Courses.Add(course);
            context.SaveChanges();

            SetFlash("success", "You have successfully created course!");

            return RedirectToAction("ManageCourses");
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult CourseEdit(int id)
        {
            Courses course = GetCourse(id);

            return View(course);
        }

        [HttpPost]
        [EmployeeAuth]
        public ActionResult CourseEdit(Courses course)
        {
            ValidateCourse(course);

            if (!ModelState.IsValid)
            {
                return View();
            }

            Courses courseDb = context.Courses.Where(c => c.Id == course.Id).Single();

            courseDb.Name = course.Name;
            courseDb.Description = course.Description;
            courseDb.StartDate = course.StartDate;
            courseDb.CurrentAttendants = course.CurrentAttendants;
            courseDb.MaxAttendants = course.MaxAttendants;
            context.SaveChanges();

            SetFlash("success", "You have modified the course!");

            return RedirectToAction("ManageCourses");
        }

        [HttpGet]
        [EmployeeAuth]
        public ActionResult CourseDelete(int id)
        {
            Courses course = GetCourse(id);

            return View(course);
        }

        [HttpPost]
        [EmployeeAuth]
        public ActionResult CourseDelete(Courses course)
        {
            Courses courseDb = context.Courses.Where(c => c.Id == course.Id).Single();
            context.Courses.Remove(courseDb);
            context.SaveChanges();

            SetFlash("success", "You have successfully deleted course!");

            return RedirectToAction("ManageCourses");
        }

        private List<Courses> GetAvailableCourses()
        {
            List<Courses> courseList = new List<Courses>();

            List<Courses> courseListDb = context.Courses
                .Where(c => c.CurrentAttendants < c.MaxAttendants)
                .ToList();

            foreach(Courses course in courseListDb)
            {
                if(course.StartDate >= DateTime.Now)
                {
                    courseList.Add(course);
                }
            }

            return courseList;
        }

        private Courses GetCourse(int id)
        {
            Courses course = context.Courses
                .Where(c => c.Id == id)
                .Single();

            return course;
        }

        private Preorders GetPreorder(int id)
        {
            Preorders preorder = context.Preorders
                .Where(c => c.Id == id)
                .Single();

            return preorder;
        }

        private string GetGeneration()
        {
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;

            if (currentMonth >= 9)
            {
                return currentYear + "/" + currentYear + 1;
            }
            else
            {
                return currentYear - 1 + "/" + currentYear;
            }
        }

        private void ValidateCourse(Courses course)
        {
            if (string.IsNullOrWhiteSpace(course.Name))
            {
                ModelState.AddModelError("Name", "This field can't be empty");
            }
            else if (!(course.Name.Length >= 3 && course.Name.Length < 50))
            {
                ModelState.AddModelError("Name", "Name has to be between 3 and 50 characters long");
            }

            if (string.IsNullOrWhiteSpace(course.Description))
            {
                ModelState.AddModelError("Description", "This field can't be empty");
            }
            else if (course.Description.Length <= 5)
            {
                ModelState.AddModelError("Description", "Description has to be at least 5 words long");
            }

            if (course.CurrentAttendants > course.MaxAttendants)
            {
                ModelState.AddModelError("CurrentAttendants", "Value has to be lower or equal then Max Attendants");
            }
            else if (course.CurrentAttendants < 0)
            {
                ModelState.AddModelError("CurrentAttendants", "Value has to be 0 or bigger");
            }

            if (course.MaxAttendants <= 0)
            {
                ModelState.AddModelError("MaxAttendants", "Value has to be bigger then 0");
            }
        }

        private void ValidatePreorder(Preorders preorder)
        {
            if (string.IsNullOrWhiteSpace(preorder.FirstName))
            {
                ModelState.AddModelError("FirstName", "This field can't be empty");
            }
            else if (!(preorder.FirstName.Length >= 3 && preorder.FirstName.Length <= 50))
            {
                ModelState.AddModelError("FirstName", "Name has to be between 3 and 50 characters long");
            }

            if (string.IsNullOrWhiteSpace(preorder.LastName))
            {
                ModelState.AddModelError("LastName", "This field can't be empty");
            }
            else if (!(preorder.LastName.Length >= 2 && preorder.LastName.Length <= 50))
            {
                ModelState.AddModelError("LastName", "Last name has to be between 2 and 50 characters long");
            }

            if (string.IsNullOrWhiteSpace(preorder.Address))
            {
                ModelState.AddModelError("Address", "This field can't be empty");
            }
            else if (preorder.Address.Length > 30)
            {
                ModelState.AddModelError("Address", "Address has to be under 50 characters long");
            }

            if (string.IsNullOrWhiteSpace(preorder.Email))
            {
                ModelState.AddModelError("Email", "This field can't be empty");
            }

            if (string.IsNullOrWhiteSpace(preorder.Phone))
            {
                ModelState.AddModelError("Phone", "This field can't be empty");
            }
            else if (!(IsPhoneNumber(preorder.Phone)))
            {
                ModelState.AddModelError("Phone", "Not supported phone format");
            }
        }

        private bool IsPhoneNumber(string phone)
        {
            int count = 1;
            foreach (char c in phone)
            {
                if (!(Char.IsNumber(c)))
                {
                    if (c == '/' && count == 4) continue;
                    return false;
                }
                count++;
            }

            if (count < 9) return false;

            return true;
        }

        public void SetFlash(string type, string text)
        {
            TempData["FlashMessage.Type"] = type;
            TempData["FlashMessage.Text"] = text;
        }
    }
}