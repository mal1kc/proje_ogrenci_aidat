using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Helpers;
using OgrenciAidatSistemi.Models;
using OgrenciAidatSistemi.Models.ViewModels;
using OgrenciAidatSistemi.Services;

namespace OgrenciAidatSistemi.Controllers
{
    [Authorize(Roles = Configurations.Constants.userRoles.SiteAdmin)]
    public class ExportImportController(
        AppDbContext context,
        ExportService exportService,
        ILogger<ExportImportController> logger
    ) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ExportService _exportService = exportService;
        private readonly ILogger<ExportImportController> _logger = logger;

        public IActionResult ExportSchools(ExportRequestModel model)
        {
            try
            {
                var default_model = new ExportRequestModel();
                model.MaxCount ??= default_model.MaxCount;
                model.SortOrder ??= default_model.SortOrder ?? "id_desc";
                model.StartDate ??= default_model.StartDate;
                model.EndDate ??= default_model.EndDate;

                var maxCount = model.MaxCount ?? 100;
                var startDate = model.StartDate ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
                var endDate = model.EndDate ?? DateOnly.FromDateTime(DateTime.Now);

                var query = _context.Schools.AsQueryable();

                // Filtering last X days


                query = query.Where(s =>
                    s.CreatedAt >= startDate.ToDateTime(new TimeOnly(0, 0, 0))
                );
                query = query.Where(s =>
                    s.CreatedAt <= endDate.ToDateTime(new TimeOnly(23, 59, 59))
                );

                var QMHelper = new QueryableModelHelper<School>(query, School.SearchConfig);

                var data = query.Take(maxCount).ToList();
                var dataTable = ExportService.ToDataTable(data);
                query = QMHelper.Sort(model.SortOrder);
                if (model.IncludeRelative)
                {
                    query.Include(s => s.Students);
                    query.Include(s => s.WorkYears);

                    var relativeDataTables = new Dictionary<string, DataTable>
                    {
                        { "Schools", dataTable },
                        {
                            "Students",
                            ExportService.ToDataTable(
                                query.SelectMany(s => s.Students).ToList() ?? []
                            )
                        },
                        {
                            "WorkYears",
                            ExportService.ToDataTable(
                                query.SelectMany(s => s.WorkYears).ToList() ?? []
                            )
                        }
                    };

                    // Add more related data tables if necessary

                    var zipStream = _exportService.CreateZipWithExcelFiles(relativeDataTables);
                    return File(zipStream, "application/zip", "export.zip");
                }
                else
                {
                    var stream = _exportService.ExportToExcel(dataTable);
                    return File(
                        stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "export.xlsx"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while exporting school data.");
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult ExportStudents(ExportRequestModel model)
        {
            try
            {
                var default_model = new ExportRequestModel();
                model.MaxCount ??= default_model.MaxCount;
                model.SortOrder ??= default_model.SortOrder ?? "id_desc";
                model.StartDate ??= default_model.StartDate;
                model.EndDate ??= default_model.EndDate;

                var maxCount = model.MaxCount ?? 100;
                var startDate = model.StartDate ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
                var endDate = model.EndDate ?? DateOnly.FromDateTime(DateTime.Now);

                var query = _context.Students.AsQueryable();

                query = query.Where(s =>
                    s.CreatedAt >= startDate.ToDateTime(new TimeOnly(0, 0, 0))
                );
                query = query.Where(s =>
                    s.CreatedAt <= endDate.ToDateTime(new TimeOnly(23, 59, 59))
                );

                var QMHelper = new QueryableModelHelper<Student>(query, Student.SearchConfig);

                var data = query.Take(maxCount).ToList();
                var dataTable = ExportService.ToDataTable(data);
                query = QMHelper.Sort(model.SortOrder);
                if (model.IncludeRelative)
                {
                    query.Include(s => s.School);
                    query.Include(s => s.Payments);

                    var relativeDataTables = new Dictionary<string, DataTable>
                    {
                        { "Students", dataTable },
                        {
                            "Schools",
                            ExportService.ToDataTable(query.Select(s => s.School).ToList() ?? [])
                        },
                        {
                            "Payments",
                            ExportService.ToDataTable(
                                query.SelectMany(s => s.Payments).ToList() ?? []
                            )
                        }
                    };

                    // Add more related data tables if necessary

                    var zipStream = _exportService.CreateZipWithExcelFiles(relativeDataTables);
                    return File(zipStream, "application/zip", "export.zip");
                }
                else
                {
                    var stream = _exportService.ExportToExcel(dataTable);
                    return File(
                        stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "export.xlsx"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while exporting student data.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
