using MediatR;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.ReportingService.Application.Interfaces.Repositories;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.Application.Features.SalesReports.Queries.GetDailySalesQuery
{
    public class GetDailySalesReportQuery:IRequest<Result<IReadOnlyList<DailySalesReportDto>>>
    {
        public DailySalesReportFilter Filter { get; set; } = new();
    }
    public class GetDailySalesReportQueryHandler : IRequestHandler<GetDailySalesReportQuery, Result<IReadOnlyList<DailySalesReportDto>>>
    {
        private readonly ISalesReportRepositoryAsync _salesReportRepository;
        private readonly ICurrentUserService _currentUser;

        public GetDailySalesReportQueryHandler(
            ISalesReportRepositoryAsync salesReportRepository,
            ICurrentUserService currentUser)
        {
            _salesReportRepository = salesReportRepository;
            _currentUser = currentUser;
        }
        public async Task<Result<IReadOnlyList<DailySalesReportDto>>> Handle(GetDailySalesReportQuery request, CancellationToken cancellationToken)
        {
             var tenantId = _currentUser.TenantId;

            if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
                return Result<IReadOnlyList<DailySalesReportDto>>.Failure(
                    "A valid tenant is required.");

            var report = await _salesReportRepository.GetDailySalesReportAsync(
                tenantId.Value,
                request.Filter,
                cancellationToken);

            return Result<IReadOnlyList<DailySalesReportDto>>.Success(report);
        }
    }
}
