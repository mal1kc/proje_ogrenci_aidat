using OgrenciAidatSistemi.Models.Interfaces;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class SchoolView : IBaseDbModelView
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ISet<SchoolAdminView>? SchoolAdmins { get; set; }

        public ISet<StudentView>? Students { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public School ToModel(bool ignoreBidirectNav = false)
        {
            // TODO: Implement ToModel to all view models for more manageable code
            throw new NotImplementedException();
        }
    }
}
