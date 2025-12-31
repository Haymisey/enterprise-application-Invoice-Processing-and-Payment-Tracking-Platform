using VendorManagement.Domain.ValueObjects;
using Shared.Application.Messaging;

namespace VendorManagement.Application.Queries.GetVendors;

/// <summary>
/// Query to get a list of vendors with optional filters.
/// </summary>
public sealed record GetVendorsQuery(
    VendorStatus? Status = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedVendorResult>;

public sealed record PagedVendorResult(
    List<VendorSummaryDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
