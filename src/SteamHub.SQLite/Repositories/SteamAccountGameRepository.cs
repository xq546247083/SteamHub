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
    /// 根据 AppId 列表删除关联关系
    /// </summary>
    /// <param name="appIds">AppId 列表</param>
    public static void DeleteByAppIds(IEnumerable<string> appIds)
    {
        if (appIds == null || !appIds.Any())
        {
            return;
        }

        using var context = new SteamHubDbContext();
        var list = context.SteamAccountGames.Where(r => appIds.Contains(r.AppId)).ToList();
        if (list.Count > 0)
        {
            context.SteamAccountGames.RemoveRange(list);
            context.SaveChanges();
        }
    }

    /// <summary>
    /// 根据 SteamId 列表删除关联关系
    /// </summary>
    /// <param name="steamIds">SteamId 列表</param>
    public static void DeleteBySteamIds(IEnumerable<string> steamIds)
    {
        if (steamIds == null || !steamIds.Any())
        {
            return;
        }

        using var context = new SteamHubDbContext();
        var list = context.SteamAccountGames.Where(r => steamIds.Contains(r.SteamId)).ToList();
        if (list.Count > 0)
        {
            context.SteamAccountGames.RemoveRange(list);
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
