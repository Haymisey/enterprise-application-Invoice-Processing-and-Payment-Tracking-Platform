using PaymentTracking.Domain.Aggregates;
using PaymentTracking.Domain.Repositories;
using PaymentTracking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace PaymentTracking.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IPaymentRepository.
/// </summary>
internal sealed class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.InvoiceId == invoiceId, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.VendorId == vendorId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetDuePaymentsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Scheduled && p.ScheduledDate <= today)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetOverduePaymentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Overdue)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AnyAsync(p => p.InvoiceId == invoiceId, cancellationToken);
    }

    public async Task AddAsync(Payment aggregate, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(aggregate, cancellationToken);
        // Save immediately to ensure the entity is persisted to the correct DbContext
        // This is a workaround for the IUnitOfWork registration conflict
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Payment aggregate)
    {
        _context.Payments.Update(aggregate);
    }

    public void Remove(Payment aggregate)
    {
        _context.Payments.Remove(aggregate);
    }
}
