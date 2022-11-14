using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using FontStyle = System.Drawing.FontStyle;

public class Util
{
    //配置文件路径
    private static readonly string settingFile = Environment.CurrentDirectory + "\\Setting.json";

    public static void Init()
    {
        if (!File.Exists(settingFile))
        {
            UserSettingData userSettingData = new UserSettingData();
            userSettingData.OverlayBackground = "";
            userSettingData.Width = 303;
            userSettingData.Height = 83;
            userSettingData.X = (SystemParameters.WorkArea.Width - userSettingData.Width) / 2;
            userSettingData.Y = (SystemParameters.WorkArea.Height - userSettingData.Height) / 2;
            userSettingData.levelPath = "";
            userSettingData.TextColor = Color.Black;
            userSettingData.TextFont = new Font("Segoe UI", 12, FontStyle.Bold, GraphicsUnit.Point);
            userSettingData.Width_Info = 400;
            userSettingData.Height_Info = 500;
            userSettingData.X_Info = SystemParameters.WorkArea.Width - userSettingData.Width_Info;
            userSettingData.Y_Info = SystemParameters.WorkArea.Height - userSettingData.Height_Info;
            userSettingData.isShowFastestName = false;
            userSettingData.isOriginalViewMode = true;
            userSettingData.OverlayHotkey = "F7";
            userSettingData.RoundInfoHotkey = "F8";
            userSettingData.RoundInfoBackground = "";
            File.WriteAllText(settingFile, JsonConvert.SerializeObject(userSettingData, Formatting.Indented));
        }
    }
    public static UserSettingData Read_UserSettingData()
    {
        StreamReader streamReader = new StreamReader(settingFile, Encoding.UTF8);
        UserSettingData data = JsonConvert.DeserializeObject<UserSettingData>(streamReader.ReadToEnd());
        streamReader.Close();
        streamReader.Dispose();
        if (data.Width <= 40)
        {
            data.Width = 303;
        }
        if (data.Height <= 40)
        {
            data.Height = 83;
        }
        if (data.Width_Info <= 40)
        {
            data.Width_Info = 400;
        }
        if (data.Height_Info <= 40)
        {
            data.Height_Info = 500;
        }
        return data;
    }

    public static void Save_UserSettingData(UserSettingData userSettingData)
    {
        FileStream fs = new FileStream(settingFile, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
        string json = JsonConvert.SerializeObject(userSettingData, Formatting.Indented);
        sw.Write(json);
        sw.Close();
        sw.Dispose();
        fs.Close();
        fs.Dispose();
    }

    public static List<LevelMap> Read_LevelMap(String levelPath)
    {
        StreamReader streamReader = new StreamReader(levelPath, Encoding.UTF8);
        List<LevelMap> list_LevelMap = JsonConvert.DeserializeObject<List<LevelMap>>(streamReader.ReadToEnd());
        streamReader.Close();
        streamReader.Dispose();
        return list_LevelMap;
    }

    public static LevelMap GetLevelMap(String roundName)
    {
        LevelMap levelMap = new LevelMap();
        if (Xing.list_LevelMap != null && Xing.list_LevelMap.Count > 0)
        {
            foreach (LevelMap l in Xing.list_LevelMap)
            {
                if (l.name.Equals(roundName))
                {
                    levelMap = l;
                    break;
                }
            }
        }
        return levelMap;
    }

    /// <summary>
    /// 判断log文件是否被重置
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Boolean isLogReset(String path)
    {
        return new FileInfo(path).Length == 0;
    }

    /// <summary>
    /// 判断窗口属于哪个屏幕（目前不完善，超出会返回主屏幕，暂时想不出好的方法处理超出部分）
    /// </summary>
    /// <param name="window">窗口</param>
    /// <returns>属于的屏幕</returns>
    public static Screen getInScreen(Window window)
    {
        Screen[] screens = Screen.AllScreens;
        Dpi dpi = GetDpiBySystemParameters();
        foreach (Screen s in screens)
        {
            if (window.Left + window.Width / 2 > s.Bounds.X / dpi.X && window.Left + window.Width / 2 < (s.Bounds.X + s.Bounds.Width) / dpi.X && window.Top + window.Height / 2 > s.Bounds.Y / dpi.Y && window.Top + window.Height / 2 < (s.Bounds.Y + s.Bounds.Height) / dpi.Y)
            {
                return s;
            }
        }
        return screens[0];
    }
    /// <summary>
    /// 手动计算，将就着用
    /// </summary>
    /// <returns>Dpi</returns>
    public static Dpi GetDpiBySystemParameters()
    {
        return new Dpi(Screen.AllScreens[0].Bounds.Width / SystemParameters.PrimaryScreenWidth, Screen.AllScreens[0].Bounds.Height / SystemParameters.PrimaryScreenHeight);
    }
}