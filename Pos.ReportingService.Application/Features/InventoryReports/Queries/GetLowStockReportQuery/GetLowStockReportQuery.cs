using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using MediatR;
using Pos.ReportingService.Application.Features.InventoryReports.DTOs;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;

public class GetLowStockReportQuery : IRequest<Result<PagedResponse<IEnumerable<LowStockReportDto>>>>
{
    public GetLowStockReportQueryParameter Parameter { get; set; } = new();
}

public class GetLowStockReportQueryHandler : IRequestHandler<GetLowStockReportQuery, Result<PagedResponse<IEnumerable<LowStockReportDto>>>>
{
    private readonly IInventoryReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;

    public GetLowStockReportQueryHandler(IInventoryReportRepositoryAsync repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResponse<IEnumerable<LowStockReportDto>>>> Handle(
        GetLowStockReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<PagedResponse<IEnumerable<LowStockReportDto>>>.Failure("A valid tenant is required.");

        var report = await _repository.GetLowStockReportAsync(tenantId.Value, request.Parameter, cancellationToken);
        return Result<PagedResponse<IEnumerable<LowStockReportDto>>>.Success(report);
    }
}
