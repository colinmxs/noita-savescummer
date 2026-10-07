using System.Globalization;
using System.Text;
using System.Xml;

namespace NoitaSaveScummer.Services;

public static class PlayerXml
{
    /// <summary>
    /// Copies player.xml, rewriting the player entity's _Transform position.
    /// Numbers are always written with the invariant culture: on e.g. German Windows the old code wrote
    /// "215,000000", which Noita cannot parse.
    /// </summary>
    public static void WriteWithPosition(string sourcePath, string destinationPath, double x, double y)
    {
        var document = Load(sourcePath);

        var transform = document.SelectSingleNode("/Entity/_Transform") as XmlElement
            ?? throw new InvalidDataException("player.xml has no <Entity><_Transform> element; position not changed.");

        transform.SetAttribute("position.x", x.ToString("F6", CultureInfo.InvariantCulture));
        transform.SetAttribute("position.y", y.ToString("F6", CultureInfo.InvariantCulture));

        Save(document, destinationPath);
    }

    public static (double X, double Y)? ReadPosition(string path)
    {
        var document = Load(path);
        if (document.SelectSingleNode("/Entity/_Transform") is not XmlElement transform) return null;
        return double.TryParse(transform.GetAttribute("position.x"), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
               double.TryParse(transform.GetAttribute("position.y"), NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            ? (x, y)
            : null;
    }

    /// <summary>Loads an entity XML file preserving whitespace, with DTDs and external resolution disabled.</summary>
    internal static XmlDocument Load(string path)
    {
        var document = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
        using var reader = XmlReader.Create(path, SafeReaderSettings);
        document.Load(reader);
        return document;
    }

    internal static XmlDocument Parse(string xml)
    {
        var document = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
        using var reader = XmlReader.Create(new StringReader(xml), SafeReaderSettings);
        document.Load(reader);
        return document;
    }

    /// <summary>Writes UTF-8 without BOM, keeping an XML declaration only if the original had one.</summary>
    internal static void Save(XmlDocument document, string path)
    {
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = document.FirstChild is not XmlDeclaration,
            Indent = false,
        };
        using var writer = XmlWriter.Create(path, settings);
        document.Save(writer);
    }

    private static readonly XmlReaderSettings SafeReaderSettings = new() { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
}
