using System.Text.Json;
using System.Text.Json.Serialization;

namespace OgrenciAidatSistemi.Models.Interfaces
{
    public interface IBaseDbModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public static readonly JsonSerializerOptions JsonSerializerOptions =
            new() { ReferenceHandler = ReferenceHandler.Preserve };
    }

    public abstract class BaseDbModel : IBaseDbModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public static readonly JsonSerializerOptions JsonSerializerOptions =
            new() { ReferenceHandler = ReferenceHandler.Preserve };

        public string ToJson() =>
            JsonSerializer.Serialize(this, IBaseDbModel.JsonSerializerOptions);
    }

    public interface IBaseDbModelView : IBaseDbModel { }
}
