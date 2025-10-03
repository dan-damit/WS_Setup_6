using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.Principal;
using WS_Setup_6.Common.Interfaces;
using WS_Setup_6.Common.Logging;
using WS_Setup_6.Core.Interfaces;

namespace WS_Setup_6.Core.Services
{
    [SupportedOSPlatform("windows")]
    public class BaselineService : IBaselineService
    {
        private readonly ILogService _log;

        public BaselineService(ILogService log)
        {
            _log = log;
        }

        /// <summary>
        /// Decrypts the given input file into the output path using AES.
        /// </summary>
        public void DecryptConfig(
            string inFile,
            string outFile,
            byte[] key,
            byte[] iv)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            var data = File.ReadAllBytes(inFile);
            var plain = aes.CreateDecryptor()
                           .TransformFinalBlock(data, 0, data.Length);

            File.WriteAllBytes(outFile, plain);
            _log.Log($"Decrypted configuration to {outFile}", "INFO");
        }

        /// <summary>
        /// Executes DSC in “set” mode against the given YAML config.
        /// All output and errors are shipped through ILogService.
        /// </summary>
        public Task RunDscWithWrapperAsync(string yamlPath)
        {
            _log.Log("Configuring baseline via DSC (visible PowerShell wrapper)", "INFO");

            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var dscExe = Path.Combine(programFiles, "DSC3", "DSC.exe");
            var pwsh = Path.Combine(programFiles, "PowerShell", "7", "pwsh.exe");

            if (!File.Exists(dscExe) || !File.Exists(pwsh))
            {
                _log.Log($"Missing DSC.exe ({dscExe}) or pwsh.exe ({pwsh})", "ERROR");
                return Task.CompletedTask;
            }

            // Prepare safe-quoted values for PowerShell
            string QuoteForPs(string s) => $"'{s.Replace("'", "''")}'";
            var dscExeQuoted = QuoteForPs(dscExe);
            var yamlQuoted = QuoteForPs(yamlPath);
            var pwshDir = Path.GetDirectoryName(pwsh) ?? string.Empty;
            var pwshDirQuoted = QuoteForPs(pwshDir);

            // Create temporary ps1 wrapper
            var tempPs1 = Path.Combine(Path.GetTempPath(), $"dsc_wrapper_{Guid.NewGuid():N}.ps1");

            // Build wrapper script
            var psScript = $@"
# DSC wrapper generated {DateTime.UtcNow:O}
# Sets DSC_HOST_PATH and PATH, suppresses progress, stabilizes buffer, transcripts output, and leaves console open.

$ErrorActionPreference = 'Continue'
$ProgressPreference = 'SilentlyContinue'

try {{
    $raw = $Host.UI.RawUI
    # Attempt to set buffer height large enough to avoid frequent buffer reflow on resize
    $sz = $raw.WindowSize
    $buf = $raw.BufferSize
    $newBuf = $buf
    $newBuf.Width = $sz.Width
    $newBuf.Height = [Math]::Max($buf.Height, 300)
    $raw.BufferSize = $newBuf
}} catch {{
    # Host may not allow RawUI changes; ignore
}}

# Set environment variables to point DSC to the unpacked binary and ensure pwsh folder is in PATH
$env:DSC_HOST_PATH = {dscExeQuoted}
$env:PATH = {pwshDirQuoted} + ';' + $env:PATH

# Start transcript for a reliable, complete capture
$transcriptPath = Join-Path $env:TEMP ('dsc_run_transcript_{Guid.NewGuid():N}.txt')
try {{
    Start-Transcript -Path $transcriptPath -Force
}} catch {{
    Write-Warning 'Failed to start transcript; output will still appear in the console.'
}}

Write-Host 'Invoking DSC to apply Baseline configuration. Console will remain open after completion for tech inspection.' -ForegroundColor Cyan
Write-Host ''

try {{
    & {dscExeQuoted} config set -f {yamlQuoted}
}} catch {{
    Write-Error ""DSC invocation failed: $($_.Exception.Message)""
}} finally {{
    try {{ Stop-Transcript }} catch {{ }}
}}

Write-Host ''
Write-Host 'Apply Baseline run complete. Transcript (if created): ' -NoNewline
Write-Host $transcriptPath -ForegroundColor Yellow
Write-Host ''
Write-Host 'Leave this window open for inspection. Close it when finished.' -ForegroundColor Green
";

            try
            {
                File.WriteAllText(tempPs1, psScript);
            }
            catch (Exception ex)
            {
                _log.Log($"Failed to write wrapper script {tempPs1} — {ex.Message}", "ERROR");
                return Task.CompletedTask;
            }

            // Build ProcessStartInfo to run pwsh with the wrapper and keep console open
            var psi = new ProcessStartInfo(pwsh)
            {
                UseShellExecute = true, // required to show native console window
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = pwshDir,
                Arguments = $"-NoExit -ExecutionPolicy Bypass -File \"{tempPs1}\""
            };

            if (!IsProcessElevated())
                psi.Verb = "runas";

            _log.Log($"Launching visible PowerShell wrapper for DSC (script: {tempPs1}){(psi.Verb == "runas" ? " with elevation" : "")}", "INFO");

            return Task.Run(() =>
            {
                try
                {
                    using var proc = Process.Start(psi);
                    if (proc == null)
                    {
                        _log.Log("Failed to start PowerShell for DSC", "ERROR");
                        return;
                    }

                    proc.WaitForExit();

                    if (proc.ExitCode != 0)
                        _log.Log($"PowerShell/DSC exited with code {proc.ExitCode}", "ERROR");
                    else
                        _log.Log("PowerShell/DSC process exited (console left open for tech inspection)", "SUMMARY");
                }
                catch (System.ComponentModel.Win32Exception wex) when (wex.NativeErrorCode == 1223)
                {
                    _log.Log("Elevation cancelled by user; DSC run aborted.", "WARN");
                }
                catch (Exception ex)
                {
                    _log.Log($"Failed to launch PowerShell wrapper for DSC — {ex.Message}", "ERROR");
                }
            });
        }

        // Helper to detect elevation
        private static bool IsProcessElevated()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}