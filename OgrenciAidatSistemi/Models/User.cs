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
        [Description(Constants.userRoles.None)]
        None,

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
            return role switch
            {
                UserRole.SiteAdmin => Constants.userRoles.SiteAdmin,
                UserRole.SchoolAdmin => Constants.userRoles.SchoolAdmin,
                UserRole.Student => Constants.userRoles.Student,
                _ => Constants.userRoles.Student,
            };
        }

        public static UserRole GetRoleFromString(string role)
        {
            return role switch
            {
                Constants.userRoles.SiteAdmin => UserRole.SiteAdmin,
                Constants.userRoles.SchoolAdmin => UserRole.SchoolAdmin,
                Constants.userRoles.Student => UserRole.Student,
                _ => UserRole.Student,
            };
        }
    }

#pragma warning disable CS8618 // non-nullable field is uninitialized.

    public abstract class UserView : IBaseDbModelView
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public UserRole Role { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public String Password { get; set; }
        public String? PasswordVerify { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool CheckPasswordsMatch()
        {
            if (Password == PasswordVerify)
                return true;
            return false;
        }

        public UserViewValidationResult ValidateFieldsSignUp(AppDbContext dbctx)
        {
            if (!CheckPasswordsMatch())
                return UserViewValidationResult.PasswordsNotMatch;
            if (!CheckNamesLenght())
                return UserViewValidationResult.InvalidName;
            if (!CheckEmailAddressRegex())
                return UserViewValidationResult.EmailAddressNotMatchRegex;

            if (CheckUserExists(dbctx))
                return UserViewValidationResult.UserExists;

            return UserViewValidationResult.FieldsAreValid;
        }

        public virtual UserViewValidationResult ValidateFieldsSignIn()
        {
            if (!CheckEmailAddressRegex())
                return UserViewValidationResult.EmailAddressNotMatchRegex;
            if (string.IsNullOrEmpty(Password))
                return UserViewValidationResult.PasswordEmpty;
            return UserViewValidationResult.FieldsAreValid;
        }

        public UserViewValidationResult ValidateFieldsCreate(AppDbContext dbctx)
        {
            var baseFieldsValidation = ValidateFieldsSignIn();
            if (baseFieldsValidation != UserViewValidationResult.FieldsAreValid)
                return baseFieldsValidation;
            if (string.IsNullOrEmpty(Password))
                return UserViewValidationResult.PasswordEmpty;
            if (!CheckPasswordsMatch())
                return UserViewValidationResult.PasswordsNotMatch;
            if (!CheckNamesLenght())
                return UserViewValidationResult.InvalidName;
            if (CheckUserExists(dbctx))
                return UserViewValidationResult.UserExists;
            if (CheckEmailAddressExists(dbctx))
                return UserViewValidationResult.EmailAddressExists;

            return UserViewValidationResult.FieldsAreValid;
        }

        public UserViewValidationResult ValidateFieldsUpdate(AppDbContext dbctx)
        {
            throw new System.NotImplementedException();
        }

        public UserViewValidationResult ValidateFieldsDelete(AppDbContext dbctx)
        {
            throw new System.NotImplementedException();
        }

        /* public bool CheckUserName() */
        /* { */
        /*     // XOR */
        /*     return string.IsNullOrEmpty(Username) */
        /*         ^ ( */
        /*             Username.Length < Constants.MaxUserNameLength */
        /*             && Username.Length > Constants.MinUserNameLength */
        /*         ); */
        /* } */

        public bool CheckNamesLenght()
        {
            List<bool> nameTruths = new List<bool>
            {
                FirstName.Length < Constants.MaxUserNameLength,
                /* CheckUserName(), */
            };
            if (LastName != null)
                nameTruths.Add(LastName.Length < Constants.MaxUserNameLength);
            return nameTruths.All((value) => value);
        }

        public abstract bool CheckUserExists(AppDbContext dbctx);
        public abstract bool CheckEmailAddressExists(AppDbContext dbctx);

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
        EmailAddressExists,
        PasswordEmpty
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

        public int Id { get; set; }
        public required string PasswordHash { get; set; }
        public required string EmailAddress { get; set; }
        public UserRole Role { get; protected set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public abstract DateTime CreatedAt { get; set; }
        public abstract DateTime UpdatedAt { get; set; }
    }
}
