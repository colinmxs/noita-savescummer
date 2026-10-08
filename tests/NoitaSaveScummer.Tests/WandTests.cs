using NoitaSaveScummer.Models;
using NoitaSaveScummer.Services;

namespace NoitaSaveScummer.Tests;

public class WandTests : IDisposable
{
    private readonly TempSave _save = new();

    public void Dispose() => _save.Dispose();

    private static string Wand(int slot, string name, params string[] spells) =>
        $"<Entity name=\"\" tags=\"teleportable_NOT,wand,item\">" +
        $"<AbilityComponent ui_name=\"{name}\" mana=\"100\" mNextFrameUsable=\"98765\" mReloadNextFrameUsable=\"98000\" mCastDelayStartFrame=\"97000\" mReloadFramesLeft=\"12\">" +
        "<gun_config deck_capacity=\"4\" actions_per_round=\"1\" /></AbilityComponent>" +
        $"<ItemComponent inventory_slot.x=\"{slot}\" inventory_slot.y=\"0\" permanently_attached=\"0\" />" +
        string.Concat(spells.Select(s =>
            $"<Entity tags=\"card_action\"><ItemActionComponent action_id=\"{s}\" /><ItemComponent inventory_slot.x=\"0\" inventory_slot.y=\"0\" uses_remaining=\"-1\" /></Entity>")) +
        "</Entity>";

    private static string Player(params string[] quickItems) =>
        "<Entity name=\"DEBUG_NAME:player\" tags=\"player_unit\">\n" +
        "  <_Transform position.x=\"100\" position.y=\"200\" />\n" +
        $"  <Entity name=\"inventory_quick\">{string.Concat(quickItems)}" +
        "<Entity tags=\"potion,item\"><ItemComponent inventory_slot.x=\"0\" inventory_slot.y=\"0\" /></Entity></Entity>\n" +
        "  <Entity name=\"inventory_full\"></Entity>\n" +
        "</Entity>\n";

    private string PlayerPath => Path.Combine(_save.SavePath, "player.xml");

    [Fact]
    public void Lists_only_wands_with_their_spells_and_slots()
    {
        _save.Write("player.xml", Player(Wand(0, "Starter", "LIGHT_BULLET"), Wand(1, "Bomb wand", "BOMB", "BOMB")));

        var wands = WandXml.ListWands(PlayerPath);

        Assert.Equal(2, wands.Count);
        Assert.Equal(["BOMB", "BOMB"], wands[1].Spells);
        Assert.Equal(1, wands[1].Slot);
    }

    [Fact]
    public void Inject_uses_first_free_slot_and_resets_cooldown_frames()
    {
        _save.Write("player.xml", Player(Wand(0, "Starter", "LIGHT_BULLET"), Wand(2, "Other", "BOMB")));
        var template = Wand(3, "Black hole wand", "BLACK_HOLE");
        var output = PlayerPath + ".out";

        var slot = WandXml.InjectWand(PlayerPath, output, template);

        Assert.Equal(1, slot);
        var wands = WandXml.ListWands(output);
        var injected = Assert.Single(wands, w => w.Spells.Contains("BLACK_HOLE"));
        Assert.Equal(1, injected.Slot);
        var xml = File.ReadAllText(output);
        // Only the injected wand is reset; the two existing wands keep their counters (they belong to this run).
        Assert.Equal(2, xml.Split("mNextFrameUsable=\"98765\"").Length - 1);
        Assert.Equal(1, xml.Split("mNextFrameUsable=\"0\"").Length - 1);
        Assert.Equal(1, xml.Split("mReloadNextFrameUsable=\"0\"").Length - 1);
        Assert.Contains("tags=\"potion,item\"", xml); // other items untouched
    }

    [Fact]
    public void Inject_refuses_when_all_wand_slots_are_full()
    {
        _save.Write("player.xml", Player(Wand(0, "a"), Wand(1, "b"), Wand(2, "c"), Wand(3, "d")));
        Assert.Throws<InvalidOperationException>(() =>
            WandXml.InjectWand(PlayerPath, PlayerPath + ".out", Wand(0, "x", "BLACK_HOLE")));
        Assert.False(File.Exists(PlayerPath + ".out"));
    }

    [Fact]
    public void Non_wand_templates_are_rejected()
    {
        _save.Write("player.xml", Player());
        Assert.Throws<InvalidDataException>(() =>
            WandXml.InjectWand(PlayerPath, PlayerPath + ".out", "<Entity tags=\"potion,item\"><ItemComponent /></Entity>"));
    }

    [Fact]
    public void Template_round_trip_save_extract_and_give()
    {
        _save.Write("player.xml", Player(Wand(0, "Starter", "LIGHT_BULLET"), Wand(1, "Black hole wand", "BLACK_HOLE")));
        var store = new WandTemplateStore(Path.Combine(_save.Root, "wand_templates"));

        var saved = store.Save(WandXml.ExtractWand(PlayerPath, 1), new DateTime(2025, 1, 2, 3, 4, 5));
        File.WriteAllText(Path.Combine(_save.Root, "wand_templates", "broken.xml"), "<nope/>");
        var listed = store.List(out var problems);

        Assert.Equal("black_hole_2025-01-02_03-04-05", saved.Name);
        Assert.Equal(["BLACK_HOLE"], Assert.Single(listed).Summary.Spells);
        Assert.Single(problems);

        // New run with only the starter wand.
        _save.Write("player.xml", Player(Wand(0, "Starter", "LIGHT_BULLET")));
        WandXml.InjectWand(PlayerPath, PlayerPath + ".out", WandTemplateStore.Load(listed[0]));
        Assert.Contains(WandXml.ListWands(PlayerPath + ".out"), w => w.Spells.Contains("BLACK_HOLE") && w.Slot == 1);
    }

    [Fact]
    public async Task Edit_player_xml_takes_undo_backup_and_replaces_atomically()
    {
        _save.WriteRun("A");
        _save.Write("player.xml", Player(Wand(0, "Starter", "LIGHT_BULLET")));
        var service = _save.CreateService();

        var undo = await service.EditPlayerXmlAsync((current, temp) =>
            WandXml.InjectWand(current, temp, Wand(0, "Black hole wand", "BLACK_HOLE")));

        Assert.NotNull(undo);
        Assert.Equal(BackupKind.PreRestore, undo!.Kind);
        Assert.Contains(WandXml.ListWands(PlayerPath), w => w.Spells.Contains("BLACK_HOLE"));
        Assert.False(File.Exists(PlayerPath + ".scummer-tmp"));
    }

    [Fact]
    public async Task Failed_edit_leaves_player_xml_unchanged()
    {
        _save.WriteRun("A");
        _save.Write("player.xml", Player(Wand(0, "a"), Wand(1, "b"), Wand(2, "c"), Wand(3, "d")));
        var before = _save.Read("player.xml");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _save.CreateService().EditPlayerXmlAsync((current, temp) =>
            WandXml.InjectWand(current, temp, Wand(0, "x", "BLACK_HOLE"))));

        Assert.Equal(before, _save.Read("player.xml"));
        Assert.False(File.Exists(PlayerPath + ".scummer-tmp"));
    }
}
