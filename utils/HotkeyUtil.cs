using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

public class HotkeyUtil
{
    #region 系统api
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys modifierKeys, uint vk);

    [DllImport("user32.dll")]
    static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    #endregion

    const int WM_HOTKEY = 0x312;
    static int keyid;
    static IntPtr hWnd;
    static Dictionary<int, HotKeyCallBack> keymap = new Dictionary<int, HotKeyCallBack>();
    public delegate void HotKeyCallBack();

    public enum HotkeyModifiers
    {
        NONE = 0x0,
        ALT = 0x1,
        CONTROL = 0x2,
        SHIFT = 0x4,
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="window">持有快捷键窗口</param>
    public static void Init(Window window)
    {
        hWnd = new WindowInteropHelper(window).Handle;
        HwndSource.FromHwnd(hWnd).AddHook(WndProc);
    }

    /// <summary>
    /// 快捷键消息处理
    /// </summary>
    static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (keymap.TryGetValue(id, out var callback))
            {
                keymap[id]();
            }
        }
        return IntPtr.Zero;
    }

    /// <summary>
    /// 注册快捷键
    /// </summary>
    /// <param name="window">持有快捷键窗口</param>
    /// <param name="modifierKeys">组合键</param>
    /// <param name="key">快捷键</param>
    /// <param name="callBack">回调函数</param>
    public static void RegisterHotKey(ModifierKeys modifierKeys, Key key, HotKeyCallBack callBack)
    {
        int id = keyid++;
        var vk = KeyInterop.VirtualKeyFromKey(key);
        RegisterHotKey(hWnd, id, modifierKeys, (uint)vk);
        keymap[id] = callBack;
    }

    /// <summary>
    /// 注销快捷键
    /// </summary>
    public static void UnRegist()
    {
        foreach (KeyValuePair<int, HotKeyCallBack> var in keymap)
        {
            UnregisterHotKey(hWnd, var.Key);
        }
    }
}