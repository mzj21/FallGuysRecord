using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;

namespace FallGuysRecord
{
    public partial class Window_ListView : Window, LogListener
    {
        UserSettingData userSettingData;
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
            SolidBrush sb = new SolidBrush(userSettingData.TextColor == System.Drawing.Color.White ? System.Drawing.Color.Black : userSettingData.TextColor);
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
        }
        #region [窗口置顶]
        private void window_listview_Deactivated(object sender, EventArgs e)
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
            list_detail.ScrollToEnd();
        }
        private void list_detail_Loaded(object sender, RoutedEventArgs e)
        {
            list_detail.ScrollToEnd();
        }
        #endregion
    }
}