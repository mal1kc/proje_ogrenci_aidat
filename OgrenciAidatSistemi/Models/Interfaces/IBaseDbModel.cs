namespace OgrenciAidatSistemi.Models.Interfaces
{
    public interface IBaseDbModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public interface IBaseDbModelView : IBaseDbModel { }
}
