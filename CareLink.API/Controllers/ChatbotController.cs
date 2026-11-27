using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Chatbot;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(
        IChatbotService chatbotService,
        ILogger<ChatbotController> logger)
    {
        _chatbotService = chatbotService;
        _logger = logger;
    }

    /// <summary>
    /// Sends a message to the CareLink AI Assistant and receives an authenticated, role-aware response.
    /// Restricted to Citizen and Volunteer roles.
    /// </summary>
    [HttpPost("message")]
    [ProducesResponseType(typeof(ApiResponse<ChatbotMessageResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ChatbotMessageResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ChatbotMessageResponseDto>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendMessage(
        [FromBody] ChatbotMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ChatbotMessageResponseDto>.FailureResult(
                string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))));
        }

        var (userId, userRole, isAuthorized) = ResolveUserContext();
        if (!isAuthorized)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<ChatbotMessageResponseDto>.FailureResult(
                    "The CareLink AI Assistant is available exclusively to Citizen and Volunteer users."));
        }

        try
        {
            var response = await _chatbotService.SendMessageAsync(userId, userRole, request, cancellationToken);
            return Ok(ApiResponse<ChatbotMessageResponseDto>.SuccessResult(response));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<ChatbotMessageResponseDto>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ChatbotController.SendMessage");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<ChatbotMessageResponseDto>.FailureResult("An error occurred while generating the AI response. Please try again."));
        }
    }

    /// <summary>
    /// Gets all previous conversation summaries for the authenticated Citizen or Volunteer.
    /// </summary>
    [HttpGet("conversations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChatbotConversationSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChatbotConversationSummaryDto>>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConversations(CancellationToken cancellationToken)
    {
        var (userId, _, isAuthorized) = ResolveUserContext();
        if (!isAuthorized)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<IReadOnlyList<ChatbotConversationSummaryDto>>.FailureResult(
                    "The CareLink AI Assistant is available exclusively to Citizen and Volunteer users."));
        }

        var conversations = await _chatbotService.GetUserConversationsAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ChatbotConversationSummaryDto>>.SuccessResult(conversations));
    }

    /// <summary>
    /// Retrieves a specific conversation with all its messages.
    /// Enforces strict user ownership verification.
    /// </summary>
    [HttpGet("conversations/{id}")]
    [ProducesResponseType(typeof(ApiResponse<ChatbotConversationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ChatbotConversationDetailDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ChatbotConversationDetailDto>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConversationById(
        string id,
        CancellationToken cancellationToken)
    {
        var (userId, _, isAuthorized) = ResolveUserContext();
        if (!isAuthorized)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<ChatbotConversationDetailDto>.FailureResult(
                    "The CareLink AI Assistant is available exclusively to Citizen and Volunteer users."));
        }

        try
        {
            var conversation = await _chatbotService.GetConversationByIdAsync(userId, id, cancellationToken);
            if (conversation == null)
            {
                return NotFound(ApiResponse<ChatbotConversationDetailDto>.FailureResult("Conversation not found."));
            }

            return Ok(ApiResponse<ChatbotConversationDetailDto>.SuccessResult(conversation));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<ChatbotConversationDetailDto>.FailureResult(ex.Message));
        }
    }

    /// <summary>
    /// Deletes a specific conversation owned by the authenticated user.
    /// </summary>
    [HttpDelete("conversations/{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteConversation(
        string id,
        CancellationToken cancellationToken)
    {
        var (userId, _, isAuthorized) = ResolveUserContext();
        if (!isAuthorized)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<bool>.FailureResult(
                    "The CareLink AI Assistant is available exclusively to Citizen and Volunteer users."));
        }

        try
        {
            var deleted = await _chatbotService.DeleteConversationAsync(userId, id, cancellationToken);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.FailureResult("Conversation not found or could not be deleted."));
            }

            return Ok(ApiResponse<bool>.SuccessResult(true, "Conversation deleted successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<bool>.FailureResult(ex.Message));
        }
    }

    private (string UserId, string UserRole, bool IsAuthorized) ResolveUserContext()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("uid")?.Value
            ?? User.Identity?.Name
            ?? string.Empty;

        var roles = User.FindAll(ClaimTypes.Role)
            .Concat(User.FindAll("role"))
            .Select(c => c.Value)
            .ToList();

        // Check if user is in Citizen or Volunteer/VolunteerCoordinator role
        var isCitizen = roles.Any(r => string.Equals(r, UserRoles.Citizen, StringComparison.OrdinalIgnoreCase) ||
                                       r.Contains("Citizen", StringComparison.OrdinalIgnoreCase));

        var isVolunteer = roles.Any(r => string.Equals(r, UserRoles.Volunteer, StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(r, UserRoles.VolunteerCoordinator, StringComparison.OrdinalIgnoreCase) ||
                                         r.Contains("Volunteer", StringComparison.OrdinalIgnoreCase));

        if (!isCitizen && !isVolunteer)
        {
            // If user is Admin, allow for testing/supervision
            var isAdmin = roles.Any(r => string.Equals(r, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(r, UserRoles.SystemAdmin, StringComparison.OrdinalIgnoreCase));
            if (isAdmin)
            {
                return (userId, "Citizen", true);
            }

            return (userId, string.Empty, false);
        }

        var effectiveRole = isVolunteer ? "Volunteer" : "Citizen";
        return (userId, effectiveRole, true);
    }
}
