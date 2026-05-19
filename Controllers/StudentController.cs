using Microsoft.AspNetCore.Mvc;
using CCSMonitoringSystem.Models;
using CCSMonitoringSystem.Data;
using System.Linq;

namespace CCSMonitoringSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public StudentController(AppDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
        }

        // GET: /Student/Index  →  Login page
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Student/Login
        [HttpPost]
        public IActionResult Login(Student loginData)
        {
            ModelState.Remove("LastName");
            ModelState.Remove("FirstName");
            ModelState.Remove("Email");
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("ProfilePictureUrl");
            ModelState.Remove("ConfirmPassword");

            // ADMIN LOGIN
            if (loginData.IdNumber == "admin" && loginData.Password == "123456")
            {
                TempData["Admin"] = "Administrator";
                return RedirectToAction("AdminDashboard");
            }

            try
            {
                // Trim whitespace from input
                string idNumber = loginData.IdNumber?.Trim() ?? "";
                string password = loginData.Password?.Trim() ?? "";

                // DEBUG: Log the values being searched
                System.Diagnostics.Debug.WriteLine($"LOGIN ATTEMPT - ID: '{idNumber}' | Password: '{password}'");

                var allStudents = _db.Students.ToList();
                foreach (var s in allStudents)
                {
                    System.Diagnostics.Debug.WriteLine($"DB - ID: '{s.IdNumber?.Trim()}' | Password: '{s.Password?.Trim()}'");
                    System.Diagnostics.Debug.WriteLine($"  ID Match: {(s.IdNumber?.Trim() == idNumber)} | Password Match: {(s.Password?.Trim() == password)}");
                }

                var student = _db.Students.ToList()
                    .FirstOrDefault(s => (s.IdNumber?.Trim() ?? "") == idNumber
                                      && (s.Password?.Trim() ?? "") == password);

                if (student == null)
                {
                    ViewBag.Error = "Invalid ID Number or Password.";
                    return View("Index", loginData);
                }

                TempData["LoggedIn"] = student.FirstName + " " + student.LastName;
                TempData["StudentId"] = student.IdNumber;
                TempData["ProfilePicture"] = student.ProfilePictureUrl ?? "";

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Database error: " + ex.Message;
                return View("Index", loginData);
            }
        }

        // GET: /Student/Dashboard  (simple landing after login)
        public IActionResult Dashboard()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            TempData.Keep("LoggedIn");
            TempData.Keep("StudentId");
            TempData.Keep("ProfilePicture");

            ViewBag.StudentName = TempData["LoggedIn"] ?? "Student";
            ViewBag.StudentId = TempData["StudentId"];
            ViewBag.ProfilePicture = TempData["ProfilePicture"];

            // Get the full student data
            string studentId = TempData["StudentId"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    ViewBag.StudentEmail = student.Email ?? "Not set";
                    ViewBag.Course = student.Course ?? "Not set";
                    ViewBag.CourseLevel = student.CourseLevel ?? "Not set";
                    ViewBag.Address = student.Address ?? "Not set";
                    ViewBag.ProfilePicture = student.ProfilePictureUrl ?? "";
                }
            }

            return View();
        }

        // POST: /Student/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Index");
        }

        // POST: /Student/UploadProfilePicture
        [HttpPost]
        public IActionResult UploadProfilePicture(IFormFile ProfilePicture)
        {
            string studentId = TempData["StudentId"]?.ToString();
            if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Index");
            TempData.Keep("StudentId");
            TempData.Keep("LoggedIn");
            TempData.Keep("ProfilePicture");

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    if (!string.IsNullOrEmpty(student.ProfilePictureUrl) && student.ProfilePictureUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, student.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    student.ProfilePictureUrl = SaveProfilePicture(ProfilePicture);
                    _db.SaveChanges();
                    TempData["ProfilePicture"] = student.ProfilePictureUrl;
                }
            }
            return RedirectToAction("Dashboard");
        }

        // GET: /Student/AdminDashboard
        public IActionResult AdminDashboard()
        {
            if (TempData["Admin"] == null)
                return RedirectToAction("Index");

            ViewBag.Admin = TempData["Admin"];
            return View();
        }

        // GET: /Student/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Student/Register
        [HttpPost]
        public IActionResult Register(Student student, IFormFile profilePictureFile)
        {
            ModelState.Remove("RememberMe");
            ModelState.Remove("ProfilePictureUrl");

            if (!ModelState.IsValid)
                return View(student);

            // Check for duplicate ID Number
            bool exists = _db.Students.Any(s => s.IdNumber == student.IdNumber);
            if (exists)
            {
                ModelState.AddModelError("IdNumber", "This ID Number is already registered.");
                return View(student);
            }

            // Handle profile picture upload
            if (profilePictureFile != null && profilePictureFile.Length > 0)
            {
                student.ProfilePictureUrl = SaveProfilePicture(profilePictureFile);
            }

            _db.Students.Add(student);
            _db.SaveChanges();

            TempData["Success"] = "Registration successful! You can now log in.";
            return RedirectToAction("Index");
        }

        // POST: /Student/UpdateProfile
        [HttpPost]
        public IActionResult UpdateProfile(string idNumber, string firstName, string lastName, string email, string course, string courseLevel, string address, IFormFile profilePictureFile)
        {
            try
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == idNumber);
                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found" });
                }

                student.FirstName = firstName;
                student.LastName = lastName;
                student.Email = email;
                student.Course = course;
                student.CourseLevel = courseLevel;
                student.Address = address;

                // Handle profile picture upload
                if (profilePictureFile != null && profilePictureFile.Length > 0)
                {
                    // Delete old picture if it exists
                    if (!string.IsNullOrEmpty(student.ProfilePictureUrl) && student.ProfilePictureUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, student.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    student.ProfilePictureUrl = SaveProfilePicture(profilePictureFile);
                }

                _db.SaveChanges();
                return Json(new { success = true, message = "Profile updated successfully", profilePicture = student.ProfilePictureUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to save profile picture
        private string SaveProfilePicture(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;

                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "profiles");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                return "/uploads/profiles/" + uniqueFileName;
            }
            catch
            {
                return null;
            }
        }

        // GET: /Student/GetAllStudents - API endpoint for loading all students
        [HttpGet]
        public IActionResult GetAllStudents()
        {
            try
            {
                var students = _db.Students.ToList();
                return Json(students);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: /Student/SearchStudent - API endpoint for searching students
        [HttpGet]
        public IActionResult SearchStudent(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new List<Student>());
                }

                var students = _db.Students
                    .Where(s => s.IdNumber.Contains(query) || 
                               s.FirstName.Contains(query) || 
                               s.LastName.Contains(query))
                    .ToList();
                return Json(students);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: /Student/GetStudentById - API endpoint for getting a single student
        [HttpGet]
        public IActionResult GetStudentById(string id)
        {
            try
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == id);
                return Json(student);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST: /Student/CreateSitIn - API endpoint for creating sit-in record
        [HttpPost]
        public IActionResult CreateSitIn([FromBody] SitInRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.IdNumber))
                {
                    return Json(new { success = false, message = "ID Number is required" });
                }

                var student = _db.Students.FirstOrDefault(s => s.IdNumber == request.IdNumber);
                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found" });
                }

                return Json(new { success = true, message = "Sit-In created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Student/EditProfile
        public IActionResult EditProfile()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            TempData.Keep("LoggedIn");
            TempData.Keep("StudentId");
            TempData.Keep("ProfilePicture");

            ViewBag.StudentName = TempData["LoggedIn"] ?? "Student";
            ViewBag.StudentId = TempData["StudentId"];
            ViewBag.ProfilePicture = TempData["ProfilePicture"];

            string studentId = TempData["StudentId"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    ViewBag.StudentEmail = student.Email ?? "Not set";
                    ViewBag.Course = student.Course ?? "Not set";
                    ViewBag.CourseLevel = student.CourseLevel ?? "Not set";
                    ViewBag.Address = student.Address ?? "Not set";
                    ViewBag.ProfilePicture = student.ProfilePictureUrl ?? "";
                }
            }

            return View();
        }

        // GET: /Student/RemainingSession
        public IActionResult RemainingSession()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            TempData.Keep("LoggedIn");
            TempData.Keep("StudentId");
            TempData.Keep("ProfilePicture");

            ViewBag.StudentName = TempData["LoggedIn"] ?? "Student";
            ViewBag.StudentId = TempData["StudentId"];
            ViewBag.ProfilePicture = TempData["ProfilePicture"];

            string studentId = TempData["StudentId"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    ViewBag.Course = student.Course ?? "Not set";
                    ViewBag.CourseLevel = student.CourseLevel ?? "Not set";
                }
            }

            return View();
        }

        // GET: /Student/Announcements
        public IActionResult Announcements()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            TempData.Keep("LoggedIn");
            TempData.Keep("StudentId");
            TempData.Keep("ProfilePicture");

            ViewBag.StudentName = TempData["LoggedIn"] ?? "Student";
            ViewBag.StudentId = TempData["StudentId"];
            ViewBag.ProfilePicture = TempData["ProfilePicture"];

            string studentId = TempData["StudentId"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    ViewBag.Course = student.Course ?? "Not set";
                    ViewBag.CourseLevel = student.CourseLevel ?? "Not set";
                }
            }

            return View();
        }

        // GET: /Student/Rules
        public IActionResult Rules()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            TempData.Keep("LoggedIn");
            TempData.Keep("StudentId");
            TempData.Keep("ProfilePicture");

            ViewBag.StudentName = TempData["LoggedIn"] ?? "Student";
            ViewBag.StudentId = TempData["StudentId"];
            ViewBag.ProfilePicture = TempData["ProfilePicture"];

            string studentId = TempData["StudentId"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    ViewBag.Course = student.Course ?? "Not set";
                    ViewBag.CourseLevel = student.CourseLevel ?? "Not set";
                }
            }

            return View();
        }

        // GET: /Student/Feedback
        public IActionResult Feedback()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            TempData.Keep("LoggedIn");
            TempData.Keep("StudentId");
            TempData.Keep("ProfilePicture");

            ViewBag.StudentName = TempData["LoggedIn"] ?? "Student";
            ViewBag.StudentId = TempData["StudentId"];
            ViewBag.ProfilePicture = TempData["ProfilePicture"];

            string studentId = TempData["StudentId"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    ViewBag.Course = student.Course ?? "Not set";
                    ViewBag.CourseLevel = student.CourseLevel ?? "Not set";
                }
            }

            return View();
        }

        // GET: /Student/Reservation
        public IActionResult Reservation()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            TempData.Keep("LoggedIn");
            TempData.Keep("StudentId");
            TempData.Keep("ProfilePicture");

            ViewBag.StudentName = TempData["LoggedIn"] ?? "Student";
            ViewBag.StudentId = TempData["StudentId"];
            ViewBag.ProfilePicture = TempData["ProfilePicture"];

            string studentId = TempData["StudentId"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    ViewBag.Course = student.Course ?? "Not set";
                    ViewBag.CourseLevel = student.CourseLevel ?? "Not set";
                }
            }

            return View();
        }

        // GET: /Student/RewardsPoints
        public IActionResult RewardsPoints()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            TempData.Keep("LoggedIn");
            TempData.Keep("StudentId");
            TempData.Keep("ProfilePicture");

            ViewBag.StudentName = TempData["LoggedIn"] ?? "Student";
            ViewBag.StudentId = TempData["StudentId"];
            ViewBag.ProfilePicture = TempData["ProfilePicture"];

            string studentId = TempData["StudentId"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    ViewBag.Course = student.Course ?? "Not set";
                    ViewBag.CourseLevel = student.CourseLevel ?? "Not set";
                }
            }

            return View();
        }

        // GET: /Student/Notifications
        public IActionResult Notifications()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            TempData.Keep("LoggedIn");
            TempData.Keep("StudentId");
            TempData.Keep("ProfilePicture");

            ViewBag.StudentName = TempData["LoggedIn"] ?? "Student";
            ViewBag.StudentId = TempData["StudentId"];
            ViewBag.ProfilePicture = TempData["ProfilePicture"];

            string studentId = TempData["StudentId"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(studentId))
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == studentId);
                if (student != null)
                {
                    ViewBag.Course = student.Course ?? "Not set";
                    ViewBag.CourseLevel = student.CourseLevel ?? "Not set";
                }
            }

            return View();
        }
    }

    // Helper class for sit-in request
    public class SitInRequest
    {
        public string IdNumber { get; set; }
        public string Purpose { get; set; }
        public string Lab { get; set; }
    }
}
