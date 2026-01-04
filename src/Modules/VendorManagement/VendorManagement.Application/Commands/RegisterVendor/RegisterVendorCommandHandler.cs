using VendorManagement.Domain.Aggregates;
using VendorManagement.Domain.Repositories;
using Shared.Application.Messaging;
using Shared.Domain.Results;

namespace VendorManagement.Application.Commands.RegisterVendor;

/// <summary>
/// Handler for RegisterVendorCommand.
/// </summary>
internal sealed class RegisterVendorCommandHandler : ICommandHandler<RegisterVendorCommand, Guid>
{
    private readonly IVendorRepository _vendorRepository;

    public RegisterVendorCommandHandler(
        IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<Result<Guid>> Handle(RegisterVendorCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate tax ID
        if (await _vendorRepository.ExistsWithTaxIdAsync(request.TaxId, cancellationToken))
        {
            return Result.Failure<Guid>(VendorErrors.DuplicateTaxId(request.TaxId));
        }

        var vendor = Vendor.Register(
            request.Name,
            request.TaxId,
            request.Email,
            request.Phone,
            request.ContactPerson,
            request.CreatedBy,
            request.PaymentTermDays);

        Console.WriteLine($"[VENDOR DEBUG] Created vendor with ID: {vendor.Id.Value}");

        // AddAsync now saves automatically to the correct VendorDbContext
        await _vendorRepository.AddAsync(vendor, cancellationToken);
        Console.WriteLine($"[VENDOR DEBUG] Vendor added and saved to VendorDbContext");

        return Result.Success(vendor.Id.Value);
    }
}
