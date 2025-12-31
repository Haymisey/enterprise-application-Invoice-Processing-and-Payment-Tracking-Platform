using VendorManagement.Domain.Repositories;
using VendorManagement.Domain.ValueObjects;
using Shared.Application.Messaging;

namespace VendorManagement.Application.Queries.GetVendorById;

/// <summary>
/// Handler for GetVendorByIdQuery.
/// </summary>
internal sealed class GetVendorByIdQueryHandler : IQueryHandler<GetVendorByIdQuery, VendorDto?>
{
    private readonly IVendorRepository _vendorRepository;

    public GetVendorByIdQueryHandler(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<VendorDto?> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var vendorId = VendorId.Create(request.VendorId);
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);

        if (vendor is null)
        {
            return null;
        }

        return new VendorDto(
            vendor.Id.Value,
            vendor.Name,
            vendor.TaxId,
            vendor.Status.ToString(),
            vendor.Contact.Email,
            vendor.Contact.Phone,
            vendor.Contact.ContactPerson,
            vendor.Address?.Street,
            vendor.Address?.City,
            vendor.Address?.State,
            vendor.Address?.PostalCode,
            vendor.Address?.Country,
            vendor.PaymentTermDays,
            vendor.Notes,
            vendor.CreatedAt,
            vendor.CreatedBy,
            vendor.ActivatedAt,
            vendor.ActivatedBy);
    }
}
