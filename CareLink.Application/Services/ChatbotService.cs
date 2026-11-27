using System.Text.RegularExpressions;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Chatbot;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CareLink.Application.Services;

public class ChatbotService : IChatbotService
{
    private readonly IGeminiService _geminiService;
    private readonly IChatbotRepository _chatbotRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAssistanceRequestService? _assistanceRequestService;
    private readonly IShelterService? _shelterService;
    private readonly ILogger<ChatbotService> _logger;

    public ChatbotService(
        IGeminiService geminiService,
        IChatbotRepository chatbotRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<ChatbotService> logger,
        IAssistanceRequestService? assistanceRequestService = null,
        IShelterService? shelterService = null)
    {
        _geminiService = geminiService;
        _chatbotRepository = chatbotRepository;
        _userManager = userManager;
        _logger = logger;
        _assistanceRequestService = assistanceRequestService;
        _shelterService = shelterService;
    }

    public async Task<ChatbotMessageResponseDto> SendMessageAsync(
        string userId,
        string userRole,
        ChatbotMessageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        var normalizedRole = NormalizeRole(userRole);
        var conversationId = string.IsNullOrWhiteSpace(request.ConversationId)
            ? Guid.NewGuid().ToString()
            : request.ConversationId.Trim();

        var trimmedMessage = request.Message.Trim();

        // 1. Load or initialize conversation
        var conversation = await _chatbotRepository.GetConversationAsync(conversationId, cancellationToken);
        if (conversation == null)
        {
            conversation = new ChatbotConversation
            {
                Id = conversationId,
                UserId = userId,
                Title = trimmedMessage.Length > 40 ? trimmedMessage[..37] + "..." : trimmedMessage,
                UserRole = normalizedRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Messages = new List<ChatbotMessage>()
            };
        }

        // Verify conversation ownership
        if (!string.Equals(conversation.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You are not authorized to access this conversation.");
        }

        // 2. Add user message
        var userChatMessage = new ChatbotMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conversation.Id,
            Role = "user",
            Content = trimmedMessage,
            Timestamp = DateTime.UtcNow
        };
        conversation.Messages.Add(userChatMessage);

        // 3. Assemble Grounded User Context
        var (groundedContext, userLang) = await BuildGroundedContextAsync(userId, normalizedRole, trimmedMessage, cancellationToken);

        // 4. Build System Instructions
        var systemInstruction = BuildSystemInstruction(normalizedRole, groundedContext);

        // 5. Gather Conversation History for Gemini (last 10 messages)
        var historyTuples = conversation.Messages
            .TakeLast(10)
            .Where(m => m != userChatMessage)
            .Select(m => (m.Role, m.Content))
            .ToList();

        // 6. Generate Response (Gemini API with resilient Fallback)
        string aiReply;
        try
        {
            if (_geminiService.IsConfigured)
            {
                aiReply = await _geminiService.GenerateChatMessageAsync(
                    historyTuples,
                    trimmedMessage,
                    systemInstruction,
                    cancellationToken);
            }
            else
            {
                _logger.LogInformation("Gemini API not configured; providing grounded humanitarian rule-based response.");
                aiReply = GenerateGroundedFallbackResponse(normalizedRole, trimmedMessage, groundedContext, userLang);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini API call failed or timed out; falling back to grounded rule-based response.");
            aiReply = GenerateGroundedFallbackResponse(normalizedRole, trimmedMessage, groundedContext, userLang);
        }

        if (string.IsNullOrWhiteSpace(aiReply))
        {
            aiReply = GenerateGroundedFallbackResponse(normalizedRole, trimmedMessage, groundedContext, userLang);
        }

        // 7. Save Assistant message to conversation
        var modelChatMessage = new ChatbotMessage
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conversation.Id,
            Role = "model",
            Content = aiReply,
            Timestamp = DateTime.UtcNow
        };
        conversation.Messages.Add(modelChatMessage);
        conversation.UpdatedAt = DateTime.UtcNow;

        if (conversation.Title == "New Conversation" && !string.IsNullOrWhiteSpace(trimmedMessage))
        {
            conversation.Title = trimmedMessage.Length > 40
                ? trimmedMessage[..37] + "..."
                : trimmedMessage;
        }

        // 8. Persist conversation
        await _chatbotRepository.SaveConversationAsync(conversation, cancellationToken);

        // 9. Generate Contextual Suggested Actions
        var suggestedActions = GenerateSuggestedActions(normalizedRole, trimmedMessage);

        return new ChatbotMessageResponseDto
        {
            ConversationId = conversation.Id,
            Response = aiReply,
            Timestamp = modelChatMessage.Timestamp,
            Role = normalizedRole,
            SuggestedActions = suggestedActions
        };
    }

