using System.IO;
using System.Net;
using System.Reflection;
using Markdig;

namespace MarkdownSeek.Services;

/// <summary>Convierte Markdown en un documento HTML completo listo para WebView2.</summary>
public sealed class MarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private readonly Lazy<string> _css = new(LoadCss);

    public string RenderDocument(string markdown, string title)
    {
        var body = Markdown.ToHtml(markdown, _pipeline);
        return $"""
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset="utf-8">
            <title>{WebUtility.HtmlEncode(title)}</title>
            <style>{_css.Value}</style>
            </head>
            <body>
            {body}
            </body>
            </html>
            """;
    }

    private static string LoadCss()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("MarkdownSeek.Assets.markdown.css");
        if (stream is null) return string.Empty;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
