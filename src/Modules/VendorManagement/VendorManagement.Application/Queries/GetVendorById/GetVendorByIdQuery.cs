using Shared.Application.Messaging;

namespace VendorManagement.Application.Queries.GetVendorById;

/// <summary>
/// Query to get a vendor by its ID.
/// </summary>
public sealed record GetVendorByIdQuery(Guid VendorId) : IQuery<VendorDto?>;
