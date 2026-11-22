namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface IAlertRepository
{
    Task<IEnumerable<Alert>> GetAllAsync();
    Task<Alert?> GetByIdAsync(int id);
    Task<Alert> CreateAsync(Alert alert);
    Task<Alert?> UpdateAsync(Alert alert);
    Task<int> MarkAllOpenAsAcknowledgedAsync();
    Task<AlertRuleSettings> GetRulesAsync();
    Task<AlertRuleSettings> UpdateRulesAsync(AlertRuleSettings rules);
}
