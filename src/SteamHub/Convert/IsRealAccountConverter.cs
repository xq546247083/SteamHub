using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SteamHub.Convert;

/// <summary>
/// 判断是否为真实账号转换器
/// </summary>
public class IsRealAccountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var steamId = value as string;
        bool isReal = !AppGlobalData.IsDefaultAccount(steamId);

        if (parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
        {
            isReal = !isReal;
        }

        return isReal ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
