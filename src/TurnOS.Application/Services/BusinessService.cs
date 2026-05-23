using TurnOS.Application.DTOs.Business;
using TurnOS.Application.DTOs.Service;
using TurnOS.Application.Interfaces;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Interfaces;

namespace TurnOS.Application.Services;

public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepo;
    private readonly IServiceRepository _serviceRepo;

    public BusinessService(IBusinessRepository businessRepo, IServiceRepository serviceRepo)
    {
        _businessRepo = businessRepo;
        _serviceRepo = serviceRepo;
    }

    public async Task<IEnumerable<BusinessResponse>> GetAllAsync()
    {
        var businesses = await _businessRepo.GetAllAsync();
        return businesses.Select(MapToResponse);
    }

    public async Task<BusinessResponse> GetByIdAsync(Guid id)
    {
        var business = await _businessRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Negocio no encontrado.");
        return MapToResponse(business);
    }

    public async Task<BusinessResponse> CreateAsync(CreateBusinessRequest request, Guid ownerId)
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            Phone = request.Phone,
            OwnerId = ownerId
        };

        await _businessRepo.AddAsync(business);

        var created = await _businessRepo.GetByIdAsync(business.Id)
            ?? throw new InvalidOperationException("Error al crear el negocio.");
        return MapToResponse(created);
    }

    public async Task<BusinessResponse> UpdateAsync(Guid id, UpdateBusinessRequest request, Guid requestingUserId)
    {
        var business = await _businessRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Negocio no encontrado.");

        if (business.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("Solo el dueño puede editar este negocio.");

        business.Name = request.Name;
        business.Description = request.Description;
        business.Address = request.Address;
        business.Phone = request.Phone;

        await _businessRepo.UpdateAsync(business);
        return MapToResponse(business);
    }

    public async Task DeleteAsync(Guid id, Guid requestingUserId)
    {
        var business = await _businessRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Negocio no encontrado.");

        if (business.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("Solo el dueño puede eliminar este negocio.");

        await _businessRepo.DeleteAsync(business);
    }

    public async Task<IEnumerable<ServiceResponse>> GetServicesAsync(Guid businessId)
    {
        var exists = await _businessRepo.GetByIdAsync(businessId);
        if (exists is null)
            throw new KeyNotFoundException("Negocio no encontrado.");

        var services = await _serviceRepo.GetByBusinessIdAsync(businessId);
        return services.Select(s => new ServiceResponse
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            DurationMinutes = s.DurationMinutes,
            Price = s.Price,
            BusinessId = s.BusinessId,
            BusinessName = exists.Name
        });
    }

    private static BusinessResponse MapToResponse(Business b) => new()
    {
        Id = b.Id,
        Name = b.Name,
        Description = b.Description,
        Address = b.Address,
        Phone = b.Phone,
        OwnerId = b.OwnerId,
        OwnerName = b.Owner?.FullName ?? string.Empty
    };
}
