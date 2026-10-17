using SteamHub.Helper;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SteamHub
{
    /// <summary>
    /// steam工具类
    /// </summary>
    public static class SteamTool
    {
        /// <summary>
        /// 获取Steam安装路径
        /// </summary>
        public static string GetSteamPath()
        {
            // 启动steam
            var (getSuccess, steamPathObj) = RegistryHelper.Get(AppConstants.SteamRegistryPath, AppConstants.SteamPathKey);
            if (getSuccess == false || steamPathObj == null)
            {
                return string.Empty;
            }

            var steamPath = steamPathObj.ToString();
            if (string.IsNullOrEmpty(steamPath))
            {
                return string.Empty;
            }

            return steamPath;
        }

        /// <summary>
        /// 获取Steam.exe路径
        /// </summary>
        public static string GetSteamExe()
        {
            // 启动steam
            var (getSuccess, steamExeObj) = RegistryHelper.Get(AppConstants.SteamRegistryPath, AppConstants.SteamExeKey);
            if (getSuccess == false || steamExeObj == null)
            {
                return string.Empty;
            }

            var steamExe = steamExeObj.ToString();
            if (string.IsNullOrEmpty(steamExe))
            {
                return string.Empty;
            }

            return steamExe;
        }

        /// <summary>
        /// 设置所有账号的AllowAutoLogin为1
        /// </summary>
        public static void SetAllowAutoLogin()
        {
            var steamPath = GetSteamPath();
            if (string.IsNullOrEmpty(steamPath))
            {
                return;
            }

            var vdfPath = Path.Combine(steamPath, "config", "loginusers.vdf");
            if (!File.Exists(vdfPath))
            {
                return;
            }

            var content = File.ReadAllText(vdfPath);
            if (!content.Contains("\"AllowAutoLogin\""))
            {
                return;
            }

            var newContent = Regex.Replace(content, "\"AllowAutoLogin\"\\s+\"0\"", "\"AllowAutoLogin\"\t\t\"1\"", RegexOptions.IgnoreCase);
            if (content != newContent)
            {
                File.WriteAllText(vdfPath, newContent);
            }
        }

        /// <summary>
        /// 打开steam并切换账号
        /// </summary>
        /// <param name="account">账号</param>
        public static void OpenAccount(string account)
        {
            // 杀掉steam进程
            ProcessHelper.Kill("steam");

            // 修改 loginusers.vdf 中所有账号的 AllowAutoLogin 为 1
            SetAllowAutoLogin();

            // 设置注册信息
            RegistryHelper.Set(AppConstants.SteamRegistryPath, AppConstants.AutoLoginUserKey, account);

            // 启动Steam
            var steamExe = GetSteamExe();
            if (string.IsNullOrEmpty(steamExe))
            {
                return;
            }
            Process.Start(steamExe);
        }

        /// <summary>
        /// 打开游戏
        /// </summary>
        /// <param name="appId">游戏AppId</param>
        public static void OpenGame(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return;
            }

            Process.Start(new ProcessStartInfo($"steam://run/{appId}") { UseShellExecute = true });
        }

        /// <summary>
        /// 切换账号并打开游戏
        /// </summary>
        /// <param name="account">目标账号 (若为空则不切换账号直接打开游戏)</param>
        /// <param name="appId">游戏 AppId</param>
        public static void OpenAccountAndGame(string account, string appId)
        {
            if (!string.IsNullOrEmpty(account))
            {
                var (success, currentAutoLoginUser) = RegistryHelper.Get(AppConstants.SteamRegistryPath, AppConstants.AutoLoginUserKey);
                if (!success || currentAutoLoginUser?.ToString() != account)
                {
                    OpenAccount(account);
                }
            }

            OpenGame(appId);
        }
    }
}