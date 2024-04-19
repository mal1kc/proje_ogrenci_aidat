using System.Text.RegularExpressions;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models
{
    public class ContactInfo : IBaseDbModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required ISet<String> PhoneNumbers { get; set; }
        public required ISet<String> Addresses { get; set; }
        public required string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        bool validateEmail(string email)
        {
            string regex = Constants.EmailRegEx;
            return Regex.IsMatch(email, regex);
        }
    }
}
