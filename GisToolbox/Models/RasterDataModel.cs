namespace GisToolbox.Models;

public class RasterDataModel
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int BandCount { get; set; }
    public double[] GeoTransform { get; set; } = new double[6];
    public string Projection { get; set; } = string.Empty;
    public double NoDataValue { get; set; } = -9999;
    public byte[][]? Data { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum RasterFormat
{
    GeoTIFF,
    PNG,
    JPEG,
    BMP
}

public enum ResampleMethod
{
    NearestNeighbor,
    Bilinear,
    Cubic
}