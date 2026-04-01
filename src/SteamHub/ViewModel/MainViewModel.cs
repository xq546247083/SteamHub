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
            // 转换参数
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
            // 转换参数
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
            // 刷新账号数据
            var steamAccountSources = SteamAnalyzer.GetAllLoginAccounts();
            var steamAccounts = steamAccountSources.Select(r => new SteamAccount(Guid.NewGuid(), r.SteamId, r.AccountName, r.PersonaName ?? r.AccountName, string.Empty, 0, r.Icon)).ToList();
            SteamAccountRepository.AddList(steamAccounts);

            // 刷新游戏数据
            var steamGameSources = SteamAnalyzer.GetAllGames();
            var steamGames = steamGameSources.Select(r => new SteamGame(Guid.Empty, r.AppId, r.Name, r.Icon, r.AccountSteamId, 0)).ToList();
            SteamGameRepository.AddList(steamGames);
            SteamGameRepository.UpdateAccountSteamIds(steamGames);

            Lactor.ReLoad();
            Lactor.ShowToolTip("刷新成功!");
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

        #region 公共方法

        /// <summary>
        /// 加载所有数据
        /// </summary>
        public void LoadAllData()
        {
            LoadSteamAccounts();
            LoadSteamGames();
            LoadProcesses();
            LoadDeletedData();
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
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = System.Windows.DragDropEffects.Move;
            }
            else if (dropInfo.Data is SteamGame && dropInfo.TargetItem is SteamGame)
            {
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
                int oldIndex = SteamAccounts.IndexOf(sourceAccount);
                int newIndex = SteamAccounts.IndexOf(targetAccount);
                if (oldIndex != -1 && newIndex != -1)
                {
                    SteamAccounts.Move(oldIndex, newIndex);
                    for (int i = 0; i < SteamAccounts.Count; i++)
                    {
                        SteamAccounts[i].Order = i;
                    }

                    SteamAccountRepository.UpdateList(SteamAccounts.ToList());
                    Lactor.TrayPopupViewModel.ReLoad();
                }
            }
            else if (dropInfo.Data is SteamGame sourceGame && dropInfo.TargetItem is SteamGame targetGame)
            {
                int oldIndex = SteamGames.IndexOf(sourceGame);
                int newIndex = SteamGames.IndexOf(targetGame);
                if (oldIndex != -1 && newIndex != -1)
                {
                    SteamGames.Move(oldIndex, newIndex);
                    for (int i = 0; i < SteamGames.Count; i++)
                    {
                        SteamGames[i].Order = i;
                    }

                    SteamGameRepository.UpdateList(SteamGames.ToList());
                    Lactor.TrayPopupViewModel.ReLoad();
                }
            }
        }

        #endregion
    }
}