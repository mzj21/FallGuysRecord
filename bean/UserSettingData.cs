using System;
using System.Drawing;

public class UserSettingData
{
    public UserSettingData()
    {
    }
    public String OverlayBackground { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public String levelPath { get; set; }
    public Color TextColor { get; set; }
    public Font TextFont { get; set; }
    public double X_Info { get; set; }
    public double Y_Info { get; set; }
    public double Width_Info { get; set; }
    public double Height_Info { get; set; }
}