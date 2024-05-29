using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.ViewModels;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class StudentController(
        ILogger<StudentController> logger,
        AppDbContext dbContext,
        UserService userService,
        StudentService studentService
    ) : BaseModelController(logger)
    {
        private readonly ILogger<StudentController> _logger = logger;

        private readonly AppDbContext _dbContext = dbContext;

        private readonly UserService _userService = userService;

        private readonly StudentService _studentService = studentService;

        [Authorize(Roles = Configurations.Constants.userRoles.Student)]
        public async Task<IActionResult> Index()
        {
            var signed_user = await _userService.GetCurrentUserAsync();
            // get student with payments and paymentperiod and grades from db
            if (signed_user == null)
                return RedirectToAction("SignIn");

            var student = _dbContext
                .Students.Include(s => s.School)
                .Where(s => s.Id == signed_user.Id)
                .FirstOrDefault();

            if (student == null)
                return RedirectToAction("SignIn");

            student.Payments = await _dbContext
                .Payments.Where(pp => pp.Student != null && pp.Student.Id == student.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            student.PaymentPeriods = await _dbContext
                .PaymentPeriods.Include(g => g.WorkYear)
                .Where(pp => pp.Student != null && pp.Student.Id == student.Id)
                .OrderByDescending(pp => pp.EndDate)
                .Take(10)
                .ToListAsync();

            student.Grades = await _dbContext
                .Grades.Where(g => g.Students.Any(st => st.Id == student.Id))
                .OrderByDescending(g => g.CreatedAt)
                .Take(5)
                .ToListAsync();

            StudentView studentView = student.ToView();
            ViewBag.UserRole = signed_user.Role;

            return View(studentView);
        }

        public async Task<IActionResult> SignIn()
        {
            if (await _userService.IsUserSignedIn())
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(
            [Bind("StudentId", "Password")] StudentView studentView
        )
        {
            // some idoitic validation
            studentView.PasswordVerify = studentView.Password;
            UserViewValidationResult validationResult = studentView.ValidateFieldsSignIn();
            if (validationResult != UserViewValidationResult.FieldsAreValid)
            {
                TempData["Error"] = "Invalid fields";
                switch (validationResult)
                {
                    case UserViewValidationResult.PasswordsNotMatch:
                        ModelState.AddModelError("Password", "Passwords do not match");
                        return View(studentView);
                    case UserViewValidationResult.PasswordEmpty:
                        ModelState.AddModelError("Password", "Password is empty");
                        return View(studentView);
                    case UserViewValidationResult.InvalidName:
                        ModelState.AddModelError("StudentId", "Invalid studentId");
                        return View(studentView);
                    default:
                        return View(studentView);
                }
            }

            var passwordHash = _userService.HashPassword(studentView.Password);
            var dbStudent = _dbContext
                .Students.Where(u =>
                    u.StudentId == studentView.StudentId && u.PasswordHash == passwordHash
                )
                .FirstOrDefault();
            if (dbStudent == null)
            {
                ModelState.AddModelError("StudentId", "Invalid studentId or password");
                return RedirectToAction("SignIn");
            }
            else
            {
                _logger.LogDebug("User {0} signed in", dbStudent.StudentId);
            }

            try
            {
                if (!await _userService.SignInUser(dbStudent, UserRole.Student))
                {
                    TempData["Error"] = "Error while signing in user";
                    return RedirectToAction("SignIn");
                }
                else
                {
                    TempData["Message"] =
                        "hello, " + dbStudent.FirstName + " " + dbStudent.LastName + "!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error while signing in user {}: {}",
                    dbStudent.StudentId,
                    ex.Message
                );
                TempData["CantSignIn"] = true;
                return RedirectToAction("SignIn");
            }
            return RedirectToAction("Index");
        }

        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public async Task<IActionResult> List(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            var (usrRole, schId) = await _userService.GetUserRoleAndSchoolId();
            if (usrRole == UserRole.None)
                return RedirectToAction("SignIn");

            var students = _dbContext.Students.Include(s => s.School).AsQueryable();

            if (usrRole == UserRole.SchoolAdmin)
            {
                students = students
                    .Where(s => s.School != null && s.School.Id == schId)
                    .AsQueryable();
            }

            var modelList = new QueryableModelHelper<Student>(students, Student.SearchConfig);

            searchField ??= "";
            searchString ??= "";
            sortOrder ??= "";
            return TryListOrFail(
                () =>
                    modelList.List(
                        ViewData,
                        searchString.ToSanitizedLowercase(),
                        searchField.ToSanitizedLowercase(),
                        sortOrder.ToSanitizedLowercase(),
                        pageIndex,
                        pageSize
                    ),
                "students"
            );
        }

        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public IActionResult Create()
        {
            var (usrRole, schId) = _userService.GetUserRoleAndSchoolId().Result;

            if (usrRole != UserRole.None && usrRole != UserRole.None && usrRole != UserRole.Student)
            {
                IQueryable<School> schools = _dbContext.Schools;

                if (usrRole == UserRole.SchoolAdmin)
                {
                    schools = schools.Where(s => s.Id == schId);

                    if (!schools.Any())
                    {
                        ViewData["Error"] = "School not found";
                        _logger.LogError("School not found in db,sch id: {0}", schId);
                        return RedirectToAction("SignOutUser", "UserCommon");
                    }
                }

                ViewBag.Schools = schools;

                return View();
            }

            ViewData["Error"] = "You are not authorized to this page";
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public async Task<IActionResult> Create(StudentView studentView)
        {
            var (usrRole, schId) = await _userService.GetUserRoleAndSchoolId();

            if (usrRole == UserRole.None)
            {
                return RedirectToAction("SignIn");
            }

            if (usrRole == UserRole.SchoolAdmin && schId != studentView.SchoolId)
            {
                ModelState.AddModelError(
                    "School",
                    "You are not authorized to create a student for this school"
                );
                return View(studentView);
            }

            var validationResult = studentView.ValidateFieldsCreate(_dbContext);

            if (validationResult != UserViewValidationResult.FieldsAreValid)
            {
                HandleValidationErrors(validationResult);
                return View(studentView);
            }

            var school = _dbContext.Schools.FirstOrDefault(s => s.Id == studentView.SchoolId);

            if (school == null)
            {
                ModelState.AddModelError("School", "School is required");
                return View(studentView);
            }

            try
            {
                var newStudent = CreateNewStudent(studentView, school);
                _dbContext.Students.Add(newStudent);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                LogCreateError(ex, studentView.EmailAddress);
                return RedirectToAction("Create");
            }

            return RedirectToAction("List");
        }

        private void HandleValidationErrors(UserViewValidationResult validationResult)
        {
            TempData["CantCreate"] = true;
            switch (validationResult)
            {
                case UserViewValidationResult.EmailAddressNotMatchRegex:
                    ModelState.AddModelError("EmailAddress", "Invalid email syntax");
                    break;
                case UserViewValidationResult.PasswordsNotMatch:
                    ModelState.AddModelError("Password", "Passwords do not match");
                    break;
                case UserViewValidationResult.PasswordEmpty:
                    ModelState.AddModelError("Password", "Password is empty");
                    break;
                case UserViewValidationResult.UserExists:
                    ModelState.AddModelError("EmailAddress", "User already exists");
                    break;
                case UserViewValidationResult.InvalidName:
                    ModelState.AddModelError("FirstName", "Invalid first name or last name");
                    break;
                case UserViewValidationResult.EmailAddressExists:
                    ModelState.AddModelError("EmailAddress", "Email already exists");
                    break;
            }
        }

        private Student CreateNewStudent(StudentView studentView, School school)
        {
            return new Student
            {
                EmailAddress = studentView.EmailAddress,
                FirstName = studentView.FirstName,
                LastName = studentView.LastName,
                PasswordHash = _userService.HashPassword(studentView.Password),
                School = school,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ContactInfo = new ContactInfo
                {
                    Email = studentView.EmailAddress,
                    PhoneNumber = studentView.ContactInfo.PhoneNumber,
                    Addresses = studentView.ContactInfo.Addresses
                },
                StudentId = _studentService.GenerateStudentId(school)
            };
        }

        private void LogCreateError(Exception ex, string emailAddress)
        {
            _logger.LogError("Error while creating user {0}: {1}", emailAddress, ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {0}", ex.InnerException.Message);
            }
            if (ex.InnerException?.InnerException != null)
            {
                _logger.LogError(
                    "Inner inner exception: {0}",
                    ex.InnerException.InnerException.Message
                );
            }
            TempData["CantCreate"] = true;
        }

        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public async Task<IActionResult> Details(int? id)
        {
            (UserRole? usrRole, int? schId) = await _userService.GetUserRoleAndSchoolId();

            if (id == null)
                return NotFound();
            if (usrRole is UserRole.None or null)
                return RedirectToAction("SignIn", "Home");

            IQueryable<Student> students = _dbContext
                .Students.Include(s => s.School)
                .Include(s => s.ContactInfo)
                .Include(s => s.Payments)
                .Include(s => s.Grades)
                .Where(s => s.Id == id);

            if (usrRole == UserRole.SchoolAdmin)
                students = students.Where(s => s.School != null && s.School.Id == schId);

            Student? student = await students.FirstOrDefaultAsync();

            return student == null ? NotFound() : View(student.ToView());
        }

        [Authorize(
            Roles = Configurations.Constants.userRoles.SiteAdmin
                + ","
                + Configurations.Constants.userRoles.SchoolAdmin
        )]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var (usrRole, schId) = _userService.GetUserRoleAndSchoolId().Result;
            var studentQuerable = _dbContext.Students.Where(e => e.Id == id);

            switch (usrRole)
            {
                case UserRole.SchoolAdmin:
                    studentQuerable = studentQuerable.Where(s =>
                        s.School != null && s.School.Id == schId
                    );
                    break;
                case UserRole.None:
                    return RedirectToAction("SignIn");
                case UserRole.Student:
                    return RedirectToAction("Index");
                case UserRole.SiteAdmin:
                    break;
            }

            var student = studentQuerable.FirstOrDefault();

            if (student == null)
            {
                return NotFound();
            }

            // check if the user is logged in
            // if the user is logged in, prevent deletion

            var loggedInUserId = _userService.GetSignedInUserId();
            _logger.LogInformation("Logged in user id: {}", loggedInUserId);

            if (loggedInUserId == id)
            {
                ViewData["Message"] = "You cannot delete the account you are logged in with";
                return RedirectToAction("List");
            }

            return View(student.ToView());
        }

        [
            HttpPost,
            ValidateAntiForgeryToken,
            Authorize(
                Roles = Configurations.Constants.userRoles.SiteAdmin
                    + ","
                    + Configurations.Constants.userRoles.SchoolAdmin
            )
        ]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();
            var (usrRole, schId) = await _userService.GetUserRoleAndSchoolId();
            var studentQuerable = _dbContext.Students.Where(e => e.Id == id);
            switch (usrRole)
            {
                case UserRole.SchoolAdmin:
                    studentQuerable = studentQuerable.Where(s =>
                        s.School != null && s.School.Id == schId
                    );
                    break;
                case UserRole.None:
                    return RedirectToAction("SignIn");
                case UserRole.Student:
                    return RedirectToAction("Index");
            }
            var student = studentQuerable.FirstOrDefault();
            if (student == null)
                return NotFound();
            bool isUserDeleted = await _userService.DeleteUser(student.Id);
            if (!isUserDeleted)
                ViewData["Message"] = "Error while deleting user";
            return RedirectToAction("List");
        }

        public async Task<Student?> GetLoggedInStudent()
        {
            var student = await _userService.GetCurrentUserAsync();
            if (student == null)
            {
                return null;
            }
            return student.Role switch
            {
                UserRole.Student
                    => _dbContext
                        .Students.Include(s => s.School)
                        .Include(s => s.Payments)
                        .Include(s => s.Grades)
                        .Where(s => s.Id == student.Id)
                        .FirstOrDefault(),
                _ => null,
            };
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Edit(int? id)
        {
            _logger.LogDebug("not tested function StudentController.Edit called");
            if (id == null)
            {
                return NotFound();
            }

            var student = _dbContext.Students.Where(e => e.Id == id).FirstOrDefault();

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,EmailAddress,Password,PasswordVerify,SchoolId,FirstName,LastName")]
                Student student
        )
        {
            _logger.LogDebug("not tested function StudentController.Edit[post] called");
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContext.Update(student);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        private bool StudentExists(int id)
        {
            return _dbContext.Students.Any(e => e.Id == id);
        }
    }
}
