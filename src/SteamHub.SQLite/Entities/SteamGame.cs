namespace SteamHub.Entities;

/// <summary>
/// Steam 游戏实体
/// </summary>
public class SteamGame
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public Guid Id
    {
        get; set;
    }

    /// <summary>
    /// Steam 游戏 ID
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
    /// 是否已删除
    /// </summary>
    public bool IsDeleted
    {
        get; set;
    }

    /// <summary>
    /// 当前选中的启动账号 SteamId
    /// </summary>
    public string CurrentSelectAccountSteamId
    {
        get; set;
    } = string.Empty;

    /// <summary>
    /// 上次游玩时间 (Unix 时间戳)
    /// </summary>
    public long LastPlayed
    {
        get; set;
    }

    public SteamGame()
    {
    }

    public SteamGame(Guid id, string appId, string name, byte[] icon, byte[] hero, long lastPlayed = 0)
    {
        Id = id;
        AppId = appId;
        Name = name;
        Icon = icon;
        Hero = hero;
        LastPlayed = lastPlayed;
    }
}