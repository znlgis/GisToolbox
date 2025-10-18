using GisToolbox.Models;
using NetTopologySuite.Geometries;

namespace GisToolbox.Services.Interfaces;

/// <summary>
///     矢量数据处理服务接口
/// </summary>
public interface IVectorProcessingService
{
    /// <summary>
    ///     加载几何数据
    /// </summary>
    Task<List<GeometryDataModel>> LoadGeometryAsync(string filePath, VectorFormat format);

    /// <summary>
    ///     保存几何数据
    /// </summary>
    Task SaveGeometryAsync(List<GeometryDataModel> geometries, string filePath, VectorFormat format);

    /// <summary>
    ///     简化几何
    /// </summary>
    Geometry SimplifyGeometry(Geometry geometry, double tolerance);

    /// <summary>
    ///     缓冲区分析
    /// </summary>
    Geometry BufferGeometry(Geometry geometry, double distance);

    /// <summary>
    ///     验证几何有效性
    /// </summary>
    bool ValidateGeometry(Geometry geometry, out string? errorMessage);

    /// <summary>
    ///     修复几何
    /// </summary>
    Geometry RepairGeometry(Geometry geometry);

    /// <summary>
    ///     坐标转换
    /// </summary>
    Geometry TransformCoordinates(Geometry geometry, int sourceSRID, int targetSRID);

    /// <summary>
    ///     叠加分析 - 交集
    /// </summary>
    Geometry? Intersection(Geometry geom1, Geometry geom2);

    /// <summary>
    ///     叠加分析 - 并集
    /// </summary>
    Geometry? Union(Geometry geom1, Geometry geom2);

    /// <summary>
    ///     叠加分析 - 差集
    /// </summary>
    Geometry? Difference(Geometry geom1, Geometry geom2);

    /// <summary>
    ///     叠加分析 - 对称差
    /// </summary>
    Geometry? SymmetricDifference(Geometry geom1, Geometry geom2);

    /// <summary>
    ///     格式转换
    /// </summary>
    Task<ProcessingResult> ConvertFormatAsync(string inputPath, VectorFormat inputFormat,
        string outputPath, VectorFormat outputFormat, IProgress<int>? progress = null);
}