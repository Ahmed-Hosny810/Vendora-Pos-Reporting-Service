using Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using MediatR;
using Pos.ReportingService.Application.DTOs;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.ExportProductSalesReportQuery;

public class ExportProductSalesReportQuery : IRequest<Result<ReportExportResult>>
{
    public ProductSalesReportFilter Filter { get; set; } = new();
    public ProductSalesOrderKey OrderKey { get; set; } = ProductSalesOrderKey.SalesTotal;
    public bool OrderDescending { get; set; } = true;
}

public class ExportProductSalesReportQueryHandler : IRequestHandler<ExportProductSalesReportQuery, Result<ReportExportResult>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICsvExportService _csv;

    public ExportProductSalesReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser, ICsvExportService csv)
    {
        _repository = repository;
        _currentUser = currentUser;
        _csv = csv;
    }

    public async Task<Result<ReportExportResult>> Handle(ExportProductSalesReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<ReportExportResult>.Failure("A valid tenant is required.");
        var parameter = new GetProductSalesReportQueryParameter
        { Filter = request.Filter, OrderKey = request.OrderKey, OrderDescending = request.OrderDescending };
        var rows = await _repository.GetProductSalesForExportAsync(tenantId.Value, parameter, _csv.MaxRows, cancellationToken);
        if (rows.Count > _csv.MaxRows)
            return Result<ReportExportResult>.Failure($"The export exceeds {_csv.MaxRows} rows. Narrow the filters and try again.");

        string[] headers = { "Product Id", "Product Variant Id", "Product Name", "Variant Name", "Currency Code", "Quantity Sold", "Quantity Returned", "Net Quantity Sold", "Sales Total", "Discount Total", "Sales Tax Total", "Accepted Return Amount", "Sales Total After Returns" };
        var values = rows.Select(row => (IReadOnlyList<object?>)new object?[] { row.ProductId, row.ProductVariantId, row.ProductName, row.VariantName, row.CurrencyCode, row.QuantitySold, row.QuantityReturned, row.NetQuantitySold, row.SalesTotal, row.DiscountTotal, row.SalesTaxTotal, row.AcceptedReturnAmount, row.SalesTotalAfterReturns });
        return Result<ReportExportResult>.Success(new ReportExportResult
        {
            Content = _csv.Write(headers, values, cancellationToken),
            FileName = $"product-sales-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
        });
    }
}
