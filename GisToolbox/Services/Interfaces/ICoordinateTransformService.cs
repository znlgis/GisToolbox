using NetTopologySuite.Geometries;

namespace GisToolbox.Services.Interfaces;

/// <summary>
///     坐标转换服务接口
/// </summary>
public interface ICoordinateTransformService
{
    /// <summary>
    ///     转换几何坐标系统
    /// </summary>
    Geometry TransformGeometry(Geometry geometry, int sourceSRID, int targetSRID);

    /// <summary>
    ///     转换几何坐标系统（使用WKT定义）
    /// </summary>
    Geometry TransformGeometry(Geometry geometry, string sourceWkt, string targetWkt);

    /// <summary>
    ///     获取常用坐标系统列表
    /// </summary>
    Dictionary<string, int> GetCommonCoordinateSystems();

    /// <summary>
    ///     根据SRID获取坐标系统名称
    /// </summary>
    string? GetCoordinateSystemName(int srid);
}