using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CentralKitchen.Application.DTOs.Stores;
using CentralKitchen.Application.Interfaces;
using CentralKitchen.Domain.Entities;

namespace CentralKitchen.Application.Services;

public class StoreService : IStoreService
{
    private readonly IApplicationDbContext _context;

    public StoreService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StoreDto>> GetStoresAsync()
    {
        return await _context.Stores.Select(s => new StoreDto
        {
            Id = s.Id,
            Name = s.Name,
            Address = s.Address,
            Phone = s.Phone,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToListAsync();
    }

    public async Task<StoreDto?> GetStoreByIdAsync(Guid id)
    {
        var s = await _context.Stores.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return null;

        return new StoreDto
        {
            Id = s.Id,
            Name = s.Name,
            Address = s.Address,
            Phone = s.Phone,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }

    public async Task<StoreDto> CreateStoreAsync(CreateStoreDto dto)
    {
        var store = new Store
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Stores.Add(store);
        await _context.SaveChangesAsync();

        return new StoreDto
        {
            Id = store.Id,
            Name = store.Name,
            Address = store.Address,
            Phone = store.Phone,
            IsActive = store.IsActive,
            CreatedAt = store.CreatedAt,
            UpdatedAt = store.UpdatedAt
        };
    }

    public async Task<StoreDto?> UpdateStoreAsync(Guid id, UpdateStoreDto dto)
    {
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == id);
        if (store == null) return null;

        store.Name = dto.Name;
        store.Address = dto.Address;
        store.Phone = dto.Phone;
        store.IsActive = dto.IsActive;
        store.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new StoreDto
        {
            Id = store.Id,
            Name = store.Name,
            Address = store.Address,
            Phone = store.Phone,
            IsActive = store.IsActive,
            CreatedAt = store.CreatedAt,
            UpdatedAt = store.UpdatedAt
        };
    }
}
