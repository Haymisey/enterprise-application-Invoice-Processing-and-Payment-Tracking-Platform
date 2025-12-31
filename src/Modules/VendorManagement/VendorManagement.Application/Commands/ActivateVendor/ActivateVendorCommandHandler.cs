using VendorManagement.Domain.Repositories;
using VendorManagement.Domain.ValueObjects;
using Shared.Application.Messaging;
using Shared.Domain.Primitives;
using Shared.Domain.Results;

namespace VendorManagement.Application.Commands.ActivateVendor;

/// <summary>
/// Handler for ActivateVendorCommand.
/// </summary>
internal sealed class ActivateVendorCommandHandler : ICommandHandler<ActivateVendorCommand>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateVendorCommandHandler(
        IVendorRepository vendorRepository,
        IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendorId = VendorId.Create(request.VendorId);
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);

        if (vendor is null)
        {
            return Result.Failure(VendorErrors.NotFound(request.VendorId));
        }

        try
        {
            vendor.Activate(request.ActivatedBy);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Vendor.ActivationFailed", ex.Message));
        }

        _vendorRepository.Update(vendor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
