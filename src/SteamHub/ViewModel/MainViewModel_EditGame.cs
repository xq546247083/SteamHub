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
        private string editSteamGameAppId;

        #region 绑定属性

        /// <summary>
        /// 编辑steam游戏名称
        /// </summary>
        [ObservableProperty]
        private string editSteamGameName;

        #endregion

        #region 绑定方法

        /// <summary>
        /// 打开编辑游戏抽屉
        /// </summary>
        [RelayCommand]
        private void OpenEditSteamGameDrawer(SteamGame game)
        {
            if (game == null)
            {
                return;
            }

            EditSteamGameName = game.Name;
            editSteamGameAppId = game.AppId;
            DrawerType = DrawerType.SteamGame;
        }

        /// <summary>
        /// 关闭编辑游戏抽屉
        /// </summary>
        [RelayCommand]
        private void CloseEditSteamGameDrawer()
        {
            DrawerType = DrawerType.None;
        }

        /// <summary>
        /// 删除steam游戏信息 (支持列表直接传参或抽屉当前游戏)
        /// </summary>
        [RelayCommand]
        private void DeleteSteamGame(string appId)
        {
            var target = string.IsNullOrEmpty(appId) ? editSteamGameAppId : appId;
            if (string.IsNullOrEmpty(target))
            {
                DrawerType = DrawerType.None;
                return;
            }

            SteamGameRepository.Delete(target);

            DrawerType = DrawerType.None;
            Lactor.ReLoad();
            Lactor.ShowToolTip("删除成功！");
        }

        /// <summary>
        /// 保存steam游戏信息
        /// </summary>
        [RelayCommand]
        private void SaveSteamGame()
        {
            if (string.IsNullOrEmpty(editSteamGameAppId))
            {
                DrawerType = DrawerType.None;
                return;
            }

            if (string.IsNullOrEmpty(EditSteamGameName))
            {
                return;
            }

            var game = SteamGameRepository.GetByAppId(editSteamGameAppId);
            if (game == null)
            {
                return;
            }

            game.Name = EditSteamGameName;
            SteamGameRepository.UpdateList(new List<SteamGame>() { game });

            DrawerType = DrawerType.None;
            Lactor.ReLoad();
            Lactor.ShowToolTip("保存成功！");
        }

        #endregion
    }
}