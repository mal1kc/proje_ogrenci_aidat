using System.Collections;
using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class SchoolView : IBaseDbModelView
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<SchoolAdminView>? SchoolAdmins { get; set; }

        public ICollection<StudentView>? Students { get; set; }

        public ICollection<PaymentView>? Payments { get; set; }

        public ICollection<WorkYearView>? WorkYears { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
