
namespace Pos.ReportingService.Application.DTOs
{
    public class ReportExportResult
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = "text/csv; charset=utf-8";
    }
}
