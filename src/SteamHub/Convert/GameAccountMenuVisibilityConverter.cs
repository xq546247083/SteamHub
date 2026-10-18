using SteamHub.Repositories;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SteamHub.Convert
{
    /// <summary>
    /// 控制游戏卡片账号下拉菜单的可见性
    /// 规则：仅在"所有游戏"视图下，且该游戏拥有关联账号时才显示；如果没有关联账号，则不显示下拉
    /// </summary>
    public class GameAccountMenuVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return Visibility.Collapsed;
            }

            // 1. 判断是否处于"所有游戏"视图 (直接根据选中的账号判断)
            bool isAllGames = false;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue)
            {
                isAllGames = true;
            }
            else if (values[0] is Entities.SteamAccount account)
            {
                isAllGames = AppData.IsDefaultAccount(account);
            }
            else if (values[0] is bool boolVal)
            {
                isAllGames = boolVal;
            }

            if (!isAllGames)
            {
                return Visibility.Collapsed;
            }

            // 2. 游戏 AppId
            var appId = values[1] as string;
            if (string.IsNullOrEmpty(appId))
            {
                return Visibility.Collapsed;
            }

            // 3. 如果该游戏没有关联账号，不显示下拉
            var accounts = SteamAccountRepository.GetAccountsByAppId(appId);
            return (accounts != null && accounts.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
