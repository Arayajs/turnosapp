using TurnOS.Application.DTOs.Service;
using TurnOS.Application.Interfaces;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Interfaces;

namespace TurnOS.Application.Services;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _serviceRepo;
    private readonly IBusinessRepository _businessRepo;

    public ServiceService(IServiceRepository serviceRepo, IBusinessRepository businessRepo)
    {
        _serviceRepo = serviceRepo;
        _businessRepo = businessRepo;
    }

    public async Task<ServiceResponse> CreateAsync(CreateServiceRequest request, Guid requestingUserId)
    {
        var business = await _businessRepo.GetByIdAsync(request.BusinessId)
            ?? throw new KeyNotFoundException("Negocio no encontrado.");

        if (business.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("Solo el dueño puede agregar servicios a este negocio.");

        var service = new Service
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            BusinessId = request.BusinessId
        };

        await _serviceRepo.AddAsync(service);
        return MapToResponse(service, business.Name);
    }

    public async Task<ServiceResponse> UpdateAsync(Guid id, UpdateServiceRequest request, Guid requestingUserId)
    {
        var service = await _serviceRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        var business = await _businessRepo.GetByIdAsync(service.BusinessId)
            ?? throw new KeyNotFoundException("Negocio no encontrado.");

        if (business.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("Solo el dueño puede editar servicios de este negocio.");

        service.Name = request.Name;
        service.Description = request.Description;
        service.DurationMinutes = request.DurationMinutes;
        service.Price = request.Price;

        await _serviceRepo.UpdateAsync(service);
        return MapToResponse(service, business.Name);
    }

    public async Task DeleteAsync(Guid id, Guid requestingUserId)
    {
        var service = await _serviceRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        var business = await _businessRepo.GetByIdAsync(service.BusinessId)
            ?? throw new KeyNotFoundException("Negocio no encontrado.");

        if (business.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("Solo el dueño puede eliminar servicios de este negocio.");

        await _serviceRepo.DeleteAsync(service);
    }

    private static ServiceResponse MapToResponse(Service s, string businessName) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        DurationMinutes = s.DurationMinutes,
        Price = s.Price,
        BusinessId = s.BusinessId,
        BusinessName = businessName
    };
}
