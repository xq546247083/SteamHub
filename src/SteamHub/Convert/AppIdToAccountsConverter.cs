using SteamHub.Repositories;
using System.Globalization;
using System.Windows.Data;

namespace SteamHub.Convert
{
    /// <summary>
    /// 根据游戏 AppId 实时查询拥有该游戏的所有关联账号列表 (按该游戏最新游玩时间倒序)
    /// </summary>
    public class AppIdToAccountsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var appId = value as string;
            if (string.IsNullOrEmpty(appId))
            {
                return null;
            }

            return SteamAccountRepository.GetAccountsByAppId(appId);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
