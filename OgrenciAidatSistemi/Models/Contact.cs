using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

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
                defaultSortMethod: s => s.CreatedAt, // Set the default sort method
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

        public ContactInfoView ToView(bool ignoreBidirectNav = false)
        {
            return new ContactInfoView()
            {
                Id = Id,
                PhoneNumber = PhoneNumber,
                Addresses = Addresses,
                Email = Email,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }

        // Rest of the code...
    }
}
