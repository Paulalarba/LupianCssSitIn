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

                student.LastLoginAt = DateTime.UtcNow;
                _db.SaveChanges();

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
                    ViewBag.TotalSessions = student.TotalSessions;
                    ViewBag.RemainingSessions = student.RemainingSessions;
                    ViewBag.TotalSessionsUsed = student.TotalSessionsUsed;
                    ViewBag.Points = student.Points;
                    ViewBag.RewardPoints = student.RewardPoints;
                    ViewBag.ActiveReservations = _db.Reservations.Count(r => r.StudentId == studentId && r.Status != "Declined" && r.Status != "Completed");
                    ViewBag.ActiveSitIns = _db.SitInSessions.Count(s => s.StudentId == studentId && s.Status == "Active");
                    ViewBag.UnreadNotifications = _db.Notifications.Count(n => n.StudentId == studentId && !n.IsRead);
                    ViewBag.AnnouncementCount = _db.Announcements.Count(a => a.Audience == "All" || a.Audience == "Students");
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
        public IActionResult UploadProfilePicture(IFormFile? ProfilePicture)
        {
            string studentId = TempData["StudentId"]?.ToString() ?? string.Empty;
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

            TempData.Keep("Admin");
            ViewBag.Admin = TempData["Admin"];
            ViewBag.StudentCount = _db.Students.Count();
            ViewBag.ActiveSitIns = _db.SitInSessions.Count(session => session.Status == "Active");
            ViewBag.PendingReservations = _db.Reservations.Count(reservation => reservation.Status == "Pending");
            ViewBag.FeedbackPending = _db.Feedback.Count(feedback => feedback.Status == "Pending");
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

            student.TotalSessions = Student.DefaultSessionAllocation;
            student.RemainingSessions = Student.DefaultSessionAllocation;
            student.TotalSessionsUsed = 0;
            student.Points = 0;
            student.RewardPoints = 0;
            student.RegisteredAt = DateTime.UtcNow;

            _db.Students.Add(student);
            _db.Notifications.Add(new NotificationItem
            {
                StudentId = student.IdNumber,
                Title = "Welcome to the CCS Lab Portal",
                Message = "Your account is ready. You can now request reservations, view lab rules, and track sit-in activity.",
                Type = "Success"
            });
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
                    return string.Empty;

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
                return string.Empty;
            }
        }

        // GET: /Student/GetAllStudents - API endpoint for loading all students
        [HttpGet]
        public IActionResult GetAllStudents()
        {
            try
            {
                var students = _db.Students
                    .Select(student => new
                    {
                        student.IdNumber,
                        student.FirstName,
                        student.MiddleName,
                        student.LastName,
                        student.Email,
                        student.Course,
                        student.CourseLevel,
                        student.Address,
                        student.ProfilePictureUrl,
                        student.TotalSessions,
                        student.RemainingSessions,
                        student.TotalSessionsUsed,
                        student.Points,
                        student.RewardPoints,
                        FullName = student.FirstName + " " + student.LastName
                    })
                    .ToList();
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

                if (student.RemainingSessions <= 0)
                {
                    return Json(new { success = false, message = "No remaining sit-in sessions available" });
                }

                var activeSession = _db.SitInSessions.FirstOrDefault(session => session.StudentId == request.IdNumber && session.Status == "Active");
                if (activeSession != null)
                {
                    return Json(new { success = false, message = "Student already has an active sit-in session" });
                }

                var session = new SitInSession
                {
                    StudentId = student.IdNumber,
                    LabName = request.Lab?.Trim() ?? "Computer Laboratory",
                    Purpose = request.Purpose?.Trim() ?? "Laboratory use",
                    LanguageUsed = string.IsNullOrWhiteSpace(request.LanguageUsed) ? "Not Specified" : request.LanguageUsed.Trim(),
                    SeatNumber = request.SeatNumber?.Trim(),
                    Notes = request.Notes?.Trim(),
                    ApprovedByAdminId = "admin",
                    ApprovedAt = DateTime.UtcNow,
                    TimeIn = DateTime.UtcNow,
                    Status = "Active"
                };

                student.RemainingSessions -= 1;
                student.TotalSessionsUsed += 1;

                _db.SitInSessions.Add(session);
                _db.Notifications.Add(new NotificationItem
                {
                    StudentId = student.IdNumber,
                    Title = "Sit-in session started",
                    Message = $"{session.LabName} session for {session.Purpose} is now active.",
                    Type = "Info"
                });
                _db.SaveChanges();

                return Json(new { success = true, message = "Sit-In created successfully", sessionId = session.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CheckoutSitIn([FromBody] CheckoutSitInRequest request)
        {
            try
            {
                var session = _db.SitInSessions.FirstOrDefault(item => item.Id == request.SessionId && item.Status == "Active");
                if (session == null)
                {
                    return Json(new { success = false, message = "Active sit-in session not found" });
                }

                session.TimeOut = DateTime.UtcNow;
                session.Status = "Completed";
                session.HasViolation = request.HasViolation;
                session.ViolationRemarks = request.ViolationRemarks;
                session.Notes = request.Notes;

                if (request.PointsAwarded > 0)
                {
                    session.PointsAwarded = request.PointsAwarded;
                    session.IsRewardEvaluated = true;
                    session.RewardEvaluatedAt = DateTime.UtcNow;

                    var student = _db.Students.FirstOrDefault(item => item.IdNumber == session.StudentId);
                    if (student != null)
                    {
                        student.Points += request.PointsAwarded;
                        student.RewardPoints += request.PointsAwarded;
                    }

                    _db.RewardPoints.Add(new RewardPoint
                    {
                        StudentId = session.StudentId,
                        SitInSessionId = session.Id,
                        Points = request.PointsAwarded,
                        Source = "Completed sit-in session",
                        Notes = request.Notes,
                        AwardedByAdminId = "admin"
                    });
                }

                _db.SaveChanges();
                return Json(new { success = true, message = "Sit-in session checked out successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CreateReservation([FromBody] ReservationRequest request)
        {
            try
            {
                var studentId = string.IsNullOrWhiteSpace(request.StudentId)
                    ? TempData["StudentId"]?.ToString()
                    : request.StudentId.Trim();
                TempData.Keep("StudentId");
                TempData.Keep("LoggedIn");
                TempData.Keep("ProfilePicture");

                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return Json(new { success = false, message = "Student ID is required" });
                }

                var student = _db.Students.FirstOrDefault(item => item.IdNumber == studentId);
                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found" });
                }

                if (request.EndTime <= request.StartTime)
                {
                    return Json(new { success = false, message = "End time must be later than start time" });
                }

                var reservation = new Reservation
                {
                    StudentId = student.IdNumber,
                    LabName = request.LabName?.Trim() ?? "Computer Laboratory",
                    ReservationDate = request.ReservationDate.Date.ToUniversalTime(),
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Purpose = request.Purpose?.Trim() ?? "Laboratory use",
                    SeatNumber = request.SeatNumber?.Trim() ?? "TBD",
                    Notes = request.Notes?.Trim(),
                    Status = "Pending"
                };

                _db.Reservations.Add(reservation);
                _db.Notifications.Add(new NotificationItem
                {
                    StudentId = student.IdNumber,
                    Title = "Reservation submitted",
                    Message = $"{reservation.LabName} on {reservation.ReservationDate:MMM d, yyyy} is pending admin review.",
                    Type = "Info"
                });
                _db.SaveChanges();

                return Json(new { success = true, message = "Reservation submitted successfully", reservationId = reservation.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SubmitFeedback([FromBody] FeedbackRequest request)
        {
            try
            {
                var studentId = string.IsNullOrWhiteSpace(request.StudentId)
                    ? TempData["StudentId"]?.ToString()
                    : request.StudentId.Trim();
                TempData.Keep("StudentId");
                TempData.Keep("LoggedIn");
                TempData.Keep("ProfilePicture");

                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return Json(new { success = false, message = "Student ID is required" });
                }

                var session = _db.SitInSessions
                    .Where(item => item.StudentId == studentId)
                    .OrderByDescending(item => item.TimeIn)
                    .FirstOrDefault();

                if (session == null)
                {
                    return Json(new { success = false, message = "Feedback requires at least one sit-in session" });
                }

                var feedback = new Feedback
                {
                    StudentId = studentId,
                    SitInSessionId = session.Id,
                    Rating = request.Rating,
                    Comments = request.Comments?.Trim() ?? string.Empty,
                    Status = "Pending"
                };

                _db.Feedback.Add(feedback);
                _db.SaveChanges();

                return Json(new { success = true, message = "Feedback submitted successfully", feedbackId = feedback.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetDashboardData(string studentId)
        {
            try
            {
                var student = _db.Students.FirstOrDefault(item => item.IdNumber == studentId);
                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found" });
                }

                var data = new
                {
                    success = true,
                    student = new
                    {
                        student.IdNumber,
                        student.FullName,
                        student.Course,
                        student.CourseLevel,
                        student.TotalSessions,
                        student.RemainingSessions,
                        student.TotalSessionsUsed,
                        student.Points,
                        student.RewardPoints
                    },
                    reservations = _db.Reservations
                        .Where(item => item.StudentId == studentId)
                        .OrderByDescending(item => item.CreatedAt)
                        .Take(5)
                        .ToList(),
                    sitIns = _db.SitInSessions
                        .Where(item => item.StudentId == studentId)
                        .OrderByDescending(item => item.TimeIn)
                        .Take(5)
                        .ToList(),
                    announcements = _db.Announcements
                        .Where(item => item.Audience == "All" || item.Audience == "Students")
                        .OrderByDescending(item => item.CreatedAt)
                        .Take(5)
                        .ToList(),
                    notifications = _db.Notifications
                        .Where(item => item.StudentId == studentId)
                        .OrderByDescending(item => item.CreatedAt)
                        .Take(5)
                        .ToList()
                };

                return Json(data);
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
        public string IdNumber { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Lab { get; set; } = string.Empty;
        public string LanguageUsed { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class CheckoutSitInRequest
    {
        public int SessionId { get; set; }
        public int PointsAwarded { get; set; }
        public bool HasViolation { get; set; }
        public string ViolationRemarks { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class ReservationRequest
    {
        public string StudentId { get; set; } = string.Empty;
        public string LabName { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class FeedbackRequest
    {
        public string StudentId { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public string Comments { get; set; } = string.Empty;
    }
}
