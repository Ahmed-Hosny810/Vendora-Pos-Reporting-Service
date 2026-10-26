
namespace Pos.ReportingService.Application.Interfaces.Services
{
    public interface ICsvExportService
    {
        int MaxRows { get; }

        byte[] Write(
            IReadOnlyList<string> headers,
            IEnumerable<IReadOnlyList<object?>> rows,
            CancellationToken cancellationToken);
    }
}
