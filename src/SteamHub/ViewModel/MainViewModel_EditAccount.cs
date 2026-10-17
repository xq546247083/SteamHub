using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamHub.Entities;
using SteamHub.Enums;
using SteamHub.Manager;
using SteamHub.Repositories;

namespace SteamHub.ViewModel
{
    public partial class MainViewModel
    {
        #region 绑定属性

        /// <summary>
        /// 编辑steam账号的账号
        /// </summary>
        [ObservableProperty]
        private string editSteamAccountAccount;

        /// <summary>
        /// 编辑steam账号名称
        /// </summary>
        [ObservableProperty]
        private string editSteamAccountName;

        /// <summary>
        /// 编辑steam账号密码
        /// </summary>
        [ObservableProperty]
        private string editSteamAccountPassword;

        #endregion

        #region 绑定方法

        /// <summary>
        /// 打开编辑抽屉
        /// </summary>
        [RelayCommand]
        private void OpenEditSteamAccountDrawer(SteamAccount steamAccount)
        {
            if (steamAccount == null)
            {
                return;
            }

            EditSteamAccountAccount = steamAccount.Account;
            EditSteamAccountName = steamAccount.Name;
            EditSteamAccountPassword = steamAccount.Password;

            DrawerType = DrawerType.SteamAccount;
        }

        /// <summary>
        /// 关闭抽屉
        /// </summary>
        [RelayCommand]
        private void CloseEditSteamAccountDrawer()
        {
            DrawerType = DrawerType.None;
        }

        /// <summary>
        /// 复制账号 (支持列表直接传参或抽屉当前账号)
        /// </summary>
        [RelayCommand]
        private void CopySteamAccount(string account)
        {
            var target = string.IsNullOrEmpty(account) ? EditSteamAccountAccount : account;
            if (string.IsNullOrEmpty(target))
            {
                return;
            }

            System.Windows.Clipboard.SetDataObject(target);
            Lactor.ShowToolTip($"已复制账号: {target}");
        }

        /// <summary>
        /// 复制密码 (支持列表直接传参或抽屉当前密码)
        /// </summary>
        [RelayCommand]
        private void CopySteamPassword(string password)
        {
            var target = string.IsNullOrEmpty(password) ? EditSteamAccountPassword : password;
            if (string.IsNullOrEmpty(target))
            {
                Lactor.ShowToolTip("未设置密码！");
                return;
            }

            System.Windows.Clipboard.SetDataObject(target);
            Lactor.ShowToolTip("密码已复制！");
        }

        /// <summary>
        /// 删除steam账号信息
        /// </summary>
        [RelayCommand]
        private void DeleteSteamAccount(string account)
        {
            var target = string.IsNullOrEmpty(account) ? EditSteamAccountAccount : account;
            if (string.IsNullOrEmpty(target))
            {
                return;
            }

            if (!SteamAccountRepository.Exists(target))
            {
                return;
            }

            SteamAccountRepository.Delete(target);

            DrawerType = DrawerType.None;
            Lactor.ReLoad();
            Lactor.ShowToolTip("删除成功！");
        }

        /// <summary>
        /// 保存steam账号信息
        /// </summary>
        [RelayCommand]
        private void SaveSteamAccount()
        {
            if (string.IsNullOrEmpty(EditSteamAccountAccount))
            {
                DrawerType = DrawerType.None;
                return;
            }

            if (string.IsNullOrEmpty(EditSteamAccountName))
            {
                return;
            }

            var steamAccount = SteamAccountRepository.GetByAccount(EditSteamAccountAccount);
            if (steamAccount == null)
            {
                DrawerType = DrawerType.None;
                return;
            }

            steamAccount.Name = EditSteamAccountName;
            steamAccount.Password = EditSteamAccountPassword;
            SteamAccountRepository.Update(steamAccount);

            DrawerType = DrawerType.None;
            Lactor.ReLoad();
            Lactor.ShowToolTip("保存成功！");
        }

        #endregion
    }
}