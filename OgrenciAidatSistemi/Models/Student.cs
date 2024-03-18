using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models
{
    public class Student : User
    {
        public int StudentId { get; set; }
        public int SchoolId { get; set; }
        public School School { get; set; }
        public int GradLevel { get; set; }
        public bool IsGraduated { get; set; }
        public override DateTime createdAt { get; set; }
        public override DateTime updatedAt { get; set; }
    }

    public class StudentView : UserView
    {
        public int Id { get; set; }
        public SchoolView School { get; set; }
        public int GradLevel { get; set; }
        public bool IsGraduated { get; set; }

        public override bool CheckUsernameExists(AppDbContext dbctx)
        {
            throw new NotImplementedException();
        }
    }
}
