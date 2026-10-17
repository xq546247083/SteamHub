using SteamHub.Entities;

namespace SteamHub.Repositories;

/// <summary>
/// Steam 账号与游戏关系仓储
/// 纯粹负责 SteamAccountGame 关联关系表的持久化原子操作
/// </summary>
public static class SteamAccountGameRepository
{
    /// <summary>
    /// 批量添加或更新账号与游戏的关联关系
    /// </summary>
    public static void AddOrUpdateList(List<SteamAccountGame> accountGames)
    {
        if (accountGames == null || accountGames.Count == 0)
        {
            return;
        }

        using var context = new SteamHubDbContext();

        foreach (var item in accountGames)
        {
            var existing = context.SteamAccountGames.FirstOrDefault(r => r.SteamId == item.SteamId && r.AppId == item.AppId);
            if (existing != null)
            {
                if (item.LastPlayed > existing.LastPlayed)
                {
                    existing.LastPlayed = item.LastPlayed;
                }
                existing.IsDeleted = false;
            }
            else
            {
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                }
                context.SteamAccountGames.Add(item);
            }
        }

        context.SaveChanges();
    }

    /// <summary>
    /// 删除账号和游戏的关联关系 (软删除)
    /// </summary>
    public static void Delete(string steamId, string appId)
    {
        using var context = new SteamHubDbContext();
        var existing = context.SteamAccountGames.FirstOrDefault(r => r.SteamId == steamId && r.AppId == appId);
        if (existing != null)
        {
            existing.IsDeleted = true;
            context.SaveChanges();
        }
    }

    /// <summary>
    /// 清空所有关联关系
    /// </summary>
    public static void Clear()
    {
        using var context = new SteamHubDbContext();
        context.SteamAccountGames.RemoveRange(context.SteamAccountGames);
        context.SaveChanges();
    }
}
