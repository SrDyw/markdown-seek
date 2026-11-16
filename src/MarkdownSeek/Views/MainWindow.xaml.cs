using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MarkdownSeek.Services;
using Microsoft.Win32;

namespace MarkdownSeek.Views;

public partial class MainWindow : Window
{
    private readonly MarkdownRenderer _renderer = new();
    private string? _currentFile;

    public MainWindow(string? initialFile = null)
    {
        InitializeComponent();
        _currentFile = initialFile;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await Browser.EnsureCoreWebView2Async();
            // Los enlaces externos se abren en el navegador del sistema.
            Browser.CoreWebView2.NewWindowRequested += (_, args) =>
            {
                args.Handled = true;
                OpenExternal(args.Uri);
            };
            Browser.CoreWebView2.NavigationStarting += (_, args) =>
            {
                if (args.Uri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    args.Cancel = true;
                    OpenExternal(args.Uri);
                }
            };
        }
        catch (Exception ex)
        {
            ShowError($"No se pudo iniciar WebView2:\n{ex.Message}");
            return;
        }

        if (_currentFile is not null) LoadFile(_currentFile);
    }

    private void LoadFile(string path)
    {
        try
        {
            var markdown = File.ReadAllText(path);
            var name = Path.GetFileName(path);
            Browser.NavigateToString(_renderer.RenderDocument(markdown, name));
            _currentFile = path;
            Title = $"{name} - Markdown Seek";
            StatusText.Text = path;
        }
        catch (Exception ex)
        {
            ShowError($"No se pudo abrir el archivo:\n{ex.Message}");
        }
    }

    private void ShowError(string message) =>
        MessageBox.Show(this, message, "Markdown Seek", MessageBoxButton.OK, MessageBoxImage.Error);

    private static void OpenExternal(string uri) =>
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });

    private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Markdown (*.md;*.markdown)|*.md;*.markdown|Todos los archivos (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) == true) LoadFile(dialog.FileName);
    }

    private void Reload_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (_currentFile is not null) LoadFile(_currentFile);
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
            LoadFile(files[0]);
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void SetDefault_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            FileAssociationService.Register();
            MessageBox.Show(this,
                "Markdown Seek se registró para archivos .md.\n\n" +
                "Ahora se abrirá Configuración: busca \".md\" y elige Markdown Seek.",
                "Markdown Seek", MessageBoxButton.OK, MessageBoxImage.Information);
            FileAssociationService.OpenDefaultAppsSettings();
        }
        catch (Exception ex)
        {
            ShowError($"No se pudo registrar la asociación:\n{ex.Message}");
        }
    }
}
