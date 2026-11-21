namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface IAllocationRepository
{
    Task<IEnumerable<Allocation>> GetAllAsync();
    Task<Allocation?> GetByIdAsync(string id);
    Task<Allocation> CreateAsync(Allocation allocation);
    Task<Allocation?> UpdateAsync(Allocation allocation);
    Task<bool> DeleteAsync(string id);
}
