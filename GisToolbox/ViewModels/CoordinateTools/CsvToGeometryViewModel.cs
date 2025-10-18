using System.Text;
using GisToolbox.Models;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;
using NetTopologySuite.Geometries;

namespace GisToolbox.ViewModels.CoordinateTools;

/// <summary>
///     CSV转几何工具ViewModel
/// </summary>
public partial class CsvToGeometryViewModel : ToolViewModelBase
{
    private readonly IVectorProcessingService _vectorService;

    [ObservableProperty] private int _convertedFeatureCount;

    [ObservableProperty] private string _delimiter = ",";

    [ObservableProperty] private bool _hasHeader = true;

    [ObservableProperty] private VectorFormat _outputFormat = VectorFormat.GeoJSON;

    [ObservableProperty] private int _srid = 4326;

    [ObservableProperty] private string _xColumnName = "longitude";

    [ObservableProperty] private string _yColumnName = "latitude";

    public CsvToGeometryViewModel(IVectorProcessingService vectorService)
    {
        _vectorService = vectorService;
    }

    public override string ToolName => "CSV 转几何";

    public override string ToolDescription => "将包含坐标信息的 CSV 文件转换为矢量几何数据";

    public override ToolCategory Category => ToolCategory.CoordinateTools;

    public List<string> DelimiterOptions { get; } = new() { ",", ";", "\t", "|" };

    protected override async Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        try
        {
            progress.Report(10);

            // 读取 CSV 文件
            var geometries = await LoadCsvAsync(InputFilePath);
            progress.Report(80);

            ConvertedFeatureCount = geometries.Count;

            // 保存为几何文件
            await _vectorService.SaveGeometryAsync(geometries, OutputFilePath, OutputFormat);
            progress.Report(100);

            return ProcessingResult.CreateSuccess(
                $"成功将 {geometries.Count} 条记录转换为几何要素",
                geometries.Count);
        }
        catch (Exception ex)
        {
            return ProcessingResult.CreateError(ex.Message);
        }
    }

    private async Task<List<GeometryDataModel>> LoadCsvAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var geometries = new List<GeometryDataModel>();
            var geometryFactory = new GeometryFactory(new PrecisionModel(), Srid);

            using var reader = new StreamReader(filePath, Encoding.UTF8);

            // 读取表头
            var headerLine = reader.ReadLine();
            if (string.IsNullOrEmpty(headerLine))
                throw new InvalidOperationException("CSV 文件为空");

            var headers = headerLine.Split(Delimiter);

            // 查找坐标列索引
            var xIndex = -1;
            var yIndex = -1;

            if (HasHeader)
            {
                xIndex = Array.FindIndex(headers, h =>
                    h.Trim().Equals(XColumnName, StringComparison.OrdinalIgnoreCase));
                yIndex = Array.FindIndex(headers, h =>
                    h.Trim().Equals(YColumnName, StringComparison.OrdinalIgnoreCase));

                if (xIndex == -1 || yIndex == -1)
                    throw new InvalidOperationException(
                        $"未找到指定的坐标列: {XColumnName}, {YColumnName}");
            }
            else
            {
                // 如果没有表头，尝试使用前两列
                if (headers.Length < 2)
                    throw new InvalidOperationException("CSV 文件至少需要两列（X, Y）");

                xIndex = 0;
                yIndex = 1;

                // 将第一行作为数据处理
                reader.BaseStream.Position = 0;
                reader.DiscardBufferedData();
            }

            // 读取数据行
            var lineNumber = HasHeader ? 1 : 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split(Delimiter);

                if (values.Length <= Math.Max(xIndex, yIndex))
                    continue;

                try
                {
                    if (!double.TryParse(values[xIndex].Trim(), out var x) ||
                        !double.TryParse(values[yIndex].Trim(), out var y))
                        // 跳过无效坐标
                        continue;

                    // 创建点几何
                    var coordinate = new Coordinate(x, y);
                    var point = geometryFactory.CreatePoint(coordinate);

                    var model = new GeometryDataModel
                    {
                        Geometry = point,
                        SRID = Srid
                    };

                    // 添加属性
                    if (HasHeader)
                        for (var i = 0; i < Math.Min(headers.Length, values.Length); i++)
                        {
                            var header = headers[i].Trim();
                            var value = values[i].Trim();
                            model.Properties[header] = value;
                        }
                    else
                        for (var i = 0; i < values.Length; i++)
                            model.Properties[$"column_{i + 1}"] = values[i].Trim();

                    model.Properties["source_line"] = lineNumber;
                    model.Metadata.GeometryType = "Point";
                    model.Metadata.SourceFile = filePath;

                    geometries.Add(model);
                }
                catch
                {
                    // 跳过解析失败的行
                }
            }

            return geometries;
        });
    }

    protected override bool CanExecuteProcessing()
    {
        return base.CanExecuteProcessing() &&
               !string.IsNullOrWhiteSpace(XColumnName) &&
               !string.IsNullOrWhiteSpace(YColumnName);
    }

    partial void OnXColumnNameChanged(string value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }

    partial void OnYColumnNameChanged(string value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }
}