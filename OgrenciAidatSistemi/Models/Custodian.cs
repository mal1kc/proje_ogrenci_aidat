namespace OgrenciAidatSistemi.Models{
    public class Custodian:Person{
        public ContactInfo Contact { get; set; }
        public ISet<Student> Students { get; set; }
    }
}
