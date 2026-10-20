using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using MediatR;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;

public class GetProductSalesReportQuery : IRequest<Result<PagedResponse<IEnumerable<ProductSalesReportDto>>>>
{
    public GetProductSalesReportQueryParameter Parameter { get; set; } = new();
}

public class GetProductSalesReportQueryHandler : IRequestHandler<GetProductSalesReportQuery, Result<PagedResponse<IEnumerable<ProductSalesReportDto>>>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;

    public GetProductSalesReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResponse<IEnumerable<ProductSalesReportDto>>>> Handle(
        GetProductSalesReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<PagedResponse<IEnumerable<ProductSalesReportDto>>>.Failure("A valid tenant is required.");

        var report = await _repository.GetProductSalesReportAsync(tenantId.Value, request.Parameter, cancellationToken);
        return Result<PagedResponse<IEnumerable<ProductSalesReportDto>>>.Success(report);
    }
}
