using SteamHub.Manager;
using System.Windows;

namespace SteamHub
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = Lactor.MainViewModel;
        }

        /// <summary>
        /// 点击按钮直接弹出关联的上下文菜单
        /// </summary>
        private void OpenContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.ContextMenu != null)
            {
                fe.ContextMenu.PlacementTarget = fe;
                fe.ContextMenu.DataContext = fe.DataContext;
                fe.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }
    }
}