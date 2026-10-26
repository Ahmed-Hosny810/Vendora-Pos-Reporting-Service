using Microsoft.Extensions.Options;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.ReportingService.Infrastructure.Persistence.Options;

namespace Pos.ReportingService.Infrastructure.Persistence.Services;

public class ReportingTimeZone
{
    private readonly TimeZoneInfo _timeZone;
    public string SqlTimeZoneId { get; }

    public ReportingTimeZone(IOptions<ReportingOptions> options)
    {
        var id = options.Value.BusinessTimeZoneId;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(id);
        // SQL Server AT TIME ZONE uses Windows timezone identifiers.
        SqlTimeZoneId = TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out var windowsId)
            ? windowsId : id;
    }

    public static bool IsValidId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(id);
            return id == "UTC" || TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out _) ||
                TimeZoneInfo.TryConvertWindowsIdToIanaId(id, out _);
        }
        catch (TimeZoneNotFoundException) { return false; }
        catch (InvalidTimeZoneException) { return false; }
    }

    public SalesReportFilter ToUtcFilter(DailySalesReportFilter filter)
    {
        return new SalesReportFilter
        {
            BranchId = filter.BranchId,
            CurrencyCode = filter.CurrencyCode,
            FromUtc = filter.FromDate.HasValue ? StartOfDayUtc(filter.FromDate.Value) : null,
            ToUtcExclusive = filter.ToDateExclusive.HasValue ? StartOfDayUtc(filter.ToDateExclusive.Value) : null
        };
    }

    public DateTimeOffset StartOfDayUtc(DateOnly date)
    {
        var local = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        // Midnight can be skipped by DST. Find the first valid local minute.
        while (_timeZone.IsInvalidTime(local))
            local = local.AddMinutes(1);

        // For a repeated time, the larger offset gives its first UTC occurrence.
        if (_timeZone.IsAmbiguousTime(local))
            return new DateTimeOffset(local, _timeZone.GetAmbiguousTimeOffsets(local).Max()).ToUniversalTime();

        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, _timeZone));
    }
}
