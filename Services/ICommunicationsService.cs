using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface ICommunicationsService
{
    Task<CommunicationsOverviewDto> GetOverviewAsync();
    Task<List<AudienceOption>> GetAudiencesAsync();
    Task<List<ChannelHealth>> GetChannelHealthAsync();
    Task<PaginatedResult<Broadcast>> GetBroadcastsAsync(BroadcastQueryDto query);
    Task<Broadcast?> GetBroadcastByIdAsync(string id);
    Task<Broadcast> CreateBroadcastAsync(CreateBroadcastRequest request);
    Task<bool> DeleteBroadcastAsync(string id);
    Task<Broadcast?> ResendBroadcastAsync(string id);
    Task<CommunicationDraftDto?> GetDraftAsync();
    Task<CommunicationDraftDto> SaveDraftAsync(SaveDraftRequest request);
    Task<bool> ClearDraftAsync();
    Task<byte[]> ExportCsvAsync(BroadcastQueryDto query);
}
