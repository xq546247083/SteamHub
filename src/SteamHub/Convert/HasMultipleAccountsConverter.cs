using SteamHub.Entities;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SteamHub.Convert;

/// <summary>
/// 判断是否拥有多个关联账号的可见性转换器
/// </summary>
public class HasMultipleAccountsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ICollection<SteamAccount> list)
        {
            return list.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
