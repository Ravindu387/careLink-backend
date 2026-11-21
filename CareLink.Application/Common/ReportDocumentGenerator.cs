namespace CareLink.Application.Common;

using System.Text;

public static class ReportDocumentGenerator
{
    public static byte[] GenerateCsv(IEnumerable<string[]> rows, string[] headers)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    public static byte[] GeneratePdf(string title, string subtitle, IEnumerable<string[]> rows, string[] headers)
    {
        var rowList = rows.ToList();
        var streamContent = new StringBuilder();

        // Document Header
        streamContent.AppendLine("BT");
        streamContent.AppendLine("/F1 16 Tf");
        streamContent.AppendLine("50 740 Td");
        streamContent.AppendLine($"({EscapePdf(title)}) Tj");
        streamContent.AppendLine("ET");

        streamContent.AppendLine("BT");
        streamContent.AppendLine("/F2 10 Tf");
        streamContent.AppendLine("50 718 Td");
        streamContent.AppendLine($"({EscapePdf(subtitle)}) Tj");
        streamContent.AppendLine("ET");

        // Horizontal line separator
        streamContent.AppendLine("0.2 0.48 0.65 RG");
        streamContent.AppendLine("1.5 w");
        streamContent.AppendLine("50 705 m 562 705 l S");
        streamContent.AppendLine("0 0 0 RG");

        // Table Header
        streamContent.AppendLine("BT");
        streamContent.AppendLine("/F1 9 Tf");
        streamContent.AppendLine("50 685 Td");
        var headerLine = string.Join("  |  ", headers.Select(h => Truncate(h, 24)));
        streamContent.AppendLine($"({EscapePdf(headerLine)}) Tj");
        streamContent.AppendLine("ET");

        // Header separator
        streamContent.AppendLine("0.8 0.8 0.8 RG");
        streamContent.AppendLine("0.75 w");
        streamContent.AppendLine("50 675 m 562 675 l S");
        streamContent.AppendLine("0 0 0 RG");

        // Table Rows
        var y = 655;
        for (int i = 0; i < rowList.Count && y > 60; i++)
        {
            var r = rowList[i];
            var rowText = string.Join("  |  ", r.Select(c => Truncate(c ?? string.Empty, 24)));
            streamContent.AppendLine("BT");
            streamContent.AppendLine("/F2 9 Tf");
            streamContent.AppendLine($"50 {y} Td");
            streamContent.AppendLine($"({EscapePdf(rowText)}) Tj");
            streamContent.AppendLine("ET");
            y -= 20;
        }

        // Footer
        streamContent.AppendLine("0.85 0.85 0.85 RG");
        streamContent.AppendLine("0.5 w");
        streamContent.AppendLine("50 48 m 562 48 l S");
        streamContent.AppendLine("0 0 0 RG");

        streamContent.AppendLine("BT");
        streamContent.AppendLine("/F2 8 Tf");
        streamContent.AppendLine("50 34 Td");
        streamContent.AppendLine($"(CareLink Resource Operations Platform - Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC) Tj");
        streamContent.AppendLine("ET");

        var streamBytes = Encoding.Latin1.GetBytes(streamContent.ToString());

        var objects = new List<string>
        {
            "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
            "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n",
            "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R /F2 6 0 R >> >> >>\nendobj\n",
            $"4 0 obj\n<< /Length {streamBytes.Length} >>\nstream\n{streamContent}endstream\nendobj\n",
            "5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>\nendobj\n",
            "6 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n"
        };

        var pdf = new StringBuilder();
        pdf.Append("%PDF-1.4\n");
        var offsets = new List<int>();
        var currentOffset = Encoding.Latin1.GetByteCount(pdf.ToString());

        foreach (var obj in objects)
        {
            offsets.Add(currentOffset);
            pdf.Append(obj);
            currentOffset = Encoding.Latin1.GetByteCount(pdf.ToString());
        }

        var xrefOffset = currentOffset;
        pdf.Append("xref\n");
        pdf.Append($"0 {objects.Count + 1}\n");
        pdf.Append("0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            pdf.Append($"{offset:D10} 00000 n \n");
        }

        pdf.Append("trailer\n");
        pdf.Append($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
        pdf.Append("startxref\n");
        pdf.Append($"{xrefOffset}\n");
        pdf.Append("%%EOF\n");

        return Encoding.Latin1.GetBytes(pdf.ToString());
    }

    private static string Truncate(string text, int max)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length > max ? text[..(max - 3)] + "..." : text;
    }

    private static string EscapePdf(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }
}
