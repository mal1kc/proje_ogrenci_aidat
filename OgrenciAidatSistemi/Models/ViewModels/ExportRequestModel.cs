namespace OgrenciAidatSistemi.Models.ViewModels
{
    public class ExportRequestModel
    {
        public int? MaxCount { get; set; }
        public string? SortOrder { get; set; }
        public bool IncludeRelative { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public ExportRequestModel()
        {
            MaxCount = 100;
            SortOrder = "id_desc";
            IncludeRelative = false;
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}
