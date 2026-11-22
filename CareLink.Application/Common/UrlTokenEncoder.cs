using System.Text;

namespace CareLink.Application.Common;

public static class UrlTokenEncoder
{
    public static string Encode(string rawToken)
    {
        if (string.IsNullOrEmpty(rawToken))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(rawToken);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static string Decode(string encodedToken)
    {
        if (string.IsNullOrEmpty(encodedToken))
        {
            return string.Empty;
        }

        var incoming = encodedToken
            .Replace('-', '+')
            .Replace('_', '/');

        switch (incoming.Length % 4)
        {
            case 2:
                incoming += "==";
                break;
            case 3:
                incoming += "=";
                break;
        }

        var bytes = Convert.FromBase64String(incoming);
        return Encoding.UTF8.GetString(bytes);
    }

    public static string SafeDecode(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return string.Empty;
        }

        try
        {
            return Decode(token);
        }
        catch
        {
            // If token was not Base64Url encoded (e.g. passed raw or standard URL-escaped), return it as is
            return Uri.UnescapeDataString(token);
        }
    }
}
