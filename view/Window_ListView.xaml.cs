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

namespace FallGuysRecord
{
    public partial class Window_ListView : Window, LogListener
    {
        UserSettingData userSettingData;
        private Boolean isBottom;
        public Window_ListView()
        {
            InitializeComponent();
            userSettingData = Util.Read_UserSettingData();
            window_listview.Left = userSettingData.X_Info;
            window_listview.Top = userSettingData.Y_Info;
            window_listview.Width = userSettingData.Width_Info;
            window_listview.Height = userSettingData.Height_Info;
            if (window_listview.Left < 0)
            {
                window_listview.Left = 0;
            }
            if (window_listview.Left > SystemParameters.PrimaryScreenWidth - window_listview.Width)
            {
                window_listview.Left = SystemParameters.PrimaryScreenWidth - window_listview.Width;
            }
            if (window_listview.Top < 0)
            {
                window_listview.Top = 0;
            }
            if (window_listview.Top > SystemParameters.PrimaryScreenHeight - window_listview.Height)
            {
                window_listview.Top = SystemParameters.PrimaryScreenHeight - window_listview.Height;
            }
            SolidBrush sb = new SolidBrush(userSettingData.TextColor);
            window_listview.Foreground = new SolidColorBrush(Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B));
            window_listview.FontFamily = new FontFamily(userSettingData.TextFont.FontFamily.Name);
            window_listview.FontWeight = userSettingData.TextFont.Bold ? FontWeights.Bold : FontWeights.Regular;
            window_listview.FontStyle = userSettingData.TextFont.Italic ? FontStyles.Italic : FontStyles.Normal;
            TextDecorationCollection textDecorations = new TextDecorationCollection();
            if (userSettingData.TextFont.Underline)
                textDecorations.Add(TextDecorations.Underline);
            if (userSettingData.TextFont.Strikeout)
                textDecorations.Add(TextDecorations.Strikethrough);
            list_detail.TextDecorations = textDecorations;
            window_listview.FontSize = userSettingData.TextFont.Size;
            if (!String.IsNullOrEmpty(userSettingData.RoundInfoBackground) && File.Exists(userSettingData.RoundInfoBackground))
                roundinfo_background.Source = new BitmapImage(new Uri(userSettingData.RoundInfoBackground));
        }
        #region [窗口置顶]
        private void window_listview_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }
        private void window_listview_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
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
                    if (window_listview.Left < 0)
                    {
                        window_listview.Left = 0;
                    }
                    if (window_listview.Left > SystemParameters.PrimaryScreenWidth - window_listview.Width)
                    {
                        window_listview.Left = SystemParameters.PrimaryScreenWidth - window_listview.Width;
                    }
                    if (window_listview.Top < 0)
                    {
                        window_listview.Top = 0;
                    }
                    if (window_listview.Top > SystemParameters.PrimaryScreenHeight - window_listview.Height)
                    {
                        window_listview.Top = SystemParameters.PrimaryScreenHeight - window_listview.Height;
                    }
                }
            }
        }
        #endregion
        #region [位置移动监听]
        private void window_listview_LocationChanged(object sender, EventArgs e)
        {
            userSettingData.X_Info = window_listview.Left;
            userSettingData.Y_Info = window_listview.Top;
            Util.Save_UserSettingData(userSettingData);
        }
        #endregion
        #region [大小改变监听]
        private void window_listview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            userSettingData.Width_Info = window_listview.Width;
            userSettingData.Height_Info = window_listview.Height;
            Util.Save_UserSettingData(userSettingData);
        }
        #endregion
        #region [监听事件]
        public void Header(string head)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                list_header.Text = head;
            }));
        }
        public void Detail(string detail)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                list_detail.AppendText(detail + Environment.NewLine);
            }));
        }
        #endregion
        #region [一直最底部]
        private void list_detail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isBottom)
            {
                list_detail.ScrollToEnd();
            }
        }
        private void list_detail_Loaded(object sender, RoutedEventArgs e)
        {
            list_detail.ScrollToEnd();
        }
        #endregion
        #region [不是最底部监听]
        private void list_detail_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset < list_detail.ActualHeight)
            {
                isBottom = true;
            }
            else
            {
                isBottom = e.VerticalOffset == e.ExtentHeight - e.ViewportHeight;
            }
            list_bottom.Visibility = isBottom ? Visibility.Hidden : Visibility.Visible;
        }

        private void list_down_Click(object sender, RoutedEventArgs e)
        {
            list_detail.ScrollToEnd();
        }
        #endregion
        #region [右键操作]
        private void MenuItem_Click_All(object sender, RoutedEventArgs e)
        {
            list_detail.SelectAll();
        }
        private void MenuItem_Click_Copy(object sender, RoutedEventArgs e)
        {
            list_detail.Copy();
        }
        private void MenuItem_Click_Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDg = new SaveFileDialog();
            saveDg.Filter = @"(*.txt)|*.txt";
            saveDg.FileName = "log_save " + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + "";
            saveDg.AddExtension = true;
            saveDg.RestoreDirectory = true;
            if (saveDg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveDg.FileName, list_detail.Text);
            }
        }
        #endregion
    }
}