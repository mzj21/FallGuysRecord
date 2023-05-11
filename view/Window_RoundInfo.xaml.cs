using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;

namespace FallGuysRecord.view
{
    public partial class Window_RoundInfo : Window, LogListener
    {
        private bool isBottom;
        public Window_RoundInfo()
        {
            InitializeComponent();
            window_roundinfo.Left = Xing.userSettingData.X_Info;
            window_roundinfo.Top = Xing.userSettingData.Y_Info;
            window_roundinfo.Width = Xing.userSettingData.Width_Info;
            window_roundinfo.Height = Xing.userSettingData.Height_Info;
            changeLocation();///防止超出屏幕找不到
            SolidBrush sb = new SolidBrush(Xing.userSettingData.TextColor);
            window_roundinfo.Foreground = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
            window_roundinfo.FontFamily = new FontFamily(Xing.userSettingData.TextFont.FontFamily.Name);
            window_roundinfo.FontWeight = Xing.userSettingData.TextFont.Bold ? FontWeights.Bold : FontWeights.Regular;
            window_roundinfo.FontStyle = Xing.userSettingData.TextFont.Italic ? FontStyles.Italic : FontStyles.Normal;
            TextDecorationCollection textDecorations = new TextDecorationCollection();
            if (Xing.userSettingData.TextFont.Underline)
                textDecorations.Add(TextDecorations.Underline);
            if (Xing.userSettingData.TextFont.Strikeout)
                textDecorations.Add(TextDecorations.Strikethrough);
            roundinfo_detail.TextDecorations = textDecorations;
            window_roundinfo.FontSize = Xing.userSettingData.TextFont.Size;
            if (!string.IsNullOrEmpty(Xing.userSettingData.RoundInfoBackground) && File.Exists(Xing.userSettingData.RoundInfoBackground))
                roundinfo_background.Source = new BitmapImage(new Uri(Xing.userSettingData.RoundInfoBackground));
        }
        #region [窗口置顶]
        private void window_listview_Deactivated(object sender, EventArgs e)
        {
            Util.Show(this);
        }
        private void window_listview_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Util.Show(this);
        }
        #endregion
        #region [拖动窗口,禁止最大化]
        private void window_listview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        private void window_listview_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
            Screen s = Util.getInScreen(window_roundinfo);
            Dpi dpi = Util.GetDpiBySystemParameters();
            if (window_roundinfo.Left < s.Bounds.X / dpi.X)
            {
                window_roundinfo.Left = s.Bounds.X / dpi.X;
            }
            if (window_roundinfo.Left > (s.Bounds.X + s.Bounds.Width) / dpi.X - window_roundinfo.Width)
            {
                window_roundinfo.Left = (s.Bounds.X + s.Bounds.Width) / dpi.X - window_roundinfo.Width;
            }
            if (window_roundinfo.Top < s.Bounds.Y / dpi.Y)
            {
                window_roundinfo.Top = s.Bounds.Y / dpi.Y;
            }
            if (window_roundinfo.Top > (s.Bounds.Y + s.Bounds.Height) / dpi.Y - window_roundinfo.Height)
            {
                window_roundinfo.Top = (s.Bounds.Y + s.Bounds.Height) / dpi.Y - window_roundinfo.Height;
            }
        }
        #endregion
        #region [位置移动监听]
        private void window_listview_LocationChanged(object sender, EventArgs e)
        {
            Xing.userSettingData.X_Info = window_roundinfo.Left;
            Xing.userSettingData.Y_Info = window_roundinfo.Top;
            Util.Save_UserSettingData(Xing.userSettingData);
        }
        #endregion
        #region [大小改变监听]
        private void window_listview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Xing.userSettingData.Width_Info = window_roundinfo.Width;
            Xing.userSettingData.Height_Info = window_roundinfo.Height;
            Util.Save_UserSettingData(Xing.userSettingData);
        }
        #endregion
        #region [监听事件]
        public void Header(string head)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                roundinfo_header.Text = head;
            }));
        }
        public void Detail(string detail)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                roundinfo_detail.AppendText(detail + Environment.NewLine);
            }));
        }
        public void Clear()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                roundinfo_header.Text = "";
                roundinfo_detail.Text = "";
            }));
        }
        #endregion
        #region [一直最底部]
        private void roundinfo_detail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isBottom)
            {
                roundinfo_detail.ScrollToEnd();
            }
        }
        private void roundinfo_detail_Loaded(object sender, RoutedEventArgs e)
        {
            roundinfo_detail.ScrollToEnd();
        }
        #endregion
        #region [不是最底部监听]
        private void roundinfo_detail_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset < roundinfo_detail.ActualHeight)
            {
                isBottom = true;
            }
            else
            {
                isBottom = e.VerticalOffset == e.ExtentHeight - e.ViewportHeight;
            }
            roundinfo_bottom.Visibility = isBottom ? Visibility.Hidden : Visibility.Visible;
        }

        private void roundinfo_down_Click(object sender, RoutedEventArgs e)
        {
            roundinfo_detail.ScrollToEnd();
        }
        #endregion
        #region [右键操作]
        private void MenuItem_Click_All(object sender, RoutedEventArgs e)
        {
            roundinfo_detail.SelectAll();
        }
        private void MenuItem_Click_Copy(object sender, RoutedEventArgs e)
        {
            roundinfo_detail.Copy();
        }
        private void MenuItem_Click_Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDg = new SaveFileDialog();
            saveDg.Filter = @"(*.txt)|*.txt";
            saveDg.FileName = $"log_save {DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss")}";
            saveDg.AddExtension = true;
            saveDg.RestoreDirectory = true;
            if (saveDg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveDg.FileName, roundinfo_detail.Text);
            }
        }
        #endregion
    }
}