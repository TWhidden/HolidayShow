using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using HolidayShowEditor.Controllers;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;

namespace HolidayShowEditor
{
    public partial class ShellWindow : Window
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnityContainer _container;

        private int _major = Environment.OSVersion.Version.Major;

        public ShellWindow()
        {

        }

        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //    var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
        //    if (hwndSource != null)
        //    {
        //        hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
        //    }
        //    base.OnSourceInitialized(e);
        //}


        public ShellWindow(IEventAggregator eventAggregator, IUnityContainer container)
        {
            _eventAggregator = eventAggregator;
            _container = container;
            InitializeComponent();
            Loaded += ShellWindow_Loaded;
        }

        void ShellWindow_Loaded(object sender, RoutedEventArgs e)
        {
            

            if (_major < 6 || Program.ApplicationArgs.Contains("-disable_dwm_borders")) return;

            m_hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            m_hwndSource.AddHook(this.WndProc);


        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        [DllImport("dwmapi.dll")]
        [PreserveSig]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        [DllImport("dwmapi.dll")]
        [PreserveSig]
        private static extern int DwmIsCompositionEnabled(out bool pfEnabled);

        void TryApplyShadow()
        {
            // Windows Vista or higher.
            if (_major < 6) return;

            bool dwmEnabled = false;

            DwmIsCompositionEnabled(out dwmEnabled);

            if (!dwmEnabled)
            {
                m_triedApplyShadow = true;
                return;
            }

            var helper = new WindowInteropHelper(this);

            var m = new Margins { bottomHeight = 1, leftWidth = 0, rightWidth = 0, topHeight = 0 };
            DwmExtendFrameIntoClientArea(helper.Handle, ref m);

            m_triedApplyShadow = true;
        }

        public override void OnApplyTemplate()
        {
            //var headerBorder = GetTemplateChild("HeaderBorder") as Border;
            HeaderBorder.PreviewMouseMove += Border_MouseMove;
            HeaderBorder.PreviewMouseDown += new MouseButtonEventHandler(HeaderBorder_PreviewMouseDown);
            Closed += new EventHandler(ShellWindow_Closed);
            base.OnApplyTemplate();
        }

        void ShellWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        void HeaderBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else if (WindowState == WindowState.Normal)
                    WindowState = WindowState.Maximized;
            }
        }

        private void SizingRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (base.WindowState != WindowState.Maximized)
            {

                if ((e.ClickCount >= 2) && (((sender as System.Windows.Shapes.Rectangle) == this.TopResizeRect) || ((sender as System.Windows.Shapes.Rectangle) == this.BottomResizeRect)))
                {

                }
                else
                {
                    if ((sender as System.Windows.Shapes.Rectangle) == this.LeftResizeRect)
                    {
                        this.ResizeWindow(ResizeDirection.Left);
                    }
                    else if ((sender as System.Windows.Shapes.Rectangle) == this.TopLeftResizeRect)
                    {
                        this.ResizeWindow(ResizeDirection.TopLeft);
                    }
                    else if ((sender as System.Windows.Shapes.Rectangle) == this.TopResizeRect)
                    {
                        this.ResizeWindow(ResizeDirection.Top);
                    }
                    else if ((sender as System.Windows.Shapes.Rectangle) == this.TopRightResizeRect)
                    {
                        this.ResizeWindow(ResizeDirection.TopRight);
                    }
                    else if ((sender as System.Windows.Shapes.Rectangle) == this.RightResizeRect)
                    {
                        this.ResizeWindow(ResizeDirection.Right);
                    }
                    else if ((sender as System.Windows.Shapes.Rectangle) == this.BottomRightResizeRect)
                    {
                        this.ResizeWindow(ResizeDirection.BottomRight);
                    }
                    else if ((sender as System.Windows.Shapes.Rectangle) == this.BottomResizeRect)
                    {
                        this.ResizeWindow(ResizeDirection.Bottom);
                    }
                    else if ((sender as System.Windows.Shapes.Rectangle) == this.BottomLeftResizeRect)
                    {
                        this.ResizeWindow(ResizeDirection.BottomLeft);
                    }
                }
            }
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            if (m_hwndSource != null)
                SendMessage(m_hwndSource.Handle, 0x112, (IntPtr)(0xf000 + direction), IntPtr.Zero);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public enum ResizeDirection
        {
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5
        }

        private bool m_triedApplyShadow;
        private HwndSource m_hwndSource;

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

        private void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {

            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero)
            {

                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMinTrackSize.x = (int)MinWidth;
                mmi.ptMinTrackSize.y = (int)MinHeight;
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }


        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>            
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>            
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>            
            public int dwFlags = 0;
        }


        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;
            /// <summary> Win32 </summary>
            public int top;
            /// <summary> Win32 </summary>
            public int right;
            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }
            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }
            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }


        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// 
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);


        private const int WM_WININICHANGE = 0x001A;
        private const int WM_DEVICECHANGE = 0x219;
        private const int WM_DISPLAYCHANGE = 0x7E;
        private const int WM_THEMECHANGED = 0x031A;
        private const int WM_SYSCOLORCHANGE = 0x15;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:/* WM_GETMINMAXINFO */
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            if (_major < 6) return IntPtr.Zero;

            switch (msg)
            {
                case 0x31e:
                    TryApplyShadow();
                    handled = true;
                    break;

                case 0x86:
                    {
                        IntPtr ptr = DefWindowProc(hwnd, msg, wParam, new IntPtr(-1));
                        handled = true;
                        return ptr;
                    }
                case 0x83:
                    if (wParam == new IntPtr(1))
                    {
                        handled = true;
                    }
                    break;

                case 70:

                    if (!m_triedApplyShadow)
                    {
                        TryApplyShadow();
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        async private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
