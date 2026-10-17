using SteamHub.Entities;
using SteamHub.Repositories;
using System.Globalization;
using System.Windows.Data;

namespace SteamHub.Convert
{
    /// <summary>
    /// 获取游戏当前展示的启动账号名称
    /// 规则：优先显示选定的账号；若未选定，则默认显示该游戏最近玩过的第一个账号名称
    /// </summary>
    public class GameToAccountNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not SteamGame game)
            {
                return string.Empty;
            }

            // 1. 如果已手动指定有效账号，直接显示该账号名称
            if (!string.IsNullOrEmpty(game.CurrentSelectAccountSteamId))
            {
                var account = SteamAccountRepository.GetBySteamId(game.CurrentSelectAccountSteamId);
                if (account != null && !string.IsNullOrEmpty(account.Name))
                {
                    return account.Name;
                }
            }

            // 2. 默认就是第一个 (按该游戏最新游玩时间倒序排在首位的账号)
            var accounts = SteamAccountRepository.GetAccountsByAppId(game.AppId);
            if (accounts != null && accounts.Count > 0)
            {
                return accounts[0].Name;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
