using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamHub.Entities;
using SteamHub.Helper;
using SteamHub.Manager;
using SteamHub.Repositories;
using System.Collections.ObjectModel;

namespace SteamHub.ViewModel
{
    /// <summary>
    /// 托盘 ViewModel
    /// </summary>
    public partial class TrayPopupViewModel : ObservableObject
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public TrayPopupViewModel()
        {
            ReLoad();
        }

        #region 绑定属性

        /// <summary>
        /// 账号列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<SteamAccount> steamAccounts;

        /// <summary>
        /// 当前选中的账号
        /// </summary>
        [ObservableProperty]
        private SteamAccount selectedAccount;

        /// <summary>
        /// 右侧显示的游戏列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<SteamGame> displayGames;

        /// <summary>
        /// 是否开机自启动
        /// </summary>
        [ObservableProperty]
        private bool isLaunchOnSysPowerOn;

        /// <summary>
        /// 所有游戏总数
        /// </summary>
        [ObservableProperty]
        private int totalGameCount;

        #endregion

        #region 绑定方法

        /// <summary>
        /// 登录新账号
        /// </summary>
        [RelayCommand]
        private void LoginNewSteamAccount()
        {
            SteamTool.OpenAccount(string.Empty);
            NotifyIconManager.Close();
        }

        /// <summary>
        /// 选中账号
        /// </summary>
        [RelayCommand]
        private void SelectSteamAccount(SteamAccount account)
        {
            if (account == null)
            {
                return;
            }

            SelectedAccount = account;
        }

        /// <summary>
        /// 切换Steam账号
        /// </summary>
        /// <param name="account">目标账号</param>
        [RelayCommand]
        private void SwitchSteamAccount(SteamAccount account)
        {
            if (AppGlobalData.IsDefaultAccount(account))
            {
                return;
            }

            SteamTool.OpenAccount(account.Account);
            NotifyIconManager.Close();
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

            // 存入数据库
            SteamGameRepository.UpdateCurrentSelectAccount(game.AppId, account.SteamId);
            DisplayGames = new ObservableCollection<SteamGame>(AppGlobalData.IsDefaultAccount(SelectedAccount) ? SteamGameRepository.GetAll() : SteamGameRepository.GetBySteamId(SelectedAccount.SteamId) ?? []);
        }

        /// <summary>
        /// 打开Steam游戏
        /// </summary>
        /// <param name="game">目标游戏</param>
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
                var targetSteamId = game.CurrentSelectAccountSteamId;
                if (string.IsNullOrEmpty(targetSteamId))
                {
                    var accounts = SteamAccountRepository.GetAccountsByAppId(game.AppId);
                    targetSteamId = accounts?.FirstOrDefault()?.SteamId;
                }

                if (!string.IsNullOrEmpty(targetSteamId))
                {
                    var acc = SteamAccountRepository.GetBySteamId(targetSteamId);
                    targetAccount = acc?.Account;
                }
            }

            SteamTool.OpenAccountAndGame(targetAccount, game.AppId);
            NotifyIconManager.Close();
        }

        /// <summary>
        /// 切换自启动
        /// </summary>
        [RelayCommand]
        private void ToggleAutoStart()
        {
            var targetState = IsLaunchOnSysPowerOn;
            if (targetState)
            {
                IsLaunchOnSysPowerOn = TaskSchedulerHelper.AddLuanchTask(AppGlobal.AppName, Application.ExecutablePath);
            }
            else
            {
                TaskSchedulerHelper.Delete(AppGlobal.AppName);
            }
        }

        /// <summary>
        /// 打开主窗口
        /// </summary>
        [RelayCommand]
        private void OpenMainWindow()
        {
            Lactor.OpenMainWindow();
            NotifyIconManager.Close();
        }

        /// <summary>
        /// 退出程序
        /// </summary>
        [RelayCommand]
        private void Exit()
        {
            Environment.Exit(0);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 加载数据
        /// </summary>
        public void ReLoad()
        {
            var previousSelectedSteamId = SelectedAccount?.SteamId;

            // 加载账号
            var accounts = SteamAccountRepository.GetAll();
            var list = new List<SteamAccount> { AppGlobalData.DefaultSteamAccount };
            if (accounts != null)
            {
                list.AddRange(accounts);
            }
            SteamAccounts = new ObservableCollection<SteamAccount>(list);

            // 选中账号
            if (!string.IsNullOrEmpty(previousSelectedSteamId))
            {
                var matched = SteamAccounts.FirstOrDefault(a => a.SteamId == previousSelectedSteamId);
                SelectedAccount = matched ?? AppGlobalData.DefaultSteamAccount;
            }
            else
            {
                SelectedAccount = AppGlobalData.DefaultSteamAccount;
            }

            // 加载游戏
            TotalGameCount = SteamGameRepository.GetAll()?.Count ?? 0;
            DisplayGames = new ObservableCollection<SteamGame>(AppGlobalData.IsDefaultAccount(SelectedAccount) ? SteamGameRepository.GetAll() : SteamGameRepository.GetBySteamId(SelectedAccount.SteamId) ?? []);

            // 加载自启动状态
            IsLaunchOnSysPowerOn = TaskSchedulerHelper.Get(AppGlobal.AppName) != null;
        }

        #endregion
    }
}