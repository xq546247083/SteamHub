using System.Globalization;
using System.Windows.Data;

namespace SteamHub.Convert
{
    /// <summary>
    /// 将多个绑定参数合并为 object 数组传递给 CommandParameter
    /// </summary>
    public class ArrayMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return null;
            }

            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
