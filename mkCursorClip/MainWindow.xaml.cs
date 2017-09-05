using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;
using RamGecTools;

namespace mkCursorClip
{
    public partial class MainWindow : Window
    {
        mkCursorClip.Properties.Settings config = mkCursorClip.Properties.Settings.Default;
        public MainWindow()
        {
            InitializeComponent();
            this.addNotifycationIcon();
            this.initKeyboardHook();
        }

        #region keyboardHook handling
        KeyboardHook keyboardHook = new KeyboardHook();
        bool keyOneDown = false;
        bool keyTwoDown = false;
        KeyboardHook.VKeys keyOne;
        KeyboardHook.VKeys keyTwo;

        private void initKeyboardHook()
        {            
            keyboardHook.KeyDown += new KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
            keyboardHook.KeyUp += new KeyboardHook.KeyboardHookCallback(keyboardHook_KeyUp);
            keyboardHook.Install();
            
        }

        void keyboardHook_KeyUp(KeyboardHook.VKeys key, ref bool handled)
        {
            if (key == keyOne)
                keyOneDown = false;
            if (key == keyTwo)
                keyTwoDown = false;
        }

        void keyboardHook_KeyDown(KeyboardHook.VKeys key, ref bool handled)
        {
            config.Reload();
            keyOne = (KeyboardHook.VKeys)Enum.Parse(typeof(KeyboardHook.VKeys), config.keyOne);
            keyTwo = (KeyboardHook.VKeys)Enum.Parse(typeof(KeyboardHook.VKeys), config.keyTwo);

            if (key == keyOne)
                keyOneDown = true;
            if (key == keyTwo)
                keyTwoDown = true;

            if (keyOneDown && keyTwoDown)
            {
                this.toggleClip();
                handled = true;
            }
        }
        #endregion

        /// <summary>
        /// check for keyTwoDown and LControl to be pressed and clips cursor to window
        /// </summary>
        #region WinAPI for window picking
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion
        bool windowHooked = false; // TODO: replace with x/y check of cursor
        Rect rect = new Rect();
        private void toggleClip()
        {
            config.Reload();
            int leftBorder = config.left;
            int rightBorder = config.right + config.left;
            int topBorder = config.top;
            int bottomBorder = config.top + config.bottom;

            if (windowHooked == true)
            {
                System.Windows.Forms.Cursor.Clip = System.Drawing.Rectangle.Empty;
                windowHooked = false;
            }
            else
            {
                GetWindowRect(GetForegroundWindow(), out rect);
                System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle(rect.Left + leftBorder,
                                                                                rect.Top + topBorder,
                                                                                rect.Right - rect.Left - rightBorder,
                                                                                rect.Bottom - rect.Top - bottomBorder);
                windowHooked = true;
            }
        }

        /// <summary>
        /// Generates and adds the notification icon to the tray
        /// </summary>
        private void addNotifycationIcon()
        {
            TaskbarIcon notifyIcon = new TaskbarIcon();
            System.Windows.Controls.ContextMenu notifyMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem settingsItem = new System.Windows.Controls.MenuItem();
            System.Windows.Controls.MenuItem exitItem = new System.Windows.Controls.MenuItem();

            settingsItem.Header = "Settings";
            settingsItem.Click += new RoutedEventHandler(settingsItem_Click);
            notifyMenu.Items.Add(settingsItem);

            exitItem.Header = "Exit";
            exitItem.Click += new RoutedEventHandler(exitItem_Click);
            notifyMenu.Items.Add(exitItem);

            notifyIcon.ToolTipText = "mkCursorClip";
            notifyIcon.IconSource = System.Windows.Media.Imaging.BitmapFrame.Create(new System.Uri("pack://application:,,,/mkCursorClip.ico"));
            notifyIcon.ContextMenu = notifyMenu;
        }

        /// <summary>
        /// context menu item 'Exit'
        /// </summary>
        void exitItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// context menu item 'Settings'
        /// </summary>
        SettingsWindow settingsWindow;
        bool settingsWindowOpen = false;
        void settingsItem_Click(object sender, RoutedEventArgs e)
        {
            if (settingsWindowOpen)
            {
                settingsWindow.Focus();
            }
            else
            {
                settingsWindow = new SettingsWindow();
                settingsWindow.Show();
                settingsWindow.Closed += new EventHandler(settingsWindow_Closed);
                settingsWindowOpen = true;
            }
        }

        void settingsWindow_Closed(object sender, EventArgs e)
        {
            settingsWindow = null;
            settingsWindowOpen = false;
        }

        #region Window styles (Hide from Alt+Tab)
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
    }
}