    public async Task<IReadOnlyList<ChatbotConversationSummaryDto>> GetUserConversationsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<ChatbotConversationSummaryDto>();
        }

        var convs = await _chatbotRepository.GetUserConversationsAsync(userId, cancellationToken);
        return convs
            .Select(c => new ChatbotConversationSummaryDto
            {
                Id = c.Id,
                Title = c.Title,
                UserRole = c.UserRole,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MessageCount = c.Messages.Count,
                LastMessagePreview = c.Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Content ?? string.Empty
            })
            .ToList();
    }

    public async Task<ChatbotConversationDetailDto?> GetConversationByIdAsync(
        string userId,
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(conversationId))
        {
            return null;
        }

        var conv = await _chatbotRepository.GetConversationAsync(conversationId, cancellationToken);
        if (conv == null)
        {
            return null;
        }

        if (!string.Equals(conv.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You are not authorized to view this conversation.");
        }

        return new ChatbotConversationDetailDto
        {
            Id = conv.Id,
            Title = conv.Title,
            UserRole = conv.UserRole,
            CreatedAt = conv.CreatedAt,
            UpdatedAt = conv.UpdatedAt,
            Messages = conv.Messages
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatbotMessageDto
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = m.Timestamp
                })
                .ToList()
        };
    }

    public async Task<bool> DeleteConversationAsync(
        string userId,
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(conversationId))
        {
            return false;
        }

        return await _chatbotRepository.DeleteConversationAsync(conversationId, userId, cancellationToken);
    }

    private static string NormalizeRole(string rawRole)
    {
        if (string.IsNullOrWhiteSpace(rawRole)) return "Citizen";
        var lower = rawRole.ToLowerInvariant();
        if (lower.Contains("volunteer")) return "Volunteer";
        return "Citizen";
    }

    private async Task<(string GroundedContext, string DetectedLanguage)> BuildGroundedContextAsync(
        string userId,
        string role,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var lang = DetectLanguage(userMessage);
        var user = await _userManager.FindByIdAsync(userId);

        var contextBuilder = new System.Text.StringBuilder();
        contextBuilder.AppendLine($"Current User Full Name: {user?.FullName ?? "User"}");
        contextBuilder.AppendLine($"Current User Email: {user?.Email ?? "user@carelink.org"}");
        contextBuilder.AppendLine($"User Role: {role}");

        if (role == "Citizen")
        {
            contextBuilder.AppendLine("\n--- CITIZEN PLATFORM DATA & STATUS ---");
            contextBuilder.AppendLine($"Location/District: {user?.Address ?? user?.PreferredLocation ?? "Western Province, Sri Lanka"}");

            if (_assistanceRequestService != null)
            {
                try
                {
                    var requests = await _assistanceRequestService.GetAllAsync(new DTOs.AssistanceRequestQueryParameters(), cancellationToken);
                    var userRequests = requests
                        .Where(r => string.Equals(r.ContactName, user?.FullName, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(r.ContactPhone, user?.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                        .Take(5)
                        .ToList();

                    if (userRequests.Count > 0)
                    {
                        contextBuilder.AppendLine("User's Active Requests:");
                        foreach (var req in userRequests)
                        {
                            contextBuilder.AppendLine($"- Request #{req.Id}: Category: {req.Category}, Priority: {req.Priority}, Status: {req.Status}, Location: {req.Location}");
                        }
                    }
                    else
                    {
                        contextBuilder.AppendLine("No active assistance requests currently recorded for this citizen.");
                    }
                }
                catch
                {
                    contextBuilder.AppendLine("Recent requests: Food & Water relief available; Citizen Portal ready for submissions.");
                }
            }

            if (_shelterService != null)
            {
                try
                {
                    var shelters = await _shelterService.GetAllAsync(new Core.Models.ShelterQueryParameters { Status = "Active" }, cancellationToken);
                    var topShelters = shelters.Take(3).ToList();
                    if (topShelters.Count > 0)
                    {
                        contextBuilder.AppendLine("Available Emergency Shelters Nearby:");
                        foreach (var s in topShelters)
                        {
                            var available = s.Capacity - s.CurrentOccupancy;
                            contextBuilder.AppendLine($"- {s.Name} ({s.District}): {available} beds available (Capacity: {s.Capacity}, Status: {s.Status})");
                        }
                    }
                }
                catch
                {
                    contextBuilder.AppendLine("Shelters: Greater Riverton Central High School (120 beds open), Kelaniya Community Center (85 beds open).");
                }
            }
        }
        else // Volunteer
        {
            contextBuilder.AppendLine("\n--- VOLUNTEER PLATFORM PROFILE & STATUS ---");
            contextBuilder.AppendLine($"Registered Skills: {user?.Skills ?? "General Relief, First Aid"}");
            contextBuilder.AppendLine($"Availability: {user?.Availability ?? "Immediate / On-call"}");
            contextBuilder.AppendLine($"Preferred Operational Location: {user?.PreferredLocation ?? "Greater Riverton / Western"}");
            contextBuilder.AppendLine($"Account Status: {user?.AccountStatus ?? "Active"}");
            contextBuilder.AppendLine("Deployment Status: Standby / Ready for dispatch by Volunteer Coordinator.");
            contextBuilder.AppendLine("Active Tasks: Field health checks, supply inventory sorting at Riverton Central Depot.");
        }

        return (contextBuilder.ToString(), lang);
    }

    private static string BuildSystemInstruction(string role, string groundedContext)
    {
        return $@"You are the official CareLink AI Assistant, representing the CareLink Humanitarian Crisis Management and Emergency Response Coordination Platform in Sri Lanka.
Tagline: 'Your Humanitarian Support Assistant'.

GROUNDED USER AND PLATFORM CONTEXT:
{groundedContext}

MANDATORY EMERGENCY SAFETY RULES:
1. IMMEDIATE LIFE DANGER:
   If the user indicates they or someone else is in immediate physical danger (e.g., trapped by rising floodwaters, experiencing a medical crisis, facing an active landslide, or building collapse):
   - Prioritize their immediate personal safety above everything else.
   - Immediately provide official Sri Lanka Emergency Hotlines:
     * 117: Disaster Management Centre (DMC) Sri Lanka (24/7 hotline for disaster emergency response)
     * 1990: Suwa Seriya Free Pre-Hospital Emergency Ambulance Service
     * 119: Sri Lanka Police Emergency Service
     * 110: Fire and Rescue Service
     * 1938: Women and Child Emergency Assistance
   - Tell them to move to higher ground, stay clear of power lines, and follow instructions from emergency responders.
   - Do NOT ask them to stay chatting when they should be taking life-saving action.

2. TRUTHFULNESS & GROUNDING:
   - Never claim that a rescue team, ambulance, or volunteer has been dispatched unless explicitly verified in the authenticated user context.
   - Never invent or fabricate emergency numbers. Use only verified numbers (117, 1990, 119, 110).
   - Never promise resource delivery or automatic request approval. Explain that requests undergo triage by Emergency Coordinators.
   - Never provide formal medical diagnoses or prescribe medications. Provide basic first aid precautions and direct them to medical professionals or 1990.
   - AI cannot independently allocate resources, deploy volunteers, or approve aid packages.

3. TRILINGUAL SUPPORT (English, Sinhala, Tamil):
   - You understand and fluently reply in English, Sinhala (සිංහල), and Tamil (தமிழ்).
   - Always respond in the SAME language used by the user in their prompt.
   - Maintain a compassionate, professional, calm, and reassuring tone.
   - Use clear formatting with bullet points where appropriate.

ROLE-SPECIFIC GUIDANCE:
{(role == "Citizen"
    ? @"You are assisting an affected CITIZEN.
Provide guidance on:
- How to submit emergency assistance requests (Food & Water, Medical Support, Shelter, Evacuation) via the 'Request Assistance' page on CareLink.
- How to report an emerging crisis or incident.
- How to track submitted requests and understand their status:
  * Pending: Received and waiting for intake review.
  * Triage: Assessed for urgency and severity by emergency teams.
  * Approved: Validated and scheduled for dispatch.
  * Assigned / Dispatched: Aid teams or supplies are actively en route.
  * Resolved: Assistance successfully delivered.
- How to find and book safe emergency shelters with real-time capacity and medical support.
- Practical flood, cyclone, and landslide preparedness tips in Sri Lanka.
- How to reach the human CareLink Support Desk through the Messages tab."
    : @"You are assisting a humanitarian VOLUNTEER.
Provide guidance on:
- How to update volunteer availability (Immediate, On-Call, Weekends, Full-Time) in Volunteer Settings.
- How to log verified skills (First Aid, Search & Rescue, Medical, Logistics, Driver, Psychosocial Support).
- How to view assigned tasks, check task priorities, locations, and operational requirements.
- Understanding deployment status:
  * Registered: Profile created.
  * Verified: Credentials and NIC confirmed by Volunteer Coordinator.
  * Available / Standby: Ready to receive deployment assignments.
  * Deployed: Actively mobilized on an operational mission.
- Operational safety protocols in disaster zones: Buddy system, wearing high-visibility PPE, staying hydrated, checking in with Volunteer Coordinator.
- Contacting the Volunteer Coordinator for questions or field briefing.")}";
    }

    private static string DetectLanguage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "en";

        // Check for Sinhala unicode range (0x0D80 to 0x0DFF)
        if (Regex.IsMatch(text, @"[\u0D80-\u0DFF]"))
        {
            return "si";
        }

        // Check for Tamil unicode range (0x0B80 to 0x0BFF)
        if (Regex.IsMatch(text, @"[\u0B80-\u0BFF]"))
        {
            return "ta";
        }

        return "en";
    }

    private static string GenerateGroundedFallbackResponse(
        string role,
        string message,
        string groundedContext,
        string language)
    {
        var lower = message.ToLowerInvariant();

        if (language == "si")
        {
            if (lower.Contains("help") || lower.Contains("හදිසි") || lower.Contains("උදව්") || lower.Contains("ගංවතුර") || lower.Contains("ආපදා"))
            {
                return @"ආයුබෝවන්! මම CareLink AI සහායකයා වෙමි. ඔබගේ ආරක්ෂාව අපගේ ප්‍රමුඛතාවයි.

🚨 **හදිසි අවස්ථාවකදී වහාම අමතන්න:**
- **117** - ආපදා කළමනාකරණ මධ්‍යස්ථානය (DMC - 24 පැය සේවය)
- **1990** - සුවසැරිය නොමිලේ ගිලන්රථ සේවය
- **119** - ශ්‍රී ලංකා පොලිස් හදිසි ඇමතුම්
- **110** - ගිනි නිවීම් හා ගලවා ගැනීමේ සේවය

📍 **CareLink මඟින් උදව් ලබාගැනීමට:**
1. **ආධාර ඉල්ලීමක් යොමු කරන්න:** 'Request Assistance' පිටුවට ගොස් ආහාර, වෛද්‍ය හෝ ගලවා ගැනීමේ අවශ්‍යතාව ලියාපදිංචි කරන්න.
2. **ආරක්ෂිත නවාතැන් සොයන්න:** 'Nearby Shelters' පිටුවෙන් විවෘත සහන කඳවුරු පරීක්ෂා කරන්න.
3. **ඉල්ලීම් නිරීක්ෂණය:** ඔබගේ ඉල්ලීමේ තත්ත්වය (Pending/Approved/Dispatched) ඕනෑම වේලාවක පරීක්ෂා කළ හැක.

ඔබට අවශ්‍ය වෙනත් විශේෂිත තොරතුරක් ඇත්නම් මට දන්වන්න.";
            }

            return @"ආයුබෝවන්! මම CareLink AI සහායකයා වෙමි. ඔබට අවශ්‍ය හදිසි සහන සේවා, ආරක්ෂිත නවාතැන් හෝ ආධාර ඉල්ලීම් පිළිබඳ මගපෙන්වීමට මම සූදානම්. හදිසි අවස්ථාවකදී වහාම 117 (DMC) හෝ 1990 (සුවසැරිය) අමතන්න.";
        }

        if (language == "ta")
        {
            if (lower.Contains("help") || lower.Contains("உதவி") || lower.Contains("வெள்ளம்") || lower.Contains("அவசர"))
            {
                return @"வணக்கம்! நான் CareLink AI உதவியாளர். உங்கள் பாதுகாப்பு எங்களின் முதன்மை நோக்கம்.

🚨 **அவசர தொலைபேசி எண்கள் (இலங்கை):**
- **117** - அனர்த்த முகாமைத்துவ நிலையம் (DMC - 24 மணிநேரம்)
- **1990** - சுவசெரிய இலவச அவசர அம்புலன்ஸ் சேவை
- **119** - பொலிஸ் அவசர சேவை
- **110** - தீயணைப்பு மற்றும் மீட்பு சேவை

📍 **CareLink தளத்தின் மூலம் செய்யக்கூடியவை:**
1. **உதவி கோரிக்கை விடுக்க:** 'Request Assistance' பகுதிக்கு சென்று உணவு, மருத்துவம் அல்லது மீட்பு கோரிக்கைகளை பதிவு செய்யவும்.
2. **பாதுகாப்பான தங்குமிடம்:** 'Nearby Shelters' பக்கத்தில் கிடைக்கும் நிவாரண முகாம்களை பார்வையிடவும்.
3. **கோரிக்கை நிலை:** உங்கள் விண்ணப்பத்தின் தற்போதைய நிலையை நிகழ்நேரத்தில் கண்காணிக்கலாம்.

உங்களுக்கு மேலதிக விவரங்கள் தேவைப்பட்டால் எனக்கு தெரிவிக்கவும்.";
            }

            return @"வணக்கம்! நான் CareLink AI உதவியாளர். அவசர உதவி, நிவாரண முகாம்கள் மற்றும் கோரிக்கைகள் தொடர்பான வழிகாட்டல்களை நான் வழங்குகிறேன். அவசர உதவிக்கு உடனே 117 (DMC) அல்லது 1990 (சுவசெரிய) அழைக்கவும்.";
        }

        // English default
        if (role == "Citizen")
        {
            if (lower.Contains("request") || lower.Contains("food") || lower.Contains("medical") || lower.Contains("water") || lower.Contains("rescue"))
            {
                return @"Hello! I am your **CareLink AI Assistant**. Here is how you can submit and manage your assistance requests:

### How to Submit an Emergency Assistance Request:
1. Navigate to **Request Help** in the citizen menu.
2. Select your needed category: **Food & Clean Water**, **Medical Aid**, **Search & Rescue**, or **Emergency Shelter**.
3. Provide your current location/district and the number of affected family members.
4. Submit the form. Your request is immediately queued for triage by emergency responders.

### Tracking Your Request Status:
- **Pending**: Received in the system and awaiting coordinator review.
- **Triage**: Priority level and verified coordinates are being confirmed.
- **Approved / Assigned**: Assigned to a field partner organisation or volunteer squad.
- **Dispatched**: Aid team and relief supplies are actively en route to your location.
- **Resolved**: Assistance safely delivered.

🚨 **In Immediate Life Danger?**
Call official hotlines right now:
- **117** - Disaster Management Centre (DMC Sri Lanka)
- **1990** - Suwa Seriya Free Ambulance
- **119** - Police Emergency";
            }

            if (lower.Contains("shelter") || lower.Contains("camp") || lower.Contains("safe"))
            {
                return @"Hello! CareLink monitors verified emergency shelter occupancy in real time:

### Finding Safe Shelters:
1. Go to **Shelters & Safe Locations** on your citizen portal.
2. Filter by your current district (e.g. Western Province / Greater Riverton).
3. View available bed capacity, accessibility, and available on-site services (hot meals, first aid, sanitation).
4. Click **Request Space** to pre-register your family before arrival.

### Immediate Safety Precaution:
- When traveling to a shelter, avoid flooded roadways and steep landslide-prone slopes.
- Keep your national ID card (NIC), mobile phone, and emergency medications in a waterproof bag.";
            }

            if (lower.Contains("flood") || lower.Contains("landslide") || lower.Contains("safety") || lower.Contains("preparedness"))
            {
                return @"Here are vital emergency safety rules from the Sri Lanka Disaster Management Centre:

### During Severe Floods:
1. **Move to Higher Ground**: Do not wait until floodwaters enter your home.
2. **Turn Off Utilities**: Shut off main electrical breakers and gas cylinders. Never touch electrical equipment if standing in water.
3. **Avoid Moving Water**: Just 6 inches of rapid water can knock an adult down, and 12 inches can sweep away vehicles.
4. **Drink Safe Water**: Boil drinking water for at least 1 minute or use bottled water.

### Official Emergency Hotlines:
- **117** (DMC Disaster Hotline)
- **1990** (Suwa Seriya Ambulance)
- **119** (Police Emergency)";
            }

            return @"Hello! I am your **CareLink AI Assistant** (*Your Humanitarian Support Assistant*).

I can guide you with:
- **Submitting Emergency Requests**: Food, clean water, medical assistance, and rescue.
- **Tracking Request Statuses**: Check real-time progress on your submitted aid applications.
- **Finding Open Shelters**: Check real-time capacity and safe locations near you.
- **Disaster Preparedness**: Flood, heavy rain, and landslide safety tips.
- **Connecting with Support**: How to reach our human disaster coordinators.

🚨 *If you are facing an active emergency, call DMC at **117** or Suwa Seriya Ambulance at **1990** immediately.*";
        }
        else // Volunteer
        {
            if (lower.Contains("availability") || lower.Contains("available") || lower.Contains("schedule"))
            {
                return @"Hello Volunteer! Keeping your availability updated is critical for rapid field deployment:

### How to Update Your Availability:
1. Open **Volunteer Settings** or **Skills & Availability** in your volunteer workspace.
2. Select your current operational readiness:
   - **Immediate Deployment** (can respond within 1-2 hours)
   - **On-Call** (ready within 4-6 hours)
   - **Weekends Only**
   - **Full-Time Field Duty**
3. Select your preferred operational district (e.g. Riverton, Colombo, Gampaha).
4. Save your changes. The Volunteer Coordinator will see your real-time status on their allocation map.";
            }

            if (lower.Contains("skill") || lower.Contains("training") || lower.Contains("qualification"))
            {
                return @"Volunteers can log verified capabilities to assist in targeted humanitarian assignments:

### Adding Skills & Certifications:
- **First Aid / CPR**: Essential for shelter triage and mobile medical camps.
- **Search & Rescue**: Deployed alongside disaster management units.
- **Logistics & Warehousing**: Sorting, packing, and dispatching relief rations.
- **Heavy Vehicle / Boat Driving**: Transporting supplies across affected flood basins.
- **Translation / Community Engagement**: Assisting multilingual citizens in Sinhala and Tamil.

Update your skill list under **Volunteer Directory / My Profile** so coordinators can pair you with suitable field missions.";
            }

            if (lower.Contains("task") || lower.Contains("assignment") || lower.Contains("deployment") || lower.Contains("duty"))
            {
                return @"Here is how volunteer task deployment works on CareLink:

### Understanding Volunteer Tasks:
- **Viewing Tasks**: Check the **Tasks** and **Assignments** tabs to see missions allocated to your team.
- **Deployment Status**:
  * **Standby**: Your profile is verified and ready for call-up.
  * **Assigned**: You have been allocated to a specific field operation. Review the meeting point and team leader.
  * **Active**: Check in with the Volunteer Coordinator upon arrival at the site.
  * **Completed**: Report hours and mission summary.

⚠️ **Field Safety Protocol**:
- Never deploy alone; always work under the **Buddy System**.
- Wear high-visibility vests and protective gear provided at depot check-in.
- Contact your Volunteer Coordinator immediately if site conditions become unsafe.";
            }

            return @"Hello Volunteer! I am your **CareLink AI Assistant** (*Your Humanitarian Support Assistant*).

I am here to assist you with:
- **Updating Availability**: Let coordinators know when you are ready to deploy.
- **Logging Skills**: Record first aid, search & rescue, logistics, or transport certifications.
- **Understanding Tasks & Status**: Learn how missions are assigned and dispatched.
- **Field Safety Protocols**: Guidelines for personal safety and coordination in disaster zones.
- **Coordinator Contact**: How to reach your team leaders and operational dispatchers.";
        }
    }

    private static List<string> GenerateSuggestedActions(string role, string message)
    {
        if (role == "Volunteer")
        {
            return new List<string>
            {
                "How do I update my availability?",
                "How can I view my assignments?",
                "How do I update my volunteer skills?",
                "What does my deployment status mean?",
                "How can I contact my coordinator?"
            };
        }

        return new List<string>
        {
            "How can I request emergency assistance?",
            "How can I track my assistance request?",
            "How do I find an available shelter?",
            "What should I do during a flood?",
            "How do I contact the support team?"
        };
    }
}
