using AIClassification.Domain.Aggregates;
using AIClassification.Domain.Repositories;
using AIClassification.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using AIClassification.Infrastructure.Persistence;

namespace AIClassification.Infrastructure.Persistence.Repositories;

public sealed class ClassificationRepository : IClassificationRepository
{
    private readonly ClassificationDbContext _context;

    public ClassificationRepository(ClassificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(InvoiceClassification classification, CancellationToken cancellationToken = default)
    {
        await _context.InvoiceClassifications.AddAsync(classification, cancellationToken);
    }

    public async Task<InvoiceClassification?> GetByIdAsync(ClassificationId id, CancellationToken cancellationToken = default)
    {
        return await _context.InvoiceClassifications
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.InvoiceClassifications
            .AnyAsync(c => c.ExtractedData != null && c.ExtractedData.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public void Update(InvoiceClassification classification)
    {
        _context.InvoiceClassifications.Update(classification);
    }
}