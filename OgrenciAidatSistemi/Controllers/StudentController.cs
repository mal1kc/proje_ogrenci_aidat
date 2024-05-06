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

        public IActionResult SignIn()
        {
            if (_userService.IsUserSignedIn())
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

            if (_dbContext.Students == null)
                _dbContext.Students = _dbContext.Set<Student>();

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
                await _userService.SignInUser(dbStudent, UserRole.Student);
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
            if (_dbContext.Students == null)
            {
                _logger.LogError("Students table is null");
                _dbContext.Students = _dbContext.Set<Student>();
            }

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
        public async Task<IActionResult> Create(
            [Bind(
                "EmailAddress",
                "Password",
                "PasswordVerify",
                "SchoolId",
                "FirstName",
                "LastName"
            )]
                StudentView studentView
        )
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
                if (_dbContext.Students == null)
                {
                    _dbContext.Students = _dbContext.Set<Student>();
                }
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
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (_dbContext.Students == null)
            {
                _dbContext.Students = _dbContext.Set<Student>();
            }

            var siteAdmin = _dbContext.Students.Where(e => e.Id == id).FirstOrDefault();

            if (siteAdmin == null)
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

            return View(siteAdmin);
        }

        [
            HttpPost,
            ActionName("DeleteConfirmed"),
            ValidateAntiForgeryToken,
            Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)
        ]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            bool isUserDeleted = await _userService.DeleteUser((int)id);
            if (!isUserDeleted)
                ViewData["Message"] = "Error while deleting user";
            return RedirectToAction("List");
        }

        [Authorize]
        public async Task<PartialViewResult> SchoolViewPartial(int? id)
        {
            if (_dbContext.Schools == null)
                _dbContext.Schools = _dbContext.Set<School>();
            if (id == null)
            {
                return PartialView("_SchoolViewPartial", null);
            }
            var school = await _dbContext.Schools.FindAsync(id);
            if (school == null)
            {
                return PartialView("_SchoolViewPartial", null);
            }
            var schView = school.ToView();
            return PartialView("_SchoolViewPartial", schView);
        }

        public async Task<Student?> GetLoggedInStudent()
        {
            var student = await _userService.GetCurrentUser();
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

        // <partial name="_PaymentHistoryPartial"/>

        [Authorize(Roles = Configurations.Constants.userRoles.Student)]
        public async Task<IActionResult> PaymentHistoryPartial()
        {
            var student = await GetLoggedInStudent();

            var payments = new QueryableModelHelper<Payment>(
                _dbContext.Payments.Where(p => p.Student == student).AsQueryable(),
                Payment.SearchConfig
            )
                .Sort("PaymentDate", SortOrderEnum.ASC)
                .Take(5)
                .ToHashSet();

            return PaymentHistoryPartial(payments);
        }

        // <partial name="_PaymentHistoryPartial" model="Model.Payments" />

        [Authorize(Roles = Configurations.Constants.userRoles.Student)]
        public IActionResult PaymentHistoryPartial(HashSet<Payment> model)
        {
            return PartialView("_PaymentHistoryPartial", model);
        }

        // <partial name="_GradesPartial"/>

        [Authorize(Roles = Configurations.Constants.userRoles.Student)]
        public async Task<IActionResult> GradesPartial()
        {
            var student = await GetLoggedInStudent();
            // student have many to many relation with grades
            var grades = new QueryableModelHelper<Grade>(
                _dbContext.Grades.Where(g => g.Students.Contains(student)).AsQueryable(),
                Grade.SearchConfig
            )
                .Sort("GradeDate", SortOrderEnum.ASC)
                .Take(5)
                .ToHashSet();
            return GradesPartial(grades);
        }

        // <partial name="_GradesPartial" model="Model.Grades" />

        [Authorize(Roles = Configurations.Constants.userRoles.Student)]
        public IActionResult GradesPartial(HashSet<Grade> model)
        {
            return PartialView("_GradesPartial", model);
        }

        [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
        public IActionResult Edit(int? id)
        {
            _logger.LogDebug("not tested function StudentController.Edit called");
            if (id == null)
            {
                return NotFound();
            }

            if (_dbContext.Students == null)
            {
                _dbContext.Students = _dbContext.Set<Student>();
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
