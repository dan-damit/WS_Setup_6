using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using WS_Setup_6.Core.Models;

namespace WS_Setup_6.Core.Interfaces
{
    /// <summary>
    /// Defines methods for querying installed applications,
    /// executing their uninstallation, and handling OEM-specific removals.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public interface IUninstallService
    {
        /// <summary>
        /// Reads registry entries and returns the list of installed apps.
        /// </summary>
        Task<IReadOnlyList<UninstallEntry>> QueryInstalledAppsAsync();

        /// <summary>
        /// Runs a silent uninstall, stops services/processes first,
        /// and falls back to force-delete if needed.
        /// </summary>
        Task<UninstallResult> ExecuteUninstallAsync(
            UninstallEntry app,
            IProgress<UninstallProgress> progress,
            CancellationToken cancellationToken);

        /// <summary>
        /// Expose a method to check if an app is interactive-only.
        /// This will reorder the batch uninstall queue in the UI.
        /// </summary> 
        bool IsInteractiveOnly(UninstallEntry app);

        /// <summary>
        /// OEM purge for Dell leftover files and registry entries.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        Task<bool> IsStillInstalledAsync(UninstallEntry app);

        /// <summary>
        /// Forces the deletion of any remaining files, folders, or registry entries associated with the specified
        /// application.
        /// </summary>
        /// <remarks>This method is intended to be used in scenarios where a standard uninstallation
        /// process has left residual data. It attempts to clean up all associated remnants to ensure no traces of the
        /// application remain.</remarks>
        /// <param name="app">The application entry representing the software to remove remnants for. Cannot be null.</param>
        public void ForceDeleteRemnants(UninstallEntry app);
    }
}