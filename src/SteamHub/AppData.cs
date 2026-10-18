using SteamHub.Entities;

namespace SteamHub
{
    /// <summary>
    /// 全局应用数据与默认条目定义
    /// </summary>
    public static class AppData
    {
        /// <summary>
        /// 默认账号（包含全部游戏）
        /// </summary>
        public static readonly SteamAccount DefaultSteamAccount = new SteamAccount(Guid.Empty, AppConstants.DefaultSteamId, AppConstants.DefaultSteamAccountName, AppConstants.DefaultSteamAccountName, string.Empty, -1, null);

        /// <summary>
        /// 判断指定账号是否为默认账号（全部游戏）或为空
        /// </summary>
        /// <param name="account">账号实体</param>
        /// <returns>是否为默认账号</returns>
        public static bool IsDefaultAccount(SteamAccount account)
        {
            return account == null || account == DefaultSteamAccount || account.SteamId == AppConstants.DefaultSteamId;
        }

        /// <summary>
        /// 判断指定 SteamId 是否为默认账号（全部游戏）标识
        /// </summary>
        /// <param name="steamId">SteamId 字符串</param>
        /// <returns>是否为默认账号</returns>
        public static bool IsDefaultAccount(string steamId)
        {
            return string.IsNullOrEmpty(steamId) || steamId == AppConstants.DefaultSteamId;
        }
    }
}
