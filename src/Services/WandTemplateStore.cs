using System.Globalization;
using System.Text;

namespace NoitaSaveScummer.Services;

public sealed record WandTemplate(string Name, string FilePath, WandSummary Summary);

/// <summary>Saved wands (one wand entity per .xml file) under NoitaSaveBackups/wand_templates.</summary>
public sealed class WandTemplateStore
{
    private readonly string _directory;

    public WandTemplateStore(string directory)
    {
        _directory = directory;
    }

    /// <summary>Lists valid templates. Files that fail validation are reported in <paramref name="problems"/>, not hidden.</summary>
    public IReadOnlyList<WandTemplate> List(out IReadOnlyList<string> problems)
    {
        var found = new List<WandTemplate>();
        var errors = new List<string>();
        if (Directory.Exists(_directory))
        {
            foreach (var file in Directory.EnumerateFiles(_directory, "*.xml", FileOps.TopLevelAll).Order(StringComparer.Ordinal))
            {
                try
                {
                    found.Add(new WandTemplate(Path.GetFileNameWithoutExtension(file), file, WandXml.Describe(File.ReadAllText(file))));
                }
                catch (Exception ex) when (ex is InvalidDataException or System.Xml.XmlException)
                {
                    errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }
        problems = errors;
        return found;
    }

    public WandTemplate Save(string wandXml, DateTime now)
    {
        var summary = WandXml.Describe(wandXml); // validate before writing
        var name = $"{Slug(summary.Spells.FirstOrDefault() ?? summary.Name)}_{now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)}";
        var path = Path.Combine(_directory, name + ".xml");
        for (var i = 2; File.Exists(path); i++)
            path = Path.Combine(_directory, $"{name}_{i.ToString(CultureInfo.InvariantCulture)}.xml");

        FileOps.WriteAllTextAtomic(path, wandXml);
        return new WandTemplate(Path.GetFileNameWithoutExtension(path), path, summary);
    }

    public static string Load(WandTemplate template) => File.ReadAllText(template.FilePath);

    private static string Slug(string value)
    {
        var sb = new StringBuilder();
        foreach (var c in value.ToLowerInvariant())
            sb.Append(char.IsAsciiLetterOrDigit(c) ? c : '_');
        var slug = sb.ToString().Trim('_');
        return slug.Length == 0 ? "wand" : slug;
    }
}
