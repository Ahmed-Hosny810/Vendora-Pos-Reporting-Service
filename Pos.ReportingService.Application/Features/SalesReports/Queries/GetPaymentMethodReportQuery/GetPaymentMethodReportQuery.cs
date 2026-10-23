using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using MediatR;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;

public class GetPaymentMethodReportQuery : IRequest<Result<PagedResponse<IEnumerable<PaymentMethodReportDto>>>>
{
    public GetPaymentMethodReportQueryParameter Parameter { get; set; } = new();
}

public class GetPaymentMethodReportQueryHandler : IRequestHandler<GetPaymentMethodReportQuery, Result<PagedResponse<IEnumerable<PaymentMethodReportDto>>>>
{
    private readonly ISalesReportRepositoryAsync _repository;
    private readonly ICurrentUserService _currentUser;

    public GetPaymentMethodReportQueryHandler(ISalesReportRepositoryAsync repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResponse<IEnumerable<PaymentMethodReportDto>>>> Handle(
        GetPaymentMethodReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result<PagedResponse<IEnumerable<PaymentMethodReportDto>>>.Failure("A valid tenant is required.");

        var report = await _repository.GetPaymentMethodReportAsync(tenantId.Value, request.Parameter, cancellationToken);
        return Result<PagedResponse<IEnumerable<PaymentMethodReportDto>>>.Success(report);
    }
}
