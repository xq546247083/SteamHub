using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using System.Reflection;

namespace SteamHub.Manager
{
    /// <summary>
    /// 通知管理
    /// </summary>
    public static class NotifyIconManager
    {
        #region 私有属性

        /// <summary>
        /// 系统托盘
        /// </summary>
        private static TaskbarIcon taskbarIcon;

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            taskbarIcon = new TaskbarIcon();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamHub.Resources.app.ico"))
            {
                taskbarIcon.Icon = new Icon(stream);
            }
            taskbarIcon.TrayPopup = Locator.TrayPopupControl;
            taskbarIcon.TrayMouseDoubleClick += TaskbarIcon_TrayMouseDoubleClick;
            taskbarIcon.TrayRightMouseUp += TaskbarIcon_TrayRightMouseUp;

            PreLoadNotifyUI();
        }

        /// <summary>
        /// 关闭托盘弹窗
        /// </summary>
        public static void Close()
        {
            taskbarIcon?.CloseTrayPopup();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 双击系统推盘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TaskbarIcon_TrayMouseDoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Locator.OpenMainWindow();
        }

        /// <summary>
        /// 右键点击系统托盘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TaskbarIcon_TrayRightMouseUp(object sender, System.Windows.RoutedEventArgs e)
        {
            taskbarIcon.ShowTrayPopup();
        }

        /// <summary>
        /// 预热菜单
        /// </summary>
        private static void PreLoadNotifyUI()
        {
            // 预热系统托盘菜单的元素，解决第一次打开系统托盘菜单略微卡顿的问题
            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                var dummyWindow = new System.Windows.Window
                {
                    Width = 0,
                    Height = 0,
                    WindowStyle = System.Windows.WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    AllowsTransparency = true,
                    Background = System.Windows.Media.Brushes.Transparent,
                    Content = Locator.TrayPopupControl
                };

                dummyWindow.Show();
                dummyWindow.Content = null;
                dummyWindow.Close();

                // 重新绑定回托盘
                taskbarIcon.TrayPopup = Locator.TrayPopupControl;
            }));
        }

        #endregion
    }
}
