using SteamHub.Entities;

namespace SteamHub.Repositories
{
    /// <summary>
    /// Steam游戏仓储
    /// </summary>
    public static class SteamGameRepository
    {
        /// <summary>
        /// 获取所有游戏
        /// </summary>
        public static List<SteamGame> GetAll()
        {
            using var context = new SteamHubDbContext();
            return context.SteamGames
                .Where(g => !g.IsDeleted)
                .OrderBy(g => g.Order)
                .ThenBy(g => g.Name)
                .ToList();
        }

        /// <summary>
        /// 获取所有已删除游戏
        /// </summary>
        public static List<SteamGame> GetListOfDeleted()
        {
            using var context = new SteamHubDbContext();
            return context.SteamGames
                .Where(g => g.IsDeleted)
                .OrderBy(g => g.Order)
                .ThenBy(g => g.Name)
                .ToList();
        }

        /// <summary>
        /// 根据 SteamId 获取游戏列表
        /// </summary>
        public static List<SteamGame> GetBySteamId(string accountSteamId)
        {
            using var context = new SteamHubDbContext();
            return context.SteamGames
                .Where(g => g.AccountSteamId == accountSteamId && !g.IsDeleted)
                .OrderBy(g => g.Order)
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
                existing.Order = game.Order;
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
        /// 批量更新游戏的Steam账号ID
        /// </summary>
        public static void UpdateAccountSteamIds(List<SteamGame> games)
        {
            using var context = new SteamHubDbContext();

            foreach (var game in games)
            {
                var existing = context.SteamGames.FirstOrDefault(g => g.AppId == game.AppId);
                if (existing == null)
                {
                    continue;
                }

                existing.AccountSteamId = game.AccountSteamId;
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
                existing.Order = game.Order;
            }

            context.SaveChanges();
        }

        /// <summary>
        /// 删除游戏(软删除)
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
        /// 恢复游戏
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