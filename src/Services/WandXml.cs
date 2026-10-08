using System.Globalization;
using System.Xml;

namespace NoitaSaveScummer.Services;

public sealed record WandSummary(int Index, int? Slot, string Name, IReadOnlyList<string> Spells)
{
    public string Label =>
        $"{Name}: {(Spells.Count == 0 ? "no spells" : string.Join(", ", Spells))}" +
        (Slot is { } s ? $"  (slot {s + 1})" : string.Empty);
}

/// <summary>
/// Reads and edits wands in player.xml. Structure (as serialized by Noita):
///   /Entity                          the player
///     Entity name="inventory_quick"  hotbar
///       Entity tags="...wand..."     a wand: AbilityComponent (stats, cooldown counters), ItemComponent (inventory_slot.x)
///         Entity tags="card_action"  a spell: ItemActionComponent action_id="BLACK_HOLE"
/// Wands are copied verbatim from a real save rather than generated, so the format is always one Noita wrote itself.
/// </summary>
public static class WandXml
{
    public const int WandSlots = 4;

    // AbilityComponent stores cooldowns as absolute frame numbers of the run it came from.
    // Copied into a new run (frame counter near 0) the wand would be stuck in cooldown, so reset them.
    private static readonly string[] FrameCounterAttributes =
        ["mCastDelayStartFrame", "mNextFrameUsable", "mReloadFramesLeft", "mReloadNextFrameUsable"];

    public static IReadOnlyList<WandSummary> ListWands(string playerXmlPath)
    {
        var document = PlayerXml.Load(playerXmlPath);
        return Wands(QuickInventory(document)).Select(Summarize).ToList();
    }

    public static string ExtractWand(string playerXmlPath, int index)
    {
        var wands = Wands(QuickInventory(PlayerXml.Load(playerXmlPath))).ToList();
        if (index < 0 || index >= wands.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"player.xml has {wands.Count} wand(s); index {index} does not exist.");
        return wands[index].OuterXml;
    }

    public static WandSummary Describe(string wandXml) => Summarize(ValidateTemplate(PlayerXml.Parse(wandXml)), 0);

    /// <summary>Adds the wand to the first free hotbar wand slot. Returns the zero-based slot used.</summary>
    public static int InjectWand(string sourcePlayerXml, string destinationPlayerXml, string wandXml)
    {
        var document = PlayerXml.Load(sourcePlayerXml);
        var quick = QuickInventory(document);

        var used = Wands(quick).Select(SlotOf).OfType<int>().ToHashSet();
        var free = Enumerable.Range(0, WandSlots).FirstOrDefault(s => !used.Contains(s), -1);
        if (free < 0)
            throw new InvalidOperationException("All 4 wand slots are full. Drop a wand in Noita, Save & Quit, then try again.");

        var wand = (XmlElement)document.ImportNode(ValidateTemplate(PlayerXml.Parse(wandXml)), deep: true);
        var item = (XmlElement)wand.SelectSingleNode("ItemComponent")!;
        item.SetAttribute("inventory_slot.x", free.ToString(CultureInfo.InvariantCulture));
        item.SetAttribute("inventory_slot.y", "0");

        var ability = (XmlElement)wand.SelectSingleNode("AbilityComponent")!;
        foreach (var attribute in FrameCounterAttributes)
        {
            if (ability.HasAttribute(attribute)) ability.SetAttribute(attribute, "0");
        }

        quick.AppendChild(wand);
        PlayerXml.Save(document, destinationPlayerXml);
        return free;
    }

    private static XmlElement ValidateTemplate(XmlDocument template)
    {
        var root = template.DocumentElement;
        if (root is null || root.Name != "Entity" || !HasTag(root, "wand") ||
            root.SelectSingleNode("AbilityComponent") is null || root.SelectSingleNode("ItemComponent") is null)
        {
            throw new InvalidDataException("Not a wand template (expected <Entity tags=\"...wand...\"> with AbilityComponent and ItemComponent).");
        }
        return root;
    }

    private static XmlElement QuickInventory(XmlDocument document) =>
        document.SelectSingleNode("/Entity/Entity[@name='inventory_quick']") as XmlElement
        ?? throw new InvalidDataException("player.xml has no inventory_quick. Is a run in progress?");

    private static IEnumerable<XmlElement> Wands(XmlElement quickInventory) =>
        quickInventory.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "Entity" && HasTag(e, "wand"));

    private static bool HasTag(XmlElement element, string tag) =>
        element.GetAttribute("tags")
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Contains(tag, StringComparer.Ordinal);

    private static int? SlotOf(XmlElement wand) =>
        wand.SelectSingleNode("ItemComponent") is XmlElement item &&
        int.TryParse(item.GetAttribute("inventory_slot.x"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var slot)
            ? slot
            : null;

    private static WandSummary Summarize(XmlElement wand, int index)
    {
        var uiName = (wand.SelectSingleNode("AbilityComponent") as XmlElement)?.GetAttribute("ui_name");
        var name = string.IsNullOrWhiteSpace(uiName) || uiName.StartsWith('$') ? "Wand" : uiName;
        var spells = wand.ChildNodes.OfType<XmlElement>()
            .Where(e => e.Name == "Entity" && HasTag(e, "card_action"))
            .Select(card => (card.SelectSingleNode("ItemActionComponent") as XmlElement)?.GetAttribute("action_id"))
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => id!)
            .ToList();
        return new WandSummary(index, SlotOf(wand), name, spells);
    }
}
