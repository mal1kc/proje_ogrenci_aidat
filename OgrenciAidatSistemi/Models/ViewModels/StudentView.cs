using OgrenciAidatSistemi.Data;

namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class StudentView : UserView
    {
        public int SchoolId { get; set; }
        public SchoolView? School { get; set; }
        public string StudentId { get; set; }

        public ContactInfoView ContactInfo { get; set; }

        public int GradLevel { get; set; }
        public bool IsLeftSchool { get; set; }

        public ICollection<PaymentView>? Payments { get; set; }
        public ICollection<Grade>? Grades { get; set; }

        public ICollection<PaymentPeriodView>? PaymentPeriods { get; set; }

        public ICollection<PaymentView>? UnPaidPayments
        {
            get { return Payments?.Where(p => p.PaymentMethod == PaymentMethod.UnPaid).ToList(); }
        }

        public PaymentPeriodView? CurrentPaymentPeriod
        {
            get
            {
                return PaymentPeriods
                    ?.OrderByDescending(pp => pp.EndDate)
                    .Where(pp => !pp.IsExhausted)
                    .FirstOrDefault();
            }
        }

        public override bool CheckUserExists(AppDbContext dbctx)
        {
            if (dbctx.Students.Where(s => s.StudentId == this.StudentId).FirstOrDefault() != null)
            {
                return true;
            }

            return false;
        }

        public override bool CheckEmailAddressExists(AppDbContext dbctx)
        {
            if (
                dbctx.Users.Where(u => u.EmailAddress == this.EmailAddress).FirstOrDefault() != null
            )
            {
                return true;
            }
            return false;
        }

        public override UserViewValidationResult ValidateFieldsSignIn()
        {
            if (!IsStudentIdValid(StudentId))
                return UserViewValidationResult.InvalidName;
            if (string.IsNullOrEmpty(Password))
                return UserViewValidationResult.PasswordEmpty;
            return UserViewValidationResult.FieldsAreValid;
        }

        public static bool IsStudentIdValid(string studentId)
        {
            if (studentId.Length != 10)
                return false;
            if (!studentId.Substring(5, 2).All(char.IsDigit))
                return false;
            if (!studentId.Substring(7, 3).All(char.IsDigit))
                return false;
            return true;
        }
    }
}
