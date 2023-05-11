using Newtonsoft.Json;
using System;
using System.Diagnostics;
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
            userSettingData.Language = "English";
            File.WriteAllText(settingFile, JsonConvert.SerializeObject(userSettingData, Formatting.Indented));
        }
        Xing.userSettingData = Read_UserSettingData();
        ReadRound(Xing.userSettingData.Language);
    }

    public static UserSettingData Read_UserSettingData()
    {
        using (StreamReader streamReader = new StreamReader(settingFile, Encoding.UTF8))
        {
            UserSettingData data = JsonConvert.DeserializeObject<UserSettingData>(streamReader.ReadToEnd());
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
    }

    public static void Save_UserSettingData(UserSettingData userSettingData)
    {
        using (FileStream fs = new FileStream(settingFile, FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                string json = JsonConvert.SerializeObject(userSettingData, Formatting.Indented);
                sw.Write(json);
            }
        }
    }

    public static void ReadRound(string Language)
    {
        var resources = $"FallGuysRecord.resources.round_{Language}.json";
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resources))
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                Round round = JsonConvert.DeserializeObject<Round>(reader.ReadToEnd());
                Xing.list_Levels = round.levels;
                Xing.list_Shows = round.shows;
            }
        }
    }

    public static Levels GetLevels(string roundName)
    {
        Levels level = new Levels();
        if (Xing.list_Levels != null && Xing.list_Levels.Count > 0)
        {
            foreach (Levels l in Xing.list_Levels)
            {
                if (l.name.Equals(roundName))
                {
                    level = l;
                    break;
                }
            }
        }
        if (roundName.Contains("ugc-"))
        {
            level.showname = roundName.Replace("ugc-", "");
        }
        return level;
    }

    public static Shows GetShows(string id)
    {
        Shows shows = new Shows();
        if (Xing.list_Shows != null && Xing.list_Shows.Count > 0)
        {
            foreach (Shows s in Xing.list_Shows)
            {
                if (s.id.Equals(id))
                {
                    shows = s;
                    break;
                }
            }
        }
        return shows;
    }

    /// <summary>
    /// 判断log文件是否被重置
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool isLogReset(string path)
    {
        FileInfo fileInfo = new FileInfo(path);
        if (fileInfo.Exists && new FileInfo(path).Length == 0)
            return true;
        return false;
    }

    /// <summary>
    /// 判断文件是否文仔
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static bool FileExists(string path)
    {
        return new FileInfo(path).Exists;
    }

    /// <summary>
    /// 判断窗口属于哪个屏幕
    /// 窗口中心点到屏幕中心点 / 屏幕左上角到右下角
    /// </summary>
    /// <param name="window">需要判断的窗口</param>
    /// <returns>属于的屏幕</returns>
    public static Screen getInScreen(Window window)
    {
        Screen[] screens = Screen.AllScreens;
        Dpi dpi = GetDpiBySystemParameters();
        Dpi d_windos = new Dpi(window.Left + window.Width / 2, window.Top + window.Height / 2);
        double dd = double.MaxValue;
        Screen returnScreen = screens[0];
        foreach (Screen s in screens)
        {
            Dpi d_center = new Dpi(s.Bounds.X / dpi.X + s.Bounds.Width / dpi.X / 2, s.Bounds.Y / dpi.Y + s.Bounds.Height / dpi.Y / 2);//屏幕中心点坐标
            Dpi d_f = new Dpi(s.Bounds.X / dpi.X, s.Bounds.Y / dpi.Y);//屏幕左上角坐标
            Dpi d_end = new Dpi(s.Bounds.X / dpi.X + s.Bounds.Width / dpi.X, s.Bounds.Y / dpi.Y + s.Bounds.Height / dpi.Y);//屏幕右下角坐标
            double ddd = (GetDistance(d_windos, d_center) / GetDistance(d_f, d_end));
            if (ddd < dd)
            {
                dd = ddd;
                returnScreen = s;
            }
        }
        return returnScreen;
    }
    /// <summary>
    /// 手动计算，将就着用
    /// </summary>
    /// <returns>Dpi</returns>
    public static Dpi GetDpiBySystemParameters()
    {
        return new Dpi(Screen.AllScreens[0].Bounds.Width / SystemParameters.PrimaryScreenWidth, Screen.AllScreens[0].Bounds.Height / SystemParameters.PrimaryScreenHeight);
    }

    private static double GetDistance(Dpi d1, Dpi d2)
    {
        return Math.Sqrt((d1.X - d2.X) * (d1.X - d2.X) + (d1.Y - d2.Y) * (d1.Y - d2.Y));
    }

    /// <summary>
    /// 将窗口置顶到前台
    /// </summary>
    /// <param name="window">窗口</param>
    public static void Show(Window window)
    {
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }
        window.Activate();
        window.Topmost = true;
    }

    /// <summary>
    /// 判断糖豆人进程是否存在
    /// </summary>
    /// <returns></returns>
    public static bool isFallGuysAlive()
    {
        return Process.GetProcessesByName("FallGuys_client_game").Length != 0;
    }

    /// <summary>
    /// 获取字典中的string值
    /// </summary>
    /// <param name="key">key</param>
    /// <returns>string值</returns>
    public static string getResourcesString(string key)
    {
        return (string)System.Windows.Application.Current.Resources[key];
    }

    /// <summary>
    /// 补齐数字，使得对齐
    /// </summary>
    /// <param name="totallength">需要对齐的总长度</param>
    /// <param name="length">自身长度</param>
    /// <returns></returns>
    public static string NumPadRight(int totallength, int length)
    {
        string re = string.Empty;
        return re.PadRight(totallength - length, '0');
    }
}