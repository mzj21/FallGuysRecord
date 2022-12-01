using System.Drawing;

public class UserSettingData
{
    public UserSettingData() { }
    public string OverlayBackground { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public Color TextColor { get; set; }
    public Font TextFont { get; set; }
    public double X_Info { get; set; }
    public double Y_Info { get; set; }
    public double Width_Info { get; set; }
    public double Height_Info { get; set; }
    public bool isShowFastestName { get; set; }
    public bool isOriginalViewMode { get; set; } = true;
    public string OverlayHotkey { get; set; } = "F7";
    public string RoundInfoHotkey { get; set; } = "F8";
    public string RoundInfoBackground { get; set; }
    public string Language { get; set; }
}