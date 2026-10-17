namespace SteamHub.Entities;

/// <summary>
/// Steam 账号与游戏关联实体
/// </summary>
public class SteamAccountGame
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public Guid Id
    {
        get; set;
    }

    /// <summary>
    /// 账号 SteamId (64位)
    /// </summary>
    public string SteamId
    {
        get; set;
    }

    /// <summary>
    /// 游戏 AppId
    /// </summary>
    public string AppId
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

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted
    {
        get; set;
    }

    public SteamAccountGame()
    {
    }

    public SteamAccountGame(Guid id, string steamId, string appId, long lastPlayed)
    {
        Id = id;
        SteamId = steamId;
        AppId = appId;
        LastPlayed = lastPlayed;
    }
}
