using System.Text.Json;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models.ViewModels
{
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
