using NetTopologySuite.Geometries;

namespace GisToolbox.Models;

/// <summary>
///     几何数据模型
/// </summary>
public class GeometryDataModel
{
    /// <summary>
    ///     唯一标识符
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    ///     几何对象（NetTopologySuite）
    /// </summary>
    public Geometry? Geometry { get; set; }

    /// <summary>
    ///     属性数据
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    ///     坐标参考系统（SRID）
    /// </summary>
    public int SRID { get; set; } = 4326; // 默认 WGS84

    /// <summary>
    ///     元数据
    /// </summary>
    public GeometryMetadata Metadata { get; set; } = new();
}

/// <summary>
///     几何元数据
/// </summary>
public class GeometryMetadata
{
    /// <summary>
    ///     源文件路径
    /// </summary>
    public string? SourceFile { get; set; }

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    ///     几何类型
    /// </summary>
    public string? GeometryType { get; set; }

    /// <summary>
    ///     边界框
    /// </summary>
    public Envelope? BoundingBox { get; set; }

    /// <summary>
    ///     要素数量
    /// </summary>
    public int FeatureCount { get; set; }
}