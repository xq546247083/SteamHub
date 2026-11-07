using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SteamHub.Convert;

public class ByteToImageSourceConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, BitmapImage> _imageCache = new ConcurrentDictionary<string, BitmapImage>();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte[] bytes && bytes.Length > 0)
        {
            try
            {
                // Calculate hash of the byte array to use as cache key
                string hashKey = ConvertToHashString(bytes);

                if (_imageCache.TryGetValue(hashKey, out var cachedImage))
                {
                    return cachedImage;
                }

                var image = new BitmapImage();
                using (var memorystream = new MemoryStream(bytes))
                {
                    memorystream.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = memorystream;
                    image.EndInit();
                }
                image.Freeze();

                _imageCache[hashKey] = image;

                return image;
            }
            catch
            {
                // Return null or a default image if conversion fails
                return null;
            }
        }
        return null;
    }

    private static string ConvertToHashString(byte[] bytes)
    {
        var hashBytes = SHA256.HashData(bytes);
        return System.Convert.ToBase64String(hashBytes);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
