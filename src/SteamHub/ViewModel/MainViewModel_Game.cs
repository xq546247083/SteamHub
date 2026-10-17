using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamHub.Entities;
using SteamHub.Manager;
using SteamHub.Repositories;
using System.Collections.ObjectModel;

namespace SteamHub.ViewModel
{
    public partial class MainViewModel
    {
        #region 绑定属性

        /// <summary>
        /// 游戏列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<SteamGame> steamGames;

        /// <summary>
        /// 搜索游戏名称
        /// </summary>
        [ObservableProperty]
        private string searchSteamGameName;

        /// <summary>
        /// 所有游戏总数
        /// </summary>
        [ObservableProperty]
        private int totalGameCount;

        /// <summary>
        /// 当前选中账号游戏数
        /// </summary>
        [ObservableProperty]
        private int currentAccountGameCount;

        #endregion

        #region 属性变化引发的事件

        /// <summary>
        /// 搜索游戏名称改变
        /// </summary>
        partial void OnSearchSteamGameNameChanged(string value)
        {
            LoadSteamGames();
        }

        #endregion

        #region 绑定方法

        /// <summary>
        /// 打开Steam游戏 (点击卡片或启动按钮)
        /// </summary>
        /// <param name="game">游戏</param>
        [RelayCommand]
        private void OpenSteamGame(SteamGame game)
        {
            if (game == null)
            {
                return;
            }

            // 如果当前在特定账号视图下，直接使用该账号启动
            var targetAccount = string.Empty;
            if (!AppGlobalData.IsDefaultAccount(SelectedAccount))
            {
                targetAccount = SelectedAccount.Account;
            }
            else
            {
                // 在"全部游戏"视图下
                string targetSteamId = game.CurrentSelectAccountSteamId;
                if (string.IsNullOrEmpty(targetSteamId))
                {
                    var accounts = SteamAccountRepository.GetAccountsByAppId(game.AppId);
                    targetSteamId = accounts?.FirstOrDefault()?.SteamId;
                }

                if (!string.IsNullOrEmpty(targetSteamId))
                {
                    var targetAcc = SteamAccountRepository.GetBySteamId(targetSteamId);
                    targetAccount = targetAcc?.Account;
                }
            }

            SteamTool.OpenAccountAndGame(targetAccount, game.AppId);
        }

        /// <summary>
        /// 切换并指定游戏的启动账号
        /// </summary>
        /// <param name="parameter">参数数组: [0] 为 SteamGame, [1] 为 SteamAccount</param>
        [RelayCommand]
        private void SelectGameAccount(object parameter)
        {
            SteamGame game = null;
            SteamAccount account = null;

            if (parameter is object[] arr && arr.Length >= 2)
            {
                game = arr[0] as SteamGame;
                account = arr[1] as SteamAccount;
            }

            if (game == null || account == null)
            {
                return;
            }

            // 设置关联
            SteamGameRepository.UpdateCurrentSelectAccount(game.AppId, account.SteamId);
            LoadSteamGames();
            Lactor.ShowToolTip($"已设置【{game.Name}】启动账号: {account.Name}");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 加载并应用游戏过滤
        /// </summary>
        private void LoadSteamGames()
        {
            // 查询总数
            TotalGameCount = SteamGameRepository.GetAll(false)?.Count ?? 0;

            // 过滤游戏名称
            var tempGames = (AppGlobalData.IsDefaultAccount(SelectedAccount) ? SteamGameRepository.GetAll(ShowHidden) : SteamGameRepository.GetBySteamId(SelectedAccount.SteamId, ShowHidden)) ?? [];
            var query = tempGames.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchSteamGameName))
            {
                query = query.Where(g => g.Name != null && g.Name.Contains(SearchSteamGameName, StringComparison.OrdinalIgnoreCase));
            }
            SteamGames = new ObservableCollection<SteamGame>(query.ToList());
            CurrentAccountGameCount = SteamGames.Count;
        }

        #endregion
    }
}