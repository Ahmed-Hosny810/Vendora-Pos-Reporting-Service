using MediatR;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;

public class GetSalesDashboardQuery : IRequest<Result<SalesDashboardDto>>
{
    public SalesReportFilter Filter { get; set; } = new();
}

public class GetSalesDashboardQueryHandler : IRequestHandler<GetSalesDashboardQuery, Result<SalesDashboardDto>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;

    public GetSalesDashboardQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<SalesDashboardDto>> Handle(GetSalesDashboardQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<SalesDashboardDto>.Failure("A valid tenant is required.");

        var dashboard = await _repository.GetDashboardAsync(tenantId.Value, request.Filter, cancellationToken);
        return Result<SalesDashboardDto>.Success(dashboard);
    }
}
