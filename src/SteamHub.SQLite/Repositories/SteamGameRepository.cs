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
        /// 获取所有未删除游戏 (统一按最新游戏时间倒序排序)
        /// </summary>
        public static List<SteamGame> GetAll()
        {
            using var context = new SteamHubDbContext();
            return context.SteamGames
                .Where(g => !g.IsDeleted)
                .OrderByDescending(g => g.LastPlayed)
                .ThenBy(g => g.Name)
                .ToList();
        }

        /// <summary>
        /// 根据 SteamId 获取该账号拥有的游戏列表 (按该账号游玩该游戏的时间倒序排序)
        /// </summary>
        /// <param name="steamId">账号 SteamId</param>
        /// <returns>游戏列表</returns>
        public static List<SteamGame> GetBySteamId(string steamId)
        {
            if (string.IsNullOrEmpty(steamId))
            {
                return new List<SteamGame>();
            }

            using var context = new SteamHubDbContext();

            return (from rel in context.SteamAccountGames
                    join game in context.SteamGames on rel.AppId equals game.AppId
                    where rel.SteamId == steamId && !rel.IsDeleted && !game.IsDeleted
                    orderby rel.LastPlayed descending, game.Name ascending
                    select game).ToList();
        }

        /// <summary>
        /// 获取所有已删除游戏 (回收站，按游戏时间倒序)
        /// </summary>
        public static List<SteamGame> GetListOfDeleted()
        {
            using var context = new SteamHubDbContext();
            return context.SteamGames
                .Where(g => g.IsDeleted)
                .OrderByDescending(g => g.LastPlayed)
                .ThenBy(g => g.Name)
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
                existing.Icon = game.Icon;
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
                    existing.Icon = game.Icon;
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
                existing.Icon = game.Icon;
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
        /// 删除游戏 (软删除)
        /// </summary>
        public static void Delete(string appId)
        {
            using var context = new SteamHubDbContext();
            var entity = context.SteamGames.FirstOrDefault(g => g.AppId == appId);
            if (entity != null)
            {
                entity.IsDeleted = true;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 恢复游戏 (回收站)
        /// </summary>
        public static void RestoreRecycleBin()
        {
            using var context = new SteamHubDbContext();
            var deletedGames = context.SteamGames.Where(g => g.IsDeleted).ToList();
            if (deletedGames.Any())
            {
                deletedGames.ForEach(r => r.IsDeleted = false);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 清空回收站
        /// </summary>
        public static void EmptyRecycleBin()
        {
            using var context = new SteamHubDbContext();
            var deletedGames = context.SteamGames.Where(g => g.IsDeleted).ToList();
            if (deletedGames.Any())
            {
                context.SteamGames.RemoveRange(deletedGames);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 检查游戏是否存在
        /// </summary>
        public static bool Exists(string appId)
        {
            using var context = new SteamHubDbContext();
            return context.SteamGames.Any(g => g.AppId == appId);
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