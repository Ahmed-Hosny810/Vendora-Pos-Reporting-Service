
namespace Pos.ReportingService.Infrastructure.Shared.Options
{
    public class CsvExportOptions
    {
        public int MaxRows { get; set; } = 10000;

        public string Delimiter { get; set; } = ",";

        public bool IncludeUtf8Bom { get; set; } = true;
    }
}
