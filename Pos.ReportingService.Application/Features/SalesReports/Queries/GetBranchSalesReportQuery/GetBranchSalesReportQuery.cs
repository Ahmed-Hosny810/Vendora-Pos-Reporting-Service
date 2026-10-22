using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using MediatR;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery;

public class GetBranchSalesReportQuery : IRequest<Result<PagedResponse<IEnumerable<BranchSalesReportDto>>>>
{
    public GetBranchSalesReportQueryParameter Parameter { get; set; } = new();
}

public class GetBranchSalesReportQueryHandler : IRequestHandler<GetBranchSalesReportQuery, Result<PagedResponse<IEnumerable<BranchSalesReportDto>>>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;

    public GetBranchSalesReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResponse<IEnumerable<BranchSalesReportDto>>>> Handle(
        GetBranchSalesReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<PagedResponse<IEnumerable<BranchSalesReportDto>>>.Failure("A valid tenant is required.");

        var report = await _repository.GetBranchSalesReportAsync(tenantId.Value, request.Parameter, cancellationToken);
        return Result<PagedResponse<IEnumerable<BranchSalesReportDto>>>.Success(report);
    }
}
