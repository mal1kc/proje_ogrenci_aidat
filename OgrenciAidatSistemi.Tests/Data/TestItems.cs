using Microsoft.EntityFrameworkCore;

namespace OgrenciAidatSistemi.Tests.Data
{
    public class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateOnly CreatedDate { get; set; }

        public decimal Amount { get; set; }

        public TestSecondItem? TestSeconds { get; set; }
    }

    public class TestSecondItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public ICollection<TestItem>? TestItems { get; set; }
    }
}
