namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface ISupplierRepository
{
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(string id);
    Task<Supplier> CreateAsync(Supplier supplier);
    Task<Supplier?> UpdateAsync(Supplier supplier);
    Task<bool> DeleteAsync(string id);
}
