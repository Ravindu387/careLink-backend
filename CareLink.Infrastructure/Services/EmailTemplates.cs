using System.Net;

namespace CareLink.Infrastructure.Services;

public static class EmailTemplates
{
    public static string GetVerificationEmailSubject() => "Verify your CareLink account";

    public static string GetCitizenWelcomeSubject() => "Welcome to CareLink - Your Citizen Account is Active";

    public static string GetVolunteerWelcomeSubject() => "Welcome to CareLink - Your Volunteer Account is Active";

    public static string GetPartnerPendingSubject() => "CareLink Partner Organisation Registration - Pending Administrator Review";

    public static string GetPartnerApprovedSubject() => "CareLink Partner Organisation Approved - Account Ready";

    public static string GetPartnerRejectedSubject() => "CareLink Partner Organisation Registration Decision";

    public static string GetPartnerAdditionalInfoSubject() => "Additional Information Required - CareLink Partner Organisation Verification";

    public static string GetAdminNewPartnerSubject(string orgName) => $"New Partner Organisation Registration Pending Review: {orgName}";

    public static string GetAdminCreatedUserSubject() => "Your CareLink Account Has Been Created";

    public static string GetCitizenWelcomeHtml(string recipientName, string email)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeEmail = WebUtility.HtmlEncode(email);
        return BuildBrandedLayout(
            "Welcome to CareLink Humanitarian Platform",
            $@"
            <p style=""margin: 0 0 16px 0; font-size: 15px; line-height: 1.6; color: #2C4A60;"">
              Hello <strong>{safeName}</strong>,
            </p>
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Your CareLink <strong>Citizen</strong> account (username: <strong>{safeEmail}</strong>) has been created and is <strong>active immediately</strong>.
            </p>
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              You can now sign in using your registered email address to request emergency assistance, view nearby shelters, receive official disaster safety alerts, and manage your household support profile.
            </p>");
    }

    public static string GetCitizenWelcomePlainText(string recipientName, string email)
    {
        return $@"Hello {recipientName},

Welcome to CareLink Humanitarian Aid Platform!
Your Citizen account (username: {email}) has been created and is active immediately.
You can now sign in with your email address to access Citizen features.

Regards,
CareLink Admin Team
CareLink Humanitarian Aid Platform";
    }

