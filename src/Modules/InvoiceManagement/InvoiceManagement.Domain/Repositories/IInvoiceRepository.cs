using Shared.Domain.Primitives;
using InvoiceManagement.Domain.Aggregates;
using InvoiceManagement.Domain.ValueObjects;

namespace InvoiceManagement.Domain.Repositories;

/// <summary>
/// Repository interface for Invoice aggregate.
/// Following DDD, the repository provides collection-like access to aggregates.
/// </summary>
public interface IInvoiceRepository : IRepository<Invoice, InvoiceId>
{
    /// <summary>
    /// Gets an invoice by its invoice number.
    /// </summary>
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invoices for a specific vendor.
    /// </summary>
    Task<IReadOnlyList<Invoice>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invoices with a specific status.
    /// </summary>
    Task<IReadOnlyList<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all overdue invoices that need to be marked as overdue.
    /// </summary>
    Task<IReadOnlyList<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an invoice number already exists (for duplicate detection).
    /// </summary>
    Task<bool> ExistsWithInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invoice by its associated AI classification ID.
    /// </summary>
    Task<Invoice?> GetByClassificationIdAsync(Guid classificationId, CancellationToken cancellationToken = default);
}
