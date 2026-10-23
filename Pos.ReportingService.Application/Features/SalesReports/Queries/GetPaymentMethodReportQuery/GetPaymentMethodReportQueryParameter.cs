using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.SalesService.Application.Parameters;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;

public class GetPaymentMethodReportQueryParameter : RequestParameter<PaymentMethodOrderKey>
{
    public GetPaymentMethodReportQueryParameter()
    {
        OrderKey = PaymentMethodOrderKey.CollectedAmount;
        OrderDescending = true;
    }
    public SalesReportFilter Filter { get; set; } = new();
}

public enum PaymentMethodOrderKey
{
    CompletedPaymentCount,
    CollectedAmount,
    NetCollectedAmount,
    RefundAmount
}
