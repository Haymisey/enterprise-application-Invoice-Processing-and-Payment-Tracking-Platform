using VendorManagement.Domain.Aggregates;
using VendorManagement.Domain.Repositories;
using VendorManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace VendorManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IVendorRepository.
/// </summary>
internal sealed class VendorRepository : IVendorRepository
{
    private readonly VendorDbContext _context;

    public VendorRepository(VendorDbContext context)
    {
        _context = context;
    }

    public async Task<Vendor?> GetByIdAsync(VendorId id, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Vendor?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .FirstOrDefaultAsync(v => v.TaxId == taxId, cancellationToken);
    }

    public async Task<IReadOnlyList<Vendor>> GetByStatusAsync(VendorStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .Where(v => v.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vendor>> GetActiveVendorsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .Where(v => v.Status == VendorStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .AnyAsync(v => v.TaxId == taxId, cancellationToken);
    }

    public async Task<IReadOnlyList<Vendor>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .Where(v => v.Name.Contains(searchTerm))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Vendor aggregate, CancellationToken cancellationToken = default)
    {
        await _context.Vendors.AddAsync(aggregate, cancellationToken);
        // Save immediately to ensure the entity is persisted to the correct DbContext
        // This is a workaround for the IUnitOfWork registration conflict
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Vendor aggregate)
    {
        _context.Vendors.Update(aggregate);
    }

    public void Remove(Vendor aggregate)
    {
        _context.Vendors.Remove(aggregate);
    }
}
