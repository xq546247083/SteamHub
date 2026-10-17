namespace SteamHub.Entities;

/// <summary>
/// Steam 账号实体
/// </summary>
public class SteamAccount
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public Guid Id
    {
        get; set;
    }

    /// <summary>
    /// SteamId
    /// </summary>
    public string SteamId
    {
        get; set;
    }

    /// <summary>
    /// 账号（唯一）
    /// </summary>
    public string Account
    {
        get; set;
    }

    /// <summary>
    /// 名字
    /// </summary>
    public string Name
    {
        get; set;
    }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password
    {
        get; set;
    }

    /// <summary>
    /// 排序序号
    /// </summary>
    public int Order
    {
        get; set;
    }

    /// <summary>
    /// 头像数据
    /// </summary>
    public byte[] Icon
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
    /// 账号拥有的游戏数量
    /// </summary>
    public int GameCount
    {
        get; set;
    }

    /// <summary>
    /// 最新游戏时间
    /// </summary>
    public long LastPlayed
    {
        get; set;
    }

    public SteamAccount()
    {
    }

    public SteamAccount(Guid id, string steamId, string account, string name, string password, int order, byte[] icon, int gameCount = 0, long lastPlayed = 0)
    {
        Id = id;
        SteamId = steamId;
        Account = account;
        Name = name;
        Password = password;
        Order = order;
        Icon = icon;
        GameCount = gameCount;
        LastPlayed = lastPlayed;
    }
}