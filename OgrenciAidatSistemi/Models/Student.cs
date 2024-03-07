using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models
{
    public class EducationInfo : IBaseDbModel
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public int StudentId { get; set; }
        public string PreviousSchoolName { get; set; }
        public ISet<DateTimeOffset> PreviousYearOfEnrollment { get; set; }
        public bool IsTransferStudent { get; set; }

        public EnrollmentStatus EnrollmentStatus { get; set; }
        public int EnrollmentStatusId { get; set; }
        public DateTime createdAt { get ; set ; }
        public DateTime updatedAt { get ; set ; }
    }

    public class Student : Person
    {
        public School School { get; set; }
        public int SchoolId { get; set; }

        public int SchoolNumber {get;set;}


        public Custodian Custodian { get; set; }
        public int CustodianId { get; set; }

        public PaymentPeriode PaymentPeriode { get; set; }
        public int PaymentPeriodId { get; set; }
        public ContactInfo Contact { get => Custodian.Contact; }

        public ISet<Payment> Payments { get; set; }

        public bool IsTransferStudent { get; set; }
        public EducationInfo EducationInfo { get; set; }

        public EducationPeriode EducationPeriode { get; set; }

    }
    public class StudentView : IBaseDbModelView
    {
        public int Id { get; set; }
        public SchoolView School { get; set; }
        public DateTime createdAt { get ; set ; }
        public DateTime updatedAt { get ; set ; }
    }
}
