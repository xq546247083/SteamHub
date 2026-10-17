using SteamHub.Entities;

namespace SteamHub.Repositories
{
    /// <summary>
    /// Steam 账号仓储
    /// 统一负责所有与 SteamAccount 实体相关的数据访问
    /// </summary>
    public static class SteamAccountRepository
    {
        /// <summary>
        /// 获取所有账号
        /// </summary>
        /// <param name="includeHidden">是否包含已隐藏账号</param>
        public static List<SteamAccount> GetAll(bool includeHidden = false)
        {
            using var context = new SteamHubDbContext();
            var query = context.SteamAccounts.AsQueryable();
            if (!includeHidden)
            {
                query = query.Where(a => !a.IsHidden);
            }
            return query
                .OrderBy(a => a.Order)
                .ThenBy(a => a.Account)
                .ToList();
        }

        /// <summary>
        /// 根据账号名获取账号
        /// </summary>
        public static SteamAccount GetByAccount(string account)
        {
            using var context = new SteamHubDbContext();
            return context.SteamAccounts.FirstOrDefault(a => a.Account == account);
        }

        /// <summary>
        /// 根据 SteamId 获取账号
        /// </summary>
        public static SteamAccount GetBySteamId(string steamId)
        {
            using var context = new SteamHubDbContext();
            return context.SteamAccounts.FirstOrDefault(a => a.SteamId == steamId);
        }

        /// <summary>
        /// 设置账号隐藏状态
        /// </summary>
        /// <param name="account">账号名称</param>
        /// <param name="isHidden">是否隐藏</param>
        public static void SetHidden(string account, bool isHidden)
        {
            using var context = new SteamHubDbContext();
            var entity = context.SteamAccounts.FirstOrDefault(a => a.Account == account);
            if (entity != null)
            {
                entity.IsHidden = isHidden;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 获取拥有指定游戏的所有真实账号列表
        /// </summary>
        /// <param name="appId">游戏 AppId</param>
        /// <returns>账号列表 (纯实体数据)</returns>
        public static List<SteamAccount> GetAccountsByAppId(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return new List<SteamAccount>();
            }

            using var context = new SteamHubDbContext();

            return (from rel in context.SteamAccountGames
                    join acc in context.SteamAccounts on rel.SteamId equals acc.SteamId
                    where rel.AppId == appId && !acc.IsHidden
                    orderby rel.LastPlayed descending, acc.Order ascending
                    select acc).ToList();
        }

        /// <summary>
        /// 更新账号
        /// </summary>
        public static void Update(SteamAccount account)
        {
            using var context = new SteamHubDbContext();
            var existing = context.SteamAccounts.FirstOrDefault(a => a.Account == account.Account);
            if (existing != null)
            {
                existing.Name = account.Name;
                existing.Password = account.Password;
                existing.Order = account.Order;
                existing.Icon = account.Icon;
                existing.GameCount = account.GameCount;
                existing.LastPlayed = account.LastPlayed;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 检查账号是否存在
        /// </summary>
        public static bool Exists(string account)
        {
            using var context = new SteamHubDbContext();
            return context.SteamAccounts.Any(a => a.Account == account);
        }

        /// <summary>
        /// 批量添加或同步账号基础数据
        /// </summary>
        public static void AddList(List<SteamAccount> steamAccounts)
        {
            using var context = new SteamHubDbContext();

            foreach (var steamAccount in steamAccounts)
            {
                var existing = context.SteamAccounts.FirstOrDefault(g => g.Account == steamAccount.Account);
                if (existing != null)
                {
                    existing.Name = steamAccount.Name;
                    existing.Icon = steamAccount.Icon;
                    existing.GameCount = steamAccount.GameCount;
                    existing.LastPlayed = steamAccount.LastPlayed;
                    continue;
                }

                if (steamAccount.Id == Guid.Empty)
                {
                    steamAccount.Id = Guid.NewGuid();
                }
                context.SteamAccounts.Add(steamAccount);
            }

            context.SaveChanges();
        }

        /// <summary>
        /// 批量更新账号
        /// </summary>
        public static void UpdateList(List<SteamAccount> steamAccounts)
        {
            using var context = new SteamHubDbContext();

            foreach (var steamAccount in steamAccounts)
            {
                var existing = context.SteamAccounts.FirstOrDefault(g => g.Account == steamAccount.Account);
                if (existing == null)
                {
                    continue;
                }

                existing.Name = steamAccount.Name;
                existing.Password = steamAccount.Password;
                existing.Order = steamAccount.Order;
                existing.Icon = steamAccount.Icon;
                existing.GameCount = steamAccount.GameCount;
                existing.LastPlayed = steamAccount.LastPlayed;
            }

            context.SaveChanges();
        }

        /// <summary>
        /// 根据账号列表物理删除账号
        /// </summary>
        /// <param name="accounts">账号列表</param>
        public static void DeleteByAccounts(IEnumerable<string> accounts)
        {
            if (accounts == null || !accounts.Any())
            {
                return;
            }

            using var context = new SteamHubDbContext();
            var list = context.SteamAccounts.Where(a => accounts.Contains(a.Account)).ToList();
            if (list.Count > 0)
            {
                context.SteamAccounts.RemoveRange(list);
                context.SaveChanges();
            }
        }
    }
}