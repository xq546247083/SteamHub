namespace SteamHub
{
    /// <summary>
    /// 全局应用常量定义
    /// </summary>
    public static class AppConstants
    {
        #region Steam 账号与虚拟分类

        /// <summary>
        /// 默认账号（全部游戏）的 SteamId 标识
        /// </summary>
        public const string DefaultSteamId = "10000000000000000";

        /// <summary>
        /// 默认账号显示名称（全部游戏）
        /// </summary>
        public const string DefaultSteamAccountName = "全部游戏";

        #endregion

        #region 注册表相关常量

        /// <summary>
        /// Steam 注册表键路径
        /// </summary>
        public const string SteamRegistryPath = @"Software\Valve\Steam";

        /// <summary>
        /// Steam 自动登录用户键名
        /// </summary>
        public const string AutoLoginUserKey = "AutoLoginUser";

        /// <summary>
        /// Steam 安装路径键名
        /// </summary>
        public const string SteamPathKey = "SteamPath";

        /// <summary>
        /// Steam.exe 路径键名
        /// </summary>
        public const string SteamExeKey = "SteamExe";

        #endregion

        #region 数据库常量

        /// <summary>
        /// SQLite 数据库文件名称
        /// </summary>
        public const string DbFileName = "steamhub.db";

        #endregion
    }
}
