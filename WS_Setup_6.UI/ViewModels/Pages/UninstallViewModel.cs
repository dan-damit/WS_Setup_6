using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;
using WS_Setup_6.Common.Interfaces;
using WS_Setup_6.Core.Interfaces;
using WS_Setup_6.Core.Models;

namespace WS_Setup_6.UI.ViewModels
{
    [SupportedOSPlatform("windows")]
    public partial class UninstallViewModel : ObservableObject
    {
        private readonly ILogService _log;
        private readonly IUninstallService _uninstallService;
        private readonly IAppInventoryService _appInventoryService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private CancellationTokenSource? _cts;

        public ObservableCollection<UninstallEntry> InstalledApps { get; }
        public ObservableCollection<UninstallEntry> SelectedApps { get; set; }

        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isUninstalling;

        // Batch progress tracking
        public int BatchMax { get; private set; }
        private double _batchProgress;
        public double BatchProgress
        {
            get => _batchProgress;
            private set
            {
                if (SetProperty(ref _batchProgress, value))
                    OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        // Computed property for progress percentage
        public double ProgressPercentage => BatchMax == 0 ? 0 : (BatchProgress / BatchMax) * 100;

        // Load apps and Main uninstall hooks
        public IAsyncRelayCommand LoadAppsCommand { get; }
        public IAsyncRelayCommand UninstallSelectedCommand { get; }
        public IAsyncRelayCommand PurgeLeftoversCommand { get; }

        // Enables or disables buttons based on user input
        public bool CanExecute => SelectedApps.Any() && !IsUninstalling;

        public UninstallViewModel(
            IUninstallService uninstallService,
            ILogService log,
            IAppInventoryService appInventoryService,
            IDialogCoordinator dialogCoordinator)
        {
            _uninstallService = uninstallService;
            _log = log;
            _appInventoryService = appInventoryService;
            _dialogCoordinator = dialogCoordinator;

            InstalledApps = new ObservableCollection<UninstallEntry>();
            SelectedApps = new ObservableCollection<UninstallEntry>();

            LoadAppsCommand = new AsyncRelayCommand(LoadAppsAsync);
            UninstallSelectedCommand = new AsyncRelayCommand(
                ExecuteBatchUninstallAsync,
                () => CanExecute
            );
            PurgeLeftoversCommand = new AsyncRelayCommand(ExecutePurgeLeftoversAsync);
            
            // Hook into selection changes so CanExecute re‑evaluates
            SelectedApps.CollectionChanged += (_, __) =>
            UninstallSelectedCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsUninstallingChanged(bool oldValue, bool newValue) =>
            UninstallSelectedCommand.NotifyCanExecuteChanged();

        // Scan for and load all installed apps based on what apps are currently registered
        private async Task LoadAppsAsync()
        {
            InstalledApps.Clear();
            var entries = await _appInventoryService.ScanInstalledAppsAsync();
            foreach (var entry in entries)
            {
                InstalledApps.Add(entry);
            }
        }

        // Main uninstall method (ochestrator)
        private async Task ExecuteBatchUninstallAsync()
        {
            IsUninstalling = true;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var apps = SelectedApps.ToList();
            int total = apps.Count;
            BatchMax = 100;
            BatchProgress = 0;

            // Reorder: silent/MSI first, interactive-only last
            var silentApps = apps.Where(app => !_uninstallService.IsInteractiveOnly(app)).ToList();
            var interactiveApps = apps.Where(app => _uninstallService.IsInteractiveOnly(app)).ToList();
            var orderedApps = silentApps.Concat(interactiveApps).ToList();

            int completed = 0;

            foreach (var app in orderedApps)
            {
                StatusMessage = $"Uninstalling {completed + 1} of {total}: {app.DisplayName}";

                var progress = new Progress<UninstallProgress>(_ => { });
                var result = await _uninstallService.ExecuteUninstallAsync(app, progress, _cts.Token);

                app.Success = result.Success;
                app.ExitCode = result.ExitCode;
                app.WasCancelled = result.WasCancelled;

                completed++;
                BatchProgress = (int)((completed / (double)total) * BatchMax);
                await Task.Yield();
            }

            StatusMessage = $"Batch uninstall complete. {completed} apps processed.";

            // Refresh app list
            await LoadAppsAsync();

            // Fallback: retry interactive uninstall for apps still present
            var remaining = InstalledApps.Where(installed =>
                apps.Any(original =>
                    string.Equals(original.DisplayName, installed.DisplayName, StringComparison.OrdinalIgnoreCase)
                    && _uninstallService.IsInteractiveOnly(installed)))
                .ToList();

            foreach (var app in remaining)
            {
                StatusMessage = $"Retrying interactively: {app.DisplayName}";
                _log.Log($"[Fallback] Launching interactive uninstall for {app.DisplayName}", "INFO");

                var result = await _uninstallService.ExecuteUninstallAsync(app, new Progress<UninstallProgress>(_ => { }), CancellationToken.None);

                app.Success = result.Success;
                app.ExitCode = result.ExitCode;
                app.WasCancelled = result.WasCancelled;
                await Task.Yield();
            }

            StatusMessage = $"Uninstall complete. {completed} apps processed.";
            IsUninstalling = false;
            await LoadAppsAsync();
        }

        // Final "purge leftovers" command to scrub any remnants if any
        private async Task ExecutePurgeLeftoversAsync()
        {
            var settings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Proceed",
                NegativeButtonText = "Cancel",
                AnimateShow = true,
                AnimateHide = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            if (InstalledApps == null || InstalledApps.Count == 0)
            {
                StatusMessage = "No apps available for cleanup.";
                return;
            }

            var result = await _dialogCoordinator.ShowMessageAsync(
                "MainHost",
                "Confirm Cleanup",
                "This will permanently delete leftover files and registry entries from previously uninstalled applications.\n\n" +
                "⚠️ This should only be run *after* conventional uninstall methods have been attempted.\n\n" +
                "Do you want to proceed?",
                MessageDialogStyle.AffirmativeAndNegative,
                settings
            );

            if (result == MessageDialogResult.Affirmative)
            {
                    foreach (var app in InstalledApps)
                {
                    if (await _uninstallService.IsStillInstalledAsync(app))
                    {
                        _log.Log($"Detected leftover install remnants for {app.DisplayName}, performing forced cleanup", "WARN");
                        _uninstallService.ForceDeleteRemnants(app);
                    }
                }

                StatusMessage = "OEM leftovers purged.";
            }
            else
            {
                StatusMessage = "Cleanup canceled.";
            }
            
            await LoadAppsAsync();
        }
    }
}
