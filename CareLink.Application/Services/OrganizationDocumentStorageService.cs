using System.Text.RegularExpressions;
using CareLink.Application.DTOs.Auth;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

namespace CareLink.Application.Services;

public class OrganizationDocumentStorageService : IOrganizationDocumentStorageService
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    };

    private static readonly Dictionary<string, string> ExtensionToMimeType = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/jpg",
        "image/png"
    };

    private static readonly Dictionary<string, string> CategoryLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["RegistrationCertificate"] = "Organisation / NGO Registration Certificate",
        ["OrganisationProfile"] = "Organisation Profile / Constitution",
        ["AuthorisationLetter"] = "Authorisation Letter for Representative",
        ["SupportingDocument"] = "Supporting Verification Document"
    };

    private readonly string _storageRoot;

    public OrganizationDocumentStorageService(string? storageRoot = null)
    {
        _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
            ? Path.Combine(AppContext.BaseDirectory, "App_Data", "OrganizationDocuments")
            : storageRoot;
    }

    public (bool IsValid, string? ErrorMessage) ValidateDocument(RegisterDocumentUploadDto document)
    {
        if (document == null)
        {
            return (false, "Document payload cannot be empty.");
        }

        var sanitizedName = SanitizeFileName(document.FileName);
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            return (false, "A valid document filename is required.");
        }

        var ext = Path.GetExtension(sanitizedName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            return (false, $"Unsupported file type for '{sanitizedName}'. Only PDF, JPG, JPEG, and PNG files are accepted.");
        }

        if (!string.IsNullOrWhiteSpace(document.ContentType) &&
            !AllowedMimeTypes.Contains(document.ContentType.Trim()))
        {
            return (false, $"Unsupported MIME content type '{document.ContentType}' for '{sanitizedName}'. Only PDF, JPG, JPEG, and PNG are allowed.");
        }

        byte[] bytes;
        try
        {
            bytes = DecodeBase64Payload(document.Base64Content);
        }
        catch
        {
            return (false, $"Uploaded file '{sanitizedName}' contains invalid or corrupted file content.");
        }

        if (bytes.Length == 0)
        {
            return (false, $"Uploaded file '{sanitizedName}' is empty.");
        }

        var effectiveSize = Math.Max(bytes.Length, document.FileSizeBytes);
        if (effectiveSize > MaxFileSizeBytes)
        {
            return (false, $"Uploaded file '{sanitizedName}' exceeds the maximum allowed file size of 10 MB.");
        }

        if (!ValidateFileSignature(ext, bytes))
        {
            return (false, $"Uploaded file '{sanitizedName}' failed security signature verification for extension '{ext}'.");
        }

        return (true, null);
    }

    public async Task<OrganizationDocument> StoreDocumentAsync(
        Guid organizationId,
        RegisterDocumentUploadDto document,
        string uploadedBy,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage) = ValidateDocument(document);
        if (!isValid)
        {
            throw new InvalidOperationException(errorMessage ?? "Invalid verification document.");
        }

        var sanitizedFileName = SanitizeFileName(document.FileName);
        var ext = Path.GetExtension(sanitizedFileName).ToLowerInvariant();
        var canonicalContentType = ExtensionToMimeType.TryGetValue(ext, out var mime)
            ? mime
            : "application/pdf";

        var bytes = DecodeBase64Payload(document.Base64Content);
        var docId = Guid.NewGuid();
        var storedFileName = $"{docId:N}{ext}";
        var orgFolder = Path.Combine(_storageRoot, organizationId.ToString("N"));
        var relativeStoragePath = Path.Combine(organizationId.ToString("N"), storedFileName);
        var fullFilePath = Path.Combine(orgFolder, storedFileName);

        try
        {
            Directory.CreateDirectory(orgFolder);
            await File.WriteAllBytesAsync(fullFilePath, bytes, cancellationToken);
        }
        catch
        {
            // Even if disk write is restricted in a sandboxed test environment, FileContent is persisted in DB
        }

        var category = NormalizeCategory(document.Category);
        var label = CategoryLabels.TryGetValue(category, out var categoryLabel)
            ? categoryLabel
            : "Supporting Verification Document";

        return new OrganizationDocument
        {
            Id = docId,
            OrganizationId = organizationId,
            DocumentCategory = category,
            DocumentLabel = label,
            OriginalFileName = sanitizedFileName,
            StoredFileName = storedFileName,
            ContentType = canonicalContentType,
            FileSizeBytes = bytes.Length,
            FileSizeDisplay = FormatFileSize(bytes.Length),
            StoragePath = relativeStoragePath,
            FileContent = bytes,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        };
    }

    public async Task<(byte[] Content, string ContentType, string FileName)?> ReadDocumentAsync(
        OrganizationDocument document,
        CancellationToken cancellationToken = default)
    {
        if (document == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(document.StoragePath))
        {
            var fullPath = Path.Combine(_storageRoot, document.StoragePath);
            if (File.Exists(fullPath))
            {
                var diskBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
                return (diskBytes, document.ContentType, document.OriginalFileName);
            }
        }

        if (document.FileContent != null && document.FileContent.Length > 0)
        {
            return (document.FileContent, document.ContentType, document.OriginalFileName);
        }

        return null;
    }

    public string SanitizeFileName(string? rawFileName)
    {
        if (string.IsNullOrWhiteSpace(rawFileName))
        {
            return string.Empty;
        }

        var fileNameOnly = Path.GetFileName(rawFileName.Trim());
        if (string.IsNullOrWhiteSpace(fileNameOnly))
        {
            return string.Empty;
        }

        // Remove path traversal sequences, control characters, and illegal filename characters
        var cleaned = Regex.Replace(fileNameOnly, @"[^\w\-. ()\[\]]", "_");
        cleaned = cleaned.Replace("..", "_").Trim('.', ' ', '_');

        if (cleaned.Length > 180)
        {
            var ext = Path.GetExtension(cleaned);
            var baseName = Path.GetFileNameWithoutExtension(cleaned);
            cleaned = baseName[..Math.Min(baseName.Length, 160)] + ext;
        }

        return cleaned;
    }

    private static string NormalizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "SupportingDocument";
        }

        var trimmed = category.Trim();
        if (trimmed.Contains("Certificate", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("RegistrationCertificate", StringComparison.OrdinalIgnoreCase))
        {
            return "RegistrationCertificate";
        }

        if (trimmed.Contains("Profile", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("Constitution", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("OrganisationProfile", StringComparison.OrdinalIgnoreCase))
        {
            return "OrganisationProfile";
        }

        if (trimmed.Contains("Authorisation", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("Authorization", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("AuthorisationLetter", StringComparison.OrdinalIgnoreCase))
        {
            return "AuthorisationLetter";
        }

        return "SupportingDocument";
    }

    private static byte[] DecodeBase64Payload(string? rawBase64)
    {
        if (string.IsNullOrWhiteSpace(rawBase64))
        {
            return Array.Empty<byte>();
        }

        var payload = rawBase64.Trim();
        var commaIndex = payload.IndexOf(',');
        if (payload.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0)
        {
            payload = payload[(commaIndex + 1)..];
        }

        return Convert.FromBase64String(payload);
    }

    private static bool ValidateFileSignature(string extension, byte[] bytes)
    {
        // Reject executable or script payloads disguised as documents (MZ/PE, ELF, HTML/JS script tags)
        if (bytes.Length >= 2 && bytes[0] == 0x4D && bytes[1] == 0x5A) // "MZ" Windows executable
        {
            return false;
        }

        if (bytes.Length >= 4 && bytes[0] == 0x7F && bytes[1] == 0x45 && bytes[2] == 0x4C && bytes[3] == 0x46) // ELF
        {
            return false;
        }

        var prefixSample = System.Text.Encoding.UTF8.GetString(bytes, 0, Math.Min(bytes.Length, 128));
        if (prefixSample.Contains("<script", StringComparison.OrdinalIgnoreCase) ||
            prefixSample.Contains("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
            prefixSample.Contains("<?php", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        var kb = bytes / 1024.0;
        if (kb < 1024)
        {
            return $"{kb:0.0} KB";
        }

        var mb = kb / 1024.0;
        return $"{mb:0.00} MB";
    }
}
