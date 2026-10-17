namespace SteamHub.Model
{
    /// <summary>
    /// Steam游戏数据源模型
    /// </summary>
    public class SteamGameSource
    {
        /// <summary>
        /// 游戏 ID
        /// </summary>
        public string AppId
        {
            get; set;
        }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// 游戏图标数据
        /// </summary>
        public byte[] Icon
        {
            get; set;
        }

        /// <summary>
        /// Steam 游戏大图 (library_hero)
        /// </summary>
        public byte[] Hero
        {
            get; set;
        }

        /// <summary>
        /// 上次游玩时间 (Unix 时间戳)
        /// </summary>
        public long LastPlayed
        {
            get; set;
        }
    }
}