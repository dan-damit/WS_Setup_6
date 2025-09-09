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

            vm.PropertyChanged += Vm_PropertyChanged;

            // Initial state sync
            UpdateVisualState(vm.ProgressState);
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ConfigurationPageViewModel.ProgressState))
                return;

            if (sender is ConfigurationPageViewModel vm)
                UpdateVisualState(vm.ProgressState);
        }

        private void UpdateVisualState(ConfigurationPageViewModel.ProgressVisualState state)
        {
            VisualStateManager.GoToState(ProgressArea, state.ToString(), useTransitions: true);
        }
    }
}