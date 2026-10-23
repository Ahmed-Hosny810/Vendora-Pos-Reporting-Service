using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using MediatR;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;

public class GetShiftReconciliationReportQuery : IRequest<Result<PagedResponse<IEnumerable<ShiftReconciliationReportDto>>>>
{
    public GetShiftReconciliationReportQueryParameter Parameter { get; set; } = new();
}

public class GetShiftReconciliationReportQueryHandler : IRequestHandler<GetShiftReconciliationReportQuery, Result<PagedResponse<IEnumerable<ShiftReconciliationReportDto>>>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;

    public GetShiftReconciliationReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResponse<IEnumerable<ShiftReconciliationReportDto>>>> Handle(
        GetShiftReconciliationReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<PagedResponse<IEnumerable<ShiftReconciliationReportDto>>>.Failure("A valid tenant is required.");

        var report = await _repository.GetShiftReconciliationReportAsync(tenantId.Value, request.Parameter, cancellationToken);
        return Result<PagedResponse<IEnumerable<ShiftReconciliationReportDto>>>.Success(report);
    }
}
