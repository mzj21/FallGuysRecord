using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Window = System.Windows.Window;

namespace FallGuysRecord.view
{
    /// <summary>
    /// Window_Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Setting : Window
    {
        private UserSettingData userSettingData;
        private Window_Overlay overlay;
        private Window_RoundInfo roundinfo;
        private KeyboardHook hook_setting_hotkey_overlay, hook_setting_hotkey_roundinfo;

        public Window_Setting(Window_Overlay overlay, Window_RoundInfo roundinfo)
        {
            InitializeComponent();
            this.overlay = overlay;
            this.roundinfo = roundinfo;
            initView();
        }

        private void initView()
        {
            userSettingData = Util.Read_UserSettingData();
            if (!string.IsNullOrEmpty(userSettingData.OverlayBackground) && File.Exists(userSettingData.OverlayBackground))
                overlay_background.Source = new BitmapImage(new Uri(userSettingData.OverlayBackground));
            overlay_background_path.Text = userSettingData.OverlayBackground;
            if (!string.IsNullOrEmpty(userSettingData.RoundInfoBackground) && File.Exists(userSettingData.RoundInfoBackground))
                roundinfo_background.Source = new BitmapImage(new Uri(userSettingData.RoundInfoBackground));
            roundinfo_background_path.Text = userSettingData.RoundInfoBackground;
            setting_easymode.IsChecked = !userSettingData.isOriginalViewMode;
            setting_language.SelectedItem = userSettingData.Language;
            setting_font.Text = userSettingData.TextFont.ToString();
            SolidBrush sb = new SolidBrush(userSettingData.TextColor);
            setting_color.Background = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
            setting_hotkey_overlay.Text = userSettingData.OverlayHotkey;
            setting_hotkey_roundinfo.Text = userSettingData.RoundInfoHotkey;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        #region [置顶]
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Util.Show(this);
        }

        private void Window_PreviewLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Util.Show(this);
        }
        #endregion
        #region [语言选项加载]
        private void setting_language_Loaded(object sender, RoutedEventArgs e)
        {
            setting_language.Items.Add("English");
            setting_language.Items.Add("Deutsch");
            setting_language.Items.Add("Español");
            setting_language.Items.Add("Español (Latinoamérica)");
            setting_language.Items.Add("Français");
            setting_language.Items.Add("Italiano");
            setting_language.Items.Add("日本語");
            setting_language.Items.Add("한국어");
            setting_language.Items.Add("Polski");
            setting_language.Items.Add("Português Brasileiro");
            setting_language.Items.Add("Русский");
            setting_language.Items.Add("简体中文");
            setting_language.Items.Add("繁體中文");
        }
        #endregion
        #region [语言切换]
        private void setting_language_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ResourceDictionary resourceDictionary = System.Windows.Application.LoadComponent(new Uri(@"resources\language\" + setting_language.SelectedValue + ".xaml", UriKind.Relative)) as ResourceDictionary;
            if (Resources.MergedDictionaries.Count > 0)
            {
                Resources.MergedDictionaries.Clear();
            }
            Resources.MergedDictionaries.Add(resourceDictionary);
            userSettingData.Language = (string)setting_language.SelectedValue;
            Util.Save_UserSettingData(userSettingData);
            Util.ReadRound(userSettingData.Language);

            roundinfo.LaunageChange(userSettingData.Language);

            overlay.logReader.ChangelevelMap();
            Levels levelMap = Util.GetLevels(overlay.roundName);
            string nn = "";
            if (overlay.t4.Text.IndexOf('(') > 0)
            {
                nn = overlay.t4.Text.Substring(overlay.t4.Text.IndexOf('('));
            }
            overlay.SetText("", "", levelMap.showname + (overlay.num > 0 ? "(" + overlay.num + ")" : ""), levelMap.typename + nn, "", "", "", "");
        }
        #endregion
        #region [修改浮窗背景]
        private void Button_Click_overlay_background(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = userSettingData.OverlayBackground,
                Filter = @"(*.jpg,*.png,)|*.jpeg;*.jpg;*.png"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                overlay.overlay_background.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                overlay_background.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                userSettingData.OverlayBackground = openFileDialog.FileName;
                Util.Save_UserSettingData(userSettingData);
            }
        }
        #endregion
        #region [修改回合信息背景]
        private void Button_Click_roundinfo_background(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = userSettingData.OverlayBackground,
                Filter = @"(*.jpg,*.png,)|*.jpeg;*.jpg;*.png"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                roundinfo.roundinfo_background.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                roundinfo_background.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                userSettingData.RoundInfoBackground = openFileDialog.FileName;
                Util.Save_UserSettingData(userSettingData);
            }
        }
        #endregion
        #region [简易模式]
        private void setting_easymode_Checked(object sender, RoutedEventArgs e)
        {
            userSettingData.isOriginalViewMode = false;
            Util.Save_UserSettingData(userSettingData);
            overlay.l1.Visibility = Visibility.Hidden;
            overlay.r1.Visibility = Visibility.Hidden;
            overlay.c1.Visibility = Visibility.Visible;
        }
        private void setting_easymode_Unchecked(object sender, RoutedEventArgs e)
        {
            userSettingData.isOriginalViewMode = true;
            Util.Save_UserSettingData(userSettingData);
            overlay.l1.Visibility = Visibility.Visible;
            overlay.r1.Visibility = Visibility.Visible;
            overlay.c1.Visibility = Visibility.Hidden;
        }
        #endregion
        #region [修改字体]
        private void Button_Click_setting_font(object sender, RoutedEventArgs e)
        {
            FontDialog fontDialog = new FontDialog
            {
                Font = userSettingData.TextFont
            };
            if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Font f = fontDialog.Font;
                overlay.overlay_window.FontFamily = new FontFamily(f.FontFamily.Name);
                roundinfo.window_roundinfo.FontFamily = new FontFamily(f.FontFamily.Name);
                overlay.overlay_window.FontWeight = f.Bold ? FontWeights.Bold : FontWeights.Regular;
                roundinfo.window_roundinfo.FontWeight = f.Bold ? FontWeights.Bold : FontWeights.Regular;
                overlay.overlay_window.FontStyle = f.Italic ? FontStyles.Italic : FontStyles.Normal;
                roundinfo.window_roundinfo.FontStyle = f.Italic ? FontStyles.Italic : FontStyles.Normal;
                TextDecorationCollection textDecorations = new TextDecorationCollection();
                if (f.Underline)
                    textDecorations.Add(TextDecorations.Underline);
                if (f.Strikeout)
                    textDecorations.Add(TextDecorations.Strikethrough);
                overlay.t1.TextDecorations = textDecorations;
                overlay.overlay_window.FontSize = f.Size;
                roundinfo.window_roundinfo.FontSize = f.Size;
                userSettingData.TextFont = f;
                overlay.t9.FontSize = f.Size + 2;
                setting_font.Text = f.ToString();
                Util.Save_UserSettingData(userSettingData);
            }
        }
        #endregion
        #region [修改颜色]
        private void Button_Click_setting_color(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog
            {
                Color = userSettingData.TextColor
            };
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SolidBrush sb = new SolidBrush(colorDialog.Color);
                SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
                overlay.overlay_window.Foreground = solidColorBrush;
                roundinfo.window_roundinfo.Foreground = solidColorBrush;
                setting_color.Background = solidColorBrush;
                userSettingData.TextColor = colorDialog.Color;
                Util.Save_UserSettingData(userSettingData);
            }
        }
        #endregion
        #region [log修改，用于debug]
        private void setting_log_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(setting_logpath.Text))
            {
                Xing.LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Mediatonic", "FallGuys_client", "Player.log");
            }
            else
            {
                Xing.LogFile = setting_logpath.Text;
            }
        }
        #endregion
        #region [热键]
        private void hook_KeyDown_setting_hotkey_overlay(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("hook_setting_hotkey_overlay e.KeyCode = " + e.KeyCode + "   Control.ModifierKeys = " + Control.ModifierKeys);
            string ModifierKeys = "";
            if (e.KeyValue >= 160 && e.KeyValue <= 165)
                return;
            if (Control.ModifierKeys != Keys.None)
            {
                ModifierKeys = Control.ModifierKeys.ToString();
                if (ModifierKeys.Contains(","))
                {
                    ModifierKeys = ModifierKeys.Substring(0, ModifierKeys.IndexOf(","));
                }
                ModifierKeys = ModifierKeys + " + ";
            }
            setting_hotkey_overlay.Text = ModifierKeys + e.KeyCode.ToString();
            userSettingData.OverlayHotkey = ModifierKeys + e.KeyCode.ToString();
            Util.Save_UserSettingData(userSettingData);
            overlay.RegisterHotKey();
        }

        private void setting_hotkey_overlay_GotFocus(object sender, RoutedEventArgs e)
        {
            hook_setting_hotkey_overlay = new KeyboardHook();
            hook_setting_hotkey_overlay.KeyDownEvent += new KeyEventHandler(hook_KeyDown_setting_hotkey_overlay);
            hook_setting_hotkey_overlay.Start();
        }

        private void setting_hotkey_overlay_LostFocus(object sender, RoutedEventArgs e)
        {
            hook_setting_hotkey_overlay.Stop();
        }

        private void hook_KeyDown_setting_hotkey_roundinfo(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("hook_setting_hotkey_roundinfo e.KeyData = " + e.KeyData + "   Control.ModifierKeys = " + Control.ModifierKeys);
            string ModifierKeys = "";
            if (e.KeyValue >= 160 && e.KeyValue <= 165)
                return;
            if (Control.ModifierKeys != Keys.None)
            {
                ModifierKeys = Control.ModifierKeys.ToString();
                if (ModifierKeys.Contains(","))
                {
                    ModifierKeys = ModifierKeys.Substring(0, ModifierKeys.IndexOf(","));
                }
                ModifierKeys = ModifierKeys + " + ";
            }
            setting_hotkey_roundinfo.Text = ModifierKeys +  e.KeyCode.ToString();
            userSettingData.RoundInfoHotkey = ModifierKeys + e.KeyCode.ToString();
            Util.Save_UserSettingData(userSettingData);
            overlay.RegisterHotKey();
        }

        private void setting_hotkey_roundinfo_GotFocus(object sender, RoutedEventArgs e)
        {
            hook_setting_hotkey_roundinfo = new KeyboardHook();
            hook_setting_hotkey_roundinfo.KeyDownEvent += new KeyEventHandler(hook_KeyDown_setting_hotkey_roundinfo);
            hook_setting_hotkey_roundinfo.Start();
        }

        private void setting_hotkey_roundinfo_LostFocus(object sender, RoutedEventArgs e)
        {
            hook_setting_hotkey_roundinfo.Stop();
        }
        #endregion
    }
}
