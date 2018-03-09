using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;
using SharpDX;

public class Settings : SettingsBase
{
    public Settings()
    {
        Enable = true;
        PosX = new RangeNode<int>(200, 0, 2000);
        PosY = new RangeNode<int>(30, 0, 2000);

        Width = new RangeNode<int>(500, 0, 1000);
        Height = new RangeNode<int>(30, 0, 100);
        Spacing = new RangeNode<int>(5, 0, 100);

        BGColor = Color.Black;

        UniqueHpColor = Color.Brown;
        RareHpColor = Color.Orange;
        MagicHpColor = new Color(0, 128, 255);

        BorderColor = Color.White;
        ShowMagic = true;
        GroupMagic = true;

        GroupingDistance = new RangeNode<float>(100, 5, 200);
    }

    [Menu("Pos X")]
    public RangeNode<int> PosX { get; set; }

    [Menu("Pos Y")]
    public RangeNode<int> PosY { get; set; }

    [Menu("Width")]
    public RangeNode<int> Width { get; set; }

    [Menu("Height")]
    public RangeNode<int> Height { get; set; }

    [Menu("Spacing")]
    public RangeNode<int> Spacing { get; set; }

    [Menu("BG Color")]
    public ColorNode BGColor { get; set; }

    [Menu("Unique HP Color")]
    public ColorNode UniqueHpColor { get; set; }

    [Menu("Rare HP Color")]
    public ColorNode RareHpColor { get; set; }

    [Menu("Magic HP Color")]
    public ColorNode MagicHpColor { get; set; }


    [Menu("Show Magic monsters", 100)]
    public ToggleNode ShowMagic { get; set; }

    [Menu("Group Magic monsters", 110, 100)]
    public ToggleNode GroupMagic { get; set; }

    [Menu("Border Color")]
    public ColorNode BorderColor { get; set; }

    [Menu("Grouping Distance")]
    public RangeNode<float> GroupingDistance { get; set; }
}