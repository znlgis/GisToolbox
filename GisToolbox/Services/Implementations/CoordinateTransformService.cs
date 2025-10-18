using GisToolbox.Services.Interfaces;
using NetTopologySuite.Geometries;

namespace GisToolbox.Services.Implementations;

/// <summary>
///     坐标转换服务实现（简化版）
/// </summary>
public class CoordinateTransformService : ICoordinateTransformService
{
    private readonly Dictionary<string, int> _commonCoordinateSystems = new()
    {
        { "WGS 84 (EPSG:4326)", 4326 },
        { "WGS 84 / Pseudo-Mercator (EPSG:3857)", 3857 },
        { "中国大地坐标系 2000 (EPSG:4490)", 4490 },
        { "Beijing 1954 (EPSG:4214)", 4214 },
        { "Xian 1980 (EPSG:4610)", 4610 },
        { "CGCS2000 / 3-degree Gauss-Kruger zone 37 (EPSG:4507)", 4507 },
        { "CGCS2000 / 3-degree Gauss-Kruger zone 38 (EPSG:4508)", 4508 },
        { "CGCS2000 / 3-degree Gauss-Kruger zone 39 (EPSG:4509)", 4509 },
        { "CGCS2000 / 3-degree Gauss-Kruger zone 40 (EPSG:4510)", 4510 }
    };

    public Geometry TransformGeometry(Geometry geometry, int sourceSRID, int targetSRID)
    {
        if (sourceSRID == targetSRID)
            return geometry;

        // TODO: 实现真实的坐标转换
        // 目前仅更新 SRID
        var cloned = (Geometry)geometry.Copy();
        cloned.SRID = targetSRID;
        return cloned;
    }

    public Geometry TransformGeometry(Geometry geometry, string sourceWkt, string targetWkt)
    {
        // TODO: 实现基于 WKT 的坐标转换
        return (Geometry)geometry.Copy();
    }

    public Dictionary<string, int> GetCommonCoordinateSystems()
    {
        return new Dictionary<string, int>(_commonCoordinateSystems);
    }

    public string? GetCoordinateSystemName(int srid)
    {
        return _commonCoordinateSystems.FirstOrDefault(x => x.Value == srid).Key ?? $"EPSG:{srid}";
    }
}