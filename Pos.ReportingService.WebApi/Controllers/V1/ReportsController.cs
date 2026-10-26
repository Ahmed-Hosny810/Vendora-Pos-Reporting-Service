using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetLowStockReportQuery;
using Pos.ReportingService.Application.Features.InventoryReports.DTOs;
using Pos.ReportingService.Application.Features.InventoryReports.Queries.GetCurrentInventoryReportQuery;
using Pos.SalesService.Application.Wrappers;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetShiftReconciliationReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetReturnsRefundsReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetPaymentMethodReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetCashierSalesReportQuery;
using Pos.ReportingService.Application.Features.SalesReports.Queries.GetBranchSalesReportQuery;
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

    [HttpGet("branch-sales")]
    public async Task<ActionResult<PagedResponse<IEnumerable<BranchSalesReportDto>>>> GetBranchSalesReport(
        [FromQuery] GetBranchSalesReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetBranchSalesReportQuery { Parameter = parameter }, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new Response<IEnumerable<BranchSalesReportDto>>(string.Join(", ", result.Errors)) { Errors = result.Errors });
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(new Response<IEnumerable<BranchSalesReportDto>>(exception.Message) { Errors = exception.Errors });
        }
    }
    [HttpGet("cashier-sales")]
    public async Task<ActionResult<PagedResponse<IEnumerable<CashierSalesReportDto>>>> GetCashierSalesReport(
        [FromQuery] GetCashierSalesReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetCashierSalesReportQuery { Parameter = parameter }, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new Response<IEnumerable<CashierSalesReportDto>>(string.Join(", ", result.Errors)) { Errors = result.Errors });
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(new Response<IEnumerable<CashierSalesReportDto>>(exception.Message) { Errors = exception.Errors });
        }
    }
    [HttpGet("payment-methods")]
    public async Task<ActionResult<PagedResponse<IEnumerable<PaymentMethodReportDto>>>> GetPaymentMethodReport(
        [FromQuery] GetPaymentMethodReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetPaymentMethodReportQuery { Parameter = parameter }, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new Response<IEnumerable<PaymentMethodReportDto>>(string.Join(", ", result.Errors)) { Errors = result.Errors });
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(new Response<IEnumerable<PaymentMethodReportDto>>(exception.Message) { Errors = exception.Errors });
        }
    }
    [HttpGet("returns-refunds")]
    public async Task<ActionResult<PagedResponse<IEnumerable<ReturnsRefundsReportDto>>>> GetReturnsRefundsReport(
        [FromQuery] GetReturnsRefundsReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetReturnsRefundsReportQuery { Parameter = parameter }, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new Response<IEnumerable<ReturnsRefundsReportDto>>(string.Join(", ", result.Errors)) { Errors = result.Errors });
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(new Response<IEnumerable<ReturnsRefundsReportDto>>(exception.Message) { Errors = exception.Errors });
        }
    }
    [HttpGet("shift-reconciliation")]
    public async Task<ActionResult<PagedResponse<IEnumerable<ShiftReconciliationReportDto>>>> GetShiftReconciliationReport(
        [FromQuery] GetShiftReconciliationReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetShiftReconciliationReportQuery { Parameter = parameter }, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new Response<IEnumerable<ShiftReconciliationReportDto>>(string.Join(", ", result.Errors)) { Errors = result.Errors });
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(new Response<IEnumerable<ShiftReconciliationReportDto>>(exception.Message) { Errors = exception.Errors });
        }
    }
    [HttpGet("current-inventory")]
    public async Task<ActionResult<PagedResponse<IEnumerable<CurrentInventoryReportDto>>>> GetCurrentInventoryReport(
        [FromQuery] GetCurrentInventoryReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetCurrentInventoryReportQuery { Parameter = parameter }, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new Response<IEnumerable<CurrentInventoryReportDto>>(string.Join(", ", result.Errors)) { Errors = result.Errors });
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(new Response<IEnumerable<CurrentInventoryReportDto>>(exception.Message) { Errors = exception.Errors });
        }
    }
    [HttpGet("low-stock")]
    public async Task<ActionResult<PagedResponse<IEnumerable<LowStockReportDto>>>> GetLowStockReport(
        [FromQuery] GetLowStockReportQueryParameter parameter, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetLowStockReportQuery { Parameter = parameter }, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new Response<IEnumerable<LowStockReportDto>>(string.Join(", ", result.Errors)) { Errors = result.Errors });
        }
        catch (Application.Exceptions.ValidationException exception)
        {
            return BadRequest(new Response<IEnumerable<LowStockReportDto>>(exception.Message) { Errors = exception.Errors });
        }
    }
}
