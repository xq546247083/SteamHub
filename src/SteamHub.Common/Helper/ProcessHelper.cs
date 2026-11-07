using System.Diagnostics;

namespace SteamHub.Helper
{
    /// <summary>
    /// 进程帮助类
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// 杀指定进程并等待退出
        /// </summary>
        /// <param name="processName">进程名</param>
        public static void Kill(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return;
            }

            var handleProcessName = processName.ToLower().Split('.')[0];
            var processList = Process.GetProcessesByName(handleProcessName);
            foreach (var item in processList)
            {
                try
                {
                    if (!item.HasExited)
                    {
                        item.Kill();
                        item.WaitForExit(10000);
                    }
                }
                catch (Exception) { }
                finally
                {
                    item.Dispose();
                }
            }
        }
    }
}
