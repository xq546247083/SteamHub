using SteamHub.Manager;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace SteamHub
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// 应用启动
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 如果存在，直接返回
            if (ExistsCurrentProcess())
            {
                Environment.Exit(0);
            }

            RegisterGlobalExceptionHandling();
            DbInitializer.Initialize();
            NotifyIconManager.Init();
        }

        /// <summary>
        /// 注册全局异常处理
        /// </summary>
        private void RegisterGlobalExceptionHandling()
        {
            DispatcherUnhandledException += Application_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// <summary>
        /// UI 线程未捕获异常处理
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Locator.ShowToolTip($"异常！错误信息为：{e.Exception?.Message ?? string.Empty}");
        }

        /// <summary>
        /// 非 UI 线程未捕获异常处理
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Locator.ShowToolTip($"异常！错误信息为：{exception?.Message ?? string.Empty}");
        }

        /// <summary>
        /// Task 未捕获异常处理
        /// </summary>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Locator.ShowToolTip($"异常！错误信息为：{e.Exception?.Message ?? string.Empty}");
        }

        /// <summary>
        /// 是否存在当前进程
        /// </summary>
        private static bool ExistsCurrentProcess()
        {
            var currentProcess = Process.GetCurrentProcess();

            var processList = Process.GetProcesses();
            foreach (Process item in processList)
            {
                if (item.ProcessName.ToLower() == currentProcess.ProcessName.ToLower() && item.Id != currentProcess.Id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}