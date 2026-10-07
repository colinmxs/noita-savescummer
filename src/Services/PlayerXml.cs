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
        var document = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
        using (var reader = XmlReader.Create(sourcePath, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }))
        {
            document.Load(reader);
        }

        var transform = document.SelectSingleNode("/Entity/_Transform") as XmlElement
            ?? throw new InvalidDataException("player.xml has no <Entity><_Transform> element; position not changed.");

        transform.SetAttribute("position.x", x.ToString("F6", CultureInfo.InvariantCulture));
        transform.SetAttribute("position.y", y.ToString("F6", CultureInfo.InvariantCulture));

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), // no BOM
            OmitXmlDeclaration = document.FirstChild is not XmlDeclaration,
            Indent = false,
        };
        using var writer = XmlWriter.Create(destinationPath, settings);
        document.Save(writer);
    }

    public static (double X, double Y)? ReadPosition(string path)
    {
        var document = new XmlDocument { XmlResolver = null };
        using (var reader = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }))
        {
            document.Load(reader);
        }
        if (document.SelectSingleNode("/Entity/_Transform") is not XmlElement transform) return null;
        return double.TryParse(transform.GetAttribute("position.x"), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
               double.TryParse(transform.GetAttribute("position.y"), NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            ? (x, y)
            : null;
    }
}
