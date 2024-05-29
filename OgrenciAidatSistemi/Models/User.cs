using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

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

    public abstract class User : BaseDbModel
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

        public required string PasswordHash { get; set; }
        public required string EmailAddress { get; set; }
        public UserRole Role { get; protected set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }

        public abstract UserView ToView(bool ignoreBidirectNav = false);
    }
}
