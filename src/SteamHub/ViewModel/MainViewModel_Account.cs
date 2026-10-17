using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamHub.Entities;
using SteamHub.Helper;
using SteamHub.Manager;
using SteamHub.Repositories;
using System.Collections.ObjectModel;

namespace SteamHub.ViewModel
{
    public partial class MainViewModel
    {
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

        #endregion

        #region 属性变化引发的事件

        /// <summary>
        /// 选中账号变更
        /// </summary>
        partial void OnSelectedAccountChanged(SteamAccount oldValue, SteamAccount newValue)
        {
            LoadSteamGames();
        }

        #endregion

        #region 绑定方法

        /// <summary>
        /// 选中账号
        /// </summary>
        /// <param name="account">目标账号</param>
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
        /// 直接一键切换Steam登录账号
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
            LoadSteamAccounts();
            Lactor.ShowToolTip($"已切换至账号: {account.Name}");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 加载steam账号
        /// </summary>
        private void LoadSteamAccounts()
        {
            var previousSelectedSteamId = SelectedAccount?.SteamId;

            // 获取账号列表
            var accounts = SteamAccountRepository.GetAll(ShowHidden);
            var list = new List<SteamAccount> { AppGlobalData.DefaultSteamAccount };
            if (accounts != null)
            {
                list.AddRange(accounts);
            }
            SteamAccounts = new ObservableCollection<SteamAccount>(list);

            // 过滤数据
            if (!string.IsNullOrEmpty(previousSelectedSteamId))
            {
                var matched = SteamAccounts.FirstOrDefault(a => a.SteamId == previousSelectedSteamId);
                SelectedAccount = matched ?? AppGlobalData.DefaultSteamAccount;
            }
            else
            {
                SelectedAccount = AppGlobalData.DefaultSteamAccount;
            }
        }

        #endregion
    }
}