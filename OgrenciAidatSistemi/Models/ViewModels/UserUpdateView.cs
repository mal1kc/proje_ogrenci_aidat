using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class UserUpdateView
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public string? PasswordConfirm { get; set; }

        public UserRole Role { get; set; }

        // public ContactInfoView ContactInfo { get; set; } = new();

        // is valid

        public ModelStateDictionary IsValid(ModelStateDictionary modelState)
        {
            if (string.IsNullOrEmpty(Username))
            {
                modelState.AddModelError("Username", "Username is required");
            }

            if (string.IsNullOrEmpty(FirstName))
            {
                modelState.AddModelError("FirstName", "First Name is required");
            }

            if (string.IsNullOrEmpty(LastName))
            {
                modelState.AddModelError("LastName", "Last Name is required");
            }

            if (string.IsNullOrEmpty(Email))
            {
                modelState.AddModelError("Email", "Email is required");
            }

            if (string.IsNullOrEmpty(PhoneNumber))
            {
                modelState.AddModelError("PhoneNumber", "Phone Number is required");
            }

            // if password field entered, password confirm must be entered
            if (!string.IsNullOrEmpty(Password) && string.IsNullOrEmpty(PasswordConfirm))
            {
                modelState.AddModelError("PasswordConfirm", "Password Confirm is required");
            }

            return modelState;
        }

        public static UserUpdateView FromUserView(UserView user)
        {
            return new UserUpdateView
            {
                Id = user.Id,
                Username = user.Role == UserRole.SiteAdmin ? ((SiteAdminView)user).Username : null,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.EmailAddress,
                // if user is school admin or student they have contact info
                PhoneNumber =
                    user.Role == UserRole.SiteAdmin
                        ? null
                        : user.Role == UserRole.SchoolAdmin
                            ? ((SchoolAdminView)user).ContactInfo?.PhoneNumber
                            : ((StudentView)user).ContactInfo?.PhoneNumber,
                Role = user.Role,
            };
        }
    }
}
