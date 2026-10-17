using SteamHub.Repositories;
using System.Globalization;
using System.Windows.Data;

namespace SteamHub.Convert
{
    /// <summary>
    /// 将 SteamId 实时转换为账号名称
    /// </summary>
    public class SteamIdToAccountNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var steamId = value as string;
            var defaultText = parameter as string ?? "选账号";

            if (string.IsNullOrEmpty(steamId))
            {
                return defaultText;
            }

            var account = SteamAccountRepository.GetBySteamId(steamId);
            return !string.IsNullOrEmpty(account?.Name) ? account.Name : defaultText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
