using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public enum UserRole
    {
        [Description(Constants.userRoles.SchoolAdmin)]
        SiteAdmin,

        [Description(Constants.userRoles.SiteAdmin)]
        SchoolAdmin,

        [Description(Constants.userRoles.Student)]
        Student
    }

    public static class UserRoleExtensions
    {
        public static string GetRoleString(this UserRole role)
        {
            if (role == UserRole.SiteAdmin)
                return Constants.userRoles.SiteAdmin;
            if (role == UserRole.SchoolAdmin)
                return Constants.userRoles.SchoolAdmin;
            if (role == UserRole.Student)
                return Constants.userRoles.Student;
            return Constants.userRoles.Student;
        }

        public static UserRole GetRoleFromString(string role)
        {
            if (role == Constants.userRoles.SiteAdmin)
                return UserRole.SiteAdmin;
            if (role == Constants.userRoles.SchoolAdmin)
                return UserRole.SchoolAdmin;
            if (role == Constants.userRoles.Student)
                return UserRole.Student;
            return UserRole.Student;
        }
    }

#pragma warning disable CS8618 // non-nullable field is uninitialized.

    public abstract class UserView : IBaseDbModelView
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public UserRole Role { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public String Password { get; set; }
        public String? PasswordVerify { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool PasswordsMatch()
        {
            if (Password == PasswordVerify)
                return true;
            return false;
        }

        public UserViewValidationResult ValidateFields(AppDbContext dbctx)
        {
            if (!PasswordsMatch())
                return UserViewValidationResult.PasswordsNotMatch;
            if (!CheckNamesLenght())
                return UserViewValidationResult.InvalidName;
            if (!CheckEmailAddressRegex())
                return UserViewValidationResult.EmailAddressNotMatchRegex;

            bool? usernm_exits = CheckUsernameExists(dbctx);
            if (usernm_exits == null)
                return UserViewValidationResult.UserExists;
            else if (usernm_exits == true)
                return UserViewValidationResult.UserExists;

            return UserViewValidationResult.FieldsAreValid;
        }

        public abstract bool? CheckUsernameExists(AppDbContext dbctx);

        public bool CheckNamesLenght()
        {
            List<bool> nameTruths = new List<bool>
            {
                FirstName?.Length < Constants.MaxUserNameLength,
                Username.Length < Constants.MaxUserNameLength
                    && Username.Length > Constants.MinUserNameLength
            };
            if (LastName != null)
                nameTruths.Add(LastName.Length < Constants.MaxUserNameLength);
            return nameTruths.All((value) => value);
        }

        public bool CheckEmailAddressRegex() =>
            Regex.IsMatch(EmailAddress, Constants.EmailRegEx, RegexOptions.IgnoreCase);
    }

    public enum UserViewValidationResult
    {
        FieldsAreValid,
        PasswordsNotMatch,
        InvalidName,
        EmailAddressNotMatchRegex,
        UserExists,
    }

    public abstract class User : IBaseDbModel
    {
        public bool CheckPassword(string password)
        {
            string generatedHash = ComputeHash(password);
            return generatedHash == PasswordHash;
        }

        public static string ComputeHash(string rawData, string salt = Constants.PasswdSalt)
        {
            // Create a SHA256
            // ComputeHash - returns byte array
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{rawData}{salt}"));

            // Convert byte array to a string
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                _ = builder.Append(bytes[i].ToString("x2"));
                // x2 formats to hexadecimal
                // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#the-hexadecimal-x-format-specifier
            }
            return builder.ToString();
        }

        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string EmailAddress { get; set; }
        public UserRole Role { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public abstract DateTime CreatedAt { get; set; }
        public abstract DateTime UpdatedAt { get; set; }
        public int Id
        {
            get => UserId;
            set => UserId = value;
        }
    }
}
