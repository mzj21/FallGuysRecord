using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using FontStyle = System.Drawing.FontStyle;

public class Util
{
    //配置文件路径
    private static readonly string settingFile = Environment.CurrentDirectory + "\\Setting.json";
    private static readonly string levelFile = Environment.CurrentDirectory + "\\level_zh.json";

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
            userSettingData.levelPath = levelFile;
            userSettingData.TextColor = Color.Black;
            userSettingData.TextFont = new Font("Segoe UI", 12, FontStyle.Bold, GraphicsUnit.Point);
            userSettingData.Width_Info = 400;
            userSettingData.Height_Info = 500;
            userSettingData.X_Info = SystemParameters.WorkArea.Width - userSettingData.Width_Info;
            userSettingData.Y_Info = SystemParameters.WorkArea.Height - userSettingData.Height_Info;
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
}