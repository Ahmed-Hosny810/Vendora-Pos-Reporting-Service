using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using MediatR;
using Pos.ReportingService.Application.Features.InventoryReports.DTOs;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;

public class GetCurrentInventoryReportQuery : IRequest<Result<PagedResponse<IEnumerable<CurrentInventoryReportDto>>>>
{
    public GetCurrentInventoryReportQueryParameter Parameter { get; set; } = new();
}

public class GetCurrentInventoryReportQueryHandler : IRequestHandler<GetCurrentInventoryReportQuery, Result<PagedResponse<IEnumerable<CurrentInventoryReportDto>>>>
{
    private readonly IInventoryReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentInventoryReportQueryHandler(IInventoryReportRepositoryAsync repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResponse<IEnumerable<CurrentInventoryReportDto>>>> Handle(
        GetCurrentInventoryReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<PagedResponse<IEnumerable<CurrentInventoryReportDto>>>.Failure("A valid tenant is required.");

        var report = await _repository.GetCurrentInventoryReportAsync(tenantId.Value, request.Parameter, cancellationToken);
        return Result<PagedResponse<IEnumerable<CurrentInventoryReportDto>>>.Success(report);
    }
}
