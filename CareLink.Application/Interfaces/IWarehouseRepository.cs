namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface IWarehouseRepository
{
    Task<IEnumerable<Warehouse>> GetAllAsync();
    Task<Warehouse?> GetByIdAsync(string id);
    Task<Warehouse> CreateAsync(Warehouse warehouse);
    Task<Warehouse?> UpdateAsync(Warehouse warehouse);
    Task<bool> DeleteAsync(string id);
}
