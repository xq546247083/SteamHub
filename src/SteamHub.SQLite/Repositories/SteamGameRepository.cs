using SteamHub.Entities;

namespace SteamHub.Repositories
{
    /// <summary>
    /// Steam 游戏仓储
    /// 统一负责所有与 SteamGame 实体相关的数据访问 (按游玩时间自动排序)
    /// </summary>
    public static class SteamGameRepository
    {
        /// <summary>
        /// 获取所有游戏 (统一按最新游戏时间倒序排序)
        /// </summary>
        /// <param name="includeHidden">是否包含已隐藏游戏</param>
        public static List<SteamGame> GetAll(bool includeHidden = false)
        {
            using var context = new SteamHubDbContext();
            var query = context.SteamGames.AsQueryable();
            if (!includeHidden)
            {
                query = query.Where(g => !g.IsHidden);
            }
            return query
                .OrderByDescending(g => g.LastPlayed)
                .ThenBy(g => g.Name)
                .ToList();
        }

        /// <summary>
        /// 根据 SteamId 获取该账号拥有的游戏列表 (按该账号游玩该游戏的时间倒序排序)
        /// </summary>
        /// <param name="steamId">账号 SteamId</param>
        /// <param name="includeHidden">是否包含已隐藏游戏</param>
        /// <returns>游戏列表</returns>
        public static List<SteamGame> GetBySteamId(string steamId, bool includeHidden = false)
        {
            if (string.IsNullOrEmpty(steamId))
            {
                return new List<SteamGame>();
            }

            using var context = new SteamHubDbContext();

            var query = from rel in context.SteamAccountGames
                        join game in context.SteamGames on rel.AppId equals game.AppId
                        where rel.SteamId == steamId
                        select new { rel, game };

            if (!includeHidden)
            {
                query = query.Where(x => !x.game.IsHidden);
            }

            return query
                .OrderByDescending(x => x.rel.LastPlayed)
                .ThenBy(x => x.game.Name)
                .Select(x => x.game)
                .ToList();
        }

        /// <summary>
        /// 根据 AppId 获取游戏
        /// </summary>
        public static SteamGame GetByAppId(string appId)
        {
            using var context = new SteamHubDbContext();
            return context.SteamGames.FirstOrDefault(g => g.AppId == appId);
        }

        /// <summary>
        /// 添加或更新游戏
        /// </summary>
        public static void AddOrUpdate(SteamGame game)
        {
            using var context = new SteamHubDbContext();
            var existing = context.SteamGames.FirstOrDefault(g => g.AppId == game.AppId);

            if (existing != null)
            {
                existing.Name = game.Name;
                if (game.Icon != null)
                {
                    existing.Icon = game.Icon;
                }
                if (game.Hero != null)
                {
                    existing.Hero = game.Hero;
                }
                if (game.LastPlayed > 0)
                {
                    existing.LastPlayed = game.LastPlayed;
                }
            }
            else
            {
                if (game.Id == Guid.Empty)
                {
                    game.Id = Guid.NewGuid();
                }
                context.SteamGames.Add(game);
            }

            context.SaveChanges();
        }

        /// <summary>
        /// 批量添加游戏
        /// </summary>
        public static void AddList(List<SteamGame> games)
        {
            using var context = new SteamHubDbContext();

            foreach (var game in games)
            {
                var existing = context.SteamGames.FirstOrDefault(g => g.AppId == game.AppId);
                if (existing != null)
                {
                    existing.Name = game.Name;
                    if (game.Icon != null)
                    {
                        existing.Icon = game.Icon;
                    }
                    if (game.Hero != null)
                    {
                        existing.Hero = game.Hero;
                    }
                    if (game.LastPlayed > 0)
                    {
                        existing.LastPlayed = game.LastPlayed;
                    }
                    continue;
                }

                if (game.Id == Guid.Empty)
                {
                    game.Id = Guid.NewGuid();
                }
                context.SteamGames.Add(game);
            }

            context.SaveChanges();
        }

        /// <summary>
        /// 批量更新游戏
        /// </summary>
        public static void UpdateList(List<SteamGame> games)
        {
            using var context = new SteamHubDbContext();

            foreach (var game in games)
            {
                var existing = context.SteamGames.FirstOrDefault(g => g.AppId == game.AppId);
                if (existing == null)
                {
                    continue;
                }

                existing.Name = game.Name;
                if (game.Icon != null)
                {
                    existing.Icon = game.Icon;
                }
                if (game.Hero != null)
                {
                    existing.Hero = game.Hero;
                }
                if (game.LastPlayed > 0)
                {
                    existing.LastPlayed = game.LastPlayed;
                }
            }

            context.SaveChanges();
        }

        /// <summary>
        /// 更新指定游戏的当前选中启动账号
        /// </summary>
        /// <param name="appId">游戏 AppId</param>
        /// <param name="steamId">账号 SteamId</param>
        public static void UpdateCurrentSelectAccount(string appId, string steamId)
        {
            using var context = new SteamHubDbContext();
            var existing = context.SteamGames.FirstOrDefault(g => g.AppId == appId);
            if (existing != null)
            {
                existing.CurrentSelectAccountSteamId = steamId ?? string.Empty;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 设置游戏隐藏状态
        /// </summary>
        /// <param name="appId">游戏 AppId</param>
        /// <param name="isHidden">是否隐藏</param>
        public static void SetHidden(string appId, bool isHidden)
        {
            using var context = new SteamHubDbContext();
            var entity = context.SteamGames.FirstOrDefault(g => g.AppId == appId);
            if (entity != null)
            {
                entity.IsHidden = isHidden;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 隐藏游戏
        /// </summary>
        public static void Hide(string appId) => SetHidden(appId, true);

        /// <summary>
        /// 取消隐藏游戏
        /// </summary>
        public static void Unhide(string appId) => SetHidden(appId, false);

        /// <summary>
        /// 检查游戏是否存在
        /// </summary>
        public static bool Exists(string appId)
        {
            using var context = new SteamHubDbContext();
            return context.SteamGames.Any(g => g.AppId == appId);
        }

        /// <summary>
        /// 根据 AppId 列表物理删除游戏
        /// </summary>
        /// <param name="appIds">AppId 列表</param>
        public static void DeleteByAppIds(IEnumerable<string> appIds)
        {
            if (appIds == null || !appIds.Any())
            {
                return;
            }

            using var context = new SteamHubDbContext();
            var list = context.SteamGames.Where(g => appIds.Contains(g.AppId)).ToList();
            if (list.Count > 0)
            {
                context.SteamGames.RemoveRange(list);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 清空所有游戏
        /// </summary>
        public static void Clear()
        {
            using var context = new SteamHubDbContext();
            context.SteamGames.RemoveRange(context.SteamGames);
            context.SaveChanges();
        }
    }
}