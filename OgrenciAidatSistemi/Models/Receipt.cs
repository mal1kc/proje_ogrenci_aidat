using OgrenciAidatSistemi.Models.Interfaces;
using OgrenciAidatSistemi.Models.ViewModels;

namespace OgrenciAidatSistemi.Models
{
    public class Receipt(
        string path,
        string name,
        string extension,
        string contentType,
        long size,
        string description
    ) : FilePath(path, name, extension, contentType, size, description), ISearchableModel<Receipt>
    {
        public Payment? Payment { get; set; }
        public int? PaymentId { get; set; }

        public static ModelSearchConfig<Receipt> SearchConfig =>
            new(
                defaultSortMethod: s => s.CreatedAt,
                sortingMethods: new()
                {
                    { "Id", static s => s.Id },
                    { "Name", static s => s.Name },
                    { "Extension", static s => s.Extension },
                    { "ContentType", static s => s.ContentType },
                    { "Size", static s => s.Size },
                    { "CreatedAt", static s => s.CreatedAt },
                    { "UpdatedAt", static s => s.UpdatedAt },
                    { "PaymentId", static s => s.PaymentId },
                    { "Payment", static s => s.Payment != null ? s.Payment.Id : 0 }
                },
                searchMethods: new()
                {
                    {
                        "Name",
                        static (s, searchString) =>
                            s.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    // size is equal or greater than the search string
                    {
                        "Size",
                        static (s, searchString) =>
                            s
                                .Size.ToString()
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    // search by year and month not complete date
                    {
                        "CreatedAt",
                        static (s, searchString) =>
                            s
                                .CreatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    {
                        "UpdatedAt",
                        static (s, searchString) =>
                            s
                                .UpdatedAt.ToString("yyyy-MM")
                                .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    },
                    // Extension and ContentType are searched by the same method
                    {
                        "Extension",
                        static (s, searchString) =>
                            s.Extension.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                            || s.ContentType.Contains(
                                searchString,
                                StringComparison.OrdinalIgnoreCase
                            )
                    },
                    {
                        "ContentType",
                        static (s, searchString) =>
                            s.Extension.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                            || s.ContentType.Contains(
                                searchString,
                                StringComparison.OrdinalIgnoreCase
                            )
                    }
                }
            );

        public ReceiptView ToView()
        {
            return new()
            {
                Id = Id,
                Path = Path,
                Name = Name,
                Extension = Extension,
                ContentType = ContentType,
                Size = Size,
                Description = Description,
                CreatedBy = CreatedBy,
                FileHash = FileHash,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Payment = Payment?.ToView(ignoreBidirectNav: true),
                PaymentId = PaymentId
            };
        }
    }
}
