﻿using System;
using System.Drawing;
using System.Windows.Forms;

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
    public Boolean isPrimaryScreen { get; set; }
    public String levelPath { get; set; }
    public Color TextColor { get; set; }
    public Font TextFont { get; set; }
    public double X_Info { get; set; }
    public double Y_Info { get; set; }
    public double Width_Info { get; set; }
    public double Height_Info { get; set; }
    public Boolean isPrimaryScreen_Info { get; set; }
    public Boolean isShowFastestName { get; set; }
    public Boolean isOriginalViewMode { get; set; } = true;
    public String OverlayHotkey { get; set; } = "F7";
    public String RoundInfoHotkey { get; set; } = "F8";
    public String RoundInfoBackground { get; set; }
}