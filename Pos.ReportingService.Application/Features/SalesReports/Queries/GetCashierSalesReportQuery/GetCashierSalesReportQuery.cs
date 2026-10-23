using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using MediatR;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;

public class GetCashierSalesReportQuery : IRequest<Result<PagedResponse<IEnumerable<CashierSalesReportDto>>>>
{
    public GetCashierSalesReportQueryParameter Parameter { get; set; } = new();
}

public class GetCashierSalesReportQueryHandler : IRequestHandler<GetCashierSalesReportQuery, Result<PagedResponse<IEnumerable<CashierSalesReportDto>>>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;

    public GetCashierSalesReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResponse<IEnumerable<CashierSalesReportDto>>>> Handle(
        GetCashierSalesReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<PagedResponse<IEnumerable<CashierSalesReportDto>>>.Failure("A valid tenant is required.");

        var report = await _repository.GetCashierSalesReportAsync(tenantId.Value, request.Parameter, cancellationToken);
        return Result<PagedResponse<IEnumerable<CashierSalesReportDto>>>.Success(report);
    }
}
