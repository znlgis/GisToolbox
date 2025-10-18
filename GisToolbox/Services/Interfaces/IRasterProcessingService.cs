using GisToolbox.Models;

namespace GisToolbox.Services.Interfaces;

public interface IRasterProcessingService
{
    Task<RasterDataModel> LoadRasterAsync(string filePath, RasterFormat format);
    Task SaveRasterAsync(RasterDataModel raster, string filePath, RasterFormat format);
    Task<RasterDataModel> ResampleAsync(RasterDataModel raster, int newWidth, int newHeight, ResampleMethod method);

    Task<ProcessingResult> ConvertFormatAsync(string inputPath, RasterFormat inputFormat, string outputPath,
        RasterFormat outputFormat, IProgress<int>? progress = null);

    Task<ProcessingResult> ResampleRasterAsync(string inputPath, RasterFormat inputFormat, string outputPath,
        int newWidth, int newHeight, ResampleMethod method, IProgress<int>? progress = null);
}