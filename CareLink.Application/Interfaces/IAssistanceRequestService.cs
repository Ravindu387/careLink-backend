using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IAssistanceRequestService
{
    Task<IReadOnlyList<AssistanceRequestResponseDto>> GetAllAsync(
        AssistanceRequestQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto> CreateAsync(
        CreateAssistanceRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto?> UpdateAsync(
        string id,
        UpdateAssistanceRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto?> AssignAsync(
        string id,
        AssignAssistanceRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestResponseDto?> UpdateStatusAsync(
        string id,
        UpdateAssistanceRequestStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequestMetricsDto> GetMetricsAsync(
        CancellationToken cancellationToken = default);

    Task ResetDataAsync(
        CancellationToken cancellationToken = default);
}
