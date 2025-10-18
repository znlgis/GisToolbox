using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GisToolbox.Models;

namespace GisToolbox.Helpers;

/// <summary>
///     文件对话框辅助类
/// </summary>
public static class FileDialogHelper
{
    /// <summary>
    ///     打开文件对话框
    /// </summary>
    public static async Task<string?> OpenFileAsync(Window owner, VectorFormat? format = null)
    {
        var filters = GetFileTypeFilters(format);

        var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择文件",
            AllowMultiple = false,
            FileTypeFilter = filters
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    /// <summary>
    ///     保存文件对话框
    /// </summary>
    public static async Task<string?> SaveFileAsync(Window owner, VectorFormat? format = null,
        string? suggestedFileName = null)
    {
        var filters = GetFileTypeFilters(format);

        var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "保存文件",
            SuggestedFileName = suggestedFileName,
            FileTypeChoices = filters
        });

        return file?.Path.LocalPath;
    }

    private static List<FilePickerFileType> GetFileTypeFilters(VectorFormat? format)
    {
        var filters = new List<FilePickerFileType>();

        if (format == null || format == VectorFormat.Shapefile)
            filters.Add(new FilePickerFileType("Shapefile")
            {
                Patterns = new[] { "*.shp" }
            });

        if (format == null || format == VectorFormat.GeoJSON)
            filters.Add(new FilePickerFileType("GeoJSON")
            {
                Patterns = new[] { "*.geojson", "*.json" }
            });

        if (format == null || format == VectorFormat.WKT)
            filters.Add(new FilePickerFileType("WKT")
            {
                Patterns = new[] { "*.wkt", "*.txt" }
            });

        if (format == null)
            filters.Add(new FilePickerFileType("所有文件")
            {
                Patterns = new[] { "*.*" }
            });

        return filters;
    }
}