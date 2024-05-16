using System.Text.RegularExpressions;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class ContactInfo : IBaseDbModel, ISearchableModel
    {
        public int Id { get; set; }

        public User? User { get; set; }
        public String? PhoneNumber { get; set; }
        public IList<String>? Addresses { get; set; }
        public required string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public static ModelSearchConfig SearchConfig =>
            new ModelSearchConfig(
                ContactSearchConfig.AllowedFieldsForSearch,
                ContactSearchConfig.AllowedFieldsForSort
            );

        bool validateEmail(string email)
        {
            string regex = Constants.EmailRegEx;
            return Regex.IsMatch(email, regex);
        }

        bool validatePhoneNumber(string phoneNumber)
        {
            string regex = Constants.PhoneNumberRegEx;
            return Regex.IsMatch(phoneNumber, regex);
        }

        public ContactInfoView ToView(bool ignoreBidirectNav = false)
        {
            return new ContactInfoView
            {
                Id = Id,
                PhoneNumber = PhoneNumber,
                Addresses = Addresses,
                Email = Email,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }
    }

    public class ContactInfoView : IBaseDbModelView
    {
        public int Id { get; set; }
        public String? PhoneNumber { get; set; }
        public IList<String>? Addresses { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ContactSearchConfig
    {
        public static readonly string[] AllowedFieldsForSearch = new string[]
        {
            "Name",
            "PhoneNumber",
            "Addresses",
            "Email"
        };
        public static readonly string[] AllowedFieldsForSort = new string[] { "Name", "Email" };
    }
}
