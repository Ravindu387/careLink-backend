namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface IFeedbackRepository
{
    Task<IEnumerable<CitizenFeedback>> GetAllAsync();
    Task<CitizenFeedback?> GetByIdAsync(string id);
    Task<CitizenFeedback> CreateAsync(CitizenFeedback feedback);
    Task<CitizenFeedback?> UpdateAsync(CitizenFeedback feedback);
    Task<bool> DeleteAsync(string id);
}
