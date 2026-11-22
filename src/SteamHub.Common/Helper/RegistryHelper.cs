using Microsoft.Win32;

namespace SteamHub.Helper
{
    /// <summary>
    /// 注册表帮助类
    /// </summary>
    public static class RegistryHelper
    {
        /// <summary>
        /// 设置key
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Set(string keyPath, string key, object value, RegistryKey topKey = null)
        {
            if (topKey == null)
            {
                topKey = Registry.CurrentUser;
            }

            using (var rk = topKey.OpenSubKey(keyPath, true))
            {
                if (rk == null)
                {
                    return false;
                }

                rk.SetValue(key, value);
                return true;
            }
        }

        /// <summary>
        /// 删除注册表信息
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Delete(string keyPath, string key, RegistryKey topKey = null)
        {
            if (topKey == null)
            {
                topKey = Registry.CurrentUser;
            }

            using (var rk = topKey.OpenSubKey(keyPath, true))
            {
                if (rk == null)
                {
                    return true;
                }

                rk.DeleteValue(key, false);
                return true;
            }
        }

        /// <summary>
        /// 获取注册表信息
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static (bool, object) Get(string keyPath, string key, RegistryKey topKey = null)
        {
            if (topKey == null)
            {
                topKey = Registry.CurrentUser;
            }

            using (var rk = topKey.OpenSubKey(keyPath, true))
            {
                if (rk == null)
                {
                    return (false, string.Empty);
                }

                var obj = rk.GetValue(key);
                if (obj == null)
                {
                    return (false, string.Empty);
                }

                return (true, obj);
            }
        }
    }
}