using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using WS_Setup_6.UI.ViewModels.Pages;

namespace WS_Setup_6.UI.Windows.Pages
{
    [SupportedOSPlatform("windows")]
    public partial class ConfigurationPage : UserControl
    {
        public ConfigurationPage(ConfigurationPageViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}