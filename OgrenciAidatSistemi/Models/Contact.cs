using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class ContactInfo : BaseDbModel, ISearchableModel<ContactInfo>
    {
        public User? User { get; set; }
        public String? PhoneNumber { get; set; }
        public IList<String>? Addresses { get; set; }
        public required string Email { get; set; }

        public static ModelSearchConfig<ContactInfo> SearchConfig =>
            new(
                sortingMethods: new()
                {
                    { "PhoneNumber", static s => s.PhoneNumber ?? "" },
                    { "Email", static s => s.Email },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt }
                },
                searchMethods: new()
                {
                    {
                        "PhoneNumber",
                        static (s, searchString) =>
                            s.PhoneNumber != null
                            && s.PhoneNumber.Contains(
                                searchString,
                                StringComparison.OrdinalIgnoreCase
                            )
                    },
                    {
                        "Email",
                        static (s, searchString) =>
                            s.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    // search by year and month not complete date
                    {
                        "CreatedAt",
                        static (s, searchString) =>
                            s
                                .CreatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "UpdatedAt",
                        static (s, searchString) =>
                            s
                                .UpdatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    }
                }
            );

        static bool ValidateEmail(string email)
        {
            string regex = Constants.EmailRegEx;
            return Regex.IsMatch(email, regex);
        }

        static bool ValidatePhoneNumber(string phoneNumber)
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
        public string? PhoneNumber { get; set; }
        public IList<string>? Addresses { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string ToJson() =>
            JsonSerializer.Serialize(this, IBaseDbModel.JsonSerializerOptions);
    }
}
