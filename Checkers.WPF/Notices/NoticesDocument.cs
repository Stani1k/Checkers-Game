using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Checkers.WPF.Notices;

public sealed record NoticePackage(string Name, Uri? Url, string Version, string License, string UsedBy);

public sealed record NoticeLicense(string Id, string AppliesTo, string Copyright, string Text);

public sealed record NoticesDocument(IReadOnlyList<NoticePackage> Packages, IReadOnlyList<NoticeLicense> Licenses)
{
    public static NoticesDocument Empty { get; } = new(Array.Empty<NoticePackage>(), Array.Empty<NoticeLicense>());

    private static readonly Regex PackageRow = new(
        @"^\|\s*\[(?<name>[^\]]+)\]\((?<url>[^)]*)\)\s*\|\s*(?<version>[^|]*?)\s*\|\s*(?<license>[^|]*?)\s*\|\s*(?<used>[^|]*?)\s*\|\s*$",
        RegexOptions.Compiled);

    /// <summary>Parses the markdown produced by scripts/Update-ThirdPartyNotices.ps1.</summary>
    public static NoticesDocument Parse(string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var packages = new List<NoticePackage>();
        var licenses = new List<NoticeLicense>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var row = PackageRow.Match(line);
            if (row.Success)
            {
                Uri.TryCreate(row.Groups["url"].Value, UriKind.Absolute, out var url);
                if (url is { Scheme: not ("http" or "https") }) url = null;
                packages.Add(new NoticePackage(row.Groups["name"].Value, url, row.Groups["version"].Value,
                    row.Groups["license"].Value, row.Groups["used"].Value));
            }
            else if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                licenses.Add(ParseLicense(lines, ref i));
            }
        }

        return new NoticesDocument(packages, licenses);
    }

    private static NoticeLicense ParseLicense(string[] lines, ref int index)
    {
        var id = lines[index][4..].Trim();
        var applies = new List<string>();
        var copyright = new List<string>();
        var text = new List<string>();
        var inFence = false;
        var inApplies = false;

        for (index++; index < lines.Length && (inFence || !lines[index].StartsWith("#", StringComparison.Ordinal)); index++)
        {
            var line = lines[index];
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                inFence = !inFence;
                continue;
            }

            if (inFence) text.Add(line);
            else if (line.StartsWith("Applies to:", StringComparison.Ordinal))
            {
                inApplies = true;
                applies.Add(line["Applies to:".Length..].Trim());
            }
            else if (string.IsNullOrWhiteSpace(line)) inApplies = false;
            else if (line.StartsWith("- ", StringComparison.Ordinal)) copyright.Add(line[2..].Trim());
            else if (inApplies) applies.Add(line.Trim());
        }

        index--;
        return new NoticeLicense(id, string.Join(" ", applies), string.Join(Environment.NewLine, copyright),
            string.Join(Environment.NewLine, text).Trim());
    }
}
