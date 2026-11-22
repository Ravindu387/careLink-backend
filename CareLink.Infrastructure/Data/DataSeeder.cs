using CareLink.Core.Entities;

namespace CareLink.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(CareLinkDbContext context)
    {
        if (!context.Shelters.Any())
        {
            var shelters = new List<Shelter>
            {
                new() { Id = "SH-001", Name = "Riverton Central School", Location = "Riverside, Ward 2", District = "Central District", Type = "Public School",    Manager = "A. Perera",      Status = "Near Capacity", Capacity = 660, CurrentOccupancy = 612, Reserved = 0 },
                new() { Id = "SH-002", Name = "North Community Hall",    Location = "North Ward",         District = "North District",   Type = "Community Centre", Manager = "M. Silva",       Status = "Active",        Capacity = 500, CurrentOccupancy = 284, Reserved = 0 },
                new() { Id = "SH-003", Name = "St. Mary Civic Centre",   Location = "Eastbank, Sector 4", District = "Eastbank",         Type = "Religious Centre", Manager = "D. Fernando",    Status = "Full",          Capacity = 450, CurrentOccupancy = 450, Reserved = 0 },
                new() { Id = "SH-004", Name = "Lakeside Sports Hall",    Location = "Old Town",           District = "West District",    Type = "Sports Facility",  Manager = "N. Wijesinghe",  Status = "Active",        Capacity = 400, CurrentOccupancy = 196, Reserved = 0 },
                new() { Id = "SH-005", Name = "Hillview College",        Location = "North Hill Road",    District = "North District",   Type = "Public School",    Manager = "R. Jayasinghe",  Status = "Maintenance",   Capacity = 320, CurrentOccupancy = 0,   Reserved = 46 }
            };

            await context.Shelters.AddRangeAsync(shelters);
            await context.SaveChangesAsync();
        }

        if (!context.AccommodationRequests.Any())
        {
            var baseTime = DateTime.UtcNow;

            var requests = new List<AccommodationRequest>
            {
                new()
                {
                    Id = "AR-2841",
                    Applicant = "D. Jayawardena",
                    FamilyMembers = 6,
                    CurrentLocation = "Riverside, Ward 2",
                    RequestedShelter = "Riverton Central",
                    ShelterId = "SH-001",
                    Priority = "Critical",
                    Status = "Under Review",
                    RequestDate = baseTime.AddHours(-1)
                },
                new()
                {
                    Id = "AR-2839",
                    Applicant = "M. Rasheed",
                    FamilyMembers = 3,
                    CurrentLocation = "Eastbank, Sector 4",
                    RequestedShelter = "North Community",
                    ShelterId = "SH-002",
                    Priority = "High",
                    Status = "Pending",
                    RequestDate = baseTime.AddHours(-2)
                },
                new()
                {
                    Id = "AR-2832",
                    Applicant = "S. Fernando",
                    FamilyMembers = 4,
                    CurrentLocation = "Old Town",
                    RequestedShelter = "Lakeside Sports",
                    ShelterId = "SH-004",
                    Priority = "Medium",
                    Status = "Approved",
                    RequestDate = baseTime.AddHours(-3)
                },
                new()
                {
                    Id = "AR-2821",
                    Applicant = "P. Nadarajah",
                    FamilyMembers = 2,
                    CurrentLocation = "North Ward",
                    RequestedShelter = "North Community",
                    ShelterId = "SH-002",
                    Priority = "High",
                    Status = "Assigned",
                    RequestDate = baseTime.AddHours(-4)
                },
                new()
                {
                    Id = "AR-2815",
                    Applicant = "K. Wickramasinghe",
                    FamilyMembers = 5,
                    CurrentLocation = "Riverside, Ward 3",
                    RequestedShelter = "Riverton Central",
                    ShelterId = "SH-001",
                    Priority = "Critical",
                    Status = "Pending",
                    RequestDate = baseTime.AddHours(-5)
                },
                new()
                {
                    Id = "AR-2808",
                    Applicant = "A. Gunasekara",
                    FamilyMembers = 1,
                    CurrentLocation = "West End, Block C",
                    RequestedShelter = "St. Mary Civic Centre",
                    ShelterId = "SH-003",
                    Priority = "Low",
                    Status = "Approved",
                    RequestDate = baseTime.AddHours(-6)
                },
                new()
                {
                    Id = "AR-2795",
                    Applicant = "R. Mohamed",
                    FamilyMembers = 7,
                    CurrentLocation = "Lowland Flats",
                    RequestedShelter = "Riverton Central",
                    ShelterId = "SH-001",
                    Priority = "Critical",
                    Status = "Under Review",
                    RequestDate = baseTime.AddHours(-7)
                },
                new()
                {
                    Id = "AR-2782",
                    Applicant = "T. Perera",
                    FamilyMembers = 3,
                    CurrentLocation = "South Quay",
                    RequestedShelter = "Lakeside Sports",
                    ShelterId = "SH-004",
                    Priority = "Medium",
                    Status = "Assigned",
                    RequestDate = baseTime.AddHours(-8)
                },
                new()
                {
                    Id = "AR-2771",
                    Applicant = "N. Silva",
                    FamilyMembers = 4,
                    CurrentLocation = "Eastbank, Sector 2",
                    RequestedShelter = "North Community",
                    ShelterId = "SH-002",
                    Priority = "High",
                    Status = "Pending",
                    RequestDate = baseTime.AddDays(-1)
                },
                new()
                {
                    Id = "AR-2760",
                    Applicant = "H. De Silva",
                    FamilyMembers = 2,
                    CurrentLocation = "Old Town Central",
                    RequestedShelter = "St. Mary Civic Centre",
                    ShelterId = "SH-003",
                    Priority = "Medium",
                    Status = "Approved",
                    RequestDate = baseTime.AddDays(-1).AddHours(-2)
                },
                new()
                {
                    Id = "AR-2755",
                    Applicant = "C. Rajapaksa",
                    FamilyMembers = 5,
                    CurrentLocation = "North Hills",
                    RequestedShelter = "North Community",
                    ShelterId = "SH-002",
                    Priority = "Critical",
                    Status = "Pending",
                    RequestDate = baseTime.AddDays(-2)
                },
                new()
                {
                    Id = "AR-2748",
                    Applicant = "V. Balasubramaniam",
                    FamilyMembers = 3,
                    CurrentLocation = "Riverside, Ward 1",
                    RequestedShelter = "Riverton Central",
                    ShelterId = "SH-001",
                    Priority = "High",
                    Status = "Assigned",
                    RequestDate = baseTime.AddDays(-2).AddHours(-4)
                }
            };

            await context.AccommodationRequests.AddRangeAsync(requests);
            await context.SaveChangesAsync();
        }

        if (!context.EmergencyContacts.Any())
        {
            var contacts = new List<EmergencyContact>
            {
                new() { Name = "Disaster Management Centre", Number = "117", Department = "Emergency Coordination" },
                new() { Name = "Emergency Medical", Number = "1990", Department = "Medical Ambulance" },
                new() { Name = "Police Emergency", Number = "119", Department = "Law Enforcement" },
                new() { Name = "CareLink Coordination", Number = "011 234 8800", Department = "Shelter Support" }
            };

            await context.EmergencyContacts.AddRangeAsync(contacts);
            await context.SaveChangesAsync();
        }

        if (!context.DisasterUpdates.Any())
        {
            var now = DateTime.UtcNow;

            var updates = new List<DisasterUpdate>
            {
                new()
                {
                    Id = "DU-101",
                    Type = "Critical",
                    Title = "Immediate evacuation order - Riverside North",
                    Description = "Residents in low-lying zones N2 and N3 must move to assigned shelters before 16:00. Water levels expected to rise rapidly.",
                    Source = "National Disaster Authority",
                    Location = "North Ward",
                    PublishedBy = "National Disaster Authority",
                    Instructions = "Residents in low-lying areas must move to designated safe shelters immediately. Follow evacuation routes and listen to emergency personnel.",
                    Archived = false,
                    IsDraft = false,
                    CreatedAt = now.AddHours(-1)
                },
                new()
                {
                    Id = "DU-102",
                    Type = "Warning",
                    Title = "Station Road access restricted",
                    Description = "The eastern approach to Riverton Central is closed. Use Lake Road entrance.",
                    Source = "Municipal Operations",
                    Location = "Riverton Central",
                    PublishedBy = "Municipal Operations",
                    Instructions = "Avoid Station Road. Emergency vehicles should use Lake Road entrance until further notice.",
                    Archived = false,
                    IsDraft = false,
                    CreatedAt = now.AddHours(-2)
                },
                new()
                {
                    Id = "DU-103",
                    Type = "Update",
                    Title = "New clean water distribution point",
                    Description = "A verified distribution point is operational at Central Market from 14:00.",
                    Source = "CareLink Coordination",
                    Location = "Central Market",
                    PublishedBy = "CareLink Coordination",
                    Instructions = "Residents can collect clean drinking water at the Central Market distribution point. Bring personal water containers.",
                    Archived = false,
                    IsDraft = false,
                    CreatedAt = now.AddHours(-3)
                },
                new()
                {
                    Id = "DU-104",
                    Type = "Update",
                    Title = "Weather outlook for the next 24 hours",
                    Description = "Heavy rainfall is expected to reduce after midnight. Continue monitoring official alerts.",
                    Source = "Meteorological Service",
                    Location = "Greater Riverton",
                    PublishedBy = "Meteorological Service",
                    Instructions = "Continue monitoring official weather announcements. Avoid unnecessary travel during heavy rainfall.",
                    Archived = false,
                    IsDraft = false,
                    CreatedAt = now.AddHours(-4)
                },
                new()
                {
                    Id = "DU-105",
                    Type = "Announcement",
                    Title = "Emergency medical support available",
                    Description = "Additional medical support teams are available at North Community Hall.",
                    Source = "CareLink Coordination",
                    Location = "North Community",
                    PublishedBy = "CareLink Coordination",
                    Instructions = "Residents requiring urgent medical assistance should contact the emergency medical team at North Community Hall.",
                    Archived = false,
                    IsDraft = false,
                    CreatedAt = now.AddHours(-5)
                }
            };

            await context.DisasterUpdates.AddRangeAsync(updates);
            await context.SaveChangesAsync();
        }

        if (!context.SafetyChecks.Any())
        {
            var checks = new List<SafetyCheck>
            {
                new() { Title = "Fire safety check", Description = "Completed · 21 Jun", Color = "green", IconKey = "Check", Category = "FireSafety" },
                new() { Title = "Sanitation inspection", Description = "Due today", Color = "orange", IconKey = "AlertTriangle", Category = "Sanitation" },
                new() { Title = "Evacuation drill", Description = "Scheduled · 24 Jun", Color = "blue", IconKey = "ShieldCheck", Category = "Evacuation" },
                new() { Title = "Accessibility review", Description = "2 concerns open", Color = "red", IconKey = "Accessibility", Category = "Accessibility" }
            };

            await context.SafetyChecks.AddRangeAsync(checks);
            await context.SaveChangesAsync();
        }

        if (!context.HealthSafetyIncidents.Any())
        {
            var now = DateTime.UtcNow;

            var incidents = new List<HealthSafetyIncident>
            {
                new()
                {
                    Id = "HI-205",
                    Type = "Fire safety",
                    Location = "Dormitory Block C",
                    ReportedBy = "K. Fernando",
                    Severity = "Critical",
                    AssignedStaff = "Facilities Team",
                    Status = "Action Required",
                    Description = "Fire alarm triggered due to sensor fault in Dormitory C. Immediate emergency inspection needed.",
                    IncidentDate = now.AddHours(-1),
                    CreatedAt = now.AddHours(-1)
                },
                new()
                {
                    Id = "HI-203",
                    Type = "Water safety",
                    Location = "Main Water Storage",
                    ReportedBy = "T. Jayasinghe",
                    Severity = "High",
                    AssignedStaff = "Maintenance",
                    Status = "Under Investigation",
                    Description = "Water pressure drop and turbidity observed in drinking water line after heavy rainfall.",
                    IncidentDate = now.AddHours(-3),
                    CreatedAt = now.AddHours(-3)
                },
                new()
                {
                    Id = "HI-202",
                    Type = "Medical assistance",
                    Location = "Resident RES-10831",
                    ReportedBy = "Nurse M. Rao",
                    Severity = "High",
                    AssignedStaff = "Dr. M. Rao",
                    Status = "Action Required",
                    Description = "A resident requires medical assistance and immediate assessment by the medical team.",
                    IncidentDate = now.AddHours(-2),
                    CreatedAt = now.AddHours(-2)
                },
                new()
                {
                    Id = "HI-200",
                    Type = "Food safety",
                    Location = "Dining Hall Central",
                    ReportedBy = "Nurse M. Rao",
                    Severity = "Medium",
                    AssignedStaff = "Dr. M. Rao",
                    Status = "Under Investigation",
                    Description = "Report of resident showing mild allergic reaction to unlabelled ingredient in midday meal.",
                    IncidentDate = now.AddHours(-6),
                    CreatedAt = now.AddHours(-6)
                },
                new()
                {
                    Id = "HI-198",
                    Type = "Sanitation inspection",
                    Location = "Kitchen Area B",
                    ReportedBy = "S. Nirosha",
                    Severity = "Medium",
                    AssignedStaff = "Facilities Team",
                    Status = "Under Investigation",
                    Description = "A sanitation concern was reported in the kitchen area. Inspection is required.",
                    IncidentDate = now.AddHours(-4),
                    CreatedAt = now.AddHours(-4)
                },
                new()
                {
                    Id = "HI-195",
                    Type = "Evacuation",
                    Location = "Assembly Area 2",
                    ReportedBy = "Security Team",
                    Severity = "Low",
                    AssignedStaff = "Security Team",
                    Status = "Resolved",
                    Description = "Emergency exit pathway obstructed by temporary supply crates; crates relocated and pathway cleared.",
                    IncidentDate = now.AddDays(-1).AddHours(-2),
                    CreatedAt = now.AddDays(-1).AddHours(-2)
                },
                new()
                {
                    Id = "HI-191",
                    Type = "Accessibility concern",
                    Location = "Entrance Ramp",
                    ReportedBy = "A. Perera",
                    Severity = "Low",
                    AssignedStaff = "Maintenance",
                    Status = "Resolved",
                    Description = "An accessibility issue was identified near the entrance ramp and referred to maintenance.",
                    IncidentDate = now.AddDays(-1),
                    CreatedAt = now.AddDays(-1)
                },
                new()
                {
                    Id = "HI-188",
                    Type = "Medical assistance",
                    Location = "Shelter Clinic",
                    ReportedBy = "Dr. M. Rao",
                    Severity = "Critical",
                    AssignedStaff = "Dr. M. Rao",
                    Status = "Resolved",
                    Description = "Elderly resident with acute respiratory distress stabilized and transferred to base hospital.",
                    IncidentDate = now.AddDays(-2),
                    CreatedAt = now.AddDays(-2)
                },
                new()
                {
                    Id = "HI-185",
                    Type = "Sanitation inspection",
                    Location = "Restroom Block East",
                    ReportedBy = "S. Nirosha",
                    Severity = "Low",
                    AssignedStaff = "Facilities Team",
                    Status = "Resolved",
                    Description = "Plumbing drain overflow contained and sanitized thoroughly.",
                    IncidentDate = now.AddDays(-3),
                    CreatedAt = now.AddDays(-3)
                }
            };

            await context.HealthSafetyIncidents.AddRangeAsync(incidents);
            await context.SaveChangesAsync();
        }

        if (!context.MaintenanceRequests.Any())
        {
            var now = DateTime.UtcNow;

            var requests = new List<MaintenanceRequest>
            {
                new()
                {
                    Id = "MR-341",
                    Issue = "Water pump pressure failure",
                    Category = "Plumbing",
                    Shelter = "St. Mary Civic",
                    Room = "Utility Room",
                    Priority = "Critical",
                    Technician = "N. Kumara",
                    Status = "In Progress",
                    Description = "Water pump pressure has dropped significantly. The maintenance team needs to inspect the pump and restore normal water supply.",
                    ReportedAt = now.AddHours(-2),
                    CreatedAt = now.AddHours(-2)
                },
                new()
                {
                    Id = "MR-337",
                    Issue = "Lighting fault",
                    Category = "Electrical",
                    Shelter = "Riverton Central",
                    Room = "Corridor B",
                    Priority = "Medium",
                    Technician = "D. Silva",
                    Status = "Assigned",
                    Description = "Several corridor lights are not functioning. Electrical inspection and replacement are required.",
                    ReportedAt = now.AddHours(-5),
                    CreatedAt = now.AddHours(-5)
                },
                new()
                {
                    Id = "MR-329",
                    Issue = "Blocked drainage",
                    Category = "Plumbing",
                    Shelter = "North Community",
                    Room = "Rear Yard",
                    Priority = "High",
                    Technician = "M. Perera",
                    Status = "Completed",
                    Description = "Drainage blockage reported in the rear yard. Cleaning and inspection have been completed.",
                    ReportedAt = now.AddDays(-1),
                    CreatedAt = now.AddDays(-1)
                },
                new()
                {
                    Id = "MR-325",
                    Issue = "HVAC ventilation fan noise",
                    Category = "HVAC",
                    Shelter = "Lakeside Sports",
                    Room = "Dormitory 3",
                    Priority = "Low",
                    Technician = "Facilities Team",
                    Status = "Pending",
                    Description = "Excessive vibration and noise from overhead ventilation unit in Dormitory 3.",
                    ReportedAt = now.AddDays(-1).AddHours(-3),
                    CreatedAt = now.AddDays(-1).AddHours(-3)
                },
                new()
                {
                    Id = "MR-320",
                    Issue = "Window latch broken",
                    Category = "Structural",
                    Shelter = "Riverton Central",
                    Room = "Room 104",
                    Priority = "Medium",
                    Technician = "Maintenance Team",
                    Status = "On Hold",
                    Description = "Security window latch loose and unable to lock completely. Awaiting replacement hardware.",
                    ReportedAt = now.AddDays(-2),
                    CreatedAt = now.AddDays(-2)
                }
            };

            await context.MaintenanceRequests.AddRangeAsync(requests);
            await context.SaveChangesAsync();
        }

        if (!context.Notifications.Any())
        {
            var now = DateTime.UtcNow;

            var notifications = new List<NotificationItem>
            {
                new()
                {
                    Id = 1,
                    Title = "New critical accommodation request",
                    Description = "AR-2841 · Family of 6 in Riverside requires accessible shelter.",
                    Type = "Requests",
                    Icon = "request",
                    Unread = true,
                    Details = "A new critical accommodation request has been submitted. A family of six in Riverside requires accessible shelter accommodation. Review the request and assign an appropriate shelter.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddMinutes(-2)
                },
                new()
                {
                    Id = 2,
                    Title = "Shelter capacity warning",
                    Description = "Riverton Central has reached 92% occupancy.",
                    Type = "Critical",
                    Icon = "warning",
                    Unread = true,
                    Details = "Riverton Central School has reached 92% occupancy. Review current allocations and consider redirecting incoming residents to alternative shelters.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddMinutes(-18)
                },
                new()
                {
                    Id = 3,
                    Title = "Inventory below minimum level",
                    Description = "Family Food Packs are below the configured minimum.",
                    Type = "Inventory",
                    Icon = "inventory",
                    Unread = true,
                    Details = "Family Food Packs have fallen below the configured minimum stock level. Review the inventory and arrange replenishment or redistribution.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddMinutes(-34)
                },
                new()
                {
                    Id = 4,
                    Title = "Medical assistance request assigned",
                    Description = "HI-202 has been assigned to Dr. Malini Rao.",
                    Type = "Requests",
                    Icon = "medical",
                    Unread = false,
                    Details = "Medical incident HI-202 has been assigned to Dr. Malini Rao. The assigned medical officer can now review the case and provide assistance.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-1)
                },
                new()
                {
                    Id = 5,
                    Title = "Maintenance request updated",
                    Description = "Water pump repair is now in progress.",
                    Type = "System",
                    Icon = "maintenance",
                    Unread = false,
                    Details = "The maintenance team has updated the water pump repair request. The repair is currently in progress.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-2)
                },
                new()
                {
                    Id = 6,
                    Title = "New resident registration",
                    Description = "RES-10842 was checked into Room A-12.",
                    Type = "System",
                    Icon = "resident",
                    Unread = false,
                    Details = "Resident RES-10842 has successfully completed registration and has been checked into Room A-12.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-3)
                },
                new()
                {
                    Id = 7,
                    Title = "Critical flood warning updated",
                    Description = "North Ward evacuation instructions have changed.",
                    Type = "Critical",
                    Icon = "warning",
                    Unread = true,
                    Details = "Updated flood evacuation instructions are available for North Ward. Review the latest disaster update before coordinating resident movements.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-4)
                },
                new()
                {
                    Id = 8,
                    Title = "Resource distribution completed",
                    Description = "Food supplies delivered to North Community Hall.",
                    Type = "Inventory",
                    Icon = "inventory",
                    Unread = false,
                    Details = "The scheduled food supply distribution has been completed at North Community Hall.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-5)
                },
                new()
                {
                    Id = 9,
                    Title = "Staff shift updated",
                    Description = "Evening shift coverage has been updated.",
                    Type = "System",
                    Icon = "resident",
                    Unread = false,
                    Details = "The shelter administrator has updated the evening shift schedule.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-6)
                },
                new()
                {
                    Id = 10,
                    Title = "New accommodation request",
                    Description = "AR-2850 requires review and approval.",
                    Type = "Requests",
                    Icon = "request",
                    Unread = true,
                    Details = "A new accommodation request requires administrator review and shelter assignment.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-7)
                },
                new()
                {
                    Id = 11,
                    Title = "First aid stock alert",
                    Description = "First Aid Kits are below the minimum quantity.",
                    Type = "Inventory",
                    Icon = "inventory",
                    Unread = true,
                    Details = "The available First Aid Kit stock has fallen below the configured minimum quantity.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-8)
                },
                new()
                {
                    Id = 12,
                    Title = "Shelter inspection completed",
                    Description = "The latest safety inspection was completed.",
                    Type = "System",
                    Icon = "maintenance",
                    Unread = false,
                    Details = "The latest shelter safety inspection has been completed and recorded.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-9)
                },
                new()
                {
                    Id = 13,
                    Title = "New resident assistance request",
                    Description = "A resident requires additional support.",
                    Type = "Requests",
                    Icon = "medical",
                    Unread = false,
                    Details = "A new resident assistance request has been recorded.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-10)
                },
                new()
                {
                    Id = 14,
                    Title = "Resource delivery scheduled",
                    Description = "Additional supplies are expected tomorrow.",
                    Type = "Inventory",
                    Icon = "inventory",
                    Unread = false,
                    Details = "An additional resource delivery has been scheduled.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-11)
                },
                new()
                {
                    Id = 15,
                    Title = "Shelter occupancy report generated",
                    Description = "The latest operational report is available.",
                    Type = "System",
                    Icon = "resident",
                    Unread = false,
                    Details = "The latest shelter occupancy report has been generated.",
                    Role = "ShelterAdmin",
                    CreatedAt = now.AddHours(-12)
                }
            };

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
        }

        if (!context.Notifications.Any(n => n.Role == "FundingOfficer"))
        {
            var now = DateTime.UtcNow;
            var fundingNotifications = new List<NotificationItem>
            {
                new()
                {
                    Title = "High-risk contribution flagged",
                    Description = "DON-88398 · $18,420 requires investigation.",
                    Type = "Flags",
                    Icon = "alert",
                    Unread = true,
                    Reference = "DON-88398",
                    Details = "A contribution of $18,420 has been flagged due to an unusual source pattern. An authorised investigator should review the donor information and supporting documents.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddMinutes(-3)
                },
                new()
                {
                    Title = "16 donations awaiting verification",
                    Description = "Combined value $82,400.",
                    Type = "Verification",
                    Icon = "clock",
                    Unread = true,
                    Reference = "VER-0016",
                    Details = "There are 16 donations awaiting verification with a combined value of $82,400. Review the submitted donor information and supporting documents.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddMinutes(-18)
                },
                new()
                {
                    Title = "Allocation approved",
                    Description = "ALL-4408 · $180,000 shelter support.",
                    Type = "Allocation",
                    Icon = "check",
                    Unread = true,
                    Reference = "ALL-4408",
                    Details = "The allocation of $180,000 for emergency shelter support has been approved. The allocation is associated with the Flood Response 2025 campaign.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddMinutes(-34)
                },
                new()
                {
                    Title = "Expenditure receipt uploaded",
                    Description = "EXP-9008 · Health Response Unit.",
                    Type = "Expenditure",
                    Icon = "file",
                    Unread = false,
                    Reference = "EXP-9008",
                    Details = "A new expenditure receipt has been uploaded by the Health Response Unit. The receipt requires verification against the approved expenditure.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-1)
                },
                new()
                {
                    Title = "Campaign reached 75%",
                    Description = "Clean Water Appeal · $318K raised.",
                    Type = "Campaigns",
                    Icon = "campaign",
                    Unread = false,
                    Reference = "CAM-112",
                    Details = "The Clean Water Appeal campaign has reached 75% of its fundraising target, with $318K raised so far.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-2)
                },
                new()
                {
                    Title = "Transparency report approved",
                    Description = "Weekly Funding Summary is ready to publish.",
                    Type = "Reports",
                    Icon = "shield",
                    Unread = false,
                    Reference = "REP-204",
                    Details = "The Weekly Funding Summary has passed its review process and is ready for publication after authorised approval.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-3)
                },
                new()
                {
                    Title = "New donor registration",
                    Description = "HealthAccess International · Verification submitted.",
                    Type = "Verification",
                    Icon = "check",
                    Unread = true,
                    Reference = "DNR-2028",
                    Details = "HealthAccess International submitted a new donor registration. Review the organisation profile and verification documents.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-4)
                },
                new()
                {
                    Title = "Campaign deadline approaching",
                    Description = "Emergency Shelter Fund · 3 days remaining.",
                    Type = "Campaigns",
                    Icon = "campaign",
                    Unread = true,
                    Reference = "CAM-108",
                    Details = "The Emergency Shelter Fund campaign is approaching its closing date. Review the fundraising progress and prepare the final report.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-5)
                },
                new()
                {
                    Title = "Flagged cross-border transfer",
                    Description = "DON-88412 · $35,000 international wire review.",
                    Type = "Flags",
                    Icon = "alert",
                    Unread = true,
                    Reference = "DON-88412",
                    Details = "An international cross-border transfer exceeding single-donor thresholds requires dual-signoff and AML check.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-7)
                },
                new()
                {
                    Title = "Corporate match verification ready",
                    Description = "GlobalTech Foundation · $50,000 matching grant.",
                    Type = "Verification",
                    Icon = "clock",
                    Unread = true,
                    Reference = "VER-0017",
                    Details = "GlobalTech Foundation 1:1 corporate match documentation has been uploaded for verification.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-9)
                },
                new()
                {
                    Title = "Medical supply allocation released",
                    Description = "ALL-4409 · $95,000 allocated to Mobile Clinics.",
                    Type = "Allocation",
                    Icon = "check",
                    Unread = false,
                    Reference = "ALL-4409",
                    Details = "Funds released for emergency pharmaceuticals and trauma kit distribution.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-12)
                },
                new()
                {
                    Title = "Quarterly audit report completed",
                    Description = "Q3 Independent Financial Audit signed off.",
                    Type = "Reports",
                    Icon = "shield",
                    Unread = false,
                    Reference = "REP-205",
                    Details = "The Q3 External Audit report is now available in Transparency Reports with zero unresolved non-compliances.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-15)
                },
                new()
                {
                    Title = "Winter Warmth Initiative launched",
                    Description = "Target $200,000 for displaced populations.",
                    Type = "Campaigns",
                    Icon = "campaign",
                    Unread = true,
                    Reference = "CAM-115",
                    Details = "Public fundraising campaign officially activated across regional and online donor portals.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-18)
                },
                new()
                {
                    Title = "Fuel procurement receipt pending sign-off",
                    Description = "EXP-9012 · $14,200 generator logistics.",
                    Type = "Expenditure",
                    Icon = "file",
                    Unread = false,
                    Reference = "EXP-9012",
                    Details = "Emergency power generator fuel bills submitted by central shelter facility.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddHours(-22)
                },
                new()
                {
                    Title = "Suspicious duplicate donor identity",
                    Description = "DON-88430 · Potential duplicate registration detected.",
                    Type = "Flags",
                    Icon = "alert",
                    Unread = true,
                    Reference = "DON-88430",
                    Details = "Automated fraud detection engine flagged two donations originating from identical IP addresses with conflicting billing details.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "Monthly Transparency Digest generated",
                    Description = "Donor digest ready for automated distribution.",
                    Type = "Reports",
                    Icon = "shield",
                    Unread = false,
                    Reference = "REP-206",
                    Details = "Monthly accountability report compiled for distribution to all registered tier 1 and tier 2 donors.",
                    Role = "FundingOfficer",
                    CreatedAt = now.AddDays(-2)
                }
            };

            await context.Notifications.AddRangeAsync(fundingNotifications);
            await context.SaveChangesAsync();
        }

        if (!context.NotificationPreferences.Any(p => p.Role == "FundingOfficer"))
        {
            var fundingPref = new NotificationPreference
            {
                Role = "FundingOfficer",
                InApp = true,
                Email = true,
                Sms = false,
                Critical = true,
                Verification = true,
                Campaigns = true,
                Reports = true,
                UpdatedAt = DateTime.UtcNow
            };

            await context.NotificationPreferences.AddAsync(fundingPref);
            await context.SaveChangesAsync();
        }

        if (!context.NotificationPreferences.Any(p => p.Role == "ShelterAdmin"))
        {
            var defaultPref = new NotificationPreference
            {
                Role = "ShelterAdmin",
                InApp = true,
                Email = true,
                Sms = false,
                Critical = true,
                Requests = true,
                Inventory = true,
                System = false,
                UpdatedAt = DateTime.UtcNow
            };

            await context.NotificationPreferences.AddAsync(defaultPref);
            await context.SaveChangesAsync();
        }

        if (!context.OccupancyThresholds.Any())
        {
            var defaultThreshold = new OccupancyThreshold
            {
                Id = 1,
                Warning = 80,
                Critical = 95,
                UpdatedAt = DateTime.UtcNow
            };

            await context.OccupancyThresholds.AddAsync(defaultThreshold);
            await context.SaveChangesAsync();
        }

        if (!context.ReportDefinitions.Any())
        {
            var reports = new List<ReportDefinition>
            {
                new()
                {
                    Id = "REP-01",
                    Title = "Shelter Occupancy Report",
                    Description = "Capacity, occupancy, and available beds",
                    Category = "Shelter",
                    IconKey = "file",
                    IsActive = true
                },
                new()
                {
                    Id = "REP-02",
                    Title = "Resident Registration Report",
                    Description = "Admissions, departures, and demographics",
                    Category = "Residents",
                    IconKey = "users",
                    IsActive = true
                },
                new()
                {
                    Id = "REP-03",
                    Title = "Bed Availability Report",
                    Description = "Room and bed status by shelter",
                    Category = "Shelter",
                    IconKey = "bed",
                    IsActive = true
                },
                new()
                {
                    Id = "REP-04",
                    Title = "Resource Inventory Report",
                    Description = "Stock levels, distribution, and expiry",
                    Category = "Resources",
                    IconKey = "package",
                    IsActive = true
                },
                new()
                {
                    Id = "REP-05",
                    Title = "Accommodation Request Report",
                    Description = "Request outcomes and assignment times",
                    Category = "Requests",
                    IconKey = "clipboard",
                    IsActive = true
                },
                new()
                {
                    Id = "REP-06",
                    Title = "Staff Activity Report",
                    Description = "Shifts, attendance, and responsibilities",
                    Category = "Staff",
                    IconKey = "user",
                    IsActive = true
                },
                new()
                {
                    Id = "REP-07",
                    Title = "Health & Safety Report",
                    Description = "Incidents, inspections, and resolutions",
                    Category = "Health",
                    IconKey = "heart",
                    IsActive = true
                },
                new()
                {
                    Id = "REP-08",
                    Title = "Shelter Performance Overview",
                    Description = "Cross-shelter operational metrics",
                    Category = "Performance",
                    IconKey = "chart",
                    IsActive = true
                }
            };

            await context.ReportDefinitions.AddRangeAsync(reports);
            await context.SaveChangesAsync();
        }

        if (!context.Residents.Any())
        {
            var baseDate = DateTime.UtcNow;

            var residents = new List<Resident>
            {
                new()
                {
                    Id = "RES-10842",
                    Name = "Nimali Perera",
                    Age = 42,
                    Gender = "Female",
                    Contact = "077 234 8910",
                    FamilyGroup = "FG-2041",
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central",
                    Room = "A-12",
                    Bed = "B-02",
                    CheckIn = baseDate.AddDays(-1),
                    Status = "Active",
                    AssistanceNeeds = "None",
                    CreatedAt = baseDate.AddDays(-1)
                },
                new()
                {
                    Id = "RES-10839",
                    Name = "Saman Kumara",
                    Age = 37,
                    Gender = "Male",
                    Contact = "071 528 1203",
                    FamilyGroup = "FG-2038",
                    ShelterId = "SH-002",
                    ShelterName = "North Community",
                    Room = "B-07",
                    Bed = "B-01",
                    CheckIn = baseDate.AddDays(-1),
                    Status = "Active",
                    AssistanceNeeds = "None",
                    CreatedAt = baseDate.AddDays(-1)
                },
                new()
                {
                    Id = "RES-10831",
                    Name = "Fathima Rizna",
                    Age = 68,
                    Gender = "Female",
                    Contact = "075 831 9012",
                    FamilyGroup = "FG-2031",
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central",
                    Room = "A-04",
                    Bed = "B-03",
                    CheckIn = baseDate.AddDays(-2),
                    Status = "Assistance",
                    AssistanceNeeds = "Medical assistance",
                    CreatedAt = baseDate.AddDays(-2)
                },
                new()
                {
                    Id = "RES-10822",
                    Name = "Kamal De Silva",
                    Age = 29,
                    Gender = "Male",
                    Contact = "076 445 8831",
                    FamilyGroup = "FG-2024",
                    ShelterId = "SH-004",
                    ShelterName = "Lakeside Sports",
                    Room = "C-11",
                    Bed = "B-02",
                    CheckIn = baseDate.AddDays(-2),
                    Status = "Active",
                    AssistanceNeeds = "None",
                    CreatedAt = baseDate.AddDays(-2)
                },
                new()
                {
                    Id = "RES-10815",
                    Name = "Anusha Fernando",
                    Age = 34,
                    Gender = "Female",
                    Contact = "077 651 2389",
                    FamilyGroup = "FG-2020",
                    ShelterId = "SH-002",
                    ShelterName = "North Community",
                    Room = "B-08",
                    Bed = "B-03",
                    CheckIn = baseDate.AddDays(-3),
                    Status = "Active",
                    AssistanceNeeds = "None",
                    CreatedAt = baseDate.AddDays(-3)
                },
                new()
                {
                    Id = "RES-10809",
                    Name = "Mohamed Irfan",
                    Age = 51,
                    Gender = "Male",
                    Contact = "071 872 4510",
                    FamilyGroup = "FG-2018",
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central",
                    Room = "A-08",
                    Bed = "B-01",
                    CheckIn = baseDate.AddDays(-3),
                    Status = "Assistance",
                    AssistanceNeeds = "Mobility support",
                    CreatedAt = baseDate.AddDays(-3)
                },
                new()
                {
                    Id = "RES-10798",
                    Name = "Kavindi Silva",
                    Age = 24,
                    Gender = "Female",
                    Contact = "075 234 6789",
                    FamilyGroup = "FG-2012",
                    ShelterId = "SH-004",
                    ShelterName = "Lakeside Sports",
                    Room = "C-05",
                    Bed = "B-04",
                    CheckIn = baseDate.AddDays(-4),
                    Status = "Checked Out",
                    AssistanceNeeds = "None",
                    CheckedOutAt = baseDate,
                    CreatedAt = baseDate.AddDays(-4)
                },
                new()
                {
                    Id = "RES-10791",
                    Name = "Ruwan Jayasinghe",
                    Age = 46,
                    Gender = "Male",
                    Contact = "077 987 6543",
                    FamilyGroup = "FG-2009",
                    ShelterId = "SH-002",
                    ShelterName = "North Community",
                    Room = "B-02",
                    Bed = "B-02",
                    CheckIn = baseDate.AddDays(-4),
                    Status = "Active",
                    AssistanceNeeds = "None",
                    CreatedAt = baseDate.AddDays(-4)
                },
                new()
                {
                    Id = "RES-10785",
                    Name = "Chandrika Bandara",
                    Age = 58,
                    Gender = "Female",
                    Contact = "071 456 7890",
                    FamilyGroup = "FG-2005",
                    ShelterId = "SH-003",
                    ShelterName = "St. Mary Civic Centre",
                    Room = "D-03",
                    Bed = "B-01",
                    CheckIn = baseDate.AddDays(-5),
                    Status = "Assistance",
                    AssistanceNeeds = "Psychosocial support",
                    CreatedAt = baseDate.AddDays(-5)
                },
                new()
                {
                    Id = "RES-10778",
                    Name = "Dinesh Weerasinghe",
                    Age = 22,
                    Gender = "Male",
                    Contact = "078 123 9874",
                    FamilyGroup = "Individual",
                    ShelterId = "SH-003",
                    ShelterName = "St. Mary Civic Centre",
                    Room = "D-06",
                    Bed = "B-02",
                    CheckIn = baseDate.AddDays(-5),
                    Status = "Active",
                    AssistanceNeeds = "None",
                    CreatedAt = baseDate.AddDays(-5)
                },
                new()
                {
                    Id = "RES-10770",
                    Name = "Sunil Shantha",
                    Age = 63,
                    Gender = "Male",
                    Contact = "077 345 6128",
                    FamilyGroup = "FG-1998",
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central",
                    Room = "A-15",
                    Bed = "B-01",
                    CheckIn = baseDate.AddDays(-6),
                    Status = "Assistance",
                    AssistanceNeeds = "Food assistance",
                    CreatedAt = baseDate.AddDays(-6)
                },
                new()
                {
                    Id = "RES-10762",
                    Name = "Priyani Alwis",
                    Age = 31,
                    Gender = "Female",
                    Contact = "072 876 5432",
                    FamilyGroup = "Individual",
                    ShelterId = "SH-004",
                    ShelterName = "Lakeside Sports",
                    Room = "C-08",
                    Bed = "B-03",
                    CheckIn = baseDate.AddDays(-6),
                    Status = "Active",
                    AssistanceNeeds = "None",
                    CreatedAt = baseDate.AddDays(-6)
                }
            };

            await context.Residents.AddRangeAsync(residents);
            await context.SaveChangesAsync();
        }

        if (!context.ResourceItems.Any())
        {
            var baseDate = DateTime.UtcNow;

            var resources = new List<ResourceItem>
            {
                new()
                {
                    Id = "INV-104",
                    Name = "Drinking Water - 10L",
                    Category = "Food & Water",
                    Available = 842,
                    Minimum = 500,
                    Unit = "Units",
                    ExpiryDate = null,
                    LastUpdated = baseDate,
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central School",
                    CreatedAt = baseDate.AddDays(-10)
                },
                new()
                {
                    Id = "INV-107",
                    Name = "Family Food Packs",
                    Category = "Food & Water",
                    Available = 84,
                    Minimum = 120,
                    Unit = "Packs",
                    ExpiryDate = baseDate.AddDays(14),
                    LastUpdated = baseDate,
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central School",
                    CreatedAt = baseDate.AddDays(-10)
                },
                new()
                {
                    Id = "INV-112",
                    Name = "Thermal Blankets",
                    Category = "Bedding",
                    Available = 318,
                    Minimum = 150,
                    Unit = "Units",
                    ExpiryDate = null,
                    LastUpdated = baseDate,
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central School",
                    CreatedAt = baseDate.AddDays(-10)
                },
                new()
                {
                    Id = "INV-118",
                    Name = "First Aid Kits",
                    Category = "Medical",
                    Available = 12,
                    Minimum = 25,
                    Unit = "Kits",
                    ExpiryDate = baseDate.AddDays(25),
                    LastUpdated = baseDate,
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central School",
                    CreatedAt = baseDate.AddDays(-10)
                },
                new()
                {
                    Id = "INV-124",
                    Name = "Hygiene Kits",
                    Category = "Hygiene",
                    Available = 0,
                    Minimum = 50,
                    Unit = "Kits",
                    ExpiryDate = baseDate.AddDays(60),
                    LastUpdated = baseDate,
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central School",
                    CreatedAt = baseDate.AddDays(-10)
                },
                new()
                {
                    Id = "INV-130",
                    Name = "Baby Food Packs",
                    Category = "Food & Water",
                    Available = 46,
                    Minimum = 100,
                    Unit = "Packs",
                    ExpiryDate = baseDate.AddDays(20),
                    LastUpdated = baseDate.AddDays(-1),
                    ShelterId = "SH-002",
                    ShelterName = "North Community Hall",
                    CreatedAt = baseDate.AddDays(-10)
                },
                new()
                {
                    Id = "INV-135",
                    Name = "Medical Gloves",
                    Category = "Medical",
                    Available = 210,
                    Minimum = 100,
                    Unit = "Boxes",
                    ExpiryDate = baseDate.AddDays(45),
                    LastUpdated = baseDate.AddDays(-1),
                    ShelterId = "SH-002",
                    ShelterName = "North Community Hall",
                    CreatedAt = baseDate.AddDays(-10)
                },
                new()
                {
                    Id = "INV-140",
                    Name = "Sleeping Mats",
                    Category = "Bedding",
                    Available = 75,
                    Minimum = 100,
                    Unit = "Units",
                    ExpiryDate = null,
                    LastUpdated = baseDate.AddDays(-2),
                    ShelterId = "SH-004",
                    ShelterName = "Lakeside Sports Hall",
                    CreatedAt = baseDate.AddDays(-10)
                },
                new()
                {
                    Id = "INV-145",
                    Name = "Sanitary Products",
                    Category = "Hygiene",
                    Available = 18,
                    Minimum = 60,
                    Unit = "Packs",
                    ExpiryDate = baseDate.AddDays(18),
                    LastUpdated = baseDate.AddDays(-2),
                    ShelterId = "SH-003",
                    ShelterName = "St. Mary Civic Centre",
                    CreatedAt = baseDate.AddDays(-10)
                }
            };

            await context.ResourceItems.AddRangeAsync(resources);
            await context.SaveChangesAsync();
        }

        if (!context.Rooms.Any())
        {
            var baseDate = DateTime.UtcNow;

            var rooms = new List<Room>
            {
                new()
                {
                    Id = "RM-001",
                    Name = "Room A-01",
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central School",
                    Floor = "Ground floor",
                    Type = "Family Room",
                    CreatedAt = baseDate.AddDays(-15),
                    Beds = new List<Bed>
                    {
                        new() { Id = "RM-001-B01", BedNumber = "B-01", RoomId = "RM-001", Status = "Occupied", ResidentName = "Nimali Perera", UpdatedAt = baseDate },
                        new() { Id = "RM-001-B02", BedNumber = "B-02", RoomId = "RM-001", Status = "Occupied", ResidentName = "Saman Perera", UpdatedAt = baseDate },
                        new() { Id = "RM-001-B03", BedNumber = "B-03", RoomId = "RM-001", Status = "Available", UpdatedAt = baseDate },
                        new() { Id = "RM-001-B04", BedNumber = "B-04", RoomId = "RM-001", Status = "Reserved", UpdatedAt = baseDate }
                    }
                },
                new()
                {
                    Id = "RM-002",
                    Name = "Room A-02",
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central School",
                    Floor = "Ground floor",
                    Type = "Accessible Room",
                    CreatedAt = baseDate.AddDays(-15),
                    Beds = new List<Bed>
                    {
                        new() { Id = "RM-002-B01", BedNumber = "B-01", RoomId = "RM-002", Status = "Occupied", ResidentName = "Fathima Rizna", UpdatedAt = baseDate },
                        new() { Id = "RM-002-B02", BedNumber = "B-02", RoomId = "RM-002", Status = "Available", UpdatedAt = baseDate },
                        new() { Id = "RM-002-B03", BedNumber = "B-03", RoomId = "RM-002", Status = "Available", UpdatedAt = baseDate }
                    }
                },
                new()
                {
                    Id = "RM-003",
                    Name = "Room B-07",
                    ShelterId = "SH-001",
                    ShelterName = "Riverton Central School",
                    Floor = "First floor",
                    Type = "Shared Dormitory",
                    CreatedAt = baseDate.AddDays(-15),
                    Beds = new List<Bed>
                    {
                        new() { Id = "RM-003-B01", BedNumber = "B-01", RoomId = "RM-003", Status = "Occupied", ResidentName = "Mohamed Irfan", UpdatedAt = baseDate },
                        new() { Id = "RM-003-B02", BedNumber = "B-02", RoomId = "RM-003", Status = "Occupied", UpdatedAt = baseDate },
                        new() { Id = "RM-003-B03", BedNumber = "B-03", RoomId = "RM-003", Status = "Occupied", UpdatedAt = baseDate },
                        new() { Id = "RM-003-B04", BedNumber = "B-04", RoomId = "RM-003", Status = "Available", UpdatedAt = baseDate },
                        new() { Id = "RM-003-B05", BedNumber = "B-05", RoomId = "RM-003", Status = "Reserved", UpdatedAt = baseDate },
                        new() { Id = "RM-003-B06", BedNumber = "B-06", RoomId = "RM-003", Status = "Available", UpdatedAt = baseDate }
                    }
                },
                new()
                {
                    Id = "RM-004",
                    Name = "Room C-11",
                    ShelterId = "SH-002",
                    ShelterName = "North Community Hall",
                    Floor = "First floor",
                    Type = "Women & Children",
                    CreatedAt = baseDate.AddDays(-15),
                    Beds = new List<Bed>
                    {
                        new() { Id = "RM-004-B01", BedNumber = "B-01", RoomId = "RM-004", Status = "Occupied", ResidentName = "Anusha Fernando", UpdatedAt = baseDate },
                        new() { Id = "RM-004-B02", BedNumber = "B-02", RoomId = "RM-004", Status = "Unavailable", UpdatedAt = baseDate },
                        new() { Id = "RM-004-B03", BedNumber = "B-03", RoomId = "RM-004", Status = "Available", UpdatedAt = baseDate },
                        new() { Id = "RM-004-B04", BedNumber = "B-04", RoomId = "RM-004", Status = "Available", UpdatedAt = baseDate }
                    }
                },
                new()
                {
                    Id = "RM-005",
                    Name = "Room D-03",
                    ShelterId = "SH-003",
                    ShelterName = "St. Mary Civic Centre",
                    Floor = "Ground floor",
                    Type = "General Room",
                    CreatedAt = baseDate.AddDays(-15),
                    Beds = new List<Bed>
                    {
                        new() { Id = "RM-005-B01", BedNumber = "B-01", RoomId = "RM-005", Status = "Occupied", ResidentName = "Chandrika Bandara", UpdatedAt = baseDate },
                        new() { Id = "RM-005-B02", BedNumber = "B-02", RoomId = "RM-005", Status = "Occupied", ResidentName = "Dinesh Weerasinghe", UpdatedAt = baseDate },
                        new() { Id = "RM-005-B03", BedNumber = "B-03", RoomId = "RM-005", Status = "Available", UpdatedAt = baseDate },
                        new() { Id = "RM-005-B04", BedNumber = "B-04", RoomId = "RM-005", Status = "Available", UpdatedAt = baseDate }
                    }
                },
                new()
                {
                    Id = "RM-006",
                    Name = "Room E-05",
                    ShelterId = "SH-004",
                    ShelterName = "Lakeside Sports Hall",
                    Floor = "Second floor",
                    Type = "Family Room",
                    CreatedAt = baseDate.AddDays(-15),
                    Beds = new List<Bed>
                    {
                        new() { Id = "RM-006-B01", BedNumber = "B-01", RoomId = "RM-006", Status = "Occupied", ResidentName = "Kamal De Silva", UpdatedAt = baseDate },
                        new() { Id = "RM-006-B02", BedNumber = "B-02", RoomId = "RM-006", Status = "Available", UpdatedAt = baseDate },
                        new() { Id = "RM-006-B03", BedNumber = "B-03", RoomId = "RM-006", Status = "Available", UpdatedAt = baseDate },
                        new() { Id = "RM-006-B04", BedNumber = "B-04", RoomId = "RM-006", Status = "Reserved", UpdatedAt = baseDate }
                    }
                }
            };

            await context.Rooms.AddRangeAsync(rooms);
            await context.SaveChangesAsync();
        }

        if (!context.StaffMembers.Any())
        {
            var staff = new List<StaffMember>
            {
                new() { Id = "ST-014", Name = "Anura Perera", Designation = "Shelter Manager", Contact = "077 245 8891", Email = "anura@carelink.org", Shelter = "Riverton Central", Shift = "Morning", Availability = "Available", Status = "Active" },
                new() { Id = "ST-027", Name = "Dr. Malini Rao", Designation = "Medical Officer", Contact = "071 928 1140", Email = "malini@carelink.org", Shelter = "Riverton Central", Shift = "Day", Availability = "On duty", Status = "Active" },
                new() { Id = "ST-042", Name = "K. Bandara", Designation = "Security Officer", Contact = "075 318 0944", Email = "bandara@carelink.org", Shelter = "North Community", Shift = "Night", Availability = "Available", Status = "Active" },
                new() { Id = "ST-061", Name = "S. Nirosha", Designation = "Kitchen Staff", Contact = "077 624 0122", Email = "nirosha@carelink.org", Shelter = "Lakeside Sports", Shift = "Evening", Availability = "Leave", Status = "Active" },
                new() { Id = "ST-075", Name = "Sunil Jayasuriya", Designation = "Volunteer Coordinator", Contact = "077 412 9005", Email = "sunil.j@carelink.org", Shelter = "St. Mary Civic Centre", Shift = "Day", Availability = "On duty", Status = "Active" },
                new() { Id = "ST-083", Name = "Nalika Fernando", Designation = "Nurse", Contact = "076 890 2234", Email = "nalika@carelink.org", Shelter = "Riverton Central", Shift = "Morning", Availability = "On duty", Status = "Active" },
                new() { Id = "ST-095", Name = "Pradeep Kumara", Designation = "Cleaning Staff", Contact = "072 554 1189", Email = "pradeep@carelink.org", Shelter = "Hillview College", Shift = "Morning", Availability = "Available", Status = "Active" },
                new() { Id = "ST-102", Name = "Dilani Wickramasinghe", Designation = "Administrative Officer", Contact = "071 332 7780", Email = "dilani@carelink.org", Shelter = "North Community", Shift = "Day", Availability = "Absent", Status = "Active" }
            };

            await context.StaffMembers.AddRangeAsync(staff);
            await context.SaveChangesAsync();
        }

        if (!context.ShiftCoverages.Any())
        {
            var coverage = new List<ShiftCoverage>
            {
                new() { Day = "Mon", Date = 22, Assigned = 18, Capacity = 24, Shelter = "Riverton Central" },
                new() { Day = "Tue", Date = 23, Assigned = 19, Capacity = 24, Shelter = "Riverton Central" },
                new() { Day = "Wed", Date = 24, Assigned = 20, Capacity = 24, Shelter = "Riverton Central" },
                new() { Day = "Thu", Date = 25, Assigned = 21, Capacity = 24, Shelter = "Riverton Central" },
                new() { Day = "Fri", Date = 26, Assigned = 18, Capacity = 24, Shelter = "Riverton Central" },
                new() { Day = "Sat", Date = 27, Assigned = 19, Capacity = 24, Shelter = "Riverton Central" },
                new() { Day = "Sun", Date = 28, Assigned = 20, Capacity = 24, Shelter = "Riverton Central" }
            };

            await context.ShiftCoverages.AddRangeAsync(coverage);
            await context.SaveChangesAsync();
        }

        if (!context.Campaigns.Any())
        {
            var now = DateTime.UtcNow.Date;
            var campaigns = new List<Campaign>
            {
                new()
                {
                    Id = "CAM-104",
                    Name = "Flood Response 2025",
                    Event = "Monsoon Floods 2025",
                    Period = "18 Jun–31 Jul",
                    Raised = 1840000m,
                    Target = 2500000m,
                    Status = "Active",
                    StartDate = now.AddDays(-30),
                    EndDate = now.AddDays(25),
                    Description = "Primary emergency fund for immediate monsoon flood response, food packages, and medical supplies."
                },
                new()
                {
                    Id = "CAM-108",
                    Name = "Emergency Shelter Fund",
                    Event = "Monsoon Floods 2025",
                    Period = "20 Jun–15 Aug",
                    Raised = 612000m,
                    Target = 850000m,
                    Status = "Active",
                    StartDate = now.AddDays(-20),
                    EndDate = now.AddDays(40),
                    Description = "Targeted support for shelter infrastructure repairs, temporary tents, and sanitation facilities."
                },
                new()
                {
                    Id = "CAM-112",
                    Name = "Clean Water Appeal",
                    Event = "Monsoon Floods 2025",
                    Period = "19 Jun–20 Jul",
                    Raised = 318000m,
                    Target = 420000m,
                    Status = "Active",
                    StartDate = now.AddDays(-25),
                    EndDate = now.AddDays(8), // Ending soon within 14 days
                    Description = "Purification kits, clean bottled water distribution, and water testing kits across all shelters."
                },
                new()
                {
                    Id = "CAM-117",
                    Name = "Medical Relief",
                    Event = "Monsoon Floods 2025",
                    Period = "18 Jun–31 Aug",
                    Raised = 496000m,
                    Target = 680000m,
                    Status = "Active",
                    StartDate = now.AddDays(-28),
                    EndDate = now.AddDays(55),
                    Description = "Critical pharmaceuticals, mobile health clinic operations, and hygiene packs."
                },
                new()
                {
                    Id = "CAM-092",
                    Name = "Cyclone Recovery 2024",
                    Event = "Other Emergencies",
                    Period = "01 Jan–28 Feb",
                    Raised = 1200000m,
                    Target = 1200000m,
                    Status = "Completed",
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 2, 28),
                    Description = "Reconstruction and livelihood recovery support for coastal families affected by the cyclone."
                },
                new()
                {
                    Id = "CAM-088",
                    Name = "Winter Essentials 2024",
                    Event = "Other Emergencies",
                    Period = "10 Oct–30 Nov",
                    Raised = 450000m,
                    Target = 500000m,
                    Status = "Archived",
                    StartDate = new DateTime(2024, 10, 10),
                    EndDate = new DateTime(2024, 11, 30),
                    Description = "Warm clothing, blankets, and heating supplies for high-altitude evacuation centers."
                }
            };

            await context.Campaigns.AddRangeAsync(campaigns);
            await context.SaveChangesAsync();
        }

        if (!context.DonationRecords.Any())
        {
            var donations = new List<DonationRecord>
            {
                new()
                {
                    Id = "DON-88421",
                    Donor = "Global Relief Foundation",
                    Type = "Monetary",
                    Amount = "$125,000",
                    NumericAmount = 125000m,
                    Campaign = "Flood Response 2025",
                    Received = "22 Jun",
                    ReceivedDate = new DateTime(2025, 6, 22),
                    Verification = "Verified",
                    Allocation = "Partially Allocated",
                    Notes = "Wire transfer received via Central Relief Account."
                },
                new()
                {
                    Id = "DON-88418",
                    Donor = "Anonymous Individual",
                    Type = "Monetary",
                    Amount = "$2,500",
                    NumericAmount = 2500m,
                    Campaign = "Emergency Shelter Fund",
                    Received = "22 Jun",
                    ReceivedDate = new DateTime(2025, 6, 22),
                    Verification = "Pending",
                    Allocation = "Unallocated",
                    Notes = "Online card payment pending gateway confirmation."
                },
                new()
                {
                    Id = "DON-88412",
                    Donor = "Riverton Foods Ltd",
                    Type = "Material",
                    Amount = "4,800 food packs",
                    NumericAmount = null,
                    Campaign = "Flood Response 2025",
                    Received = "21 Jun",
                    ReceivedDate = new DateTime(2025, 6, 21),
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "Delivered directly to Riverton Central School warehouse."
                },
                new()
                {
                    Id = "DON-88404",
                    Donor = "HealthAccess International",
                    Type = "Material",
                    Amount = "320 medical kits",
                    NumericAmount = null,
                    Campaign = "Medical Relief",
                    Received = "21 Jun",
                    ReceivedDate = new DateTime(2025, 6, 21),
                    Verification = "Verified",
                    Allocation = "Partially Allocated",
                    Notes = "WHO certified trauma first aid kits."
                },
                new()
                {
                    Id = "DON-88398",
                    Donor = "Community Giving Circle",
                    Type = "Monetary",
                    Amount = "$18,420",
                    NumericAmount = 18420m,
                    Campaign = "Clean Water Appeal",
                    Received = "20 Jun",
                    ReceivedDate = new DateTime(2025, 6, 20),
                    Verification = "Flagged",
                    Allocation = "On Hold",
                    Notes = "Discrepancy in bank transfer reference number under review."
                },
                new()
                {
                    Id = "DON-88392",
                    Donor = "Apex Logistics Corp",
                    Type = "Material",
                    Amount = "1,200 emergency blankets",
                    NumericAmount = null,
                    Campaign = "Emergency Shelter Fund",
                    Received = "19 Jun",
                    ReceivedDate = new DateTime(2025, 6, 19),
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "Thermal blankets distributed across Sector 4 shelters."
                },
                new()
                {
                    Id = "DON-88385",
                    Donor = "Colombo Charitable Trust",
                    Type = "Monetary",
                    Amount = "$50,000",
                    NumericAmount = 50000m,
                    Campaign = "Flood Response 2025",
                    Received = "18 Jun",
                    ReceivedDate = new DateTime(2025, 6, 18),
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "Designated for immediate boat rescue and evacuation operations."
                },
                new()
                {
                    Id = "DON-88374",
                    Donor = "Silverline Healthcare",
                    Type = "Material",
                    Amount = "500 hygiene kits",
                    NumericAmount = null,
                    Campaign = "Medical Relief",
                    Received = "17 Jun",
                    ReceivedDate = new DateTime(2025, 6, 17),
                    Verification = "Pending",
                    Allocation = "Unallocated",
                    Notes = "Sanitation and personal hygiene kits awaiting intake inspection."
                },
                new()
                {
                    Id = "DON-88360",
                    Donor = "Zenith Energy Ltd",
                    Type = "Monetary",
                    Amount = "$75,000",
                    NumericAmount = 75000m,
                    Campaign = "Clean Water Appeal",
                    Received = "16 Jun",
                    ReceivedDate = new DateTime(2025, 6, 16),
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "CSR contribution for mobile water filtration units."
                },
                new()
                {
                    Id = "DON-88352",
                    Donor = "Private Family Office",
                    Type = "Monetary",
                    Amount = "$10,000",
                    NumericAmount = 10000m,
                    Campaign = "Emergency Shelter Fund",
                    Received = "15 Jun",
                    ReceivedDate = new DateTime(2025, 6, 15),
                    Verification = "Flagged",
                    Allocation = "On Hold",
                    Notes = "AML verification pending for overseas wire transfer."
                },
                new()
                {
                    Id = "DON-88344",
                    Donor = "Red Cross Youth Chapter",
                    Type = "Material",
                    Amount = "300 first-aid kits",
                    NumericAmount = null,
                    Campaign = "Medical Relief",
                    Received = "14 Jun",
                    ReceivedDate = new DateTime(2025, 6, 14),
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "Basic first-aid supplies deployed to mobile clinics."
                },
                new()
                {
                    Id = "DON-88330",
                    Donor = "Horizon Tech Partners",
                    Type = "Monetary",
                    Amount = "$30,000",
                    NumericAmount = 30000m,
                    Campaign = "Flood Response 2025",
                    Received = "12 Jun",
                    ReceivedDate = new DateTime(2025, 6, 12),
                    Verification = "Pending",
                    Allocation = "Unallocated",
                    Notes = "Employee donation match program pledge."
                }
            };

            await context.DonationRecords.AddRangeAsync(donations);
            await context.SaveChangesAsync();
        }

        if (!context.Donors.Any())
        {
            var donors = new List<Donor>
            {
                new()
                {
                    Id = "DNR-2048",
                    Name = "Global Relief Foundation",
                    Type = "Foundation",
                    Contact = "contact@grf.org",
                    Scope = "International",
                    Verification = "Verified",
                    Contributions = 842000m,
                    Status = "Active",
                    Address = "Geneva, Switzerland",
                    Notes = "Lead international institutional partner for humanitarian response."
                },
                new()
                {
                    Id = "DNR-2039",
                    Name = "Riverton Foods Ltd",
                    Type = "Corporate",
                    Contact = "impact@rivertonfoods.com",
                    Scope = "National",
                    Verification = "Verified",
                    Contributions = 185400m,
                    Status = "Active",
                    Address = "Industrial Zone, Colombo",
                    Notes = "Provides recurring nutrition and food pack contributions."
                },
                new()
                {
                    Id = "DNR-2028",
                    Name = "HealthAccess International",
                    Type = "International NGO",
                    Contact = "giving@healthaccess.org",
                    Scope = "International",
                    Verification = "Verified",
                    Contributions = 420000m,
                    Status = "Active",
                    Address = "London, United Kingdom",
                    Notes = "Medical supplies, trauma relief kits, and emergency medicine donor."
                },
                new()
                {
                    Id = "DNR-2014",
                    Name = "Community Giving Circle",
                    Type = "Community Group",
                    Contact = "hello@cgc.org",
                    Scope = "Greater Riverton",
                    Verification = "Under Review",
                    Contributions = 48720m,
                    Status = "Restricted",
                    Address = "Riverton District",
                    Notes = "Pending annual verification of community fund registration."
                },
                new()
                {
                    Id = "DNR-2005",
                    Name = "Colombo Charitable Trust",
                    Type = "Foundation",
                    Contact = "trust@colombocharity.lk",
                    Scope = "National",
                    Verification = "Verified",
                    Contributions = 320000m,
                    Status = "Active",
                    Address = "Colombo 03, Sri Lanka",
                    Notes = "Emergency response grant foundation supporting shelter development."
                },
                new()
                {
                    Id = "DNR-1998",
                    Name = "Zenith Energy Ltd",
                    Type = "Corporate",
                    Contact = "csr@zenithenergy.com",
                    Scope = "National",
                    Verification = "Verified",
                    Contributions = 250000m,
                    Status = "Active",
                    Address = "Central Business District, Colombo",
                    Notes = "Corporate social responsibility grants for clean water appeals."
                },
                new()
                {
                    Id = "DNR-1982",
                    Name = "Silverline Healthcare",
                    Type = "Corporate",
                    Contact = "giving@silverlinehealth.com",
                    Scope = "National",
                    Verification = "Pending",
                    Contributions = 95000m,
                    Status = "Active",
                    Address = "Kandy Road, Kiribathgoda",
                    Notes = "Material and monetary donations for clinic operations."
                },
                new()
                {
                    Id = "DNR-1970",
                    Name = "Horizon Tech Partners",
                    Type = "Corporate",
                    Contact = "impact@horizontech.io",
                    Scope = "International",
                    Verification = "Pending",
                    Contributions = 65000m,
                    Status = "Active",
                    Address = "Singapore",
                    Notes = "Tech equipment, connectivity solutions, and matching funds."
                },
                new()
                {
                    Id = "DNR-1955",
                    Name = "Red Cross Youth Chapter",
                    Type = "Community Group",
                    Contact = "youth@redcross.lk",
                    Scope = "Greater Riverton",
                    Verification = "Verified",
                    Contributions = 34500m,
                    Status = "Active",
                    Address = "Riverton Central",
                    Notes = "Volunteer-driven community relief drives."
                },
                new()
                {
                    Id = "DNR-1940",
                    Name = "Private Family Office",
                    Type = "Individual",
                    Contact = "pfo-philanthropy@familyoffice.sg",
                    Scope = "International",
                    Verification = "Under Review",
                    Contributions = 150000m,
                    Status = "Restricted",
                    Address = "Singapore",
                    Notes = "Enhanced due diligence check currently in progress."
                }
            };

            await context.Donors.AddRangeAsync(donors);
            await context.SaveChangesAsync();
        }

        if (!context.Expenditures.Any())
        {
            var expenditures = new List<Expenditure>
            {
                new()
                {
                    Id = "EXP-9021",
                    Amount = 24840m,
                    Description = "Shelter kits - Batch 18",
                    Recipient = "CareLink Shelter Network",
                    Invoice = "INV-4821",
                    Date = "21 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-2),
                    Verification = "Verified",
                    PaymentStatus = "Paid",
                    Type = "Shelter",
                    Notes = "Emergency shelter kit procurement for Batch 18 distribution.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Id = "EXP-9014",
                    Amount = 18240m,
                    Description = "Emergency vehicle fuel",
                    Recipient = "Logistics Unit",
                    Invoice = "INV-4814",
                    Date = "21 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-2),
                    Verification = "Verified",
                    PaymentStatus = "Paid",
                    Type = "Transport",
                    Notes = "Fuel costs for emergency response vehicles deployed during relief operations.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Id = "EXP-9008",
                    Amount = 42600m,
                    Description = "Medical supplies",
                    Recipient = "Health Response Unit",
                    Invoice = "INV-4808",
                    Date = "20 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-3),
                    Verification = "Pending Review",
                    PaymentStatus = "On Hold",
                    Type = "Medical",
                    Notes = "Medical supplies for flood relief — awaiting officer verification.",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    Id = "EXP-8996",
                    Amount = 12480m,
                    Description = "Water distribution",
                    Recipient = "Municipal Water Team",
                    Invoice = "INV-4796",
                    Date = "20 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-3),
                    Verification = "Verified",
                    PaymentStatus = "Paid",
                    Type = "Water",
                    Notes = "Safe drinking water distribution across 5 shelters.",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = "EXP-8980",
                    Amount = 9500m,
                    Description = "Food package delivery - Zone A",
                    Recipient = "Community Relief Fund",
                    Invoice = "INV-4780",
                    Date = "19 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-4),
                    Verification = "Verified",
                    PaymentStatus = "Pending",
                    Type = "Shelter",
                    Notes = "Food package delivery for Zone A displaced families.",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    Id = "EXP-8964",
                    Amount = 7200m,
                    Description = "Transport for medical volunteers",
                    Recipient = "Red Crescent Field Office",
                    Invoice = "INV-4764",
                    Date = "18 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-5),
                    Verification = "Verified",
                    PaymentStatus = "Paid",
                    Type = "Transport",
                    Notes = "Bus and van hire to transport medical volunteers to rural shelters.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new()
                {
                    Id = "EXP-8947",
                    Amount = 31800m,
                    Description = "Emergency generator rental",
                    Recipient = "PowerLink Solutions",
                    Invoice = "INV-4747",
                    Date = "17 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-6),
                    Verification = "Pending Review",
                    PaymentStatus = "On Hold",
                    Type = "Other",
                    Notes = "Generator rental for 3 shelters experiencing power outages.",
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new()
                {
                    Id = "EXP-8930",
                    Amount = 5600m,
                    Description = "Water purification tablets",
                    Recipient = "Global Health Supply Co.",
                    Invoice = "INV-4730",
                    Date = "16 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-7),
                    Verification = "Rejected",
                    PaymentStatus = "On Hold",
                    Type = "Water",
                    Notes = "Rejected — duplicate invoice; resubmit with correct reference.",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new()
                {
                    Id = "EXP-8912",
                    Amount = 15300m,
                    Description = "First aid and trauma kits",
                    Recipient = "MedAid International",
                    Invoice = "INV-4712",
                    Date = "15 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-8),
                    Verification = "Verified",
                    PaymentStatus = "Paid",
                    Type = "Medical",
                    Notes = "Trauma kits distributed to 8 field medical posts.",
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new()
                {
                    Id = "EXP-8895",
                    Amount = 4800m,
                    Description = "Child care supplies",
                    Recipient = "UNICEF Partner Office",
                    Invoice = "INV-4695",
                    Date = "14 Jun",
                    ExpenseDate = DateTime.UtcNow.AddDays(-9),
                    Verification = "Verified",
                    PaymentStatus = "Paid",
                    Type = "Shelter",
                    Notes = "Diapers, formula, and hygiene supplies for displaced families with children.",
                    CreatedAt = DateTime.UtcNow.AddDays(-9),
                    UpdatedAt = DateTime.UtcNow.AddDays(-8)
                }
            };

            await context.Expenditures.AddRangeAsync(expenditures);
            await context.SaveChangesAsync();
        }

        if (!context.FlaggedTransactions.Any())
        {
            var baseTime = DateTime.UtcNow;

            var flags = new List<FlaggedTransaction>
            {
                new()
                {
                    Id = "FLG-208",
                    Record = "DON-88398",
                    Reason = "Unusual source pattern",
                    Amount = 18420m,
                    AmountDisplay = "$18,420",
                    Party = "Community Giving Circle",
                    Severity = "High",
                    Flagged = "22 Jun · 11:42",
                    FlaggedAt = baseTime.AddDays(-1).AddHours(-2).AddMinutes(-18),
                    Status = "Investigation Required",
                    Description = "A contribution has been flagged due to an unusual funding source pattern. The transaction requires manual verification before further allocation.",
                    Investigator = "Not assigned",
                    RecordType = "Donations",
                    ResolutionNotes = "",
                    ResolvedAt = null,
                    CreatedAt = baseTime.AddDays(-1),
                    UpdatedAt = baseTime.AddDays(-1)
                },
                new()
                {
                    Id = "FLG-204",
                    Record = "EXP-9008",
                    Reason = "Invoice amount variance",
                    Amount = 42600m,
                    AmountDisplay = "$42,600",
                    Party = "Health Response Unit",
                    Severity = "Medium",
                    Flagged = "22 Jun · 09:18",
                    FlaggedAt = baseTime.AddDays(-1).AddHours(-4).AddMinutes(-42),
                    Status = "Under Review",
                    Description = "The submitted invoice amount differs from the expected expenditure amount. Supporting financial documentation needs to be checked.",
                    Investigator = "M. Perera",
                    RecordType = "Expenditure",
                    ResolutionNotes = "",
                    ResolvedAt = null,
                    CreatedAt = baseTime.AddDays(-1),
                    UpdatedAt = baseTime.AddDays(-1)
                },
                new()
                {
                    Id = "FLG-199",
                    Record = "ALL-4371",
                    Reason = "Duplicate allocation reference",
                    Amount = 16800m,
                    AmountDisplay = "$16,800",
                    Party = "Partner Network",
                    Severity = "High",
                    Flagged = "21 Jun · 16:05",
                    FlaggedAt = baseTime.AddDays(-2).AddHours(2).AddMinutes(5),
                    Status = "Under Review",
                    Description = "A possible duplicate allocation reference was detected. Review the associated allocation records and approval history.",
                    Investigator = "S. Fernando",
                    RecordType = "Allocations",
                    ResolutionNotes = "",
                    ResolvedAt = null,
                    CreatedAt = baseTime.AddDays(-2),
                    UpdatedAt = baseTime.AddDays(-2)
                },
                new()
                {
                    Id = "FLG-192",
                    Record = "DON-88374",
                    Reason = "Verification mismatch",
                    Amount = 8200m,
                    AmountDisplay = "$8,200",
                    Party = "Anonymous Donor",
                    Severity = "Low",
                    Flagged = "21 Jun · 12:40",
                    FlaggedAt = baseTime.AddDays(-2).AddHours(-1).AddMinutes(-20),
                    Status = "Deferred",
                    Description = "The submitted donor verification information does not fully match the available transaction information.",
                    Investigator = "Not assigned",
                    RecordType = "Donations",
                    ResolutionNotes = "",
                    ResolvedAt = null,
                    CreatedAt = baseTime.AddDays(-2),
                    UpdatedAt = baseTime.AddDays(-2)
                },
                new()
                {
                    Id = "FLG-187",
                    Record = "EXP-8981",
                    Reason = "Unusual expenditure pattern",
                    Amount = 12500m,
                    AmountDisplay = "$12,500",
                    Party = "Logistics Unit",
                    Severity = "Medium",
                    Flagged = "20 Jun · 14:25",
                    FlaggedAt = baseTime.AddDays(-3).AddHours(0).AddMinutes(25),
                    Status = "Resolved",
                    Description = "The expenditure pattern was reviewed and supporting documents were accepted.",
                    Investigator = "A. Perera",
                    RecordType = "Expenditure",
                    ResolutionNotes = "All supporting documentation verified. Pattern explained by emergency procurement procedure.",
                    ResolvedAt = baseTime.AddDays(-2),
                    CreatedAt = baseTime.AddDays(-3),
                    UpdatedAt = baseTime.AddDays(-2)
                },
                new()
                {
                    Id = "FLG-181",
                    Record = "DON-88290",
                    Reason = "Rapid successive donations",
                    Amount = 25000m,
                    AmountDisplay = "$25,000",
                    Party = "Sunrise Relief Fund",
                    Severity = "High",
                    Flagged = "19 Jun · 10:30",
                    FlaggedAt = baseTime.AddDays(-4).AddHours(-3).AddMinutes(-30),
                    Status = "Investigation Required",
                    Description = "Three large donations were received within 24 hours from related entities. Transaction structuring is suspected.",
                    Investigator = "Not assigned",
                    RecordType = "Donations",
                    ResolutionNotes = "",
                    ResolvedAt = null,
                    CreatedAt = baseTime.AddDays(-4),
                    UpdatedAt = baseTime.AddDays(-4)
                },
                new()
                {
                    Id = "FLG-176",
                    Record = "ALL-4312",
                    Reason = "Allocation exceeds campaign budget",
                    Amount = 38000m,
                    AmountDisplay = "$38,000",
                    Party = "CareLink Field Operations",
                    Severity = "Medium",
                    Flagged = "18 Jun · 08:15",
                    FlaggedAt = baseTime.AddDays(-5).AddHours(-5).AddMinutes(-45),
                    Status = "Deferred",
                    Description = "The allocated amount exceeds the remaining campaign budget by 12%. Authorisation for over-allocation is pending.",
                    Investigator = "Not assigned",
                    RecordType = "Allocations",
                    ResolutionNotes = "",
                    ResolvedAt = null,
                    CreatedAt = baseTime.AddDays(-5),
                    UpdatedAt = baseTime.AddDays(-5)
                },
                new()
                {
                    Id = "FLG-168",
                    Record = "EXP-8930",
                    Reason = "Duplicate invoice",
                    Amount = 5600m,
                    AmountDisplay = "$5,600",
                    Party = "Global Health Supply Co.",
                    Severity = "Low",
                    Flagged = "16 Jun · 13:00",
                    FlaggedAt = baseTime.AddDays(-7).AddHours(-1),
                    Status = "Resolved",
                    Description = "Invoice INV-4730 was submitted twice with minor variations. The duplicate has been voided.",
                    Investigator = "R. Wijesiri",
                    RecordType = "Expenditure",
                    ResolutionNotes = "Duplicate invoice confirmed and voided. Vendor notified.",
                    ResolvedAt = baseTime.AddDays(-6),
                    CreatedAt = baseTime.AddDays(-7),
                    UpdatedAt = baseTime.AddDays(-6)
                }
            };

            await context.FlaggedTransactions.AddRangeAsync(flags);
            await context.SaveChangesAsync();
        }

        if (!context.FundAllocations.Any())
        {
            var baseTime = DateTime.UtcNow;

            var allocations = new List<FundAllocation>
            {
                new()
                {
                    Id = "ALL-4408",
                    Amount = 180000m,
                    Source = "Flood Response 2025",
                    Purpose = "Emergency Shelter Kits",
                    Recipient = "CareLink Shelter Network",
                    Date = "22 Jun",
                    AllocationDate = baseTime.AddDays(-1),
                    Approval = "Approved",
                    Disbursement = "68% Disbursed",
                    Notes = "Phase 1 allocation for riverbank shelter network reinforcement.",
                    CreatedAt = baseTime.AddDays(-1),
                    UpdatedAt = baseTime.AddDays(-1)
                },
                new()
                {
                    Id = "ALL-4402",
                    Amount = 92500m,
                    Source = "Medical Relief",
                    Purpose = "Trauma & First Aid Kits",
                    Recipient = "Health Response Unit",
                    Date = "21 Jun",
                    AllocationDate = baseTime.AddDays(-2),
                    Approval = "Approved",
                    Disbursement = "Disbursed",
                    Notes = "Procurement of critical trauma care and wound dressing equipment.",
                    CreatedAt = baseTime.AddDays(-2),
                    UpdatedAt = baseTime.AddDays(-2)
                },
                new()
                {
                    Id = "ALL-4396",
                    Amount = 64200m,
                    Source = "Clean Water Appeal",
                    Purpose = "Water Purification",
                    Recipient = "Municipal Water Team",
                    Date = "21 Jun",
                    AllocationDate = baseTime.AddDays(-2),
                    Approval = "Under Review",
                    Disbursement = "Pending",
                    Notes = "Water filtration units for southern displacement camps pending board sign-off.",
                    CreatedAt = baseTime.AddDays(-2),
                    UpdatedAt = baseTime.AddDays(-2)
                },
                new()
                {
                    Id = "ALL-4384",
                    Amount = 48000m,
                    Source = "Flood Response 2025",
                    Purpose = "Emergency Transport",
                    Recipient = "Logistics Unit",
                    Date = "20 Jun",
                    AllocationDate = baseTime.AddDays(-3),
                    Approval = "Approved",
                    Disbursement = "Disbursed",
                    Notes = "Fuel subsidies and vehicle hire for rapid evacuation corridor.",
                    CreatedAt = baseTime.AddDays(-3),
                    UpdatedAt = baseTime.AddDays(-3)
                },
                new()
                {
                    Id = "ALL-4371",
                    Amount = 35000m,
                    Source = "Emergency Shelter Fund",
                    Purpose = "Temporary Tents & Bedding",
                    Recipient = "Red Cross Field Chapter",
                    Date = "19 Jun",
                    AllocationDate = baseTime.AddDays(-4),
                    Approval = "Approved",
                    Disbursement = "Disbursed",
                    Notes = "All-weather tents and sleeping pads distributed across 4 locations.",
                    CreatedAt = baseTime.AddDays(-4),
                    UpdatedAt = baseTime.AddDays(-4)
                },
                new()
                {
                    Id = "ALL-4355",
                    Amount = 72000m,
                    Source = "Medical Relief",
                    Purpose = "Vaccines & Cold Chain",
                    Recipient = "District Health Services",
                    Date = "18 Jun",
                    AllocationDate = baseTime.AddDays(-5),
                    Approval = "Under Review",
                    Disbursement = "Pending",
                    Notes = "Disease prevention stockpiling for anticipated post-flood hazards.",
                    CreatedAt = baseTime.AddDays(-5),
                    UpdatedAt = baseTime.AddDays(-5)
                },
                new()
                {
                    Id = "ALL-4340",
                    Amount = 55400m,
                    Source = "Clean Water Appeal",
                    Purpose = "Mobile Water Tankers",
                    Recipient = "Community Water Board",
                    Date = "17 Jun",
                    AllocationDate = baseTime.AddDays(-6),
                    Approval = "Approved",
                    Disbursement = "Disbursed",
                    Notes = "Fleet of 6 mobile bowsers servicing remote temporary settlements.",
                    CreatedAt = baseTime.AddDays(-6),
                    UpdatedAt = baseTime.AddDays(-6)
                },
                new()
                {
                    Id = "ALL-4328",
                    Amount = 28900m,
                    Source = "Flood Response 2025",
                    Purpose = "Sanitation & Hygiene Packs",
                    Recipient = "Women's Relief Network",
                    Date = "16 Jun",
                    AllocationDate = baseTime.AddDays(-7),
                    Approval = "Approved",
                    Disbursement = "Disbursed",
                    Notes = "Dignity kits and water safety tablets for 1,200 families.",
                    CreatedAt = baseTime.AddDays(-7),
                    UpdatedAt = baseTime.AddDays(-7)
                },
                new()
                {
                    Id = "ALL-4312",
                    Amount = 38000m,
                    Source = "Emergency Shelter Fund",
                    Purpose = "Community Kitchen Fuel & Equipment",
                    Recipient = "Field Relief Kitchens",
                    Date = "15 Jun",
                    AllocationDate = baseTime.AddDays(-8),
                    Approval = "Rejected",
                    Disbursement = "Not Disbursed",
                    Notes = "Duplicate submission flagged by audit system. Replaced by revised grant.",
                    CreatedAt = baseTime.AddDays(-8),
                    UpdatedAt = baseTime.AddDays(-8)
                },
                new()
                {
                    Id = "ALL-4290",
                    Amount = 86000m,
                    Source = "Flood Response 2025",
                    Purpose = "Flood Barrier Construction Material",
                    Recipient = "Municipal Works Dept",
                    Date = "14 Jun",
                    AllocationDate = baseTime.AddDays(-9),
                    Approval = "Approved",
                    Disbursement = "Disbursed",
                    Notes = "Sandbags, geotextiles and trench shoring along canal embankment.",
                    CreatedAt = baseTime.AddDays(-9),
                    UpdatedAt = baseTime.AddDays(-9)
                }
            };

            await context.FundAllocations.AddRangeAsync(allocations);
            await context.SaveChangesAsync();
        }

        if (!context.FundingSettings.Any())
        {
            var defaultSettings = new FundingSettings
            {
                Id = "funding-officer-default",
                FullName = "Maria Kone",
                Email = "maria@reliefos.org",
                Contact = "+94 77 412 8041",
                Currency = "USD",
                ApprovalLimit = 50000m,
                RequireApproval = true,
                DualApproval = true,
                AutoVerification = false,
                RestrictedAllocation = true,
                EmailNotifications = true,
                InAppNotifications = true,
                SmsNotifications = false,
                NotifyDonations = true,
                NotifyApprovals = true,
                NotifyCampaigns = true,
                NotifySecurity = true,
                TwoFactorEnabled = true,
                ActiveDevice = "Windows · Chrome",
                LastPasswordUpdate = "Recently",
                UpdatedAt = DateTime.UtcNow
            };

            await context.FundingSettings.AddAsync(defaultSettings);
            await context.SaveChangesAsync();
        }

        if (!context.ResourceContributions.Any())
        {
            var contributions = new List<ResourceContribution>
            {
                new()
                {
                    Id = "DON-88412",
                    Donor = "Riverton Foods Ltd",
                    Type = "Material",
                    Quantity = "4,800 food packs",
                    Campaign = "Flood Response 2025",
                    ReceivedDate = new DateTime(2025, 6, 21),
                    Received = "21 Jun",
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "High-protein family ration packs ready for distribution.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = "DON-88404",
                    Donor = "HealthAccess International",
                    Type = "Material",
                    Quantity = "320 medical kits",
                    Campaign = "Medical Relief",
                    ReceivedDate = new DateTime(2025, 6, 21),
                    Received = "21 Jun",
                    Verification = "Verified",
                    Allocation = "Partially Allocated",
                    Notes = "Trauma and surgical basic emergency supplies.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = "DON-88399",
                    Donor = "AquaPure Systems Inc",
                    Type = "Material",
                    Quantity = "1,200 water purification kits",
                    Campaign = "Clean Water Appeal",
                    ReceivedDate = new DateTime(2025, 6, 20),
                    Received = "20 Jun",
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "Rapid-deploy ceramic filtration units.",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    Id = "DON-88392",
                    Donor = "Apex Global Logistics",
                    Type = "Material",
                    Quantity = "450 heavy-duty tents",
                    Campaign = "Emergency Shelter Fund",
                    ReceivedDate = new DateTime(2025, 6, 19),
                    Received = "19 Jun",
                    Verification = "Pending",
                    Allocation = "Unallocated",
                    Notes = "All-weather 6-person family shelters.",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new()
                {
                    Id = "DON-88385",
                    Donor = "Global Health Partners",
                    Type = "Material",
                    Quantity = "15,000 hygiene kits",
                    Campaign = "Flood Response 2025",
                    ReceivedDate = new DateTime(2025, 6, 18),
                    Received = "18 Jun",
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "Soap, sanitizers, and sanitary towels.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new()
                {
                    Id = "DON-88378",
                    Donor = "Beacon Logistics",
                    Type = "Material",
                    Quantity = "80 portable power generators",
                    Campaign = "Emergency Shelter Fund",
                    ReceivedDate = new DateTime(2025, 6, 17),
                    Received = "17 Jun",
                    Verification = "Flagged",
                    Allocation = "Unallocated",
                    Notes = "Pending fuel type specification and customs clearance verification.",
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new()
                {
                    Id = "DON-88371",
                    Donor = "Direct Aid Corp",
                    Type = "Monetary",
                    Quantity = "$75,000 earmarked",
                    Campaign = "Flood Response 2025",
                    ReceivedDate = new DateTime(2025, 6, 16),
                    Received = "16 Jun",
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "Designated for local procurement of fresh vegetables and cooking gas.",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new()
                {
                    Id = "DON-88364",
                    Donor = "Sunrise Pharma Ltd",
                    Type = "Material",
                    Quantity = "85,000 antibiotic doses",
                    Campaign = "Medical Relief",
                    ReceivedDate = new DateTime(2025, 6, 15),
                    Received = "15 Jun",
                    Verification = "Verified",
                    Allocation = "Partially Allocated",
                    Notes = "Cold chain compliant amoxicillin and cephalexin.",
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new()
                {
                    Id = "DON-88357",
                    Donor = "Highland Relief Aid",
                    Type = "Material",
                    Quantity = "2,500 wool blankets",
                    Campaign = "Emergency Shelter Fund",
                    ReceivedDate = new DateTime(2025, 6, 14),
                    Received = "14 Jun",
                    Verification = "Pending",
                    Allocation = "Unallocated",
                    Notes = "Winterized heavy wool blankets for high-altitude evacuation centers.",
                    CreatedAt = DateTime.UtcNow.AddDays(-9),
                    UpdatedAt = DateTime.UtcNow.AddDays(-9)
                },
                new()
                {
                    Id = "DON-88350",
                    Donor = "PureFlow Water Ltd",
                    Type = "Material",
                    Quantity = "10,000 Jerrycans & Tablets",
                    Campaign = "Clean Water Appeal",
                    ReceivedDate = new DateTime(2025, 6, 13),
                    Received = "13 Jun",
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "20L collapsible containers and Aquatabs.",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new()
                {
                    Id = "DON-88343",
                    Donor = "Metropolitan Retailers",
                    Type = "Material",
                    Quantity = "3,000 children's clothing sets",
                    Campaign = "Flood Response 2025",
                    ReceivedDate = new DateTime(2025, 6, 12),
                    Received = "12 Jun",
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "Assorted ages 2-12 waterproof jackets and footwear.",
                    CreatedAt = DateTime.UtcNow.AddDays(-11),
                    UpdatedAt = DateTime.UtcNow.AddDays(-11)
                },
                new()
                {
                    Id = "DON-88336",
                    Donor = "Horizon Foundation",
                    Type = "Monetary",
                    Quantity = "$40,000 earmarked",
                    Campaign = "Clean Water Appeal",
                    ReceivedDate = new DateTime(2025, 6, 11),
                    Received = "11 Jun",
                    Verification = "Verified",
                    Allocation = "Allocated",
                    Notes = "Well drilling engineering supplies subsidy.",
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-12)
                }
            };

            await context.ResourceContributions.AddRangeAsync(contributions);
            await context.SaveChangesAsync();
        }

        if (!context.TransparencyReports.Any())
        {
            var reports = new List<TransparencyReport>
            {
                new()
                {
                    Id = "REP-204",
                    Title = "Weekly Funding Summary",
                    Reference = "REP-204",
                    Date = "25 Jun",
                    ReportDate = new DateTime(2025, 6, 25),
                    Status = "Published",
                    Description = "Comprehensive weekly financial reporting of incoming verified donations and emergency allocations.",
                    Period = "18–25 June 2025",
                    VerifiedContributions = 4820000m,
                    AllocatedFunds = 3910000m,
                    Expenditure = 2740000m,
                    AvailableBalance = 910000m,
                    Author = "Maria Kone",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Id = "REP-198",
                    Title = "Campaign Allocation Detail",
                    Reference = "REP-198",
                    Date = "22 Jun",
                    ReportDate = new DateTime(2025, 6, 22),
                    Status = "Published",
                    Description = "Line-item disbursement breakdown for Flood Response and Medical Relief campaigns.",
                    Period = "15–22 June 2025",
                    VerifiedContributions = 4210000m,
                    AllocatedFunds = 3560000m,
                    Expenditure = 2410000m,
                    AvailableBalance = 650000m,
                    Author = "Maria Kone",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    Id = "REP-191",
                    Title = "Material Contributions Report",
                    Reference = "REP-191",
                    Date = "21 Jun",
                    ReportDate = new DateTime(2025, 6, 21),
                    Status = "Draft",
                    Description = "Audited quantification of donated in-kind humanitarian supplies and logistical tracking.",
                    Period = "14–21 June 2025",
                    VerifiedContributions = 3950000m,
                    AllocatedFunds = 3120000m,
                    Expenditure = 2100000m,
                    AvailableBalance = 830000m,
                    Author = "Maria Kone",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new()
                {
                    Id = "REP-184",
                    Title = "Partner Expenditure Summary",
                    Reference = "REP-184",
                    Date = "20 Jun",
                    ReportDate = new DateTime(2025, 6, 20),
                    Status = "Under Review",
                    Description = "Receipt and invoice reconciliation of regional relief implementing partners.",
                    Period = "01–20 June 2025",
                    VerifiedContributions = 3800000m,
                    AllocatedFunds = 2950000m,
                    Expenditure = 1980000m,
                    AvailableBalance = 850000m,
                    Author = "Maria Kone",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new()
                {
                    Id = "REP-177",
                    Title = "Clean Water Initiative Quarterly Audit",
                    Reference = "REP-177",
                    Date = "15 Jun",
                    ReportDate = new DateTime(2025, 6, 15),
                    Status = "Published",
                    Description = "Quarterly impact and financial accountability statement for water purification programs.",
                    Period = "Q2 2025",
                    VerifiedContributions = 2600000m,
                    AllocatedFunds = 2200000m,
                    Expenditure = 1750000m,
                    AvailableBalance = 400000m,
                    Author = "Maria Kone",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new()
                {
                    Id = "REP-170",
                    Title = "Emergency Shelter Logistics Review",
                    Reference = "REP-170",
                    Date = "10 Jun",
                    ReportDate = new DateTime(2025, 6, 10),
                    Status = "Published",
                    Description = "Temporary shelter material procurement and deployment financial verification.",
                    Period = "01–10 June 2025",
                    VerifiedContributions = 1900000m,
                    AllocatedFunds = 1650000m,
                    Expenditure = 1200000m,
                    AvailableBalance = 250000m,
                    Author = "Maria Kone",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15)
                }
            };

            await context.TransparencyReports.AddRangeAsync(reports);
            await context.SaveChangesAsync();
        }
    }
}
