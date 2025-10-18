using System.Diagnostics;
using System.Runtime.Versioning;
using System.Drawing;
using System.Drawing.Imaging;
using GisToolbox.Models;
using GisToolbox.Services.Interfaces;

namespace GisToolbox.Services.Implementations;

[SupportedOSPlatform("windows")]
public class RasterProcessingService : IRasterProcessingService
{
    public async Task<RasterDataModel> LoadRasterAsync(string filePath, RasterFormat format)
    {
        return await Task.Run(() =>
        {
            using var bitmap = new Bitmap(filePath);
            var raster = new RasterDataModel
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                BandCount = 3,
                Data = new byte[3][]
            };

            for (var i = 0; i < 3; i++) raster.Data[i] = new byte[bitmap.Width * bitmap.Height];

            for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                var index = y * bitmap.Width + x;
                raster.Data[0][index] = pixel.R;
                raster.Data[1][index] = pixel.G;
                raster.Data[2][index] = pixel.B;
            }

            return raster;
        });
    }

    public async Task SaveRasterAsync(RasterDataModel raster, string filePath, RasterFormat format)
    {
        await Task.Run(() =>
        {
            using var bitmap = new Bitmap(raster.Width, raster.Height);

            for (var y = 0; y < raster.Height; y++)
            for (var x = 0; x < raster.Width; x++)
            {
                var index = y * raster.Width + x;
                var r = raster.Data?[0][index] ?? 0;
                var g = raster.BandCount > 1 ? raster.Data?[1][index] ?? 0 : r;
                var b = raster.BandCount > 2 ? raster.Data?[2][index] ?? 0 : r;
                bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
            }

            var imageFormat = format switch
            {
                RasterFormat.PNG => ImageFormat.Png,
                RasterFormat.JPEG => ImageFormat.Jpeg,
                RasterFormat.BMP => ImageFormat.Bmp,
                _ => ImageFormat.Png
            };

            bitmap.Save(filePath, imageFormat);
        });
    }

    public async Task<RasterDataModel> ResampleAsync(RasterDataModel raster, int newWidth, int newHeight,
        ResampleMethod method)
    {
        return await Task.Run(() =>
        {
            var resampled = new RasterDataModel
            {
                Width = newWidth,
                Height = newHeight,
                BandCount = raster.BandCount,
                Data = new byte[raster.BandCount][]
            };

            for (var i = 0; i < raster.BandCount; i++) resampled.Data[i] = new byte[newWidth * newHeight];

            var xRatio = (double)raster.Width / newWidth;
            var yRatio = (double)raster.Height / newHeight;

            for (var y = 0; y < newHeight; y++)
            for (var x = 0; x < newWidth; x++)
            {
                var index = y * newWidth + x;

                for (var band = 0; band < raster.BandCount; band++)
                    resampled.Data[band][index] = method switch
                    {
                        ResampleMethod.NearestNeighbor => NearestNeighbor(raster.Data![band], x, y, xRatio, yRatio,
                            raster.Width),
                        ResampleMethod.Bilinear => Bilinear(raster.Data![band], x, y, xRatio, yRatio, raster.Width,
                            raster.Height),
                        ResampleMethod.Cubic => Cubic(raster.Data![band], x, y, xRatio, yRatio, raster.Width,
                            raster.Height),
                        _ => NearestNeighbor(raster.Data![band], x, y, xRatio, yRatio, raster.Width)
                    };
            }

            return resampled;
        });
    }

    public async Task<ProcessingResult> ConvertFormatAsync(string inputPath, RasterFormat inputFormat,
        string outputPath, RasterFormat outputFormat, IProgress<int>? progress = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            progress?.Report(10);
            var raster = await LoadRasterAsync(inputPath, inputFormat);

            progress?.Report(50);
            await SaveRasterAsync(raster, outputPath, outputFormat);

            progress?.Report(100);
            stopwatch.Stop();

            return new ProcessingResult
            {
                Success = true,
                Message = "栅格格式转换成功",
                ProcessedFeatures = 1,
                OutputFilePath = outputPath,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Message = "栅格格式转换失败",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task<ProcessingResult> ResampleRasterAsync(string inputPath, RasterFormat inputFormat,
        string outputPath, int newWidth, int newHeight, ResampleMethod method, IProgress<int>? progress = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            progress?.Report(10);
            var raster = await LoadRasterAsync(inputPath, inputFormat);

            progress?.Report(30);
            var resampled = await ResampleAsync(raster, newWidth, newHeight, method);

            progress?.Report(80);
            await SaveRasterAsync(resampled, outputPath, inputFormat);

            progress?.Report(100);
            stopwatch.Stop();

            return new ProcessingResult
            {
                Success = true,
                Message = $"栅格重采样成功 ({raster.Width}x{raster.Height} → {newWidth}x{newHeight})",
                ProcessedFeatures = 1,
                OutputFilePath = outputPath,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Message = "栅格重采样失败",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private byte NearestNeighbor(byte[] data, int x, int y, double xRatio, double yRatio, int width)
    {
        var srcX = (int)(x * xRatio);
        var srcY = (int)(y * yRatio);
        return data[srcY * width + srcX];
    }

    private byte Bilinear(byte[] data, int x, int y, double xRatio, double yRatio, int width, int height)
    {
        var srcX = x * xRatio;
        var srcY = y * yRatio;

        var x1 = (int)srcX;
        var y1 = (int)srcY;
        var x2 = Math.Min(x1 + 1, width - 1);
        var y2 = Math.Min(y1 + 1, height - 1);

        var dx = srcX - x1;
        var dy = srcY - y1;

        var v11 = data[y1 * width + x1];
        var v21 = data[y1 * width + x2];
        var v12 = data[y2 * width + x1];
        var v22 = data[y2 * width + x2];

        var value = v11 * (1 - dx) * (1 - dy) +
                    v21 * dx * (1 - dy) +
                    v12 * (1 - dx) * dy +
                    v22 * dx * dy;

        return (byte)Math.Clamp(value, 0, 255);
    }

    private byte Cubic(byte[] data, int x, int y, double xRatio, double yRatio, int width, int height)
    {
        var srcX = x * xRatio;
        var srcY = y * yRatio;

        var x0 = (int)srcX;
        var y0 = (int)srcY;

        double sum = 0;
        double weightSum = 0;

        for (var dy = -1; dy <= 2; dy++)
        for (var dx = -1; dx <= 2; dx++)
        {
            var ix = Math.Clamp(x0 + dx, 0, width - 1);
            var iy = Math.Clamp(y0 + dy, 0, height - 1);

            var wx = CubicWeight(srcX - (x0 + dx));
            var wy = CubicWeight(srcY - (y0 + dy));
            var weight = wx * wy;

            sum += data[iy * width + ix] * weight;
            weightSum += weight;
        }

        return (byte)Math.Clamp(sum / weightSum, 0, 255);
    }

    private double CubicWeight(double x)
    {
        x = Math.Abs(x);
        if (x <= 1)
            return 1.5 * x * x * x - 2.5 * x * x + 1;
        if (x < 2)
            return -0.5 * x * x * x + 2.5 * x * x - 4 * x + 2;
        return 0;
    }
}