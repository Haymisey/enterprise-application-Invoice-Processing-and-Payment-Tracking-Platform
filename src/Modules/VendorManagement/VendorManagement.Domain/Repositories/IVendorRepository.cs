using Shared.Domain.Primitives;
using VendorManagement.Domain.Aggregates;
using VendorManagement.Domain.ValueObjects;

namespace VendorManagement.Domain.Repositories;

/// <summary>
/// Repository interface for Vendor aggregate.
/// </summary>
public interface IVendorRepository : IRepository<Vendor, VendorId>
{
    /// <summary>
    /// Gets a vendor by tax ID.
    /// </summary>
    Task<Vendor?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all vendors with a specific status.
    /// </summary>
    Task<IReadOnlyList<Vendor>> GetByStatusAsync(VendorStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active vendors.
    /// </summary>
    Task<IReadOnlyList<Vendor>> GetActiveVendorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a vendor exists with the given tax ID.
    /// </summary>
    Task<bool> ExistsWithTaxIdAsync(string taxId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches vendors by name.
    /// </summary>
    Task<IReadOnlyList<Vendor>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
}
