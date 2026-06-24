using System.Collections.Generic;
using System.Threading.Tasks;
using CentralKitchen.Application.DTOs.Stores;

namespace CentralKitchen.Application.Interfaces;

public interface IStoreService
{
    Task<List<StoreDto>> GetStoresAsync();
    Task<StoreDto?> GetStoreByIdAsync(Guid id);
    Task<StoreDto> CreateStoreAsync(CreateStoreDto dto);
    Task<StoreDto?> UpdateStoreAsync(Guid id, UpdateStoreDto dto);
}
