using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.ReportingService.Application.Features.SalesReports.DTOs;
using Pos.ReportingService.Application.Features.SalesReports.Parameters;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetDailySalesQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetProductSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetSalesDashboardQuery;
using Pos.ReportingService.WebApi.Policies;
using Pos.SalesService.Application.Wrappers;

namespace Pos.ReportingService.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = AppPolicies.ViewReports)]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportsController(IMediator mediator)
    {
        _mediator = mediator; 
    }

    [HttpGet("sales-dashboard")]
    public async Task<ActionResult<Response<SalesDashboardDto>>> GetSalesDashboard(
        [FromQuery] SalesReportFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetSalesDashboardQuery { Filter = filter }, cancellationToken);
            return result.IsSuccess
                ? Ok(new Response<SalesDashboardDto>(result.Value!))
                : BadRequest(new Response<SalesDashboardDto>(string.Join(", ", result.Errors)) { Errors = result.Errors });
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(new Response<SalesDashboardDto>(exception.Message) { Errors = exception.Errors });
        }
    }
    [HttpGet("daily-sales")]
    public async Task<ActionResult<Response<IReadOnlyList<DailySalesReportDto>>>> GetDailySalesReport(
        [FromQuery] SalesReportFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetDailySalesReportQuery { Filter = filter }, cancellationToken);
            return result.IsSuccess
                ? Ok(new Response<IReadOnlyList<DailySalesReportDto>>(result.Value!))
                : BadRequest(new Response<IReadOnlyList<DailySalesReportDto>>(string.Join(", ", result.Errors)) { Errors = result.Errors });
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(new Response<IReadOnlyList<DailySalesReportDto>>(exception.Message) { Errors = exception.Errors });
        }
    }

    [HttpGet("product-sales")]
    public async Task<ActionResult<PagedResponse<IEnumerable<ProductSalesReportDto>>>>
    GetProductSalesReport(
        [FromQuery] GetProductSalesReportQueryParameter parameter,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetProductSalesReportQuery{ Parameter = parameter}, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(
                    new Response<IEnumerable<ProductSalesReportDto>>(
                        string.Join(", ", result.Errors))
                    {
                        Errors = result.Errors
                    });
            }

            return Ok(result.Value);
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(
                new Response<IEnumerable<ProductSalesReportDto>>(exception.Message)
                {
                    Errors = exception.Errors
                });
        }
    }
}
