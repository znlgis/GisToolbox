using System.Diagnostics;
using System.Text;
using GisToolbox.Models;
using GisToolbox.Services.Interfaces;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.Operation.Valid;
using NetTopologySuite.Simplify;
using Newtonsoft.Json;

namespace GisToolbox.Services.Implementations;

/// <summary>
///     矢量数据处理服务实现
/// </summary>
public class VectorProcessingService : IVectorProcessingService
{
    private readonly GeometryFactory _geometryFactory;

    static VectorProcessingService()
    {
        // 注册编码提供程序以支持 GB2312 等中文编码（只需要注册一次）
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public VectorProcessingService()
    {
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<List<GeometryDataModel>> LoadGeometryAsync(string filePath, VectorFormat format)
    {
        return await Task.Run(() =>
        {
            var geometries = new List<GeometryDataModel>();

            try
            {
                switch (format)
                {
                    case VectorFormat.Shapefile:
                        geometries = LoadShapefile(filePath);
                        break;
                    case VectorFormat.GeoJSON:
                        geometries = LoadGeoJson(filePath);
                        break;
                    case VectorFormat.WKT:
                        geometries = LoadWKT(filePath);
                        break;
                    default:
                        throw new NotSupportedException($"格式 {format} 暂不支持");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载文件失败: {ex.Message}", ex);
            }

            return geometries;
        });
    }

    public async Task SaveGeometryAsync(List<GeometryDataModel> geometries, string filePath, VectorFormat format)
    {
        await Task.Run(() =>
        {
            try
            {
                switch (format)
                {
                    case VectorFormat.Shapefile:
                        SaveShapefile(geometries, filePath);
                        break;
                    case VectorFormat.GeoJSON:
                        SaveGeoJson(geometries, filePath);
                        break;
                    case VectorFormat.WKT:
                        SaveWKT(geometries, filePath);
                        break;
                    default:
                        throw new NotSupportedException($"格式 {format} 暂不支持");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"保存文件失败: {ex.Message}", ex);
            }
        });
    }

    public Geometry SimplifyGeometry(Geometry geometry, double tolerance)
    {
        var simplifier = new DouglasPeuckerSimplifier(geometry)
        {
            DistanceTolerance = tolerance
        };
        return simplifier.GetResultGeometry();
    }

    public Geometry BufferGeometry(Geometry geometry, double distance)
    {
        return geometry.Buffer(distance);
    }

    public bool ValidateGeometry(Geometry geometry, out string? errorMessage)
    {
        var validator = new IsValidOp(geometry);
        var isValid = validator.IsValid;
        errorMessage = isValid ? null : validator.ValidationError?.ToString();
        return isValid;
    }

    public Geometry RepairGeometry(Geometry geometry)
    {
        if (geometry.IsValid)
            return geometry;

        // 使用 Buffer(0) 技巧修复简单的拓扑错误
        try
        {
            return geometry.Buffer(0);
        }
        catch
        {
            return geometry;
        }
    }

    public Geometry TransformCoordinates(Geometry geometry, int sourceSRID, int targetSRID)
    {
        // 简化版本 - 实际应该使用 ProjNet 进行真正的坐标转换
        // 这里只是设置 SRID
        var transformed = (Geometry)geometry.Copy();
        transformed.SRID = targetSRID;
        return transformed;
    }

    public Geometry? Intersection(Geometry geom1, Geometry geom2)
    {
        return TryOperation(geom1.Intersection, geom2);
    }

    public Geometry? Union(Geometry geom1, Geometry geom2)
    {
        return TryOperation(geom1.Union, geom2);
    }

    public Geometry? Difference(Geometry geom1, Geometry geom2)
    {
        return TryOperation(geom1.Difference, geom2);
    }

    public Geometry? SymmetricDifference(Geometry geom1, Geometry geom2)
    {
        return TryOperation(geom1.SymmetricDifference, geom2);
    }

    public async Task<ProcessingResult> ConvertFormatAsync(string inputPath, VectorFormat inputFormat,
        string outputPath, VectorFormat outputFormat, IProgress<int>? progress = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            progress?.Report(10);
            var geometries = await LoadGeometryAsync(inputPath, inputFormat);

            progress?.Report(50);
            await SaveGeometryAsync(geometries, outputPath, outputFormat);

            progress?.Report(100);
            stopwatch.Stop();

            return new ProcessingResult
            {
                Success = true,
                Message = "格式转换成功",
                ProcessedFeatures = geometries.Count,
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
                Message = "格式转换失败",
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private static Geometry? TryOperation(Func<Geometry, Geometry> operation, Geometry geom2)
    {
        try
        {
            return operation(geom2);
        }
        catch
        {
            return null;
        }
    }

    #region Private Methods

    private List<GeometryDataModel> LoadShapefile(string filePath)
    {
        var geometries = new List<GeometryDataModel>();

        // 确保路径包含 .shp 扩展名
        var shpPath = filePath.EndsWith(".shp", StringComparison.OrdinalIgnoreCase)
            ? filePath
            : filePath + ".shp";

        using var reader = Shapefile.OpenRead(shpPath);

        foreach (var feature in reader)
        {
            var geometry = feature.Geometry;
            var model = new GeometryDataModel
            {
                Geometry = geometry,
                SRID = geometry.SRID
            };

            // 读取属性
            if (feature.Attributes != null)
                foreach (var attrName in feature.Attributes.GetNames())
                {
                    var value = feature.Attributes[attrName];
                    model.Properties[attrName] = value ?? DBNull.Value;
                }

            model.Metadata.GeometryType = geometry.GeometryType;
            model.Metadata.SourceFile = filePath;

            geometries.Add(model);
        }

        return geometries;
    }

    private List<GeometryDataModel> LoadGeoJson(string filePath)
    {
        var geometries = new List<GeometryDataModel>();
        // 使用 UTF-8 编码读取文件，防止中文乱码
        var json = File.ReadAllText(filePath, Encoding.UTF8);

        var serializer = GeoJsonSerializer.Create();
        using var stringReader = new StringReader(json);
        using var jsonReader = new JsonTextReader(stringReader);

        var featureCollection = serializer.Deserialize<FeatureCollection>(jsonReader);

        if (featureCollection != null)
            foreach (var feature in featureCollection)
            {
                var model = new GeometryDataModel
                {
                    Geometry = feature.Geometry,
                    SRID = feature.Geometry.SRID
                };

                if (feature.Attributes != null)
                    foreach (var attr in feature.Attributes.GetNames())
                        model.Properties[attr] = feature.Attributes[attr] ?? DBNull.Value;

                model.Metadata.GeometryType = feature.Geometry.GeometryType;
                model.Metadata.SourceFile = filePath;

                geometries.Add(model);
            }

        return geometries;
    }

    private List<GeometryDataModel> LoadWKT(string filePath)
    {
        var geometries = new List<GeometryDataModel>();
        var reader = new WKTReader();

        // 使用 UTF-8 编码读取文件，防止中文乱码
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var geometry = reader.Read(line);
                var model = new GeometryDataModel
                {
                    Geometry = geometry,
                    SRID = geometry.SRID,
                    Metadata = new GeometryMetadata
                    {
                        GeometryType = geometry.GeometryType,
                        SourceFile = filePath
                    }
                };
                geometries.Add(model);
            }
            catch
            {
                // 跳过无效的WKT行
            }
        }

        return geometries;
    }

    private void SaveShapefile(List<GeometryDataModel> geometries, string filePath)
    {
        if (!geometries.Any())
            throw new InvalidOperationException("没有要保存的几何对象");

        // 确保路径包含 .shp 扩展名
        var shpPath = filePath.EndsWith(".shp", StringComparison.OrdinalIgnoreCase)
            ? filePath
            : filePath + ".shp";

        var features = geometries.Select(g => new Feature(g.Geometry,
            new AttributesTable(g.Properties))).ToList();

        Shapefile.WriteAllFeatures(features, shpPath);
    }

    private void SaveGeoJson(List<GeometryDataModel> geometries, string filePath)
    {
        var features = new FeatureCollection();

        foreach (var geom in geometries)
        {
            var attributes = new AttributesTable(geom.Properties);
            var feature = new Feature(geom.Geometry, attributes);
            features.Add(feature);
        }

        var serializer = GeoJsonSerializer.Create();
        // 使用 UTF-8 编码写入文件，防止中文乱码
        using var streamWriter = new StreamWriter(filePath, false, Encoding.UTF8);
        using var jsonWriter = new JsonTextWriter(streamWriter)
        {
            Formatting = Formatting.Indented
        };

        serializer.Serialize(jsonWriter, features);
    }

    private void SaveWKT(List<GeometryDataModel> geometries, string filePath)
    {
        var writer = new WKTWriter();
        var lines = geometries.Select(g => writer.Write(g.Geometry));
        // 使用 UTF-8 编码写入文件，防止中文乱码
        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    #endregion
}