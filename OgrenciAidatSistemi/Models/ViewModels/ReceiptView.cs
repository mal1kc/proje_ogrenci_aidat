using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class ReceiptView : IBaseDbModelView
    {
        public int Id { get; set; }
        public string? Path { get; set; }
        public string? Name { get; set; }
        public string? Extension { get; set; }
        public string? ContentType { get; set; }
        public long Size { get; set; }
        public string? Description { get; set; }
        public User? CreatedBy { get; set; }
        public string? FileHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PaymentView? Payment { get; set; }
        public int? PaymentId { get; set; }
    }
}
