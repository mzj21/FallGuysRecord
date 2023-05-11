using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Window = System.Windows.Window;

namespace FallGuysRecord.view
{
    public partial class Window_Overlay : Window, ReaderListener
    {
        private System.Timers.Timer timer;
        private DateTime startTime;
        private TimeSpan timeSpan;
        public int num;
        public string roundName;
        private Window_RoundInfo roundinfo;
        public LogReader logReader;
        private Window_Setting window_Setting;

        #region [初始化界面]
        public Window_Overlay()
        {
            InitializeComponent();
            initView();
            #region [秒表计时器]
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            #endregion
            #region [开启线程读取log]
            roundinfo = new Window_RoundInfo();
            logReader = new LogReader(this, roundinfo);
            logReader.Start();
            #endregion
        }
        #endregion
        public void initView()
        {
            if (!string.IsNullOrEmpty(Xing.userSettingData.OverlayBackground) && File.Exists(Xing.userSettingData.OverlayBackground))
                overlay_background.Source = new BitmapImage(new Uri(Xing.userSettingData.OverlayBackground));
            overlay_window.Left = Xing.userSettingData.X;
            overlay_window.Top = Xing.userSettingData.Y;
            overlay_window.Width = Xing.userSettingData.Width;
            overlay_window.Height = Xing.userSettingData.Height;
            changeLocation();///防止超出屏幕找不到
            SolidBrush sb = new SolidBrush(Xing.userSettingData.TextColor);
            overlay_window.Foreground = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
            overlay_window.FontFamily = new FontFamily(Xing.userSettingData.TextFont.FontFamily.Name);
            overlay_window.FontWeight = Xing.userSettingData.TextFont.Bold ? FontWeights.Bold : FontWeights.Regular;
            overlay_window.FontStyle = Xing.userSettingData.TextFont.Italic ? FontStyles.Italic : FontStyles.Normal;
            TextDecorationCollection textDecorations = new TextDecorationCollection();
            if (Xing.userSettingData.TextFont.Underline)
                textDecorations.Add(TextDecorations.Underline);
            if (Xing.userSettingData.TextFont.Strikeout)
                textDecorations.Add(TextDecorations.Strikethrough);
            t1.TextDecorations = textDecorations;
            overlay_window.FontSize = Xing.userSettingData.TextFont.Size;
            t9.FontSize = Xing.userSettingData.TextFont.Size + 2;
            l1.Visibility = Xing.userSettingData.isOriginalViewMode ? Visibility.Visible : Visibility.Hidden;
            r1.Visibility = Xing.userSettingData.isOriginalViewMode ? Visibility.Visible : Visibility.Hidden;
            c1.Visibility = Xing.userSettingData.isOriginalViewMode ? Visibility.Hidden : Visibility.Visible;

            if (App.Current.Resources.MergedDictionaries.Count > 0)
            {
                for (int i = App.Current.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
                {
                    ResourceDictionary item = App.Current.Resources.MergedDictionaries[i];
                    if (item.Source.ToString().Contains(Xing.userSettingData.Language))
                    {
                        continue;
                    }
                    if (item.Source.ToString().Contains("resources/language"))
                    {
                        App.Current.Resources.MergedDictionaries.Remove(item);
                    }
                }
            }
        }
        #region [窗口置顶]
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Util.Show(this);
        }
        private void overlay_window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Util.Show(this);
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
                    changeLocation();
                }
            }
        }
        private void changeLocation()
        {
            Screen s = Util.getInScreen(overlay_window);
            Dpi dpi = Util.GetDpiBySystemParameters();
            if (overlay_window.Left < s.Bounds.X / dpi.X)
            {
                overlay_window.Left = s.Bounds.X / dpi.X;
            }
            if (overlay_window.Left > (s.Bounds.X + s.Bounds.Width) / dpi.X - overlay_window.Width)
            {
                overlay_window.Left = (s.Bounds.X + s.Bounds.Width) / dpi.X - overlay_window.Width;
            }
            if (overlay_window.Top < s.Bounds.Y / dpi.Y)
            {
                overlay_window.Top = s.Bounds.Y / dpi.Y;
            }
            if (overlay_window.Top > (s.Bounds.Y + s.Bounds.Height) / dpi.Y - overlay_window.Height)
            {
                overlay_window.Top = (s.Bounds.Y + s.Bounds.Height) / dpi.Y - overlay_window.Height;
            }
        }
        #endregion
        #region [设置]
        private void MenuItem_Click_Setting(object sender, RoutedEventArgs e)
        {
            if (window_Setting == null || PresentationSource.FromVisual(window_Setting) == null)
            {
                window_Setting = new Window_Setting(this, roundinfo);
                window_Setting.Show();
            }
        }
        #endregion
        #region [位置移动监听]
        private void Overlay_window_LocationChanged(object sender, EventArgs e)
        {
            Xing.userSettingData.X = overlay_window.Left;
            Xing.userSettingData.Y = overlay_window.Top;
            Util.Save_UserSettingData(Xing.userSettingData);
        }
        #endregion
        #region [大小改变监听]
        private void Overlay_window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Xing.userSettingData.Width = overlay_window.Width;
            Xing.userSettingData.Height = overlay_window.Height;
            Util.Save_UserSettingData(Xing.userSettingData);
        }
        #endregion
        #region [修改文本]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="win">Win:获胜数/比赛场数|连胜</param>
        /// <param name="ping">PING</param>
        /// <param name="roundShowName">回合名</param>
        /// <param name="roundType">回合类型</param>
        /// <param name="timeAll">计时器时间(回合结束关闭)</param>
        /// <param name="timeMe">自己的时间</param>
        /// <param name="crown">皇冠</param>
        /// <param name="crownshard">皇冠碎片</param>
        /// <param name="PB">第一名字</param>
        /// /
        /// 
        public void SetText(string win, string ping, string roundShowName, string roundType, string timeAll, string timeMe, string crown, string crownshard, string PB)
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
                if (!string.IsNullOrEmpty(crown) && !string.IsNullOrEmpty(crownshard))
                {
                    t7.Text = crown;
                    t11.Text = crownshard;
                }
                if (!string.IsNullOrEmpty(PB))
                    t8.Text = PB;
            }));
        }
        private void SetTextEasy(string time1, string time2)
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
        public void RoundInit(int num, Levels levelMap)
        {
            this.num = num;
            roundName = levelMap.name;
            SetText("", "", $"{levelMap.showname}({num})", levelMap.typename, "--:--", "--:--:---", "", "", "");
            SetTextEasy("--:--", "--:--:---");
        }

        public void RoundStart(DateTime roundStartTime, bool isPlayerMEAlive)
        {
            startTime = roundStartTime;
            timer.Interval = 1000;
            timer.Start();
            SetText("", "", "", "", "00:00", isPlayerMEAlive ? "00:00:000" : "--:--:---", "", "", "");
            SetTextEasy("00:00", "--:--:---");
        }

        public void RoundUpdateFirst(Player player, string time)
        {
            //SetText("", "", "", "", "", "", time, Xing.userSettingData.isShowFastestName ? player.playerName : $"({player.platform})");
        }

        public void RoundUpdateMe(Player player, string time, int rank)
        {
            SetText("", "", "", "", "", $"#{rank}-{time}", "", "", "");
            SetTextEasy("", $"#{rank}-{time}");
        }

        public void RoundUpdateTotal(string time)
        {
        }

        public void RoundBalance(string balance)
        {
            SetText("", "", "", balance, "", "", "", "", "");
        }

        public void RoundEnd(string endtime, bool isPlaying)
        {
            timer.Stop();
            SetText("", "", "", "", endtime, isPlaying ? "--:--:---" : "", "", "", "");
            SetTextEasy(endtime.Substring(0, 5), "");
        }

        public void RoundExit(int match, int win, int winstreak, string wins)
        {
            timer.Stop();
            SetText($"win({win}/{match}|{winstreak})", "", "", wins, "", "", "", "", "");
        }

        public void RoundCompletedEpisodeDto(int crown, int crownShard)
        {
            SetText("", "", "", "", "", "", crown.ToString(), crownShard.ToString(), "");
        }

        public void RoundPB(Levels levelMap, string time)
        {
            SetText("", "", "", "", "", "", "", "", $"PB-{time}");
        }

        public void Ping(string ping)
        {
            SetText("", $"Ping:{ping}ms", "", "", "", "", "", "", "");
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Util.isFallGuysAlive())
            {
                timer.Stop();
            }
            timeSpan = DateTime.Now.ToUniversalTime() - startTime;
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                t5.Text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
            }));
            SetTextEasy(string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds), "");
        }

        public void Clear()
        {
            SetText("win(0/0|0)", "Ping:ms", "------", "------", "--:--", "--:--:---", "0", "0", "------");
        }
        #endregion
        #region [注册快捷键]
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HotkeyUtil.Init(this);
            RegisterHotKey();
        }
        /// <summary>
        /// 注册快捷键
        /// </summary>
        public void RegisterHotKey()
        {
            HotkeyUtil.UnRegist();
            if (!string.IsNullOrEmpty(Xing.userSettingData.OverlayHotkey))
            {
                string ModifierKeys = "None";
                string key = Xing.userSettingData.OverlayHotkey;
                if (key.Contains("+"))
                {
                    ModifierKeys = key.Substring(0, key.IndexOf("+")).Trim();
                    key = key.Substring(key.IndexOf("+") + 1, key.Length - key.IndexOf("+") - 1).Trim();
                }
                HotkeyUtil.RegisterHotKey((ModifierKeys)Enum.Parse(typeof(ModifierKeys), ModifierKeys), (Key)Enum.Parse(typeof(Key), key), () =>
                {
                    this.Visibility = IsVisible ? Visibility.Hidden : Visibility.Visible;
                    if (this.Visibility == Visibility.Visible)
                    {
                        Util.Show(this);
                    }
                    else
                    {
                        this.Topmost = false;
                    }
                });
            }
            if (!string.IsNullOrEmpty(Xing.userSettingData.RoundInfoHotkey))
            {
                string ModifierKeys = "None";
                string key = Xing.userSettingData.RoundInfoHotkey;
                if (Xing.userSettingData.RoundInfoHotkey.Contains("+"))
                {
                    ModifierKeys = key.Substring(0, key.IndexOf("+")).Trim();
                    key = key.Substring(key.IndexOf("+") + 1, key.Length - key.IndexOf("+") - 1).Trim();
                }
                HotkeyUtil.RegisterHotKey((ModifierKeys)Enum.Parse(typeof(ModifierKeys), ModifierKeys), (Key)Enum.Parse(typeof(Key), key), () =>
                {
                    bool isDisposed = (bool)typeof(Window).GetProperty("IsDisposed", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(roundinfo);
                    if (!isDisposed)
                    {
                        roundinfo.Visibility = roundinfo.IsVisible ? Visibility.Hidden : Visibility.Visible;
                        if (roundinfo.Visibility == Visibility.Visible)
                        {
                            Util.Show(roundinfo);
                        }
                        else
                        {
                            roundinfo.Topmost = false;
                        }
                    }
                });
            }
        }
        #endregion
    }
}