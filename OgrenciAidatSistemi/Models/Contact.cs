using System.Text.RegularExpressions;
using OgrenciAidatSistemi.Configurations;
namespace OgrenciAidatSistemi.Models
{
    public class ContactInfo:IBaseDbModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ISet<String> PhoneNumbers { get; set; }
        public ISet<String> Addresses { get; set; }
        public string Email { get; set;     }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        bool validateEmail(string email)
        {
            string regex = Constants.EmailRegEx;
            return Regex.IsMatch(email, regex);
        }

    }


}
