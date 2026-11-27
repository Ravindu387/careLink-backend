using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Chatbot;
using CareLink.Application.Interfaces;
using CareLink.Application.Services;
using CareLink.Core.Entities;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using CareLink.Infrastructure.Repositories;
using CareLink.Tests.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CareLink.Tests.UnitTests.Services;

public class ChatbotServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly FakeGeminiService _fakeGemini;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IAssistanceRequestService> _assistanceRequestMock;
    private readonly Mock<IShelterService> _shelterMock;
    private readonly IChatbotRepository _chatbotRepository;
    private readonly ChatbotService _service;

    public ChatbotServiceTests()
    {
        _dbContext = TestDbContextFactory.CreateInMemory();
        _fakeGemini = new FakeGeminiService();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _assistanceRequestMock = new Mock<IAssistanceRequestService>();
        _shelterMock = new Mock<IShelterService>();

        _chatbotRepository = new ChatbotRepository(_dbContext, NullLogger<ChatbotRepository>.Instance);

        _service = new ChatbotService(
            _fakeGemini,
            _chatbotRepository,
            _userManagerMock.Object,
            NullLogger<ChatbotService>.Instance,
            _assistanceRequestMock.Object,
            _shelterMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task SendMessageAsync_CitizenQuery_GeneratesGroundedResponseAndSuggestedActions()
    {
        // Arrange
        const string userId = "citizen-user-1";
        const string citizenName = "Sunil Perera";

        var citizenUser = new ApplicationUser
        {
            Id = userId,
            UserName = "sunil@carelink.lk",
            Email = "sunil@carelink.lk",
            FullName = citizenName,
            PhoneNumber = "0771234567",
            Address = "Gampaha District, Western Province"
        };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(citizenUser);

        _assistanceRequestMock.Setup(m => m.GetAllAsync(It.IsAny<AssistanceRequestQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AssistanceRequestResponseDto>
            {
                new AssistanceRequestResponseDto
                {
                    Id = "REQ-101",
                    ContactName = citizenName,
                    ContactPhone = "0771234567",
                    Category = "Flood Relief Food & Water",
                    Priority = "High",
                    Status = "Dispatched",
                    Location = "Gampaha"
                }
            });

        _shelterMock.Setup(m => m.GetAllAsync(It.IsAny<ShelterQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShelterDto>
            {
                new ShelterDto
                {
                    Id = "SHELTER-01",
                    Name = "Gampaha Maha Vidyalaya Relief Shelter",
                    District = "Gampaha",
                    Capacity = 150,
                    CurrentOccupancy = 45,
                    Status = "Active"
                }
            });

        _fakeGemini.CustomRawResult = "Hello Sunil, I can see your request #REQ-101 is currently Dispatched! The nearby shelter at Gampaha Maha Vidyalaya also has 105 available beds.";

        var request = new ChatbotMessageRequestDto
        {
            Message = "What is the status of my flood assistance request and where is the closest shelter?"
        };

        // Act
        var response = await _service.SendMessageAsync(userId, "Citizen", request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Citizen", response.Role);
        Assert.False(string.IsNullOrWhiteSpace(response.ConversationId));
        Assert.Contains("REQ-101", response.Response);
        Assert.NotEmpty(response.SuggestedActions);
        Assert.Contains(response.SuggestedActions, a => a.Contains("track") || a.Contains("assistance") || a.Contains("shelter"));

        // Verify conversation was persisted
        var saved = await _chatbotRepository.GetConversationAsync(response.ConversationId);
        Assert.NotNull(saved);
        Assert.Equal(userId, saved.UserId);
        Assert.Equal(2, saved.Messages.Count); // User prompt + AI answer
    }

    [Fact]
    public async Task SendMessageAsync_VolunteerQuery_GroundsWithVolunteerProfileAndSkills()
    {
        // Arrange
        const string userId = "volunteer-user-2";
        const string volunteerName = "Kasun Jayasuriya";

        var volunteerUser = new ApplicationUser
        {
            Id = userId,
            UserName = "kasun@carelink.lk",
            Email = "kasun@carelink.lk",
            FullName = volunteerName,
            PhoneNumber = "0719876543",
            Skills = "First Aid, Swiftwater Search & Rescue, Logistics",
            Availability = "Immediate",
            PreferredLocation = "Western Province"
        };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId))
            .ReturnsAsync(volunteerUser);

        _fakeGemini.CustomRawResult = "Welcome Kasun! Your profile is set to Immediate availability with First Aid and Swiftwater Rescue skills.";

        var request = new ChatbotMessageRequestDto
        {
            Message = "How do I update my deployment status or find active relief tasks?"
        };

        // Act
        var response = await _service.SendMessageAsync(userId, "Volunteer", request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Volunteer", response.Role);
        Assert.Contains("Immediate", response.Response);
        Assert.NotEmpty(response.SuggestedActions);
        Assert.Contains(response.SuggestedActions, a => a.Contains("availability") || a.Contains("assignments") || a.Contains("skills"));
    }

    [Fact]
    public async Task SendMessageAsync_ImmediateDangerKeywords_IncludesEmergencyHotlinesPriority()
    {
        // Arrange
        const string userId = "citizen-danger-1";
        var user = new ApplicationUser { Id = userId, FullName = "Kamal", Email = "kamal@carelink.lk" };
        _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

        var request = new ChatbotMessageRequestDto
        {
            Message = "Help! We are trapped by flood water on the roof and the water is rising fast! Need immediate rescue!"
        };

        _fakeGemini.CustomRawResult = "IF YOU ARE IN IMMEDIATE LIFE DANGER, CALL 117 OR 1990 IMMEDIATELY. Rescue teams are triaging urgent reports.";

        // Act
        var response = await _service.SendMessageAsync(userId, "Citizen", request);

        // Assert
        Assert.NotNull(response);
        Assert.Contains("117", response.Response);
        Assert.NotEmpty(response.SuggestedActions);
    }

    [Fact]
    public async Task SendMessageAsync_TrilingualQuery_DetectsLanguageAndReturnsCorrectResponse()
    {
        // Arrange
        const string userId = "citizen-lang-1";
        var user = new ApplicationUser { Id = userId, FullName = "Amara", Email = "amara@carelink.lk" };
        _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

        // 1. Sinhala test
        var sinhalaRequest = new ChatbotMessageRequestDto
        {
            Message = "මට හදිසි කෑම සහ ජලය අවශ්‍යයි, ආධාර ඉල්ලුම් කරන්නේ කෙසේද?"
        };
        _fakeGemini.CustomRawResult = "ආයුබෝවන්, CareLink හරහා හදිසි සහන ඉල්ලුම් කිරීමට 'Request Assistance' පිටුවට පිවිසෙන්න.";

        var sinhalaResponse = await _service.SendMessageAsync(userId, "Citizen", sinhalaRequest);
        Assert.NotNull(sinhalaResponse);
        Assert.Contains("CareLink", sinhalaResponse.Response);

        // 2. Tamil test
        var tamilRequest = new ChatbotMessageRequestDto
        {
            Message = "எனக்கு அவசர உணவு உதவி தேவை, நிவாரணம் பெறுவது எப்படி?"
        };
        _fakeGemini.CustomRawResult = "வணக்கம், CareLink மூலம் அவசர உதவி கோரிக்கையை சமர்ப்பிக்கவும்.";

        var tamilResponse = await _service.SendMessageAsync(userId, "Citizen", tamilRequest);
        Assert.NotNull(tamilResponse);
        Assert.Contains("CareLink", tamilResponse.Response);
    }

    [Fact]
    public async Task SendMessageAsync_GeminiServiceFailure_GracefullyFallsBackToRuleBasedResponse()
    {
        // Arrange
        const string userId = "citizen-fallback-1";
        var user = new ApplicationUser { Id = userId, FullName = "Nimal", Email = "nimal@carelink.lk" };
        _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

        // Simulate API error / network failure
        _fakeGemini.Behavior = FakeGeminiBehavior.ApiError;

        var request = new ChatbotMessageRequestDto
        {
            Message = "How do I register and track my requests?"
        };

        // Act - should NOT throw an unhandled 500 exception
        var response = await _service.SendMessageAsync(userId, "Citizen", request);

        // Assert
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Response));
        // Fallback message must provide official hotlines and guidance
        Assert.Contains("117", response.Response);
        Assert.NotEmpty(response.SuggestedActions);
    }

    [Fact]
    public async Task SendMessageAsync_DifferentUserAccessingExistingConversation_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        const string ownerUserId = "user-owner-1";
        const string attackerUserId = "user-attacker-2";

        var ownerUser = new ApplicationUser { Id = ownerUserId, FullName = "Owner", Email = "owner@carelink.lk" };
        var attackerUser = new ApplicationUser { Id = attackerUserId, FullName = "Attacker", Email = "attacker@carelink.lk" };

        _userManagerMock.Setup(m => m.FindByIdAsync(ownerUserId)).ReturnsAsync(ownerUser);
        _userManagerMock.Setup(m => m.FindByIdAsync(attackerUserId)).ReturnsAsync(attackerUser);

        // Owner creates conversation
        var firstResponse = await _service.SendMessageAsync(ownerUserId, "Citizen", new ChatbotMessageRequestDto
        {
            Message = "Hello from owner"
        });

        var conversationId = firstResponse.ConversationId;

        // Attacker attempts to post into owner's conversation
        var attackRequest = new ChatbotMessageRequestDto
        {
            ConversationId = conversationId,
            Message = "I am attempting to tamper with this conversation"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.SendMessageAsync(attackerUserId, "Citizen", attackRequest));
    }

    [Fact]
    public async Task GetUserConversationsAsync_ReturnsOnlyUserOwnedConversations()
    {
        // Arrange
        const string user1 = "user-alpha";
        const string user2 = "user-beta";

        _userManagerMock.Setup(m => m.FindByIdAsync(user1)).ReturnsAsync(new ApplicationUser { Id = user1 });
        _userManagerMock.Setup(m => m.FindByIdAsync(user2)).ReturnsAsync(new ApplicationUser { Id = user2 });

        await _service.SendMessageAsync(user1, "Citizen", new ChatbotMessageRequestDto { Message = "Query 1 by User 1" });
        await _service.SendMessageAsync(user1, "Citizen", new ChatbotMessageRequestDto { Message = "Query 2 by User 1" });
        await _service.SendMessageAsync(user2, "Volunteer", new ChatbotMessageRequestDto { Message = "Query by User 2" });

        // Act
        var user1Conversations = await _service.GetUserConversationsAsync(user1);
        var user2Conversations = await _service.GetUserConversationsAsync(user2);

        // Assert
        Assert.Equal(2, user1Conversations.Count);
        Assert.Single(user2Conversations);
        Assert.All(user1Conversations, c => Assert.Equal("Citizen", c.UserRole));
        Assert.All(user2Conversations, c => Assert.Equal("Volunteer", c.UserRole));
    }

    [Fact]
    public async Task GetConversationByIdAsync_OtherUserAccess_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        const string user1 = "user-alice";
        const string user2 = "user-bob";

        _userManagerMock.Setup(m => m.FindByIdAsync(user1)).ReturnsAsync(new ApplicationUser { Id = user1 });
        _userManagerMock.Setup(m => m.FindByIdAsync(user2)).ReturnsAsync(new ApplicationUser { Id = user2 });

        var created = await _service.SendMessageAsync(user1, "Citizen", new ChatbotMessageRequestDto { Message = "Alice private inquiry" });

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GetConversationByIdAsync(user2, created.ConversationId));
    }

    [Fact]
    public async Task DeleteConversationAsync_OwnerAccess_DeletesConversationSuccessfully()
    {
        // Arrange
        const string user = "user-charlie";
        _userManagerMock.Setup(m => m.FindByIdAsync(user)).ReturnsAsync(new ApplicationUser { Id = user });

        var created = await _service.SendMessageAsync(user, "Citizen", new ChatbotMessageRequestDto { Message = "Inquiry to delete" });

        // Act
        var deleteResult = await _service.DeleteConversationAsync(user, created.ConversationId);
        var postDeleteDetail = await _service.GetConversationByIdAsync(user, created.ConversationId);

        // Assert
        Assert.True(deleteResult);
        Assert.Null(postDeleteDetail);
    }

    [Fact]
    public async Task DeleteConversationAsync_OtherUserAccess_ReturnsFalse()
    {
        // Arrange
        const string user1 = "user-dave";
        const string user2 = "user-eve";

        _userManagerMock.Setup(m => m.FindByIdAsync(user1)).ReturnsAsync(new ApplicationUser { Id = user1 });
        _userManagerMock.Setup(m => m.FindByIdAsync(user2)).ReturnsAsync(new ApplicationUser { Id = user2 });

        var created = await _service.SendMessageAsync(user1, "Citizen", new ChatbotMessageRequestDto { Message = "Dave conversation" });

        // Act
        var deleteResult = await _service.DeleteConversationAsync(user2, created.ConversationId);

        // Assert
        Assert.False(deleteResult);
    }
}
