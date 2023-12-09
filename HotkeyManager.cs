using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace simclick
{
    public static class HotKeyManager
    {
        public static event EventHandler<HotKeyEventArgs> HotKeyPressed;

        public static void RegisterHotKey(Keys key, KeyModifiers modifiers = KeyModifiers.None, int id = 0)
        {
            _windowReadyEvent.WaitOne();
            _wnd.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), _hwnd, id, (uint)modifiers, (uint)key);
        }

        public static void UnregisterHotKey(int id = 0)
        {
            _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, id);
        }

        delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
        delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);

        private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key)
        {
            RegisterHotKey(hwnd, id, modifiers, key);
        }

        private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id)
        {
            UnregisterHotKey(_hwnd, id);
        }

        private static void OnHotKeyPressed(HotKeyEventArgs e)
        {
            if (HotKeyPressed != null)
            {
                HotKeyPressed(null, e);
            }
        }

        private static volatile MessageWindow _wnd;
        private static volatile IntPtr _hwnd;
        private static ManualResetEvent _windowReadyEvent = new ManualResetEvent(false);
        static HotKeyManager()
        {
            Thread messageLoop = new Thread(delegate ()
              {
                  Application.Run(new MessageWindow());
              });
            messageLoop.Name = "MessageLoopThread";
            messageLoop.IsBackground = true;
            messageLoop.Start();
        }

        private class MessageWindow : Form
        {
            public MessageWindow()
            {
                _wnd = this;
                _hwnd = this.Handle;
                _windowReadyEvent.Set();
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    HotKeyEventArgs e = new HotKeyEventArgs(m.LParam);
                    OnHotKeyPressed(e);
                }
                else if (m.Msg == WM_EXIT)
                {
                    UnregisterHotKey();
                }

                base.WndProc(ref m);
            }

            protected override void SetVisibleCore(bool value)
            {
                // Ensure the window never becomes visible
                base.SetVisibleCore(false);
            }

            private const int WM_HOTKEY = 0x312;
            private const int WM_EXIT = 0x002;
        }

        [DllImport("user32", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }


    public class HotKeyEventArgs : KeyEventArgs
    {
        public readonly KeyModifiers HotKeyModifiers;

        public HotKeyEventArgs(IntPtr hotKeyParam) : base((Keys)(((uint)hotKeyParam.ToInt64() & 0xffff0000) >> 16))
        {
            uint param = (uint)hotKeyParam.ToInt64();
            HotKeyModifiers = (KeyModifiers)(param & 0x0000ffff);
        }

        public HotKeyEventArgs(KeyEventArgs args) : base(args.KeyCode)
        {
            if (args.Control)
            {
                HotKeyModifiers = KeyModifiers.Control;
            }
            else if (args.Shift)
            {
                HotKeyModifiers = KeyModifiers.Shift;
            }
            else if (args.Alt)
            {
                HotKeyModifiers = KeyModifiers.Alt;
            }
        }

        public override bool Shift => HotKeyModifiers == KeyModifiers.Shift;
        public override bool Alt => HotKeyModifiers == KeyModifiers.Alt;
        public bool Win => HotKeyModifiers == KeyModifiers.Windows;
        public new bool Control => HotKeyModifiers == KeyModifiers.Control;
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0x0000,
        Alt = 0x0001,
        Control = 0x002,
        Shift = 0x004,
        Windows = 0x008,
        NoRepeat = 0x4000,
        ShiftNControl = 0x002 | 0x004
    }
}
