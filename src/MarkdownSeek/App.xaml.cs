using System.Windows;
using MarkdownSeek.Views;

namespace MarkdownSeek;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Cuando Windows abre un .md con esta app, la ruta llega como primer argumento.
        var window = new MainWindow(e.Args.FirstOrDefault());
        window.Show();
    }
}
