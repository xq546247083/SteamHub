using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using MaterialDesignThemes.Wpf;
using SteamHub.Common;
using SteamHub.Entities;
using SteamHub.Enums;
using SteamHub.Manager;
using SteamHub.Repositories;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace SteamHub.ViewModel
{
    /// <summary>
    /// 主界面的ViewModel
    /// </summary>
    public partial class MainViewModel : ObservableObject, GongSolutions.Wpf.DragDrop.IDropTarget
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public MainViewModel()
        {
            AppTitle = $"{AppGlobal.AppChineseName} V{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";
            LoadAllData();
        }

        #region 绑定属性

        /// <summary>
        /// 抽屉类型
        /// </summary>
        [ObservableProperty]
        private DrawerType drawerType;

        /// <summary>
        /// 程序名字
        /// </summary>
        [ObservableProperty]
        private string appTitle;

        /// <summary>
        /// 是否显示隐藏
        /// </summary>
        [ObservableProperty]
        private bool showHidden;

        /// <summary>
        /// Snackbar消息队列
        /// </summary>
        public SnackbarMessageQueue SnackbarMessageQueue
        {
            get;
        } = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));

        #endregion

        #region 绑定方法

        /// <summary>
        /// 按键
        /// </summary>
        /// <param name="commandParameterEx">参数</param>
        [RelayCommand]
        private void MainWindowKeyDown(CommandParameterEx commandParameterEx)
        {
            var keyEventArgs = commandParameterEx.EventArgs as System.Windows.Input.KeyEventArgs;
            if (keyEventArgs == null)
            {
                return;
            }

            switch (keyEventArgs.Key)
            {
                case Key.F1:
                    break;
            }
        }

        /// <summary>
        /// 正在关闭窗口
        /// </summary>
        /// <param name="commandParameterEx">参数</param>
        [RelayCommand]
        private void MainWindowClosing(CommandParameterEx commandParameterEx)
        {
            var cancelEventArgs = commandParameterEx.EventArgs as CancelEventArgs;
            if (cancelEventArgs == null)
            {
                return;
            }

            Lactor.MainWindow.Visibility = System.Windows.Visibility.Hidden;
            Lactor.MainWindow.ShowInTaskbar = false;
            cancelEventArgs.Cancel = true;
        }

        /// <summary>
        /// 刷新 Steam 数据
        /// </summary>
        [RelayCommand]
        private void Refresh()
        {
            ReAnalyzer();
            Lactor.ReLoad();
        }

        /// <summary>
        /// 一键清理本地已不存在的账号和游戏
        /// </summary>
        [RelayCommand]
        private void CleanUp()
        {
            // 1. 获取本地真实存在的游戏与登录账号
            var steamGameSources = SteamAnalyzer.GetAllGames();
            var validAppIds = steamGameSources.Select(g => g.AppId).ToHashSet();

            var steamAccountSources = SteamAnalyzer.GetAllLoginAccounts();
            var validAccounts = steamAccountSources.Select(a => a.AccountName).ToHashSet();
            var validSteamIds = steamAccountSources.Select(a => a.SteamId).ToHashSet();

            // 2. 查询数据库中的所有数据 (包括已隐藏的)
            var dbGames = SteamGameRepository.GetAll(includeHidden: true);
            var dbAccounts = SteamAccountRepository.GetAll(includeHidden: true);

            // 3. 找出本地已不存在的记录
            var invalidGames = dbGames.Where(g => !validAppIds.Contains(g.AppId)).ToList();
            var invalidAccounts = dbAccounts.Where(a => !validAccounts.Contains(a.Account)).ToList();

            if (invalidGames.Count == 0 && invalidAccounts.Count == 0)
            {
                Lactor.ShowToolTip("本地账号与游戏均有效，无需清理！");
                return;
            }

            // 4. 清理数据库中的无效数据
            var invalidAppIds = invalidGames.Select(g => g.AppId).ToList();
            var invalidAccountNames = invalidAccounts.Select(a => a.Account).ToList();
            var invalidSteamIds = invalidAccounts.Select(a => a.SteamId).ToList();

            SteamGameRepository.DeleteByAppIds(invalidAppIds);
            SteamAccountGameRepository.DeleteByAppIds(invalidAppIds);
            SteamAccountRepository.DeleteByAccounts(invalidAccountNames);
            SteamAccountGameRepository.DeleteBySteamIds(invalidSteamIds);

            // 5. 关闭抽屉，重新分析与重载界面
            DrawerType = DrawerType.None;
            ReAnalyzer();
            Lactor.ReLoad();

            Lactor.ShowToolTip($"清理完成！已移除 {invalidGames.Count} 个失效游戏，{invalidAccounts.Count} 个失效账号。");
        }

        /// <summary>
        /// 打开 GitHub 仓库
        /// </summary>
        [RelayCommand]
        private void OpenGitHub()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/xq546247083/SteamHub",
                UseShellExecute = true
            });
        }

        #endregion

        #region 属性变化引发的事件

        /// <summary>
        /// 显示已隐藏游戏状态改变
        /// </summary>
        partial void OnShowHiddenChanged(bool value)
        {
            LoadSteamAccounts();
            LoadSteamGames();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 解析数据
        /// </summary>
        public void ReAnalyzer()
        {
            // 1. 刷新游戏数据 (包含从 acf 解析的 LastPlayed)
            var steamGameSources = SteamAnalyzer.GetAllGames();
            var steamGames = steamGameSources.Select(r => new SteamGame(Guid.Empty, r.AppId, r.Name, r.Icon, r.Hero, r.LastPlayed)).ToList();
            SteamGameRepository.AddList(steamGames);

            // 2. 刷新账号与游戏多对多关联及 LastPlayed (业务层清空再添加)
            SteamAccountGameRepository.Clear();
            var accountGameSources = SteamAnalyzer.GetAllAccountPlayGames();
            var accountGames = accountGameSources.Select(r => new SteamAccountGame(Guid.NewGuid(), r.SteamId, r.AppId, r.LastPlayed)).ToList();
            SteamAccountGameRepository.AddOrUpdateList(accountGames);

            // 3. 业务层统计账号拥有的本地已安装游戏数量与最新游戏时间，存入账号数据
            var installedAppIds = steamGames.Select(g => g.AppId).ToHashSet();
            var steamAccountSources = SteamAnalyzer.GetAllLoginAccounts();
            var steamAccounts = steamAccountSources.Select(r =>
            {
                var playedGames = accountGames.Where(g => g.SteamId == r.SteamId && installedAppIds.Contains(g.AppId)).ToList();
                int gameCount = playedGames.Count;
                long lastPlayed = playedGames.Count > 0 ? playedGames.Max(g => g.LastPlayed) : 0;
                return new SteamAccount(Guid.NewGuid(), r.SteamId, r.AccountName, r.PersonaName ?? r.AccountName, string.Empty, 0, r.Icon, gameCount, lastPlayed);
            }).ToList();
            SteamAccountRepository.AddList(steamAccounts);
        }

        /// <summary>
        /// 加载所有数据
        /// </summary>
        public void LoadAllData()
        {
            LoadSteamAccounts();
            LoadSteamGames();
        }

        #endregion

        #region 拖拽

        /// <summary>
        /// 拖拽
        /// </summary>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data == dropInfo.TargetItem)
            {
                return;
            }

            if (dropInfo.Data is SteamAccount && dropInfo.TargetItem is SteamAccount)
            {
                var source = dropInfo.Data as SteamAccount;
                var target = dropInfo.TargetItem as SteamAccount;
                if (AppGlobalData.IsDefaultAccount(source) || AppGlobalData.IsDefaultAccount(target))
                {
                    return;
                }

                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = System.Windows.DragDropEffects.Move;
            }
        }

        /// <summary>
        /// 拖拽放下
        /// </summary>
        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is SteamAccount sourceAccount && dropInfo.TargetItem is SteamAccount targetAccount)
            {
                if (AppGlobalData.IsDefaultAccount(sourceAccount) || AppGlobalData.IsDefaultAccount(targetAccount))
                {
                    return;
                }

                int oldIndex = SteamAccounts.IndexOf(sourceAccount);
                int newIndex = SteamAccounts.IndexOf(targetAccount);
                if (oldIndex != -1 && newIndex != -1)
                {
                    SteamAccounts.Move(oldIndex, newIndex);
                    var realAccounts = SteamAccounts.Where(a => !AppGlobalData.IsDefaultAccount(a)).ToList();
                    for (int i = 0; i < realAccounts.Count; i++)
                    {
                        realAccounts[i].Order = i;
                    }

                    SteamAccountRepository.UpdateList(realAccounts);
                    Lactor.TrayPopupViewModel.ReLoad();
                }
            }
        }

        #endregion
    }
}