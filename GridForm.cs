using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace simclick
{
    public class GridForm : Form
    {
        private string HlRowKey;
        private string HlColKey;
        private bool InputStatus { get { return HlRowKey == null || HlColKey == null; } }
        private int _mode;
        private int Mode
        {
            get { return _mode; }
            set
            {
                if (_mode != value)
                {
                    var oldMode = _mode;
                    _mode = value;
                    SwitchMoveMode(_mode == 2);
                    if (_mode == 0)
                    {
                        Hide();
                        if (oldMode != 2 && !IsStickyMode)
                        {
                            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.AbsoluteMove, PPI, PPI);
                        }
                    }
                    else if (_mode > 0)
                    {
                        if (oldMode == 0)
                        {
                            Show();
                        }
                        Refresh();
                        SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
                    }
                }
            }
        }
        private bool _isStickyMode = false;
        private bool IsStickyMode
        {
            get { return _isStickyMode; }
            set
            {
                _isStickyMode = value;
                if (_isStickyMode)
                {
                    StickyCnt = 0;
                }
            }
        }
        private bool IsMouseDown = false;
        private bool IsShiftMode = false;
        private int StickyCnt = 0;
        private int ColorIdx = 0;
        private int ScreenWidth;
        private int ScreenHeight;
        private int BoxCnt = 30;
        private int OneBoxWidth;
        private int OneBoxHeight;
        private int HalfBoxWidth;
        private int HalfBoxHeight;
        private int FontXStart;
        private int FontYStart;
        private int OffSetX;
        private int OffSetY;
        private string LastKey;
        private DateTime LastTime;

        private Dictionary<string, string> DictKey = new Dictionary<string, string>();
        private Dictionary<string, int[]> DictPos = new Dictionary<string, int[]>();
        private string KeyString = "QWERTYUIOPASDFGHJKL;ZXCVBNM,./";
        private static int DefaultFontSize = 10;
        private static int PPI = 65536;
        private Pen RedPen = new Pen(Color.Red);
        private Pen GreenPen = new Pen(Color.Green);
        private Pen InvisiblePen = new Pen(Color.FromArgb(100, 100, 100)) { DashStyle = DashStyle.Dash };
        private Brush RedBrush = new SolidBrush(Color.Red);
        private Brush GreenBrush = new SolidBrush(Color.Green);
        private Brush BlueBrush = new SolidBrush(Color.Blue);
        private Brush[] BrushArray = new Brush[] {
            new SolidBrush(Color.Black),
            new SolidBrush(Color.Red),
        };
        private Brush TextHightlightColor = new SolidBrush(Color.Red);
        private Brush TextBgColor = new SolidBrush(Color.FromArgb(255, 255, 190));
        private Font DefautlFont = new Font("Sarasa Term TC", (float)DefaultFontSize, GraphicsUnit.Point) { };
        private System.Timers.Timer KeyTimer = new System.Timers.Timer() { AutoReset = false, Enabled = false, Interval = 200 };
        private KeyEventArgs E = null;
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public GridForm()
        {
            Rectangle bounds = Screen.GetBounds(new Point(0, 0));
            ScreenWidth = bounds.Right;
            ScreenHeight = bounds.Bottom;
            OneBoxWidth = ScreenWidth / BoxCnt;
            OneBoxHeight = ScreenHeight / BoxCnt;
            HalfBoxWidth = OneBoxWidth / 2;
            HalfBoxHeight = OneBoxHeight / 2;
            FontXStart = HalfBoxWidth - (DefaultFontSize / 2);
            FontYStart = HalfBoxHeight - DefaultFontSize;
            OffSetX = (ScreenWidth - BoxCnt * OneBoxWidth) / 2;
            OffSetY = (ScreenHeight - BoxCnt * OneBoxHeight) / 2;
            InitializeComponent();
            InitDictKey();

            KeyTimer.Elapsed += (s, o) =>
            {
                Invoke((MethodInvoker)delegate ()
                {
                    Trigger(E, "Space", true);
                });
            };
            HotKeyManager.RegisterHotKey(Keys.Space, KeyModifiers.Shift | KeyModifiers.Control);
            HotKeyManager.RegisterHotKey(Keys.Space, KeyModifiers.Control | KeyModifiers.Alt, 1);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>((s, e) =>
            {
                if (e.KeyCode == Keys.Space)
                {
                    var mode = e.HotKeyModifiers == KeyModifiers.ShiftNControl ? 1 : 2;
                    if (mode == 1)
                    {
                        if (!InputStatus)
                        {
                            Trigger(e);
                            return;
                        }
                    }
                    Mode = Mode != mode ? mode : 0;
                    if (Mode == 0)
                    {
                        IsStickyMode = false;
                        if (IsMouseDown)
                        {
                            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightUp, 0, 0);
                        }
                        IsMouseDown = false;
                        Refresh();
                    }
                    else if (Mode == 2)
                    {
                        var p = MouseOperations.GetCursorPosition();
                        if (p.X == ScreenWidth - 1 && p.Y == ScreenHeight - 1)
                        {
                            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.AbsoluteMove, PPI / 2, PPI / 2);
                        }
                    }
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    FormKeyDown(null, e);
                }
                else
                {
                    MoveMouse(e);
                }
            });
            base.TransparencyKey = Color.White;
            BackColor = Color.White;
            base.WindowState = FormWindowState.Normal;
        }
        private void SwitchMoveMode(bool status)
        {
            if (status)
            {
                HotKeyManager.RegisterHotKey(Keys.H, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.J, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.K, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.L, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.F, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.D, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.Oemcomma, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.OemPeriod, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.Left, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.Down, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.Up, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.Right, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.Escape, KeyModifiers.None, 2);
                HotKeyManager.RegisterHotKey(Keys.H, KeyModifiers.Shift, 3);
                HotKeyManager.RegisterHotKey(Keys.J, KeyModifiers.Shift, 3);
                HotKeyManager.RegisterHotKey(Keys.K, KeyModifiers.Shift, 3);
                HotKeyManager.RegisterHotKey(Keys.L, KeyModifiers.Shift, 3);
                HotKeyManager.RegisterHotKey(Keys.F, KeyModifiers.Shift, 3);
                HotKeyManager.RegisterHotKey(Keys.Oemcomma, KeyModifiers.Shift, 3);
                HotKeyManager.RegisterHotKey(Keys.OemPeriod, KeyModifiers.Shift, 3);
                HotKeyManager.RegisterHotKey(Keys.H, KeyModifiers.Alt, 4);
                HotKeyManager.RegisterHotKey(Keys.J, KeyModifiers.Alt, 4);
                HotKeyManager.RegisterHotKey(Keys.K, KeyModifiers.Alt, 4);
                HotKeyManager.RegisterHotKey(Keys.L, KeyModifiers.Alt, 4);
                HotKeyManager.RegisterHotKey(Keys.F, KeyModifiers.Alt, 4);
                HotKeyManager.RegisterHotKey(Keys.Oemcomma, KeyModifiers.Alt, 4);
                HotKeyManager.RegisterHotKey(Keys.OemPeriod, KeyModifiers.Alt, 4);
                HotKeyManager.RegisterHotKey(Keys.H, KeyModifiers.Control, 5);
                HotKeyManager.RegisterHotKey(Keys.J, KeyModifiers.Control, 5);
                HotKeyManager.RegisterHotKey(Keys.K, KeyModifiers.Control, 5);
                HotKeyManager.RegisterHotKey(Keys.L, KeyModifiers.Control, 5);
                HotKeyManager.RegisterHotKey(Keys.Oemcomma, KeyModifiers.Alt | KeyModifiers.Shift, 6);
                HotKeyManager.RegisterHotKey(Keys.OemPeriod, KeyModifiers.Alt | KeyModifiers.Shift, 6);
            }
            else
            {
                HotKeyManager.UnregisterHotKey(2);
                HotKeyManager.UnregisterHotKey(3);
                HotKeyManager.UnregisterHotKey(4);
                HotKeyManager.UnregisterHotKey(5);
                HotKeyManager.UnregisterHotKey(6);
            }
        }
        private void Trigger(KeyEventArgs e, string keyCode = null, bool quick = false)
        {
            keyCode = keyCode ?? e.KeyCode.ToString();
            if (!DictPos.ContainsKey(keyCode))
            {
                return;
            }
            if (Mode == 2)
            {
                MoveMouse(new HotKeyEventArgs(e));
                return;
            }
            var curPos = DictPos[keyCode];
            if (!IsStickyMode)
            {
                if (e.Shift && !IsShiftMode)
                {
                    Mode = 0;
                    MouseOperations.MouseEvent(!e.Control ? MouseOperations.MouseEventFlags.AbsoluteMoveRight : MouseOperations.MouseEventFlags.AbsoluteMoveLeft, curPos[0], curPos[1]);
                    if (e.Alt)
                    {
                        Thread.Sleep(200);
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.AbsoluteMoveRight, curPos[0], curPos[1]);
                    }
                }
                else
                {
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.AbsoluteMove, curPos[0], curPos[1]);
                    Mode = 2;
                }
                ResetHightlightKey(e.Shift && !IsShiftMode);
                IsShiftMode = true;
            }
            else
            {
                if (StickyCnt++ % 2 == 0)
                {
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.AbsoluteMove, curPos[0], curPos[1]);
                    if (e.Shift)
                    {
                        Mode = 2;
                    }
                    else
                    {
                        ResetHightlightKey(false);
                        Refresh();
                    }
                }
                else
                {
                    Mode = 0;
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightDown, 0, 0);
                    Thread.Sleep(100);
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.AbsoluteMove, curPos[0], curPos[1]);
                    Thread.Sleep(100);
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightUp, 0, 0);
                    IsStickyMode = false;
                }
            }
            LastKey = null;
        }
        private void MoveMouse(HotKeyEventArgs e)
        {
            var spead = !e.Control && !e.Shift && !e.Alt ? 5 : (e.Shift || e.Control ? 20 : 1);
            switch (e.KeyCode.ToString())
            {
                case "J":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Move, 0, spead);
                    break;
                case "K":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Move, 0, -spead);
                    break;
                case "H":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Move, -spead, 0);
                    break;
                case "L":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Move, spead, 0);
                    break;
                case "Down":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Move, 0, 1);
                    break;
                case "Up":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Move, 0, -1);
                    break;
                case "Left":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Move, -1, 0);
                    break;
                case "Right":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Move, 1, 0);
                    break;
                case "Oemcomma":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Wheel, 0, 0, -(!e.Alt ? 100 : 500));
                    break;
                case "OemPeriod":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Wheel, 0, 0, !e.Alt ? 100 : 500);
                    break;
                case "F":
                    if (!e.Shift)
                    {
                        if (!IsMouseDown)
                        {
                            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Right, 0, 0);
                        }
                        else
                        {
                            IsMouseDown = false;
                            Refresh();
                            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightUp, 0, 0);
                        }
                    }
                    else
                    {
                        IsMouseDown = !IsMouseDown;
                        Refresh();
                        MouseOperations.MouseEvent(IsMouseDown ? MouseOperations.MouseEventFlags.RightDown : MouseOperations.MouseEventFlags.RightUp, 0, 0);
                    }
                    if (e.Alt)
                    {
                        Thread.Sleep(200);
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Right, 0, 0);
                    }
                    break;
                case "D":
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Left, 0, 0);
                    break;
                default:
                    break;
            }
        }
        private void FormKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString() == "Escape")
            {
                Mode = 0;
                IsStickyMode = false;
                if (IsMouseDown)
                {
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightUp, 0, 0);
                }
                IsMouseDown = false;
                Refresh();
            }
            else if (e.KeyCode.ToString() == "Space")
            {
                if (InputStatus)
                {
                    if (IsStickyMode)
                    {
                        Mode = 0;
                        IsStickyMode = false;
                    }
                    else
                    {
                        IsStickyMode = true;
                    }
                }
                else if (IsStickyMode && Mode == 2)
                {
                    Mode = 1;
                    ResetHightlightKey(false);
                    Refresh();
                }
                else
                {
                    Trigger(e);
                }
            }
            else if (e.KeyCode.ToString() == "Back")
            {
                if (Mode == 1 && InputStatus && HlColKey != null)
                {
                    HlColKey = null;
                    Refresh();
                }
            }
            else if (e.KeyCode.ToString() == "ShiftKey")
            {
                if (Mode == 1 && !IsStickyMode)
                {
                    IsShiftMode = !IsShiftMode;
                }
            }
            else if (e.KeyCode.ToString() == "Return")
            {
                ColorIdx = (ColorIdx + 1) % BrushArray.Length;
                Refresh();
            }
            else if (DictKey.ContainsKey(e.KeyCode.ToString()))
            {
                if (Mode == 2)
                {
                    Trigger(e);
                    return;
                }
                if (InputStatus)
                {
                    if (HlColKey == null)
                    {
                        HlColKey = e.KeyCode.ToString();
                        Refresh();
                    }
                    else
                    {
                        HlRowKey = e.KeyCode.ToString();
                        if (IsStickyMode && StickyCnt % 2 == 1 || !IsStickyMode)
                        {
                            ComputeMainPointPos();
                            if (!IsStickyMode && !e.Shift && !e.Alt && !e.Control && LastKey == null)
                            {
                                E = e;
                                LastKey = e.KeyCode.ToString();
                                LastTime = DateTime.Now;
                                KeyTimer.Start();
                            }
                            else
                            {
                                Trigger(e, "Space", true);
                            }
                        }
                        else
                        {
                            Refresh();
                        }
                    }
                }
                else if (!IsStickyMode && LastKey == e.KeyCode.ToString() && (DateTime.Now - LastTime).TotalMilliseconds <= 200)
                {
                    KeyTimer.Stop();
                    Refresh();
                    LastKey = null;
                }
                else
                {
                    Trigger(e);
                }
            }
        }
        private void ComputeMainPointPos()
        {
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    if (Chr(i) == DictKey[HlColKey] && Chr(j) == DictKey[HlRowKey])
                    {
                        DictPos["Space"] = new int[] { (i * OneBoxWidth + HalfBoxWidth + OffSetX) * PPI / ScreenWidth, (j * OneBoxHeight + HalfBoxHeight + OffSetY) * PPI / ScreenHeight };
                    }
                }
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            using (Bitmap image = new Bitmap(Mode != 2 ? ScreenWidth - OffSetX * 2 : ScreenWidth, Mode != 2 ? ScreenHeight - OffSetY * 2 : ScreenHeight))
            using (Graphics graphics = Graphics.FromImage(image))
            {
                if (Mode != 2)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        //graphics.DrawLine(defaultPen, 0, i * OneBoxHeight, bounds.Right, i * OneBoxHeight);
                        //graphics.DrawLine(defaultPen, i * OneBoxWidth, 0, i * OneBoxWidth, bounds.Bottom);
                        for (int j = 0; j < 30; j++)
                        {
                            if (InputStatus)
                            {
                                graphics.DrawLine(InvisiblePen, i * OneBoxWidth, j * OneBoxHeight, (i + 1) * OneBoxWidth, (j + 1) * OneBoxHeight);
                                graphics.DrawLine(InvisiblePen, (i + 1) * OneBoxWidth, j * OneBoxHeight, i * OneBoxWidth, (j + 1) * OneBoxHeight);
                                graphics.FillRectangle(TextBgColor, i * OneBoxWidth + FontXStart - DefaultFontSize / 2, j * OneBoxHeight + FontYStart / 2 + DefaultFontSize / 2, DefaultFontSize * 2, DefaultFontSize * 3 / 2);
                                graphics.DrawString(Chr(i), DefautlFont, HlColKey != null && Chr(i) == DictKey[HlColKey] ? TextHightlightColor : BrushArray[ColorIdx], new Point(i * OneBoxWidth + FontXStart - DefaultFontSize / 2, j * OneBoxHeight + FontYStart + DefaultFontSize * 1 / 6));
                                graphics.DrawString(Chr(j), DefautlFont, HlRowKey != null && Chr(j) == DictKey[HlRowKey] ? TextHightlightColor : BrushArray[ColorIdx], new Point(i * OneBoxWidth + FontXStart + DefaultFontSize / 2, j * OneBoxHeight + FontYStart + DefaultFontSize * 1 / 6));
                            }
                            else if (Chr(i) == DictKey[HlColKey] && Chr(j) == DictKey[HlRowKey])
                            {
                                graphics.DrawLine(InvisiblePen, i * OneBoxWidth, j * OneBoxHeight, (i + 1) * OneBoxWidth, (j + 1) * OneBoxHeight);
                                graphics.DrawLine(InvisiblePen, (i + 1) * OneBoxWidth, j * OneBoxHeight, i * OneBoxWidth, (j + 1) * OneBoxHeight);
                                graphics.DrawLine(InvisiblePen, i * OneBoxWidth + HalfBoxWidth, j * OneBoxHeight, i * OneBoxWidth, j * OneBoxHeight + HalfBoxHeight);
                                graphics.DrawLine(InvisiblePen, i * OneBoxWidth + HalfBoxWidth, j * OneBoxHeight, (i + 1) * OneBoxWidth, j * OneBoxHeight + HalfBoxHeight);
                                graphics.DrawLine(InvisiblePen, i * OneBoxWidth + HalfBoxWidth, (j + 1) * OneBoxHeight, i * OneBoxWidth, j * OneBoxHeight + HalfBoxHeight);
                                graphics.DrawLine(InvisiblePen, i * OneBoxWidth + HalfBoxWidth, (j + 1) * OneBoxHeight, (i + 1) * OneBoxWidth, j * OneBoxHeight + HalfBoxHeight);

                                //graphics.FillRectangle(textBgColor, i * OneBoxWidth + fontXStart - defaultFontSize / 2, j * OneBoxHeight + fontYStart / 2 + defaultFontSize / 2, defaultFontSize * 2, defaultFontSize * 3 / 2);
                                //graphics.DrawString(Chr(i), font, BrushArray[ColorIdx], new Point(i * OneBoxWidth + fontXStart - defaultFontSize / 2, j * OneBoxHeight + fontYStart + defaultFontSize * 1 / 6));
                                //graphics.DrawString(Chr(j), font, BrushArray[ColorIdx], new Point(i * OneBoxWidth + fontXStart + defaultFontSize / 2, j * OneBoxHeight + fontYStart + defaultFontSize * 1 / 6));

                                DictPos["Space"] = new int[] { i * OneBoxWidth + HalfBoxWidth, j * OneBoxHeight + HalfBoxHeight };
                                DictPos["A"] = new int[] { i * OneBoxWidth + HalfBoxWidth, j * OneBoxHeight };
                                graphics.DrawString("A", DefautlFont, RedBrush, new Point(i * OneBoxWidth + FontXStart, j * OneBoxHeight - HalfBoxHeight + FontYStart + DefaultFontSize * 1 / 6));
                                DictPos["S"] = new int[] { i * OneBoxWidth, j * OneBoxHeight + HalfBoxHeight };
                                graphics.DrawString("S", DefautlFont, RedBrush, new Point(i * OneBoxWidth - HalfBoxWidth + FontXStart, j * OneBoxHeight + FontYStart + DefaultFontSize * 1 / 6));
                                DictPos["D"] = new int[] { i * OneBoxWidth + HalfBoxWidth, (j + 1) * OneBoxHeight };
                                graphics.DrawString("D", DefautlFont, RedBrush, new Point(i * OneBoxWidth + FontXStart, j * OneBoxHeight + HalfBoxHeight + FontYStart + DefaultFontSize * 1 / 6));
                                DictPos["F"] = new int[] { (i + 1) * OneBoxWidth, j * OneBoxHeight + HalfBoxHeight };
                                graphics.DrawString("F", DefautlFont, RedBrush, new Point(i * OneBoxWidth + HalfBoxWidth + FontXStart, j * OneBoxHeight + FontYStart + DefaultFontSize * 1 / 6));

                                DictPos["R"] = new int[] { i * OneBoxWidth, j * OneBoxHeight };
                                graphics.DrawString("R", DefautlFont, GreenBrush, new Point(i * OneBoxWidth - HalfBoxWidth + FontXStart, j * OneBoxHeight - HalfBoxHeight + FontYStart + DefaultFontSize * 1 / 6));
                                DictPos["G"] = new int[] { i * OneBoxWidth, (j + 1) * OneBoxHeight };
                                graphics.DrawString("G", DefautlFont, GreenBrush, new Point(i * OneBoxWidth - HalfBoxWidth + FontXStart, j * OneBoxHeight + HalfBoxHeight + FontYStart + DefaultFontSize * 1 / 6));
                                DictPos["H"] = new int[] { (i + 1) * OneBoxWidth, (j + 1) * OneBoxHeight };
                                graphics.DrawString("H", DefautlFont, GreenBrush, new Point(i * OneBoxWidth + HalfBoxWidth + FontXStart, j * OneBoxHeight + HalfBoxHeight + FontYStart + DefaultFontSize * 1 / 6));
                                DictPos["U"] = new int[] { (i + 1) * OneBoxWidth, j * OneBoxHeight };
                                graphics.DrawString("U", DefautlFont, GreenBrush, new Point(i * OneBoxWidth + HalfBoxWidth + FontXStart, j * OneBoxHeight - HalfBoxHeight + FontYStart + DefaultFontSize * 1 / 6));

                                DictPos["J"] = new int[] { i * OneBoxWidth + OneBoxWidth / 4, j * OneBoxHeight + OneBoxHeight / 4 };
                                graphics.DrawString("J", DefautlFont, BlueBrush, new Point(i * OneBoxWidth - OneBoxWidth / 4 + FontXStart, j * OneBoxHeight - OneBoxHeight / 4 + FontYStart + DefaultFontSize * 1 / 6));
                                DictPos["K"] = new int[] { i * OneBoxWidth + OneBoxWidth / 4, j * OneBoxHeight + OneBoxHeight * 3 / 4 };
                                graphics.DrawString("K", DefautlFont, BlueBrush, new Point(i * OneBoxWidth - OneBoxWidth / 4 + FontXStart, j * OneBoxHeight + OneBoxHeight / 4 + FontYStart + DefaultFontSize * 1 / 6));
                                DictPos["L"] = new int[] { i * OneBoxWidth + OneBoxWidth * 3 / 4, j * OneBoxHeight + OneBoxHeight * 3 / 4 };
                                graphics.DrawString("L", DefautlFont, BlueBrush, new Point(i * OneBoxWidth + OneBoxWidth / 4 + FontXStart, j * OneBoxHeight + OneBoxHeight / 4 + FontYStart + DefaultFontSize * 1 / 6));
                                DictPos["Oem1"] = new int[] { i * OneBoxWidth + OneBoxWidth * 3 / 4, j * OneBoxHeight + OneBoxHeight / 4 };
                                graphics.DrawString(";", DefautlFont, BlueBrush, new Point(i * OneBoxWidth + OneBoxWidth / 4 + FontXStart, j * OneBoxHeight - OneBoxHeight / 4 + FontYStart + DefaultFontSize * 1 / 6));
                                foreach (var itm in DictPos)
                                {
                                    var item = itm.Value as int[];
                                    //graphics.DrawLine(RedPen,
                                    //    item[0] + HalfBoxWidth/* + OffSetX*/,
                                    //    item[1] + HalfBoxHeight/* + OffSetY*/,
                                    //    item[0] + HalfBoxWidth/* + OffSetX*/ + 5,
                                    //    item[1] + HalfBoxHeight/* + OffSetY*/ + 5);
                                    item[0] = (item[0] + OffSetX) * PPI / ScreenWidth;
                                    item[1] = (item[1] + OffSetY) * PPI / ScreenHeight;
                                }
                            }
                        }
                    }
                    e.Graphics.DrawImage(image, new Point(OffSetX, OffSetY));
                }
                else
                {
                    //var pen = !MouseDown ? GreenPen : RedPen;
                    var brush = !IsMouseDown ? GreenBrush : RedBrush;
                    //graphics.DrawLine(pen, ScreenWidth / 2 - 40, ScreenHeight / 2, ScreenWidth / 2 + 40, ScreenHeight / 2);
                    //graphics.DrawLine(pen, ScreenWidth / 2, ScreenHeight / 2 - 40, ScreenWidth / 2, ScreenHeight / 2 + 40);
                    //graphics.DrawEllipse(pen, ScreenWidth / 2 - 40, ScreenHeight / 2 - 40, 80, 80);
                    graphics.FillRectangle(brush, ScreenWidth - 24, ScreenHeight - 24, 24, 24);
                    e.Graphics.DrawImage(image, new Point(0, 0));
                }
            }
            base.OnPaint(e);
        }
        private void ResetHightlightKey(bool isHideMouse = true)
        {
            HlColKey = null;
            HlRowKey = null;

            if (isHideMouse)
            {
                MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.AbsoluteMove, PPI, PPI);
            }
        }
        private void FormDeactivate(object sender, EventArgs e)
        {
            ResetHightlightKey(false);
            if (Mode == 1)
            {
                //Mode = 0;
            }
            //Send Alt+Tab
            //SendKeys.SendWait("%{Tab}");
        }
        private void InitDictKey()
        {
            foreach (var item in "QWERTYUIOP".ToCharArray())
            {
                DictKey[item.ToString()] = item.ToString();
            }
            foreach (var item in "ASDFGHJKL".ToCharArray())
            {
                DictKey[item.ToString()] = item.ToString();
            }
            DictKey["Oem1"] = ";";
            foreach (var item in "ZXCVBNM".ToCharArray())
            {
                DictKey[item.ToString()] = item.ToString();
            }
            DictKey["Oemcomma"] = ",";
            DictKey["OemPeriod"] = ".";
            DictKey["OemQuestion"] = "/";
        }
        public string Chr(int code)
        {
            return KeyString[code].ToString();
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 2)
            {
                HotKeyManager.UnregisterHotKey(1);
                HotKeyManager.UnregisterHotKey(2);
                HotKeyManager.UnregisterHotKey(3);
                HotKeyManager.UnregisterHotKey(4);
                HotKeyManager.UnregisterHotKey(5);
            }

            base.WndProc(ref m);
        }
        private void InitializeComponent()
        {
            SuspendLayout();
            //base.AutoScaleDimensions = new SizeF(8f, 16f);
            //base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(ScreenWidth, ScreenHeight);
            base.FormBorderStyle = FormBorderStyle.None;
            base.Name = "SimClick";
            //Text = "SimClick";
            base.Opacity = 0.7;
            base.StartPosition = FormStartPosition.Manual;
            base.TopMost = true;
            base.WindowState = FormWindowState.Maximized;
            base.Deactivate += new EventHandler(FormDeactivate);
            base.KeyDown += new KeyEventHandler(FormKeyDown);
            ResumeLayout(false);
        }
    }
}
