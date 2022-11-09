using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static HotkeyUtil;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Window = System.Windows.Window;

namespace FallGuysRecord
{
    public partial class Window_Overlay : Window, ReaderListener
    {
        UserSettingData userSettingData;
        System.Timers.Timer timer;
        DateTime startTime;
        TimeSpan timeSpan;
        private int num;
        private String roundName;
        private Window_ListView listView;
        private LogReader logReader;

        #region [初始化界面]
        public Window_Overlay()
        {
            InitializeComponent();

            Util.Init();
            userSettingData = Util.Read_UserSettingData();
            if (!String.IsNullOrEmpty(userSettingData.levelPath) && File.Exists(userSettingData.levelPath))
                Xing.list_LevelMap = Util.Read_LevelMap(userSettingData.levelPath);
            if (!String.IsNullOrEmpty(userSettingData.OverlayBackground) && File.Exists(userSettingData.OverlayBackground))
                overlay_background.Source = new BitmapImage(new Uri(userSettingData.OverlayBackground));
            overlay_window.Left = userSettingData.X;
            overlay_window.Top = userSettingData.Y;
            overlay_window.Width = userSettingData.Width;
            overlay_window.Height = userSettingData.Height;
            if (overlay_window.Left < 0)
            {
                overlay_window.Left = 0;
            }
            if (overlay_window.Left > SystemParameters.PrimaryScreenWidth - overlay_window.Width)
            {
                overlay_window.Left = SystemParameters.PrimaryScreenWidth - overlay_window.Width;
            }
            if (overlay_window.Top < 0)
            {
                overlay_window.Top = 0;
            }
            if (overlay_window.Top > SystemParameters.PrimaryScreenHeight - overlay_window.Height)
            {
                overlay_window.Top = SystemParameters.PrimaryScreenHeight - overlay_window.Height;
            }
            SolidBrush sb = new SolidBrush(userSettingData.TextColor);
            overlay_window.Foreground = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
            overlay_window.FontFamily = new FontFamily(userSettingData.TextFont.FontFamily.Name);
            overlay_window.FontWeight = userSettingData.TextFont.Bold ? FontWeights.Bold : FontWeights.Regular;
            overlay_window.FontStyle = userSettingData.TextFont.Italic ? FontStyles.Italic : FontStyles.Normal;
            TextDecorationCollection textDecorations = new TextDecorationCollection();
            if (userSettingData.TextFont.Underline)
                textDecorations.Add(TextDecorations.Underline);
            if (userSettingData.TextFont.Strikeout)
                textDecorations.Add(TextDecorations.Strikethrough);
            t1.TextDecorations = textDecorations;
            overlay_window.FontSize = userSettingData.TextFont.Size;
            t9.FontSize = userSettingData.TextFont.Size + 2;
            l1.Visibility = userSettingData.isOriginalViewMode ? Visibility.Visible : Visibility.Hidden;
            r1.Visibility = userSettingData.isOriginalViewMode ? Visibility.Visible : Visibility.Hidden;
            c1.Visibility = userSettingData.isOriginalViewMode ? Visibility.Hidden : Visibility.Visible;
            #region [秒表计时器]
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            #endregion

            #region [开启线程读取log]
            listView = new Window_ListView();
            logReader = new LogReader(this, listView);
            logReader.Start();
            #endregion
        }
        #endregion
        #region [窗口置顶]
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }
        private void overlay_window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }
        #endregion
        #region [拖动窗口,禁止最大化]
        private void overlay_window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    var windowMode = this.ResizeMode;
                    if (this.ResizeMode != ResizeMode.NoResize)
                    {
                        this.ResizeMode = ResizeMode.NoResize;
                    }
                    this.UpdateLayout();
                    DragMove();
                    if (this.ResizeMode != windowMode)
                    {
                        this.ResizeMode = windowMode;
                    }
                    this.UpdateLayout();
                }
            }
        }
        #endregion
        #region [窗口贴边]
        private void overlay_window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Mouse.LeftButton == MouseButtonState.Released)
                {
                    if (overlay_window.Left < 0)
                    {
                        overlay_window.Left = 0;
                    }
                    if (overlay_window.Left > SystemParameters.PrimaryScreenWidth - overlay_window.Width)
                    {
                        overlay_window.Left = SystemParameters.PrimaryScreenWidth - overlay_window.Width;
                    }
                    if (overlay_window.Top < 0)
                    {
                        overlay_window.Top = 0;
                    }
                    if (overlay_window.Top > SystemParameters.PrimaryScreenHeight - overlay_window.Height)
                    {
                        overlay_window.Top = SystemParameters.PrimaryScreenHeight - overlay_window.Height;
                    }
                }
            }
        }
        #endregion
        #region [修改背景图]
        private void MenuItem_Click_0(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = userSettingData.OverlayBackground,
                Filter = @"(*.jpg,*.png,)|*.jpeg;*.jpg;*.png"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                overlay_background.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                userSettingData.OverlayBackground = openFileDialog.FileName;
                Util.Save_UserSettingData(userSettingData);
            }
        }
        #endregion
        #region [修改字体]
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            FontDialog fontDialog = new FontDialog
            {
                Font = userSettingData.TextFont
            };
            if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Font f = fontDialog.Font;
                overlay_window.FontFamily = new FontFamily(f.FontFamily.Name);
                listView.window_listview.FontFamily = new FontFamily(f.FontFamily.Name);
                overlay_window.FontWeight = f.Bold ? FontWeights.Bold : FontWeights.Regular;
                listView.window_listview.FontWeight = f.Bold ? FontWeights.Bold : FontWeights.Regular;
                overlay_window.FontStyle = f.Italic ? FontStyles.Italic : FontStyles.Normal;
                listView.window_listview.FontStyle = f.Italic ? FontStyles.Italic : FontStyles.Normal;
                TextDecorationCollection textDecorations = new TextDecorationCollection();
                if (f.Underline)
                    textDecorations.Add(TextDecorations.Underline);
                if (f.Strikeout)
                    textDecorations.Add(TextDecorations.Strikethrough);
                t1.TextDecorations = textDecorations;
                overlay_window.FontSize = f.Size;
                listView.window_listview.FontSize = f.Size;
                userSettingData.TextFont = f;
                t9.FontSize = f.Size + 2;
                Util.Save_UserSettingData(userSettingData);
            }
        }
        #endregion
        #region [修改文字颜色]
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog
            {
                Color = userSettingData.TextColor
            };
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SolidBrush sb = new SolidBrush(colorDialog.Color);
                SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
                overlay_window.Foreground = solidColorBrush;
                listView.window_listview.Foreground = solidColorBrush;
                userSettingData.TextColor = colorDialog.Color;
                Util.Save_UserSettingData(userSettingData);
            }
        }
        #endregion
        #region [修改地图语言文件]
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = userSettingData.OverlayBackground,
                Filter = @"(*.json)|*.json"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                userSettingData.levelPath = openFileDialog.FileName;
                Util.Save_UserSettingData(userSettingData);
                Xing.list_LevelMap = Util.Read_LevelMap(openFileDialog.FileName);
                LevelMap levelMap = Util.GetLevelMap(roundName);
                SetText("", "", levelMap.showname + "(" + num + ")", levelMap.type, "", "", "", "");
            }
        }
        #endregion
        #region [开启回合信息流]
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            if (listView.IsVisible)
            {
                listView.Hide();
            }
            else
            {
                listView.Show();
            }
        }
        #endregion
        #region [切换显示模式]
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            userSettingData.isOriginalViewMode = !userSettingData.isOriginalViewMode;
            Util.Save_UserSettingData(userSettingData);
            l1.Visibility = userSettingData.isOriginalViewMode ? Visibility.Visible : Visibility.Hidden;
            r1.Visibility = userSettingData.isOriginalViewMode ? Visibility.Visible : Visibility.Hidden;
            c1.Visibility = userSettingData.isOriginalViewMode ? Visibility.Hidden : Visibility.Visible;
        }
        #endregion
        #region [回合信息背景切换]
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = userSettingData.OverlayBackground,
                Filter = @"(*.jpg,*.png,)|*.jpeg;*.jpg;*.png"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                listView.roundinfo_background.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                userSettingData.RoundInfoBackground = openFileDialog.FileName;
                Util.Save_UserSettingData(userSettingData);
            }
        }
        #endregion
        #region [位置移动监听]
        private void Overlay_window_LocationChanged(object sender, EventArgs e)
        {
            userSettingData.X = overlay_window.Left;
            userSettingData.Y = overlay_window.Top;
            Util.Save_UserSettingData(userSettingData);
        }
        #endregion
        #region [大小改变监听]
        private void Overlay_window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            userSettingData.Width = overlay_window.Width;
            userSettingData.Height = overlay_window.Height;
            Util.Save_UserSettingData(userSettingData);
        }
        #endregion
        #region [修改文本]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="win">'s' Win:1/1</param>
        /// <param name="ping">'s' PING</param>
        /// <param name="roundShowName">'s' 回合名</param>
        /// <param name="roundType">'s' 回合类型</param>
        /// <param name="timeAll">'s' 计时器时间(回合结束关闭)</param>
        /// <param name="timeMe">'s' 自己的时间</param>
        /// <param name="timeFirst">'s' 第一时间</param>
        /// <param name="firstName">'s' 第一名字</param>
        /// /
        /// 
        private void SetText(String win, String ping, String roundShowName, String roundType, String timeAll, String timeMe, String timeFirst, String firstName)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                if (!string.IsNullOrEmpty(win))
                    t1.Text = win;
                if (!string.IsNullOrEmpty(ping))
                    t2.Text = ping;
                if (!string.IsNullOrEmpty(roundShowName))
                    t3.Text = roundShowName;
                if (!string.IsNullOrEmpty(roundType))
                    t4.Text = roundType;
                if (!string.IsNullOrEmpty(timeAll))
                    t5.Text = timeAll;
                if (!string.IsNullOrEmpty(timeMe))
                    t6.Text = timeMe;
                if (!string.IsNullOrEmpty(timeFirst))
                    t7.Text = timeFirst;
                if (!string.IsNullOrEmpty(firstName))
                    t8.Text = firstName;
            }));
        }
        private void SetTextEasy(String time1, String time2)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                if (!string.IsNullOrEmpty(time1))
                    t9.Text = time1;
                if (!string.IsNullOrEmpty(time2))
                    t10.Text = time2;
            }));
        }
        #endregion
        #region [回合监听事件]
        public void RoundInit(int num, LevelMap levelMap)
        {
            this.num = num;
            roundName = levelMap.name;
            SetText("", "", levelMap.showname + "(" + num + ")", levelMap.type, "--:--:---", "--:--:---", "--:--:---", "------");
            SetTextEasy("--:--", "--:--:---");
        }

        public void RoundStart()
        {
            startTime = DateTime.Now;
            timer.Interval = 1000;
            timer.Start();
            SetText("", "", "", "", "00:00:000", "00:00:000", "00:00:000", "");
            SetTextEasy("00:00", "--:--:---");
        }

        public void RoundUpdateFirst(Player player, string time)
        {
            SetText("", "", "", "", "", "", time, userSettingData.isShowFastestName ? player.playerName : "(" + player.platform + ")");
        }
        public void RoundUpdateMe(Player player, string time, int rank)
        {
            SetText("", "", "", "", "", "#" + rank + " - " + time, "", "");
            SetTextEasy("", "#" + rank + " - " + time);
        }

        public void RoundUpdateTotal(string time)
        {
        }

        public void RoundBalance(string balance)
        {
            SetText("", "", "", balance, "", "", "", "");
        }

        public void RoundEnd(String endtime)
        {
            timer.Stop();
            SetText("", "", "", "", endtime, "", "", "");
            SetTextEasy(endtime.Substring(0, 5), "");
        }

        public void RoundExit(int match, int win, String wins)
        {
            timer.Stop();
            SetText("win(" + win + "/" + match + ")", "", "", wins, "", "", "", "");
        }

        public void Ping(string ping)
        {
            SetText("", "Ping:" + ping + "ms", "", "", "", "", "", "");
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Process.GetProcessesByName("FallGuys_client_game").Length == 0)
            {
                timer.Stop();
            }
            timeSpan = DateTime.Now - startTime;
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                t5.Text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds) + ":000";
            }));
            SetTextEasy(string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds), "");
        }
        #endregion
        #region [修复进程依旧存在的问题]
        private void overlay_window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HotkeyUtil.UnRegist();
            listView.Close();
        }
        #endregion
        #region [注册快捷键]
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HotkeyUtil.Init(this);
            if (!string.IsNullOrEmpty(userSettingData.OverlayHotkey))
            {
                HotkeyUtil.RegisterHotKey(ModifierKeys.None, (Key)Enum.Parse(typeof(Key), userSettingData.OverlayHotkey), () =>
                {
                    this.Visibility = IsVisible ? Visibility.Hidden : Visibility.Visible;
                });
            }
            if (!string.IsNullOrEmpty(userSettingData.RoundInfoHotkey))
            {
                HotkeyUtil.RegisterHotKey(ModifierKeys.None, (Key)Enum.Parse(typeof(Key), userSettingData.RoundInfoHotkey), () =>
                {
                    listView.Visibility = listView.IsVisible ? Visibility.Hidden : Visibility.Visible;
                });
            }
        }
        #endregion
    }
}