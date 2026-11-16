# Markdown Seek

Visor de archivos Markdown (`.md`, `.markdown`) para Windows, hecho con WPF (.NET 9).

## Características

- Renderizado de Markdown con [Markdig](https://github.com/xoofx/markdig) (tablas, listas de tareas, notas al pie, etc.).
- Vista con WebView2 y tema claro/oscuro automático.
- Abrir con `Ctrl+O`, arrastrar y soltar, recargar con `F5`.
- Registro como app predeterminada para `.md` (menú **Opciones**).

## Compilar y ejecutar

```powershell
dotnet build
dotnet run --project src/MarkdownSeek -- ruta\al\archivo.md
```

## Estructura

```
MarkdownSeek.sln
src/MarkdownSeek/
├── App.xaml(.cs)                      # Arranque; recibe la ruta del archivo por argumento
├── Views/MainWindow.xaml(.cs)         # Ventana principal con WebView2
├── Services/MarkdownRenderer.cs       # Markdown -> HTML
├── Services/FileAssociationService.cs # Registro de asociación .md (HKCU)
└── Assets/markdown.css                # Estilos del documento
```

## Requisitos

- Windows 10/11 con [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (incluido en Windows 11).
- .NET 9 SDK.
