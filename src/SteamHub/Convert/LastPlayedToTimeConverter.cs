using System.Globalization;
using System.Windows.Data;

namespace SteamHub.Convert;

/// <summary>
/// 上次游玩时间戳转换为友好文本转换器
/// </summary>
public class LastPlayedToTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long timestamp && timestamp > 0)
        {
            var playTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().DateTime;
            var now = DateTime.Now;
            var diff = now - playTime;

            if (diff.TotalMinutes < 5)
            {
                return "刚刚运行";
            }
            if (diff.TotalHours < 1)
            {
                return $"{(int)diff.TotalMinutes} 分钟前";
            }
            if (playTime.Date == now.Date)
            {
                return $"今天 {playTime:HH:mm}";
            }
            if (playTime.Date == now.Date.AddDays(-1))
            {
                return $"昨天 {playTime:HH:mm}";
            }
            if (diff.TotalDays < 7)
            {
                return $"{(int)diff.TotalDays} 天前";
            }
            if (playTime.Year == now.Year)
            {
                return playTime.ToString("MM月dd日");
            }

            return playTime.ToString("yyyy/MM/dd");
        }

        return "从未运行";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
