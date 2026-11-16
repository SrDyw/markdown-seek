using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MarkdownSeek.Services;

/// <summary>
/// Registra Markdown Seek como manejador de .md/.markdown para el usuario actual (HKCU, sin admin).
/// Windows 10/11 no permite fijar la app predeterminada por código: tras registrar se abre
/// Configuración para que el usuario la elija.
/// </summary>
public static class FileAssociationService
{
    private const string ProgId = "MarkdownSeek.md";
    private const string AppName = "Markdown Seek";
    private static readonly string[] Extensions = [".md", ".markdown"];

    public static void Register()
    {
        var exePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("No se pudo determinar la ruta del ejecutable.");

        using (var progId = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}"))
        {
            progId.SetValue("", "Markdown Document");
            using var icon = progId.CreateSubKey("DefaultIcon");
            icon.SetValue("", $"\"{exePath}\",0");
            using var open = progId.CreateSubKey(@"shell\open\command");
            open.SetValue("", $"\"{exePath}\" \"%1\"");
        }

        foreach (var ext in Extensions)
        {
            using var openWith = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ext}\OpenWithProgids");
            openWith.SetValue(ProgId, Array.Empty<byte>(), RegistryValueKind.None);
        }

        // Capabilities + RegisteredApplications hacen que aparezca en "Aplicaciones predeterminadas".
        using (var caps = Registry.CurrentUser.CreateSubKey(@"Software\MarkdownSeek\Capabilities"))
        {
            caps.SetValue("ApplicationName", AppName);
            caps.SetValue("ApplicationDescription", "Visor de archivos Markdown");
            using var assoc = caps.CreateSubKey("FileAssociations");
            foreach (var ext in Extensions) assoc.SetValue(ext, ProgId);
        }

        using (var registered = Registry.CurrentUser.CreateSubKey(@"Software\RegisteredApplications"))
        {
            registered.SetValue(AppName, @"Software\MarkdownSeek\Capabilities");
        }

        SHChangeNotify(SHCNE_ASSOCCHANGED, 0, IntPtr.Zero, IntPtr.Zero);
    }

    public static void OpenDefaultAppsSettings() =>
        Process.Start(new ProcessStartInfo("ms-settings:defaultapps") { UseShellExecute = true });

    private const int SHCNE_ASSOCCHANGED = 0x08000000;

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int eventId, uint flags, IntPtr item1, IntPtr item2);
}
