using SteamHub.Entities;
using SteamHub.Repositories;

namespace SteamHub
{
    /// <summary>
    /// app数据管理类
    /// </summary>
    public static class AppDataManager
    {
        /// <summary>
        /// 清理本地已不存在的账号和游戏
        /// </summary>
        public static (int, int) CleanUpInvalidSteamAccountAndGame()
        {
            // 1. 获取本地真实存在的游戏与登录账号
            var steamGameSources = SteamAnalyzer.GetAllGames();
            var validAppIds = steamGameSources.Select(g => g.AppId).ToHashSet();

            var steamAccountSources = SteamAnalyzer.GetAllLoginAccounts();
            var validAccounts = steamAccountSources.Select(a => a.AccountName).ToHashSet();
            var validSteamIds = steamAccountSources.Select(a => a.SteamId).ToHashSet();

            // 2. 查询数据库中的所有数据 (包括已隐藏的)
            var dbGames = SteamGameRepository.GetAll(includeHidden: true);
            var dbAccounts = SteamAccountRepository.GetAll(includeHidden: true);

            // 3. 找出本地已不存在的记录
            var invalidGames = dbGames.Where(g => !validAppIds.Contains(g.AppId)).ToList();
            var invalidAccounts = dbAccounts.Where(a => !validAccounts.Contains(a.Account)).ToList();

            if (invalidGames.Count == 0 && invalidAccounts.Count == 0)
            {
                return (0, 0);
            }

            // 4. 清理数据库中的无效数据
            var invalidAppIds = invalidGames.Select(g => g.AppId).ToList();
            var invalidAccountNames = invalidAccounts.Select(a => a.Account).ToList();
            var invalidSteamIds = invalidAccounts.Select(a => a.SteamId).ToList();

            SteamGameRepository.DeleteByAppIds(invalidAppIds);
            SteamAccountGameRepository.DeleteByAppIds(invalidAppIds);
            SteamAccountRepository.DeleteByAccounts(invalidAccountNames);
            SteamAccountGameRepository.DeleteBySteamIds(invalidSteamIds);
            return (invalidAccounts.Count, invalidGames.Count);
        }

        /// <summary>
        /// 解析游戏账号和游戏
        /// </summary>
        public static void AnalyzeSteamAccountAndGame()
        {
            // 1. 刷新游戏数据 (包含从 acf 解析的 LastPlayed)
            var steamGameSources = SteamAnalyzer.GetAllGames();
            var steamGames = steamGameSources.Select(r => new SteamGame(Guid.Empty, r.AppId, r.Name, r.Icon, r.Hero, r.LastPlayed)).ToList();
            SteamGameRepository.AddList(steamGames);

            // 2. 刷新账号与游戏多对多关联及 LastPlayed (业务层清空再添加)
            SteamAccountGameRepository.Clear();
            var accountGameSources = SteamAnalyzer.GetAllAccountPlayGames();
            var accountGames = accountGameSources.Select(r => new SteamAccountGame(Guid.NewGuid(), r.SteamId, r.AppId, r.LastPlayed)).ToList();
            SteamAccountGameRepository.AddOrUpdateList(accountGames);

            // 3. 业务层统计账号拥有的本地已安装游戏数量与最新游戏时间，存入账号数据
            var installedAppIds = steamGames.Select(g => g.AppId).ToHashSet();
            var steamAccountSources = SteamAnalyzer.GetAllLoginAccounts();
            var steamAccounts = steamAccountSources.Select(r =>
            {
                var playedGames = accountGames.Where(g => g.SteamId == r.SteamId && installedAppIds.Contains(g.AppId)).ToList();
                int gameCount = playedGames.Count;
                long lastPlayed = playedGames.Count > 0 ? playedGames.Max(g => g.LastPlayed) : 0;
                return new SteamAccount(Guid.NewGuid(), r.SteamId, r.AccountName, r.PersonaName ?? r.AccountName, string.Empty, 0, r.Icon, gameCount, lastPlayed);
            }).ToList();
            SteamAccountRepository.AddList(steamAccounts);
        }
    }
}
