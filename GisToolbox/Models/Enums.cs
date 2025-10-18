namespace GisToolbox.Models;

/// <summary>
///     支持的矢量数据格式
/// </summary>
public enum VectorFormat
{
    Shapefile,
    GeoJSON,
    WKT,
    WKB,
    KML,
    GPX,
    CSV
}

/// <summary>
///     坐标记录
/// </summary>
public class CoordinateRecord
{
    public double X { get; set; }
    public double Y { get; set; }
    public double? Z { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}

/// <summary>
///     工具分类
/// </summary>
public enum ToolCategory
{
    VectorTools,
    RasterTools,
    CoordinateTools,
    GpsTools,
    WebGis,
    Analysis
}