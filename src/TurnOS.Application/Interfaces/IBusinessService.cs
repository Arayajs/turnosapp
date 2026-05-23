using TurnOS.Application.DTOs.Business;
using TurnOS.Application.DTOs.Service;

namespace TurnOS.Application.Interfaces;

public interface IBusinessService
{
    Task<IEnumerable<BusinessResponse>> GetAllAsync();
    Task<BusinessResponse> GetByIdAsync(Guid id);
    Task<BusinessResponse> CreateAsync(CreateBusinessRequest request, Guid ownerId);
    Task<BusinessResponse> UpdateAsync(Guid id, UpdateBusinessRequest request, Guid requestingUserId);
    Task DeleteAsync(Guid id, Guid requestingUserId);
    Task<IEnumerable<ServiceResponse>> GetServicesAsync(Guid businessId);
}
