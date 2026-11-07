using SteamHub.Manager;

namespace SteamHub.View
{
    /// <summary>
    /// TrayPopupControl.xaml 的交互逻辑
    /// </summary>
    public partial class TrayPopupControl : System.Windows.Controls.UserControl
    {
        public TrayPopupControl()
        {
            InitializeComponent();
            this.DataContext = Locator.TrayPopupViewModel;
        }
    }
}