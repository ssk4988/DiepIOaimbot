using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Timers;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Interop;

namespace DiepIOaimbot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Drawing.Color tank;
        DateTime prev = DateTime.Now;
        private Boolean runscan = false;
        int shapetype = 0;
        int lastshape = 0;
        int lastX, lastY = 0;
        float lastdist;
        bool ready = true;
        private IntPtr _windowHandle;
        private HwndSource _source;
        private const int HOTKEY_ID = 9000;
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        private const uint VK_CAPITAL = 0x14;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow()
        {
            InitializeComponent();
            var myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(MyEvent);
            myTimer.Interval = 10;
            myTimer.Enabled = true;
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, VK_CAPITAL); //CTRL + CAPS_LOCK
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK_CAPITAL)
                            {
                                toggle();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }
        private void MyEvent(object source, ElapsedEventArgs e)
        {
            if (runscan && ready)
                ScanDirect();
        }
        private void ScanDirect()
        {
            Trace.WriteLine((DateTime.Now - prev).TotalMilliseconds);
            prev = DateTime.Now;
            int scanrange = 14;
            ready = false;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            shapetype = 0;
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            Bitmap bmp;
            bmp = new Bitmap(1920, 1080, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmp);
            gfxScreenshot.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size, System.Drawing.CopyPixelOperation.SourceCopy);
            LockBitmap lockBitmap = new LockBitmap(bmp);
            lockBitmap.LockBits();
            int closeX = 0;
            int closeY = 0;
            float dist;
            float tdist;
            dist = (float)Math.Sqrt((closeX - 960) * (closeX - 960) + (closeY - 540) * (closeY - 540));
            unsafe
            {
                // example assumes 24bpp image.  You need to verify your pixel depth
                // loop by row for better data locality
                System.Drawing.Color pentagon = System.Drawing.Color.FromArgb(118, 141, 252);
                System.Drawing.Color triangle = System.Drawing.Color.FromArgb(252, 118, 119);
                System.Drawing.Color square = System.Drawing.Color.FromArgb(255, 232, 105);
                System.Drawing.Color blue = System.Drawing.Color.FromArgb(0, 178, 225);
                System.Drawing.Color red = System.Drawing.Color.FromArgb(241, 78, 84);
                System.Drawing.Color green = System.Drawing.Color.FromArgb(0, 225, 110);
                System.Drawing.Color purple = System.Drawing.Color.FromArgb(191, 127, 245);
                System.Drawing.Color bcannon = System.Drawing.Color.FromArgb(24, 135, 164);
                System.Drawing.Color rcannon = System.Drawing.Color.FromArgb(182, 59, 64);
                System.Drawing.Color gcannon = System.Drawing.Color.FromArgb(27, 173, 99);
                System.Drawing.Color pcannon = System.Drawing.Color.FromArgb(144, 96, 184);
                System.Drawing.Color crasher = System.Drawing.Color.FromArgb(241, 119, 221);
                System.Drawing.Color cannon = System.Drawing.Color.FromArgb(153, 153, 153);
                while (tank != red && tank != blue && tank != green && tank != purple)
                {
                    tank = lockBitmap.GetPixel(960, 580);
                }
                Trace.WriteLine(tank);

                for (int y = 140; y < lockBitmap.Height; y += scanrange)
                {
                    if (runscan == false)
                    {
                        break;
                    }
                    if (tank != red && tank != blue && tank != green && tank != purple)
                    {
                        Trace.WriteLine("not a tank");
                        break;

                    }

                    for (int x = 0; x < lockBitmap.Width; x += scanrange)
                    {
                        // windows stores images in BGR pixel order
                        tdist = (float)Math.Sqrt((x - 960) * (x - 960) + (y - 540) * (y - 540));

                        if ((lockBitmap.GetPixel(x, y) == red || lockBitmap.GetPixel(x, y) == blue || lockBitmap.GetPixel(x, y) == green || lockBitmap.GetPixel(x, y) == purple) && lockBitmap.GetPixel(x, y) != tank)
                        {
                            int x3 = 30;
                            if (x3 + x >= 1920)
                            {
                                x3 = 1920 - x - 1;
                            }
                            if ((lockBitmap.GetPixel(x + x3, y) == red || lockBitmap.GetPixel(x + x3, y) == blue || lockBitmap.GetPixel(x + x3, y) == green || lockBitmap.GetPixel(x + x3, y) == purple) && lockBitmap.GetPixel(x + x3, y) != tank)
                            {
                                if ((tdist < dist && shapetype == 4) || shapetype < 4)
                                {
                                    shapetype = 4;
                                    closeX = x;
                                    closeY = y;
                                    dist = tdist;
                                }
                                if (lastshape >= 4 && Math.Abs(tdist - lastdist) < 145)
                                {
                                    shapetype = 5;
                                    closeX = x + (x - lastX);
                                    closeY = y + y - lastY;
                                }
                            }
                        }
                        else if (lockBitmap.GetPixel(x, y) == crasher)
                        {
                            if ((tdist < dist && shapetype == 3) || shapetype < 3)
                            {
                                shapetype = 3;
                                closeX = x;
                                closeY = y;
                                dist = tdist;
                            }
                        }
                        else if (lockBitmap.GetPixel(x, y) == pentagon)
                        {
                            if ((tdist < dist && shapetype == 3) || shapetype < 3)
                            {
                                shapetype = 3;
                                closeX = x;
                                closeY = y;
                                dist = tdist;
                            }
                        }
                        else if (lockBitmap.GetPixel(x, y) == triangle)
                        {
                            if ((tdist < dist && shapetype == 2) || shapetype < 2)
                            {
                                shapetype = 2;
                                closeX = x;
                                closeY = y;
                                dist = tdist;
                            }
                        }
                        else if (lockBitmap.GetPixel(x, y) == square)
                        {
                            if ((tdist < dist && shapetype == 1) || shapetype < 1)
                            {
                                shapetype = 1;
                                closeX = x;
                                closeY = y;
                                dist = tdist;
                            }
                        }
                        else
                        {
                        }
                        // next pixel in the row
                    }

                }
            }
            if (closeX != 0)
                SetCursorPos(closeX, closeY);
            lastshape = shapetype;
            lastX = closeX;
            lastY = closeY;
            lastdist = dist;
            lockBitmap.UnlockBits();
            bmp.Dispose();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            if (closeX != 0)
                Trace.WriteLine(closeX + " " + closeY + " " + shapetype + " " + dist + " " + elapsedMs);
            ready = true;

        }
        private void toggle()
        {
            if (!runscan)
            {
                runscan = true;
                Status.Text = "On";
            }
            else
            {
                runscan = false;
                Status.Text = "Off";
                tank = System.Drawing.Color.FromArgb(100, 100, 100);
            }
            Trace.WriteLine("Toggled to: " + runscan);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            toggle();
        }
    }
}
