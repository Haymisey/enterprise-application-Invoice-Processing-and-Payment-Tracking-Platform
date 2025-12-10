using InvoiceManagement.Domain.ValueObjects;
using Shared.Application.Messaging;

namespace InvoiceManagement.Application.Queries.GetInvoices;

/// <summary>
/// Query to get a paginated list of invoices with optional filters.
/// </summary>
public sealed record GetInvoicesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    InvoiceStatus? Status = null,
    Guid? VendorId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IQuery<PagedResult<InvoiceSummaryDto>>;

/// <summary>
/// Paginated result wrapper.
/// </summary>
public sealed record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
