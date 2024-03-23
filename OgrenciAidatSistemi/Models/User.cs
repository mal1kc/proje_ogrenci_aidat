using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;

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

#pragma warning disable CS8618 // non-nullable field is uninitialized.

    public abstract class UserView
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string EmailAdress { get; set; }
        public UserRole Role { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public String Password { get; set; }
        public String? PasswordVerify { get; set; }

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
            if (!CheckUsernameExists(dbctx))
                return UserViewValidationResult.UserExists;
            return UserViewValidationResult.FieldsAreValid;
        }

        public abstract bool CheckUsernameExists(AppDbContext dbctx);

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
            Regex.IsMatch(EmailAdress, Constants.EmailRegEx, RegexOptions.IgnoreCase);
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
