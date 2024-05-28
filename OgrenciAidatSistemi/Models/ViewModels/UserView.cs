using System.Text.RegularExpressions;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public abstract class UserView : IBaseDbModelView
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public UserRole Role { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string Password { get; set; }
        public string? PasswordVerify { get; set; }
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
            throw new NotImplementedException();
        }

        public UserViewValidationResult ValidateFieldsDelete(AppDbContext dbctx)
        {
            throw new NotImplementedException();
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
            List<bool> nameTruths =
            [
                FirstName.Length < Constants.MaxUserNameLength,
                /* CheckUserName(), */
            ];
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
}