    public static string GetVolunteerWelcomeHtml(string recipientName, string email, string? skills, string? availability)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeEmail = WebUtility.HtmlEncode(email);
        var safeSkills = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(skills) ? "General Humanitarian Support" : skills);
        var safeAvailability = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(availability) ? "Available" : availability);

        return BuildBrandedLayout(
            "Welcome to the CareLink Volunteer Network",
            $@"
            <p style=""margin: 0 0 16px 0; font-size: 15px; line-height: 1.6; color: #2C4A60;"">
              Hello <strong>{safeName}</strong>,
            </p>
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Thank you for registering as a <strong>Volunteer</strong> with CareLink. Your account (username: <strong>{safeEmail}</strong>) is <strong>active immediately</strong>.
            </p>
            <div style=""background-color: #EFF6F7; border-left: 4px solid #42B99A; padding: 14px 18px; margin: 18px 0; border-radius: 6px;"">
              <p style=""margin: 0 0 6px 0; font-size: 13px; color: #123B5C;""><strong>Registered Skills:</strong> {safeSkills}</p>
              <p style=""margin: 0; font-size: 13px; color: #123B5C;""><strong>Availability:</strong> {safeAvailability}</p>
            </div>
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              You can log in immediately using your email address to view field assignments, update your skills and availability, and coordinate with response teams.
            </p>");
    }

    public static string GetVolunteerWelcomePlainText(string recipientName, string email, string? skills, string? availability)
    {
        return $@"Hello {recipientName},

Welcome to the CareLink Volunteer Network!
Your Volunteer account (username: {email}) has been registered and is active immediately.
Registered Skills: {(string.IsNullOrWhiteSpace(skills) ? "General Humanitarian Support" : skills)}
Availability: {(string.IsNullOrWhiteSpace(availability) ? "Available" : availability)}

You can sign in now using your email address.

Regards,
CareLink Admin Team
CareLink Humanitarian Aid Platform";
    }

    public static string GetPartnerPendingHtml(string recipientName, string email, string organizationName)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeEmail = WebUtility.HtmlEncode(email);
        var safeOrg = WebUtility.HtmlEncode(organizationName);

        return BuildBrandedLayout(
            "Partner Registration Received & Awaiting Review",
            $@"
            <p style=""margin: 0 0 16px 0; font-size: 15px; line-height: 1.6; color: #2C4A60;"">
              Hello <strong>{safeName}</strong>,
            </p>
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Your CareLink Partner Organisation registration has been received and is awaiting administrator review.
            </p>
            <div style=""background-color: #FFF8EB; border-left: 4px solid #F59E0B; padding: 14px 18px; margin: 18px 0; border-radius: 6px;"">
              <p style=""margin: 0; font-size: 13px; color: #92400E;"">
                <strong>Organisation:</strong> {safeOrg}<br/>
                <strong>Account Username:</strong> {safeEmail}<br/>
                <strong>Current Account Status: Pending Administrator Approval</strong><br/>
                To protect humanitarian operations, all Partner Organisation accounts require verification and approval by a CareLink Administrator before accessing partner features.
              </p>
            </div>
            <p style=""margin: 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              You will receive an email notification as soon as an administrator reviews your application and verification documents.
            </p>");
    }

    public static string GetPartnerPendingPlainText(string recipientName, string email, string organizationName)
    {
        return $@"Hello {recipientName},

Your CareLink Partner Organisation registration has been received and is awaiting administrator review.
Organisation: {organizationName}
Username: {email}
Current Status: Pending Administrator Approval.

You will receive an email notification once an administrator reviews your registration.

Regards,
CareLink Admin Team
CareLink Humanitarian Aid Platform";
    }

    public static string GetPartnerApprovedHtml(string recipientName, string email, string organizationName, string loginUrl)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeEmail = WebUtility.HtmlEncode(email);
        var safeOrg = WebUtility.HtmlEncode(organizationName);
        var safeUrl = WebUtility.HtmlEncode(loginUrl);

        return BuildBrandedLayout(
            "Your Partner Organisation Registration is Approved",
            $@"
            <p style=""margin: 0 0 16px 0; font-size: 15px; line-height: 1.6; color: #2C4A60;"">
              Hello <strong>{safeName}</strong>,
            </p>
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Your CareLink Partner Organisation registration has been approved. You can now access the platform.
            </p>
            <p style=""margin: 0 0 20px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Your organisation profile for <strong>{safeOrg}</strong> and account (username: <strong>{safeEmail}</strong>) are now <strong>Active</strong>.
            </p>
            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" style=""margin: 24px 0;"">
              <tr>
                <td align=""center"" style=""border-radius: 8px; background-color: #123B5C;"">
                  <a href=""{safeUrl}"" target=""_blank"" style=""font-size: 14px; font-weight: 600; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 8px; display: inline-block; background-color: #123B5C;"">
                    Sign In to CareLink &rarr;
                  </a>
                </td>
              </tr>
            </table>");
    }

    public static string GetPartnerApprovedPlainText(string recipientName, string email, string organizationName, string loginUrl)
    {
        return $@"Hello {recipientName},

Your CareLink Partner Organisation registration has been approved. You can now access the platform.
Organisation: {organizationName}
Username: {email}

Sign in here: {loginUrl}

Regards,
CareLink Admin Team
CareLink Humanitarian Aid Platform";
    }

    public static string GetPartnerRejectedHtml(string recipientName, string email, string organizationName, string? rejectionReason)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeOrg = WebUtility.HtmlEncode(organizationName);
        var safeReason = WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(rejectionReason)
                ? "The submitted organisation credentials could not be verified at this time."
                : rejectionReason);

        return BuildBrandedLayout(
            "Partner Organisation Registration Decision",
            $@"
            <p style=""margin: 0 0 16px 0; font-size: 15px; line-height: 1.6; color: #2C4A60;"">
              Hello <strong>{safeName}</strong>,
            </p>
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Your CareLink Partner Organisation registration has not been approved. Please review the administrator's comments.
            </p>
            <div style=""background-color: #FEF2F2; border-left: 4px solid #EF4444; padding: 14px 18px; margin: 18px 0; border-radius: 6px;"">
              <p style=""margin: 0; font-size: 13px; color: #991B1B;"">
                <strong>Organisation:</strong> {safeOrg}<br/>
                <strong>Administrator Comments / Reason:</strong> {safeReason}
              </p>
            </div>
            <p style=""margin: 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              If you believe this decision was made in error or have updated verification documentation, please contact the CareLink administration office.
            </p>");
    }

    public static string GetPartnerRejectedPlainText(string recipientName, string email, string organizationName, string? rejectionReason)
    {
        var reason = string.IsNullOrWhiteSpace(rejectionReason)
            ? "The submitted organisation credentials could not be verified at this time."
            : rejectionReason;

        return $@"Hello {recipientName},

Your CareLink Partner Organisation registration has not been approved. Please review the administrator's comments.
Organisation: {organizationName}
Username: {email}
Administrator Comments: {reason}

Please contact CareLink administration if you have additional verification documents.

Regards,
CareLink Admin Team
CareLink Humanitarian Aid Platform";
    }

    public static string GetPartnerAdditionalInfoHtml(string recipientName, string email, string organizationName, string additionalInfoRequest)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeOrg = WebUtility.HtmlEncode(organizationName);
        var safeNote = WebUtility.HtmlEncode(additionalInfoRequest);

        return BuildBrandedLayout(
            "Additional Information Required for Organisation Verification",
            $@"
            <p style=""margin: 0 0 16px 0; font-size: 15px; line-height: 1.6; color: #2C4A60;"">
              Hello <strong>{safeName}</strong>,
            </p>
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Additional information is required to complete your organisation verification.
            </p>
            <div style=""background-color: #FFF8EB; border-left: 4px solid #F59E0B; padding: 14px 18px; margin: 18px 0; border-radius: 6px;"">
              <p style=""margin: 0; font-size: 13px; color: #92400E;"">
                <strong>Organisation:</strong> {safeOrg}<br/>
                <strong>Requested by Administrator:</strong> {safeNote}
              </p>
            </div>
            <p style=""margin: 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Your registration remains pending until the requested information is provided and reviewed by an administrator. You may submit your response and supporting documents from the CareLink sign-in page using your registered login credentials.
            </p>");
    }

    public static string GetPartnerAdditionalInfoPlainText(string recipientName, string email, string organizationName, string additionalInfoRequest)
    {
        return $@"Hello {recipientName},

Additional information is required to complete your organisation verification.
Organisation: {organizationName}
Account Username: {email}
Requested Information: {additionalInfoRequest}

Your registration remains pending until the review is completed. You can submit the requested information from the CareLink sign-in page.

Regards,
CareLink Admin Team
CareLink Humanitarian Aid Platform";
    }

    public static string GetAdminNewPartnerNotificationHtml(
        string organizationName,
        string organizationType,
        string representativeName,
        string representativeEmail)
    {
        var safeOrg = WebUtility.HtmlEncode(organizationName);
        var safeType = WebUtility.HtmlEncode(organizationType);
        var safeRep = WebUtility.HtmlEncode(representativeName);
        var safeEmail = WebUtility.HtmlEncode(representativeEmail);

        return BuildBrandedLayout(
            "New Partner Organisation Registration Pending Review",
            $@"
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              A new Partner Organisation has submitted a registration application and verification documents on CareLink and is awaiting administrator review.
            </p>
            <div style=""background-color: #EFF6F7; border-left: 4px solid #123B5C; padding: 16px 20px; margin: 20px 0; border-radius: 6px;"">
              <p style=""margin: 0 0 6px 0; font-size: 13px; color: #123B5C;"">&bull; <strong>Organisation Name:</strong> {safeOrg}</p>
              <p style=""margin: 0 0 6px 0; font-size: 13px; color: #123B5C;"">&bull; <strong>Organisation Type:</strong> {safeType}</p>
              <p style=""margin: 0 0 6px 0; font-size: 13px; color: #123B5C;"">&bull; <strong>Authorised Representative:</strong> {safeRep}</p>
              <p style=""margin: 0; font-size: 13px; color: #123B5C;"">&bull; <strong>Representative Email:</strong> {safeEmail}</p>
            </div>
            <p style=""margin: 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Please review the application and uploaded verification documents in the CareLink Admin Verification Queue or Organisation Management dashboard.
            </p>");
    }

    public static string GetAdminNewPartnerNotificationPlainText(
        string organizationName,
        string organizationType,
        string representativeName,
        string representativeEmail)
    {
        return $@"New Partner Organisation Registration Pending Review

Organisation Name: {organizationName}
Organisation Type: {organizationType}
Authorised Representative: {representativeName}
Representative Email: {representativeEmail}

Please sign in to the CareLink Admin Verification Queue to review the submitted details and verification documents.";
    }

    public static string GetAdminCreatedUserHtml(string recipientName, string email, string assignedRole, string setupPasswordUrl)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeEmail = WebUtility.HtmlEncode(email);
        var safeRole = WebUtility.HtmlEncode(assignedRole);
        var safeUrl = WebUtility.HtmlEncode(setupPasswordUrl);

        return BuildBrandedLayout(
            "Your CareLink Account Has Been Created",
            $@"
            <p style=""margin: 0 0 16px 0; font-size: 15px; line-height: 1.6; color: #2C4A60;"">
              Hello <strong>{safeName}</strong>,
            </p>
            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              An administrator has created a CareLink account for you.
            </p>
            <div style=""background-color: #EFF6F7; border-left: 4px solid #123B5C; padding: 16px 20px; margin: 20px 0; border-radius: 6px;"">
              <p style=""margin: 0 0 8px 0; font-size: 13px; font-weight: 700; color: #123B5C;"">Your account details:</p>
              <p style=""margin: 0 0 6px 0; font-size: 13px; color: #123B5C;"">&bull; <strong>Name:</strong> {safeName}</p>
              <p style=""margin: 0 0 6px 0; font-size: 13px; color: #123B5C;"">&bull; <strong>Username:</strong> {safeEmail}</p>
              <p style=""margin: 0; font-size: 13px; color: #123B5C;"">&bull; <strong>Assigned Role:</strong> {safeRole}</p>
            </div>
            <p style=""margin: 0 0 20px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
              Please complete your account setup by clicking the button below and creating your password.
            </p>
            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" style=""margin: 26px 0;"">
              <tr>
                <td align=""center"" style=""border-radius: 8px; background-color: #123B5C;"">
                  <a href=""{safeUrl}"" target=""_blank"" style=""font-size: 14px; font-weight: 600; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 8px; display: inline-block; background-color: #123B5C; letter-spacing: 0.3px;"">
                    Set Up My CareLink Account
                  </a>
                </td>
              </tr>
            </table>
            <p style=""margin: 18px 0 8px 0; font-size: 12px; color: #627D98;"">
              If the button above does not open in your email client, copy and paste this fallback link into your browser:
            </p>
            <p style=""margin: 0 0 20px 0; font-size: 12px; word-break: break-all; line-height: 1.5;"">
              <a href=""{safeUrl}"" style=""color: #2785B2; text-decoration: underline;"">{safeUrl}</a>
            </p>
            <hr style=""border: none; border-top: 1px solid #E4E7EB; margin: 24px 0 18px 0;"">
            <p style=""margin: 0 0 10px 0; font-size: 12px; color: #627D98; line-height: 1.5;"">
              The setup link is secure, unique to your account, single-use, and expires after 24 hours.
            </p>
            <p style=""margin: 0 0 18px 0; font-size: 12px; color: #627D98; line-height: 1.5;"">
              If you did not expect this invitation, please contact the CareLink administrator.
            </p>
            <p style=""margin: 0; font-size: 13px; color: #2C4A60; line-height: 1.5;"">
              Regards,<br/>
              <strong>CareLink Admin Team</strong><br/>
              CareLink Humanitarian Aid Platform
            </p>");
    }

    public static string GetAdminCreatedUserPlainText(string recipientName, string email, string assignedRole, string setupPasswordUrl)
    {
        return $@"Hello {recipientName},

An administrator has created a CareLink account for you.

Your account details:
- Name: {recipientName}
- Username: {email}
- Assigned Role: {assignedRole}

Please complete your account setup by clicking the link below and creating your password:
{setupPasswordUrl}

The setup link is secure, unique to your account, single-use, and expires after 24 hours.

If you did not expect this invitation, please contact the CareLink administrator.

Regards,
CareLink Admin Team
CareLink Humanitarian Aid Platform";
    }

    private static string BuildBrandedLayout(string heading, string innerBodyHtml)
    {
        var safeHeading = WebUtility.HtmlEncode(heading);
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{safeHeading}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #EFF6F7; font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #173B56;"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #EFF6F7; padding: 40px 15px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(18, 59, 92, 0.08); border: 1px solid #DCE7E9;"">
          <tr>
            <td style=""background-color: #123B5C; padding: 24px 36px; text-align: left;"">
              <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                <tr>
                  <td style=""vertical-align: middle; padding-right: 14px;"">
                    <img src=""http://localhost:5173/assets/carelink-logo.png"" alt=""CareLink Logo"" width=""56"" height=""56"" style=""display: block; width: 56px; height: 56px; border: 0; outline: none; text-decoration: none; object-fit: contain;"" />
                  </td>
                  <td style=""vertical-align: middle;"">
                    <div style=""color: #ffffff; font-size: 20px; font-weight: 700; letter-spacing: -0.3px; line-height: 1.2;"">
                      CareLink
                    </div>
                    <div style=""color: #55BBA3; font-size: 12px; font-weight: 500; margin-top: 3px;"">
                      Connecting Help. Saving Lives.
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""padding: 40px 40px 32px 40px;"">
              <h1 style=""margin: 0 0 16px 0; color: #123B5C; font-size: 22px; font-weight: 700; line-height: 1.3;"">
                {safeHeading}
              </h1>
              {innerBodyHtml}
            </td>
          </tr>
          <tr>
            <td style=""background-color: #F8FAFB; padding: 24px 40px; text-align: center; border-top: 1px solid #EAEFF2;"">
              <p style=""margin: 0; font-size: 11px; color: #627D98; font-weight: 500;"">
                &copy; {DateTime.UtcNow.Year} CareLink Humanitarian Aid Platform. All rights reserved.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string GetVerificationEmailHtml(string recipientName, string confirmationUrl)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeUrl = WebUtility.HtmlEncode(confirmationUrl);

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Verify your CareLink account</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #EFF6F7; font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #173B56;"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #EFF6F7; padding: 40px 15px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(18, 59, 92, 0.08); border: 1px solid #DCE7E9;"">
          <tr>
            <td style=""background-color: #123B5C; padding: 24px 36px; text-align: left;"">
              <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                <tr>
                  <td style=""vertical-align: middle; padding-right: 14px;"">
                    <img src=""http://localhost:5173/assets/carelink-logo.png"" alt=""CareLink Logo"" width=""56"" height=""56"" style=""display: block; width: 56px; height: 56px; border: 0; outline: none; text-decoration: none; object-fit: contain;"" />
                  </td>
                  <td style=""vertical-align: middle;"">
                    <div style=""color: #ffffff; font-size: 20px; font-weight: 700; letter-spacing: -0.3px; line-height: 1.2;"">
                      CareLink
                    </div>
                    <div style=""color: #55BBA3; font-size: 12px; font-weight: 500; margin-top: 3px;"">
                      Connecting Help. Saving Lives.
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""padding: 40px 40px 32px 40px;"">
              <h1 style=""margin: 0 0 16px 0; color: #123B5C; font-size: 22px; font-weight: 700; line-height: 1.3;"">
                Verify your email address
              </h1>
              <p style=""margin: 0 0 16px 0; font-size: 15px; line-height: 1.6; color: #2C4A60;"">
                Hello <strong>{safeName}</strong>,
              </p>
              <p style=""margin: 0 0 24px 0; font-size: 14px; line-height: 1.6; color: #486581;"">
                Thank you for creating an account with CareLink. To activate your access and safeguard our response network, please confirm your email address by clicking the button below:
              </p>
              <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" style=""margin: 28px 0;"">
                <tr>
                  <td align=""center"" style=""border-radius: 8px; background-color: #123B5C;"">
                    <a href=""{safeUrl}"" target=""_blank"" style=""font-size: 14px; font-weight: 600; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 8px; display: inline-block; background-color: #123B5C;"">
                      Verify My Email &rarr;
                    </a>
                  </td>
                </tr>
              </table>
              <p style=""margin: 24px 0 8px 0; font-size: 12px; color: #627D98;"">
                If the button above does not work, copy and paste this link into your web browser:
              </p>
              <p style=""margin: 0 0 24px 0; font-size: 12px; word-break: break-all; line-height: 1.5;"">
                <a href=""{safeUrl}"" style=""color: #2785B2; text-decoration: underline;"">{safeUrl}</a>
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string GetVerificationEmailPlainText(string recipientName, string confirmationUrl)
    {
        return $@"Hello {recipientName},

Thank you for registering with CareLink Humanitarian Aid Platform.

Please verify your email address by visiting the following link:
{confirmationUrl}

This link will expire in 24 hours.

--
CareLink Humanitarian Aid Platform";
    }

    public static string GetVerificationSuccessHtml(string loginUrl)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <title>Email Verified - CareLink</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #EFF6F7; font-family: sans-serif; display: flex; align-items: center; justify-content: center; min-height: 100vh;"">
  <div style=""max-width: 520px; width: 90%; margin: 40px auto; background: #ffffff; border-radius: 12px; text-align: center; padding: 48px 36px;"">
    <h1 style=""color: #123B5C;"">Email Verified Successfully</h1>
    <p style=""color: #486581;"">Your CareLink account email has been verified. You can now sign in.</p>
    <a href=""{loginUrl}"" style=""display: inline-block; background-color: #123B5C; color: #ffffff; text-decoration: none; font-weight: 600; padding: 12px 28px; border-radius: 8px;"">Proceed to Sign In &rarr;</a>
  </div>
</body>
</html>";
    }

    public static string GetVerificationErrorHtml(string errorMessage, string resendUrl)
    {
        var safeError = WebUtility.HtmlEncode(errorMessage);
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <title>Verification Failed - CareLink</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #EFF6F7; font-family: sans-serif; display: flex; align-items: center; justify-content: center; min-height: 100vh;"">
  <div style=""max-width: 520px; width: 90%; margin: 40px auto; background: #ffffff; border-radius: 12px; text-align: center; padding: 48px 36px;"">
    <h1 style=""color: #123B5C;"">Verification Failed</h1>
    <p style=""color: #C53030;"">{safeError}</p>
    <a href=""{resendUrl}"" style=""display: inline-block; background-color: #123B5C; color: #ffffff; text-decoration: none; font-weight: 600; padding: 12px 28px; border-radius: 8px;"">Return to Sign In</a>
  </div>
</body>
</html>";
    }
}
