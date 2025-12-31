using VendorManagement.Domain.Aggregates;
using VendorManagement.Domain.Repositories;
using Shared.Application.Messaging;
using Shared.Domain.Primitives;
using Shared.Domain.Results;

namespace VendorManagement.Application.Commands.RegisterVendor;

/// <summary>
/// Handler for RegisterVendorCommand.
/// </summary>
internal sealed class RegisterVendorCommandHandler : ICommandHandler<RegisterVendorCommand, Guid>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterVendorCommandHandler(
        IVendorRepository vendorRepository,
        IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
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

        await _vendorRepository.AddAsync(vendor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(vendor.Id.Value);
    }
}
