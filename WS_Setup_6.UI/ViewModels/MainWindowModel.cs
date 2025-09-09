using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using WS_Setup_6.Core.Interfaces;

namespace WS_Setup_6.UI.ViewModels
{
    [SupportedOSPlatform("windows")]
    public partial class MainWindowModel : ObservableObject
    {
        private bool _isApplyingBaseline;
        public bool IsApplyingBaseline
        {
            get => _isApplyingBaseline;
            set => SetProperty(ref _isApplyingBaseline, value);
        }
        public string? InstallPath { get; set; }
        private bool _isBusy;
        private readonly INavigationService _nav;
        private readonly IDialogCoordinator _dialogCoordinator;

        // 1) Default value in the backing field does NOT fire OnSelectedPageChanged
        [ObservableProperty]
        private string _selectedPage = "HomePage";

        // 2) This is what the ContentControl binds to
        [ObservableProperty]
        private object _currentView = default!;

        // 3) Constructor receives the NavService from DI
        public MainWindowModel(INavigationService nav, IDialogCoordinator dialogCoordinator)
        {
            _nav = nav;
            _dialogCoordinator = dialogCoordinator;

            // 4) Whenever NavService swaps the view, mirror it straight into CurrentView
            _nav.CurrentPageChanged += () =>
                {
                    // always safe because NavService only fires after NavigateTo(...)
                    CurrentView = _nav.CurrentPageView!;
                };

            // Pre-populate the installer path to Desktop\NinjaOne-Agent*.msi
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var msi = Directory
                .EnumerateFiles(desktop, "NinjaOne-Agent*.msi")
                .FirstOrDefault();
            InstallPath = msi;
        }

        // 5) Fired only when SelectedPage actually changes (i.e. user clicks a tab)
        partial void OnSelectedPageChanged(string? oldValue, string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                return;   // guard against stray empty values

            _nav.NavigateTo(newValue);
        }

        // 6) Graysout the Exit button when busy
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
                ExitCommand.NotifyCanExecuteChanged();
            }
        }

        // 7) Turns Exit button on if not busy
        private bool CanExit() => !IsBusy;

        // 8) An Exit command if still need it
        [RelayCommand(CanExecute = nameof(CanExit))]
        private async Task ExitAsync()
        {
            var settings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Reboot",
                NegativeButtonText = "Cancel",
                FirstAuxiliaryButtonText = "Exit",
                AnimateShow = true,
                AnimateHide = true,
                ColorScheme = MetroDialogColorScheme.Theme
            };

            var result = await _dialogCoordinator.ShowMessageAsync(
                "MainHost",
                "Exit Options",
                "What would you like to do?",
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                settings
            );

            switch (result)
            {
                case MessageDialogResult.Affirmative:
                    Process.Start(new ProcessStartInfo("shutdown", "/r /t 0")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                    Application.Current.Shutdown();
                    break;

                case MessageDialogResult.FirstAuxiliary:
                    Application.Current.Shutdown();
                    break;

                case MessageDialogResult.Negative:
                    // Do nothing — app stays open
                    break;
            }
        }
    }
}