using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    public class StudentController : Controller
    {
        private readonly ILogger<StudentController> _logger;

        private readonly AppDbContext _dbContext;

        private readonly UserService _userService;

        public StudentController(
            ILogger<StudentController> logger,
            AppDbContext dbContext,
            UserService userService
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _userService = userService;
        }

        [Authorize(Roles = Configurations.Constants.userRoles.Student)]
        public async Task<IActionResult> Index()
        {
            var student = await GetLoggedInStudent();
            if (student == null)
            {
                return RedirectToAction("SignIn");
            }
            return View(student.ToView());
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
            [Bind("EmailAddress", "Password")] StudentView studentView
        )
        {
            // some idoitic validation
            studentView.PasswordVerify = studentView.Password;
            UserViewValidationResult validationResult = studentView.ValidateFieldsSignIn();
            if (validationResult != UserViewValidationResult.FieldsAreValid)
            {
                TempData["CantSignIn"] = true;
                switch (validationResult)
                {
                    case UserViewValidationResult.EmailAddressNotMatchRegex:
                        ModelState.AddModelError("EmailAddress", "invalid email syntax");
                        break;
                    case UserViewValidationResult.PasswordEmpty:
                        ModelState.AddModelError("Password", "Password is empty");
                        break;
                }
                return View(studentView);
            }

            _dbContext.Students ??= _dbContext.Set<Student>();

            var passwordHash = _userService.HashPassword(studentView.Password);
            var dbStudent = _dbContext
                .Students.Where(u =>
                    u.EmailAddress == studentView.EmailAddress && u.PasswordHash == passwordHash
                )
                .FirstOrDefault();
            if (dbStudent == null)
            {
                ModelState.AddModelError("EmailAddress", "User not found");
                return RedirectToAction("SignIn");
            }
            else
            {
                _logger.LogDebug("User {0} signed in", dbStudent.EmailAddress);
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
                    "Error while signing in user {0}: {1}",
                    dbStudent.EmailAddress,
                    ex.Message
                );
                TempData["CantSignIn"] = true;
                return RedirectToAction("SignIn");
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult List(
            string? searchString = null,
            string? searchField = null,
            string? sortOrder = null,
            int pageIndex = 1,
            int pageSize = 20
        )
        {
            _dbContext.Students ??= _dbContext.Set<Student>();

            var modelList = new QueryableModelHelper<Student>(
                _dbContext.Students.Include(s => s.School).AsQueryable(),
                Student.SearchConfig
            );

            return View(
                modelList.List(ViewData, searchString, searchField, sortOrder, pageIndex, pageSize)
            );
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Create()
        {
            ViewBag.Schools = _dbContext.Schools;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Create(StudentView studentView)
        {
            ViewBag.Schools = _dbContext.Schools;
            UserViewValidationResult validationResult = studentView.ValidateFieldsCreate(
                _dbContext
            );
            // check is school exists in db
            if (validationResult != UserViewValidationResult.FieldsAreValid)
            {
                TempData["CantCreate"] = true;
                switch (validationResult)
                {
                    case UserViewValidationResult.EmailAddressNotMatchRegex:
                        ModelState.AddModelError("EmailAddress", "invalid email syntax");
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
                return View(studentView);
            }
            var school = _dbContext
                .Schools?.Where(s => s.Id == studentView.SchoolId)
                .FirstOrDefault();

            if (school == null)
            {
                TempData["CantCreate"] = true;
                ModelState.AddModelError("School", "School is required");
                return View(studentView);
            }

            // check if user exists
            _logger.LogDebug("Creating user {0}", studentView.EmailAddress);
            try
            {
                var newStdnt = new Student
                {
                    EmailAddress = studentView.EmailAddress,
                    FirstName = studentView.FirstName,
                    LastName = studentView.LastName,
                    PasswordHash = _userService.HashPassword(studentView.Password),
                    School = school,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                newStdnt.GenerateUniqueId(_dbContext);
                _logger.LogDebug("User {0} created", studentView.EmailAddress);
                _logger.LogDebug("User id generated: {0}", newStdnt.Id);
                _logger.LogDebug("saving user {0} to db", studentView.EmailAddress);
                _dbContext.Students ??= _dbContext.Set<Student>();
                _dbContext.Students.Add(newStdnt);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error while creating user {0}: {1}",
                    studentView.EmailAddress,
                    ex.Message
                );
                if (ex.InnerException != null)
                    _logger.LogError("inner exception: {0}", ex.InnerException.Message);
                if (ex.InnerException?.InnerException != null)
                    _logger.LogError(
                        "inner inner exception: {0}",
                        ex.InnerException.InnerException.Message
                    );
                TempData["CantCreate"] = true;
                return RedirectToAction("Create");
            }
            // Succes state remove schools from viewbag
            ViewBag.Schools = null;
            return RedirectToAction("List");
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _dbContext.Students ??= _dbContext.Set<Student>();

            var student = await _dbContext
                .Students.Include(s => s.School)
                .Include(s => s.Payments)
                .Include(s => s.Grades)
                .Include(s => s.ContactInfo)
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound();
            }

            return View(student.ToView());
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        // TODO: implement school admin access
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _dbContext.Students ??= _dbContext.Set<Student>();

            var student = _dbContext.Students.Where(e => e.Id == id).FirstOrDefault();

            if (student == null)
            {
                return NotFound();
            }

            // check if the user is logged in
            // if the user is logged in, prevent deletion

            var loggedInUserId = _userService.GetSignedInUserId();
            Console.WriteLine("Logged in user id: {0}", loggedInUserId);

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
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)
        ]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();
            bool isUserDeleted = await _userService.DeleteUser((int)id);
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

            _dbContext.Students ??= _dbContext.Set<Student>();

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
