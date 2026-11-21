namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface IStockTransferRepository
{
    Task<IEnumerable<StockTransfer>> GetAllAsync();
    Task<StockTransfer?> GetByIdAsync(string id);
    Task<StockTransfer> CreateAsync(StockTransfer transfer);
    Task<StockTransfer?> UpdateAsync(StockTransfer transfer);
    Task<bool> DeleteAsync(string id);
}
