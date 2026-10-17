using SteamHub.Model;
using System.Text.RegularExpressions;

namespace SteamHub;

/// <summary>
/// Steam数据分析器,用于从本地文件解析 Steam 账号和游戏数据
/// </summary>
public static class SteamAnalyzer
{
    /// <summary>
    /// 扫描游戏，跳过的游戏AppId列表
    /// </summary>
    private static List<string> skipGameAppIds = new List<string>()
    {
        "228980"
    };

    #region 公共方法

    /// <summary>
    /// 获取登录用户列表
    /// </summary>
    /// <returns></returns>
    public static List<SteamAccountSource> GetAllLoginAccounts()
    {
        var accounts = new List<SteamAccountSource>();

        var steamPath = SteamTool.GetSteamPath();
        var loginUsersPath = Path.Combine(steamPath, "config", "loginusers.vdf");
        if (!File.Exists(loginUsersPath))
        {
            return accounts;
        }

        var vdfContent = File.ReadAllText(loginUsersPath);

        // 使用正则表达式提取每个用户块
        var userPattern = @"""(\d{17})""[\s\S]*?{([\s\S]*?)}";
        var matches = Regex.Matches(vdfContent, userPattern);

        foreach (Match match in matches)
        {
            var steamId = match.Groups[1].Value;
            var userBlock = match.Groups[2].Value;

            var account = new SteamAccountSource
            {
                SteamId = steamId,
                AccountName = ExtractValue(userBlock, "AccountName"),
                PersonaName = ExtractValue(userBlock, "PersonaName"),
                Icon = LoadAccountAvatar(steamPath, steamId)
            };

            if (!string.IsNullOrEmpty(account.AccountName))
            {
                accounts.Add(account);
            }
        }

        return accounts;
    }

    /// <summary>
    /// 解析所有游戏清单文件,获取游戏信息
    /// </summary>
    /// <returns>游戏列表</returns>
    public static List<SteamGameSource> GetAllGames()
    {
        var games = new List<SteamGameSource>();

        // 获取所有库路径
        var libraryPaths = GetLibraryPaths();
        if (libraryPaths.Count == 0)
        {
            return games;
        }

        // 遍历每个库路径,扫描游戏清单文件
        foreach (var libraryPath in libraryPaths)
        {
            var steamAppsPath = Path.Combine(libraryPath, "steamapps");
            if (!Directory.Exists(steamAppsPath))
            {
                continue;
            }

            var manifestFiles = Directory.GetFiles(steamAppsPath, "appmanifest_*.acf");
            foreach (var manifestFile in manifestFiles)
            {
                var game = LoadGameSource(manifestFile);
                if (game != null && !string.IsNullOrEmpty(game.AppId) && !skipGameAppIds.Contains(game.AppId))
                {
                    games.Add(game);
                }
            }
        }

        return games;
    }

    /// <summary>
    /// 获取所有的游戏图标
    /// </summary>
    public static List<byte[]> GetAllGameIcons()
    {
        var result = new List<byte[]>();

        var steamPath = SteamTool.GetSteamPath();
        if (string.IsNullOrEmpty(steamPath))
        {
            return result;
        }

        // 获取图标路径列表
        var gameIconPaths = new List<string>();
        var steamGamesPath = Path.Combine(steamPath, "steam", "games");
        if (Directory.Exists(steamGamesPath))
        {
            gameIconPaths.AddRange(Directory.GetFiles(steamGamesPath, "*.ico"));
        }

        // 读取图标
        foreach (var iconPath in gameIconPaths)
        {
            try
            {
                var iconBytes = File.ReadAllBytes(iconPath);
                result.Add(iconBytes);
            }
            catch
            {
            }
        }

        return result;
    }

    /// <summary>
    /// 获取所有的账号图标
    /// </summary>
    public static List<byte[]> GetAllAccountIcons()
    {
        var result = new List<byte[]>();

        var steamPath = SteamTool.GetSteamPath();
        if (string.IsNullOrEmpty(steamPath))
        {
            return result;
        }

        // 获取图标路径列表
        var gameIconPaths = new List<string>();
        var steamGamesPath = Path.Combine(steamPath, "config", "avatarcache");
        if (Directory.Exists(steamGamesPath))
        {
            gameIconPaths.AddRange(Directory.GetFiles(steamGamesPath, "*.png"));
        }

        // 读取图标
        foreach (var iconPath in gameIconPaths)
        {
            try
            {
                var iconBytes = File.ReadAllBytes(iconPath);
                result.Add(iconBytes);
            }
            catch
            {
            }
        }

        return result;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 从 libraryfolders.vdf 中获取所有库路径
    /// </summary>
    /// <returns>库路径列表</returns>
    private static List<string> GetLibraryPaths()
    {
        var paths = new List<string>();

        var steamPath = SteamTool.GetSteamPath();
        var libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libraryFoldersPath))
        {
            return paths;
        }

        var vdfContent = File.ReadAllText(libraryFoldersPath);

        var pathPattern = @"""path""\s+""([^""]+)""";
        var matches = Regex.Matches(vdfContent, pathPattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var path = match.Groups[1].Value;
            path = path.Replace("\\\\", "\\");

            if (Directory.Exists(path))
            {
                paths.Add(path);
            }
        }

        return paths;
    }

    /// <summary>
    /// 获取所有账号玩过的游戏关联信息
    /// </summary>
    /// <returns>关联信息列表</returns>
    public static List<SteamAccountPlayGameSource> GetAllAccountPlayGames()
    {
        var result = new List<SteamAccountPlayGameSource>();

        var steamPath = SteamTool.GetSteamPath();
        var userDataPath = Path.Combine(steamPath, "userdata");
        if (!Directory.Exists(userDataPath))
        {
            return result;
        }

        var userDirs = Directory.GetDirectories(userDataPath);
        foreach (var userDir in userDirs)
        {
            var steamId3Str = new DirectoryInfo(userDir).Name;
            if (long.TryParse(steamId3Str, out var steamId3))
            {
                var steamId64 = (steamId3 + 76561197960265728).ToString();
                var localConfigPath = Path.Combine(userDir, "config", "localconfig.vdf");

                if (File.Exists(localConfigPath))
                {
                    var singleSteamAccountPlayGames = LoadSingleAccountPlayGames(steamId64, localConfigPath);
                    if (singleSteamAccountPlayGames != null && singleSteamAccountPlayGames.Count > 0)
                    {
                        result.AddRange(singleSteamAccountPlayGames);
                    }
                }
            }
        }

        return result.OrderBy(r => r.LastPlayed).ToList();
    }

    /// <summary>
    /// 解析 localconfig.vdf 获取用户应用列表
    /// </summary>
    private static List<SteamAccountPlayGameSource> LoadSingleAccountPlayGames(string steamId, string localConfigPath)
    {
        var result = new List<SteamAccountPlayGameSource>();
        try
        {
            var content = File.ReadAllText(localConfigPath);
            // 匹配 "apps" 块 (使用平衡组处理嵌套的大括号)
            var appsBlockMatch = Regex.Match(content, @"""apps""\s*\{((?>[^{}]+|\{(?<DEPTH>)|\}(?<-DEPTH>))*(?(DEPTH)(?!)))\}", RegexOptions.IgnoreCase);
            if (appsBlockMatch.Success)
            {
                var appsBlock = appsBlockMatch.Groups[1].Value;
                // 匹配每个 AppId 块: "AppId" { ... }
                var appPattern = @"""(\d+)""\s*\{([\s\S]*?)\}";
                var matches = Regex.Matches(appsBlock, appPattern);

                foreach (Match match in matches)
                {
                    var appId = match.Groups[1].Value;
                    var appContent = match.Groups[2].Value;

                    var lastPlayedMatch = Regex.Match(appContent, @"""LastPlayed""\s+""(\d+)""", RegexOptions.IgnoreCase);
                    long lastPlayed = 0;
                    if (lastPlayedMatch.Success)
                    {
                        long.TryParse(lastPlayedMatch.Groups[1].Value, out lastPlayed);
                    }

                    if (!result.Any(x => x.AppId == appId))
                    {
                        result.Add(new SteamAccountPlayGameSource(steamId, appId, lastPlayed));
                    }
                }
            }
        }
        catch
        {
        }

        return result;
    }

    /// <summary>
    /// 解析单个 appmanifest_*.acf 文件
    /// </summary>
    /// <param name="manifestPath">清单文件路径</param>
    /// <param name="steamPath">Steam 安装路径</param>
    /// <returns>游戏信息</returns>
    private static SteamGameSource LoadGameSource(string manifestPath)
    {
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        var vdfContent = File.ReadAllText(manifestPath);
        var appId = ExtractValue(vdfContent, "appid");
        var name = ExtractValue(vdfContent, "name");
        var lastPlayedStr = ExtractValue(vdfContent, "LastPlayed");
        long.TryParse(lastPlayedStr, out var lastPlayed);

        var (icon, hero) = LoadGameImages(appId);

        return new SteamGameSource
        {
            AppId = appId,
            Name = name,
            Icon = icon,
            Hero = hero,
            LastPlayed = lastPlayed
        };
    }

    /// <summary>
    /// 自动从 Steam 本地缓存中加载游戏图标与大图
    /// </summary>
    /// <param name="appId">游戏 AppId</param>
    /// <returns>游戏图标 Icon 与大图 Hero</returns>
    private static (byte[] Icon, byte[] Hero) LoadGameImages(string appId)
    {
        var icon = Array.Empty<byte>();
        var hero = Array.Empty<byte>();
        try
        {
            var steamPath = SteamTool.GetSteamPath();
            if (string.IsNullOrEmpty(steamPath) || string.IsNullOrEmpty(appId))
            {
                return (icon, hero);
            }

            var libraryCacheDir = Path.Combine(steamPath, "appcache", "librarycache", appId);
            if (Directory.Exists(libraryCacheDir))
            {
                var files = Directory.GetFiles(libraryCacheDir, "*.*", SearchOption.AllDirectories);

                // 1. 查找游戏图标 (40位哈希)
                var iconFile = files.FirstOrDefault(f =>
                {
                    var fileName = Path.GetFileName(f);
                    return Regex.IsMatch(fileName, @"^[0-9a-fA-F]{40}(\.jpg|\.png|\.ico)?$");
                });

                if (iconFile != null)
                {
                    // 优先从 steam/games/<hash>.ico 中读取高清矢量图标
                    var hash = Path.GetFileNameWithoutExtension(iconFile);
                    var icoPath = Path.Combine(steamPath, "steam", "games", $"{hash}.ico");
                    if (File.Exists(icoPath))
                    {
                        icon = File.ReadAllBytes(icoPath);
                    }
                    else if (File.Exists(iconFile))
                    {
                        icon = File.ReadAllBytes(iconFile);
                    }
                }

                // 2. 查找游戏大图
                // 优先级 1: 竖版海报图 (library_600x900 / library_capsule，最适配 160x245 卡片比例)
                var heroFile = files.FirstOrDefault(f => Path.GetFileName(f).Equals("library_600x900_schinese.jpg", StringComparison.OrdinalIgnoreCase))
                    ?? files.FirstOrDefault(f => Path.GetFileName(f).Equals("library_capsule_schinese.jpg", StringComparison.OrdinalIgnoreCase))
                    ?? files.FirstOrDefault(f => Path.GetFileName(f).Equals("library_600x900.jpg", StringComparison.OrdinalIgnoreCase))
                    ?? files.FirstOrDefault(f => Path.GetFileName(f).Equals("library_capsule.jpg", StringComparison.OrdinalIgnoreCase))
                    ?? files.FirstOrDefault(f =>
                    {
                        var fileName = Path.GetFileName(f);
                        return (fileName.Contains("600x900", StringComparison.OrdinalIgnoreCase) || fileName.Contains("library_capsule", StringComparison.OrdinalIgnoreCase))
                            && (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
                    })
                    // 优先级 2: 宽屏大图 (library_hero)
                    ?? files.FirstOrDefault(f => Path.GetFileName(f).Equals("library_hero_schinese.jpg", StringComparison.OrdinalIgnoreCase))
                    ?? files.FirstOrDefault(f => Path.GetFileName(f).Equals("library_hero.jpg", StringComparison.OrdinalIgnoreCase))
                    ?? files.FirstOrDefault(f =>
                    {
                        var fileName = Path.GetFileName(f);
                        return fileName.Contains("library_hero", StringComparison.OrdinalIgnoreCase)
                            && !fileName.Contains("blur", StringComparison.OrdinalIgnoreCase)
                            && (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
                    })
                    // 优先级 3: 页眉横幅 (header / library_header)
                    ?? files.FirstOrDefault(f => Path.GetFileName(f).Equals("library_header_schinese.jpg", StringComparison.OrdinalIgnoreCase))
                    ?? files.FirstOrDefault(f => Path.GetFileName(f).Equals("header_schinese.jpg", StringComparison.OrdinalIgnoreCase))
                    ?? files.FirstOrDefault(f => Path.GetFileName(f).Equals("library_header.jpg", StringComparison.OrdinalIgnoreCase))
                    ?? files.FirstOrDefault(f => Path.GetFileName(f).Equals("header.jpg", StringComparison.OrdinalIgnoreCase));

                if (heroFile != null && File.Exists(heroFile))
                {
                    hero = File.ReadAllBytes(heroFile);
                }
            }
        }
        catch
        {
        }

        // 兜底：若未获取到大图，则降级使用游戏图标
        if ((hero == null || hero.Length == 0) && icon != null && icon.Length > 0)
        {
            hero = icon;
        }

        return (icon, hero);
    }

    /// <summary>
    /// 加载用户头像
    /// </summary>
    /// <param name="steamPath">Steam 安装路径</param>
    /// <param name="steamId">Steam64 ID</param>
    /// <returns>头像数据</returns>
    private static byte[] LoadAccountAvatar(string steamPath, string steamId)
    {
        var avatarPath = Path.Combine(steamPath, "config", "avatarcache", $"{steamId}.png");
        if (File.Exists(avatarPath))
        {
            return File.ReadAllBytes(avatarPath);
        }

        return null;
    }

    /// <summary>
    /// 从 VDF 内容中提取指定键的值
    /// </summary>
    private static string ExtractValue(string vdfContent, string key)
    {
        var pattern = $@"""{key}""\s+""([^""]*)""";
        var match = Regex.Match(vdfContent, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    #endregion
}