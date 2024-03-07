namespace OgrenciAidatSistemi.Models
{
    public enum EnrollmentStatus
    {
        Active,
        Inactive,
        Graduated,
        Dropped
    }

    public class EducationPeriode : IBaseDbModel
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EnrollmentStatus Status { get; set; }
        public ISet<Student> Students { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class EducationPeriodeView
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EnrollmentStatus Status { get; set; }
    }

}
