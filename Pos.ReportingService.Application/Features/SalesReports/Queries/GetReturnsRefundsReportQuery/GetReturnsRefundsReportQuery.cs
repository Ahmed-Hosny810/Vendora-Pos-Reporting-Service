using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using MediatR;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;

public class GetReturnsRefundsReportQuery : IRequest<Result<PagedResponse<IEnumerable<ReturnsRefundsReportDto>>>>
{
    public GetReturnsRefundsReportQueryParameter Parameter { get; set; } = new();
}

public class GetReturnsRefundsReportQueryHandler : IRequestHandler<GetReturnsRefundsReportQuery, Result<PagedResponse<IEnumerable<ReturnsRefundsReportDto>>>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;

    public GetReturnsRefundsReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResponse<IEnumerable<ReturnsRefundsReportDto>>>> Handle(
        GetReturnsRefundsReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<PagedResponse<IEnumerable<ReturnsRefundsReportDto>>>.Failure("A valid tenant is required.");

        var report = await _repository.GetReturnsRefundsReportAsync(tenantId.Value, request.Parameter, cancellationToken);
        return Result<PagedResponse<IEnumerable<ReturnsRefundsReportDto>>>.Success(report);
    }
}
