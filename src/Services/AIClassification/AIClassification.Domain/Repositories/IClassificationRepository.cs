using AIClassification.Domain.Aggregates;
using AIClassification.Domain.ValueObjects;

namespace AIClassification.Domain.Repositories;

public interface IClassificationRepository
{
    Task AddAsync(InvoiceClassification classification, CancellationToken cancellationToken = default);
    
    Task<InvoiceClassification?> GetByIdAsync(ClassificationId id, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsWithInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    
    void Update(InvoiceClassification classification);
}