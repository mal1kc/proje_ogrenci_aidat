using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class UserService(
        ILogger<UserService> logger,
        AppDbContext dbContext,
        IHttpContextAccessor httpContextAccessor
    )
    {
        private readonly AppDbContext _dbContext = dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<UserService> _logger = logger;

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public async Task<User?> GetUserByIdaAsync(int userId)
        {
            if (userId == 0)
                return null;
            if (_dbContext.Users == null)
                return null;
            return await _dbContext.Users.FindAsync(userId);
        }

        public User? GetUserById(int userId)
        {
            if (userId == 0)
                return null;
            if (_dbContext.Users == null)
                return null;
            return _dbContext.Users.Find(userId);
        }

        public async Task<SiteAdmin?> GetSAdminByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;
            if (_dbContext.Users == null)
                return null;
            return await _dbContext.SiteAdmins.FirstOrDefaultAsync(u =>
                u.Username == username && u.Role == UserRole.SiteAdmin
            );
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;
            if (_dbContext.Users == null)
                return null;
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
        }

        public async Task<bool> CreateUser(User user)
        {
            try
            {
                // You may want to hash the password before storing it
                user.PasswordHash = User.ComputeHash(user.PasswordHash);
                user.CreatedAt = DateTime.UtcNow;

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception

                _logger.LogError("Error while creating user: {0}", ex.Message);
                return false;
            }
        }

        public string HashPassword(string password)
        {
            return User.ComputeHash(password);
        }

        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                // Update the user's updatedAt timestamp
                user.UpdatedAt = DateTime.UtcNow;
                if (_dbContext.Users == null)
                    return false;

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating user: {0}", ex.Message);
                return false;
            }
        }

        // you may want to edit subclases of User you can use the following method and give your custom static lambda expression as a parameter
        // do update the user's updatedAt timestamp

        public async Task<bool> UpdateUser(User user, Func<User, User> updateExpression)
        {
            try
            {
                user = updateExpression(user);
                user.UpdatedAt = DateTime.UtcNow;
                if (_dbContext.Users == null)
                    return false;

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating user: {0}", ex.Message);
                // Log or handle the exception
                return false;
            }
        }

        public async Task<bool> DeleteUser(int userId)
        {
            try
            {
                if (_dbContext.Users == null)
                    return false;
                var user = await _dbContext.Users.FindAsync(userId);

                // check if the user is signed in if so, don't delete the user and return false
                if (user != null)
                {
                    if (HttpContext != null)
                    {
                        var currentUserId = HttpContext
                            .User.FindFirst(ClaimTypes.NameIdentifier)
                            ?.Value;
                        if (currentUserId != null && int.Parse(currentUserId) == userId)
                            return false;
                    }
                    _dbContext.Users.Remove(user);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting user: {0}", ex.Message);
                return false;
            }
        }

        // abstracted method to use with the SignIn method of subclass controllers
        // you can use this method after checking the user's credentials

        // <summary>
        // Sign in the user and create a cookie for the user (use after checking the user's credentials)
        // </summary>


        public async Task<bool> SignInUserAsync(User user)
        {
            return await SignInUser(user, user.Role);
        }

        public async Task<bool> SignInUser(User user, UserRole role)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.EmailAddress),
                new(ClaimTypes.Role, UserRoleExtensions.GetRoleString(role)),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };
            switch (user.Role)
            {
                case UserRole.Student:
                    // check School then add the claim as school.id to GroupSid
                    var student = await _dbContext
                        .Students.Where(s => s.Id == user.Id)
                        .Include(s => s.School)
                        .FirstOrDefaultAsync();
                    ;
                    if (student != null)
                    {
                        claims.Add(new Claim(ClaimTypes.GroupSid, student.School.Id.ToString()));
                    }
                    else
                    {
                        _logger.LogWarning("Student not found");
                        return false;
                    }
                    break;
                case UserRole.SchoolAdmin:
                    //  check School then add the claim as school.id to GroupSid
                    var schoolAdmin = await _dbContext
                        .SchoolAdmins.Where(s => s.Id == user.Id)
                        .Include(s => s.School)
                        .FirstOrDefaultAsync();
                    if (schoolAdmin != null)
                    {
                        claims.Add(
                            new Claim(ClaimTypes.GroupSid, schoolAdmin.School.Id.ToString())
                        );
                    }
                    else
                    {
                        _logger.LogWarning("SchoolAdmin not found");
                        return false;
                    }
                    break;
                default:
                    break;
            }

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            if (HttpContext == null)
                return false;
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );
            return true;
        }

        public async Task SignOutUser()
        {
            if (HttpContext == null)
                return;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            if (HttpContext == null)
                return null;
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return null;
            return await GetUserByIdaAsync(int.Parse(userId));
        }

        public int? GetCurrentUserID()
        {
            if (HttpContext == null)
                return null;
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return null;
            return int.Parse(userId);
        }

        public async Task<(UserRole? Role, int? SchoolId)> GetUserRoleAndSchoolId()
        {
            var user = await GetCurrentUserAsync();
            if (user == null || HttpContext == null || HttpContext.User == null)
            {
                _logger.LogWarning("User is not signed in");
                return (null, null);
            }

            UserRole? role = null;
            if (HttpContext.User.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                var roleValue = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (roleValue != null)
                {
                    role = UserRoleExtensions.GetRoleFromString(roleValue);
                }
            }

            int? schoolId = null;
            if (HttpContext.User.HasClaim(c => c.Type == ClaimTypes.GroupSid))
            {
                var schoolIdValue = HttpContext.User.FindFirst(ClaimTypes.GroupSid)?.Value;
                if (schoolIdValue != null)
                {
                    schoolId = int.Parse(schoolIdValue);
                }
            }

            return (role, schoolId);
        }

        public async Task<bool> IsUserSignedIn()
        {
            if (HttpContext == null)
                return false;
            if (HttpContext.User.Identity == null)
                return false;

            var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (_dbContext.Users == null && HttpContext.User.Identity.IsAuthenticated)
                await SignOutUser();
            if (currentUserId == null)
                return false;
            if (await GetUserByIdaAsync(int.Parse(currentUserId)) == null)
                await SignOutUser();

            return HttpContext.User.Identity.IsAuthenticated;
        }

        public int GetSignedInUserId()
        {
            if (HttpContext == null)
                return 0;
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return 0;
            return int.Parse(userId);
        }
    }
}
