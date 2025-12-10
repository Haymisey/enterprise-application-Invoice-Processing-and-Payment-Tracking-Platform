using Shared.Domain.Primitives;
using PaymentTracking.Domain.Aggregates;
using PaymentTracking.Domain.ValueObjects;

namespace PaymentTracking.Domain.Repositories;

/// <summary>
/// Repository interface for Payment aggregate.
/// </summary>
public interface IPaymentRepository : IRepository<Payment, PaymentId>
{
    /// <summary>
    /// Gets a payment by invoice ID.
    /// </summary>
    Task<Payment?> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payments for a specific vendor.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all payments with a specific status.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all scheduled payments that are due today or earlier.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetDuePaymentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all overdue payments.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetOverduePaymentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a payment already exists for an invoice.
    /// </summary>
    Task<bool> ExistsForInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
