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

            // Initial visibility sync
            UpdateProgressVisibility(vm.IsProgressVisible);
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ConfigurationPageViewModel.IsProgressVisible))
                return;

            if (sender is ConfigurationPageViewModel vm)
                UpdateProgressVisibility(vm.IsProgressVisible);
        }

        private void UpdateProgressVisibility(bool isVisible)
        {
            string state = isVisible ? "Running" : "Idle";
            VisualStateManager.GoToState(ProgressArea, state, useTransitions: true);
        }
    }
}