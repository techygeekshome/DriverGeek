using System.Diagnostics;
using System.Text;
using DriverGeek.Core.Models;

namespace DriverGeek.Services;

public sealed record ExportResult(bool Ok, string Folder, string Message);

/// <summary>
/// Writes a copy of the driver a device is running now to disk, before anything replaces it.
/// pnputil does the real work for third-party packages. Windows' own in-box drivers cannot be
/// exported that way, so what gets saved for those is the INF plus a written record of exactly
/// what was installed - enough to find the same driver again, and to know what Roll Back Driver
/// in Device Manager would be putting back.
/// </summary>
public static class DriverExportService
{
    public static ExportResult Export(DeviceDriver device, DriverUpdate update, string backupRoot)
    {
        var folder = Path.Combine(backupRoot, Stamp(device));

        try
        {
            Directory.CreateDirectory(folder);
        }
        catch (Exception ex)
        {
            Log.Write("Could not create the backup folder: " + ex.Message);
            return new ExportResult(false, folder,
                "The backup folder could not be created, so there would be nothing to put back. " +
                "Nothing has been changed.");
        }

        WriteDetails(device, update, folder);

        var inf = device.InfName?.Trim() ?? "";
        if (inf.Length == 0)
            return new ExportResult(false, folder,
                "Windows does not report which INF this device is using, so the current driver " +
                "cannot be exported. Nothing has been changed.");

        if (RunPnpUtil(inf, folder, out var pnpMessage) && HasRealFiles(folder))
        {
            Log.Write($"Exported {inf} for {device.DeviceName} to {folder}");
            return new ExportResult(true, folder, $"Current driver exported to {folder}.");
        }

        // In-box drivers: pnputil will not export them. Take the INF itself and say so plainly.
        if (CopyInboxInf(inf, folder))
        {
            Log.Write($"pnputil could not export {inf} ({pnpMessage}); copied the INF instead.");
            return new ExportResult(true, folder,
                $"This is one of Windows' own drivers, which pnputil will not export. " +
                $"The INF and the full details were saved to {folder}; Device Manager's " +
                "Roll Back Driver is the way to put the old one back.");
        }

        Log.Write($"Export failed for {inf}: {pnpMessage}");
        return new ExportResult(false, folder,
            "The current driver could not be exported, so there would be nothing to put back. " +
            "Nothing has been changed.");
    }

    private static string Stamp(DeviceDriver device)
    {
        var name = new string((device.DeviceName ?? "device")
            .Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
        if (name.Length > 60) name = name[..60];
        return $"{DateTime.Now:yyyy-MM-dd_HHmm}_{name.Trim()}";
    }

    private static void WriteDetails(DeviceDriver device, DriverUpdate update, string folder)
    {
        try
        {
            var text = new StringBuilder()
                .AppendLine("Driver in place before DriverGeek replaced it")
                .AppendLine("=============================================")
                .AppendLine($"Saved         {DateTime.Now:dd MMM yyyy HH:mm}")
                .AppendLine($"Device        {device.DeviceName}")
                .AppendLine($"Manufacturer  {device.Manufacturer}")
                .AppendLine($"Class         {device.ClassName} {device.ClassGuid}")
                .AppendLine($"Device ID     {device.DeviceId}")
                .AppendLine($"INF           {device.InfName}")
                .AppendLine($"Version       {device.DriverVersion}")
                .AppendLine($"Date          {device.DriverDate:dd MMM yyyy}")
                .AppendLine($"Signed by     {(device.IsSigned ? device.Signer : "not signed")}")
                .AppendLine()
                .AppendLine("Replaced with")
                .AppendLine("-------------")
                .AppendLine($"Title         {update.Title}")
                .AppendLine($"Version       {update.DriverVersion}")
                .AppendLine($"Date          {update.DriverDate:dd MMM yyyy}")
                .AppendLine($"Update ID     {update.UpdateId}")
                .AppendLine()
                .AppendLine("To put the old driver back: Device Manager, right-click the device,")
                .AppendLine("Properties, Driver tab, Roll Back Driver. If that button is greyed out,")
                .AppendLine("use Update Driver and point it at this folder.")
                .ToString();

            File.WriteAllText(Path.Combine(folder, "driver-details.txt"), text);
        }
        catch (Exception ex)
        {
            Log.Write("Could not write the details file: " + ex.Message);
        }
    }

    private static bool RunPnpUtil(string inf, string folder, out string message)
    {
        message = "";
        try
        {
            var psi = new ProcessStartInfo("pnputil.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            psi.ArgumentList.Add("/export-driver");
            psi.ArgumentList.Add(inf);
            psi.ArgumentList.Add(folder);

            using var p = Process.Start(psi);
            if (p is null) { message = "pnputil would not start."; return false; }

            var output = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
            if (!p.WaitForExit(120_000))
            {
                try { p.Kill(true); } catch (InvalidOperationException) { }
                message = "pnputil did not finish in two minutes.";
                return false;
            }

            message = output.Trim();
            return p.ExitCode == 0;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    private static bool HasRealFiles(string folder)
    {
        try
        {
            return Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                .Any(f => !Path.GetFileName(f).Equals("driver-details.txt", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception) { return false; }
    }

    private static bool CopyInboxInf(string inf, string folder)
    {
        try
        {
            var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var source = Path.Combine(windows, "INF", inf);
            if (!File.Exists(source)) return false;

            File.Copy(source, Path.Combine(folder, inf), overwrite: true);

            var pnf = Path.ChangeExtension(source, ".PNF");
            if (File.Exists(pnf))
                File.Copy(pnf, Path.Combine(folder, Path.GetFileName(pnf)), overwrite: true);

            return true;
        }
        catch (Exception ex)
        {
            Log.Write("Could not copy the in-box INF: " + ex.Message);
            return false;
        }
    }
}
