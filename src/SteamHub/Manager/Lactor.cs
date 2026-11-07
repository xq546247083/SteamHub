using SteamHub.View;
using SteamHub.ViewModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SteamHub.Manager
{
    /// <summary>
    /// 加载器
    /// </summary>
    public static class Lactor
    {
        #region Win32 API

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        #endregion

        #region 公共属性

        /// <summary>
        /// 主窗口ViewModel
        /// </summary>
        public static MainViewModel MainViewModel
        {
            get;

        } = new MainViewModel();

        /// <summary>
        /// 主窗口
        /// </summary>
        public static MainWindow MainWindow
        {
            get;
        } = new MainWindow();

        /// <summary>
        /// 托盘菜单ViewModel
        /// </summary>
        public static TrayPopupViewModel TrayPopupViewModel
        {
            get;

        } = new TrayPopupViewModel();

        /// <summary>
        /// 托盘菜单
        /// </summary>
        public static TrayPopupControl TrayPopupControl
        {
            get;
        } = new TrayPopupControl();

        #endregion

        #region 公共方法

        /// <summary>
        /// 打开主窗口
        /// </summary>
        public static void OpenMainWindow()
        {
            // 显示窗口
            MainWindow.Show();
            MainWindow.ShowInTaskbar = true;

            // 恢复窗口状态
            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.WindowState = WindowState.Normal;
            }

            // 使用 Win32 API 强制窗口显示在前面
            var helper = new WindowInteropHelper(MainWindow);
            var hwnd = helper.Handle;

            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);

            // WPF 层面的激活
            MainWindow.Activate();
            MainWindow.Topmost = true;
            MainWindow.Topmost = false;
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void ShowToolTip(string message)
        {
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                MainViewModel.SnackbarMessageQueue.Enqueue(message);
                return;
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                MainViewModel.SnackbarMessageQueue.Enqueue(message);
            });
        }

        #endregion
    }
}