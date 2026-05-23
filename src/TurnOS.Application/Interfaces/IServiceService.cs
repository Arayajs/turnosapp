using TurnOS.Application.DTOs.Service;

namespace TurnOS.Application.Interfaces;

public interface IServiceService
{
    Task<ServiceResponse> CreateAsync(CreateServiceRequest request, Guid requestingUserId);
    Task<ServiceResponse> UpdateAsync(Guid id, UpdateServiceRequest request, Guid requestingUserId);
    Task DeleteAsync(Guid id, Guid requestingUserId);
}
