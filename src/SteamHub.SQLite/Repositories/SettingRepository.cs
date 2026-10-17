using SteamHub.Entities;

namespace SteamHub.Repositories
{
    /// <summary>
    /// 设置仓储
    /// </summary>
    public static class SettingRepository
    {
        /// <summary>
        /// 获取配置值
        /// </summary>
        public static string GetValue(string key)
        {
            using var context = new SteamHubDbContext();
            var config = context.Settings.FirstOrDefault(c => c.Key == key);
            return config?.Value;
        }

        /// <summary>
        /// 设置配置值
        /// </summary>
        public static void SetValue(string key, string value)
        {
            using var context = new SteamHubDbContext();
            var config = context.Settings.FirstOrDefault(c => c.Key == key);

            if (config != null)
            {
                config.Value = value;
            }
            else
            {
                config = new Setting
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value
                };
                context.Settings.Add(config);
            }

            context.SaveChanges();
        }
    }
}