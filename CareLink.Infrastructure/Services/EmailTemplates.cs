using System.Net;

namespace CareLink.Infrastructure.Services;

public static class EmailTemplates
{
    public static string GetVerificationEmailSubject() => "Verify your CareLink account";

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
          <!-- Header -->
          <tr>
            <td style=""background-color: #123B5C; padding: 32px 40px; text-align: left;"">
              <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
                <tr>
                  <td>
                    <div style=""display: inline-block; background-color: #55BBA3; color: #123B5C; font-weight: 800; font-size: 18px; padding: 6px 14px; border-radius: 6px; vertical-align: middle; letter-spacing: -0.3px;"">
                      CareLink
                    </div>
                    <span style=""color: #ffffff; font-size: 15px; font-weight: 600; margin-left: 12px; vertical-align: middle;"">
                      Humanitarian Aid Platform
                    </span>
                  </td>
                </tr>
              </table>
              <p style=""margin: 8px 0 0 0; color: #B8D0DF; font-size: 12px;"">
                Secure Humanitarian Response &amp; Crisis Coordination Network
              </p>
            </td>
          </tr>

          <!-- Body -->
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

              <!-- Verification Button -->
              <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" style=""margin: 28px 0;"">
                <tr>
                  <td align=""center"" style=""border-radius: 8px; background-color: #123B5C;"">
                    <a href=""{safeUrl}"" target=""_blank"" style=""font-size: 14px; font-weight: 600; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 8px; display: inline-block; background-color: #123B5C; letter-spacing: 0.3px;"">
                      Verify My Email &rarr;
                    </a>
                  </td>
                </tr>
              </table>

              <!-- Fallback Link -->
              <p style=""margin: 24px 0 8px 0; font-size: 12px; color: #627D98;"">
                If the button above does not work, copy and paste this link into your web browser:
              </p>
              <p style=""margin: 0 0 24px 0; font-size: 12px; word-break: break-all; line-height: 1.5;"">
                <a href=""{safeUrl}"" style=""color: #2785B2; text-decoration: underline;"">{safeUrl}</a>
              </p>

              <hr style=""border: none; border-top: 1px solid #E4E7EB; margin: 28px 0 20px 0;"">

              <!-- Security Note -->
              <p style=""margin: 0; font-size: 12px; color: #829AB1; line-height: 1.5;"">
                This verification link will expire in 24 hours. If you did not register for a CareLink account, you can safely disregard this message. No account will be activated without email verification.
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""background-color: #F8FAFB; padding: 24px 40px; text-align: center; border-top: 1px solid #EAEFF2;"">
              <p style=""margin: 0; font-size: 11px; color: #627D98; font-weight: 500;"">
                &copy; {DateTime.UtcNow.Year} CareLink Humanitarian Aid Platform. All rights reserved.
              </p>
              <p style=""margin: 4px 0 0 0; font-size: 10px; color: #829AB1;"">
                Coordinated response &middot; Human decisions &middot; Activity is logged for security
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

This link will expire in 24 hours. If you did not create a CareLink account, you can safely ignore this message.

--
CareLink Humanitarian Aid Platform
Coordinated response. Human decisions.";
    }

    public static string GetVerificationSuccessHtml(string loginUrl)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Email Verified - CareLink</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #EFF6F7; font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; display: flex; align-items: center; justify-content: center; min-height: 100vh;"">
  <div style=""max-width: 520px; width: 90%; margin: 40px auto; background: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(18, 59, 92, 0.08); overflow: hidden; border: 1px solid #DCE7E9; text-align: center; padding: 48px 36px;"">
    <div style=""display: inline-flex; align-items: center; justify-content: center; width: 64px; height: 64px; border-radius: 50%; background-color: #E6F7F2; margin-bottom: 24px;"">
      <svg width=""32"" height=""32"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#42B99A"" stroke-width=""2.5"" stroke-linecap=""round"" stroke-linejoin=""round"">
        <path d=""M20 6 9 17l-5-5""/>
      </svg>
    </div>
    <h1 style=""margin: 0 0 12px 0; color: #123B5C; font-size: 24px; font-weight: 700;"">Email Verified Successfully</h1>
    <p style=""margin: 0 0 28px 0; color: #486581; font-size: 14px; line-height: 1.6;"">
      Your CareLink account email has been verified. You can now sign in to access the humanitarian response network.
    </p>
    <a href=""{loginUrl}"" style=""display: inline-block; background-color: #123B5C; color: #ffffff; text-decoration: none; font-weight: 600; font-size: 14px; padding: 12px 28px; border-radius: 8px; letter-spacing: 0.3px;"">
      Proceed to Sign In &rarr;
    </a>
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
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Verification Failed - CareLink</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #EFF6F7; font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; display: flex; align-items: center; justify-content: center; min-height: 100vh;"">
  <div style=""max-width: 520px; width: 90%; margin: 40px auto; background: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(18, 59, 92, 0.08); overflow: hidden; border: 1px solid #DCE7E9; text-align: center; padding: 48px 36px;"">
    <div style=""display: inline-flex; align-items: center; justify-content: center; width: 64px; height: 64px; border-radius: 50%; background-color: #FDF0ED; margin-bottom: 24px;"">
      <svg width=""32"" height=""32"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#E64D5A"" stroke-width=""2.5"" stroke-linecap=""round"" stroke-linejoin=""round"">
        <circle cx=""12"" cy=""12"" r=""10""/>
        <line x1=""12"" y1=""8"" x2=""12"" y2=""12""/>
        <line x1=""12"" y1=""16"" x2=""12.01"" y2=""16""/>
      </svg>
    </div>
    <h1 style=""margin: 0 0 12px 0; color: #123B5C; font-size: 24px; font-weight: 700;"">Verification Failed</h1>
    <p style=""margin: 0 0 28px 0; color: #C53030; font-size: 14px; line-height: 1.6;"">
      {safeError}
    </p>
    <a href=""{resendUrl}"" style=""display: inline-block; background-color: #123B5C; color: #ffffff; text-decoration: none; font-weight: 600; font-size: 14px; padding: 12px 28px; border-radius: 8px; letter-spacing: 0.3px;"">
      Return to Sign In
    </a>
  </div>
</body>
</html>";
    }
}
