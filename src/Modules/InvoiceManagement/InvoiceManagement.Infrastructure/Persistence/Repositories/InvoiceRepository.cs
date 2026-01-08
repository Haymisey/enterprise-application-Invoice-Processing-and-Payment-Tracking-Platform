using InvoiceManagement.Domain.Aggregates;
using InvoiceManagement.Domain.Repositories;
using InvoiceManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IInvoiceRepository.
/// </summary>
internal sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoiceDbContext _context;

    public InvoiceRepository(InvoiceDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(InvoiceId id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.VendorId == VendorId.Create(vendorId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.Status == InvoiceStatus.Approved && i.Dates.DueDate < today)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .AnyAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task<Invoice?> GetByClassificationIdAsync(Guid classificationId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.ClassificationId == classificationId, cancellationToken);
    }

    public async Task AddAsync(Invoice aggregate, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(aggregate, cancellationToken);
        // Save immediately to ensure the entity is persisted to the correct DbContext
        // This is a workaround for the IUnitOfWork registration conflict
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Invoice aggregate)
    {
        _context.Invoices.Update(aggregate);
    }

    public void Remove(Invoice aggregate)
    {
        _context.Invoices.Remove(aggregate);
    }
}
