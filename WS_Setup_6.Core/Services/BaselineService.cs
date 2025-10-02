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
        public Task RunDscSimpleAsync(string yamlPath)
        {
            _log.Log("Configuring baseline via DSC", "INFO");

            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var dscExe = Path.Combine(programFiles, "DSC3", "DSC.exe");
            var pwsh = Path.Combine(programFiles, "PowerShell", "7", "pwsh.exe");

            if (!File.Exists(dscExe) || !File.Exists(pwsh))
            {
                _log.Log($"Missing DSC.exe ({dscExe}) or pwsh.exe ({pwsh})", "ERROR");
                return Task.CompletedTask;
            }

            // Build a PowerShell script that sets env vars then runs DSC
            var dscExeQuoted = $"'{dscExe.Replace("'", "''")}'";
            var yamlQuoted = $"'{yamlPath.Replace("'", "''")}'";
            var pwshDir = Path.GetDirectoryName(pwsh) ?? string.Empty;
            var pwshDirEscaped = pwshDir.Replace("'", "''");

            var psCommand = $@"
$env:DSC_HOST_PATH = {dscExeQuoted};
$env:PATH = '{pwshDirEscaped};' + $env:PATH;
& {dscExeQuoted} config set -f {yamlQuoted}
";

            var psi = new ProcessStartInfo(pwsh)
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = pwshDir,
                Arguments = $"-NoExit -ExecutionPolicy Bypass -Command \"{psCommand.Trim().Replace("\"", "\\\"")}\""
            };

            // Set Verb="runas" only when not already elevated
            if (!IsProcessElevated())
                psi.Verb = "runas";

            _log.Log($"Launching visible PowerShell to run DSC (DSC_HOST_PATH set to {dscExe}){(psi.Verb == "runas" ? " with elevation" : string.Empty)}", "INFO");

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
                catch (Win32Exception wex) when ((uint)wex.ErrorCode == 0x80004005 || wex.NativeErrorCode == 1223)
                {
                    // 1223 = ERROR_CANCELLED (user cancelled UAC)
                    _log.Log("Elevation was cancelled by the user. DSC run aborted.", "WARN");
                }
                catch (Exception ex)
                {
                    _log.Log($"Failed to launch PowerShell for DSC — {ex.Message}", "ERROR");
                }
            });
        }

        // Check if the current process is running with elevated (admin) privileges
        // Used in RunDscSimpleAsync to decide whether to set Verb="runas"
        private static bool IsProcessElevated()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}