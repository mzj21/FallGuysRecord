using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using FontStyle = System.Drawing.FontStyle;

namespace FallGuysRecord_WPF_Framework
{
    internal class Util
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
                userSettingData.X = SystemParameters.WorkArea.Width / 2;
                userSettingData.Y = SystemParameters.WorkArea.Height / 2;
                userSettingData.Width = 303;
                userSettingData.Height = 83;
                userSettingData.levelPath = levelFile;
                userSettingData.TextColor = Color.Black;
                userSettingData.TextFont = new Font("Segoe UI", 12, FontStyle.Bold, GraphicsUnit.Pixel);
                File.WriteAllText(settingFile, JsonConvert.SerializeObject(userSettingData, Formatting.Indented));
            }
        }
        public static UserSettingData Read_UserSettingData()
        {
            StreamReader streamReader = new StreamReader(settingFile, Encoding.UTF8);
            UserSettingData data = JsonConvert.DeserializeObject<UserSettingData>(streamReader.ReadToEnd());
            streamReader.Close();
            streamReader.Dispose();
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
    }
}
