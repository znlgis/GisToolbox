using GisToolbox.Models;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.CoordinateTools;

/// <summary>
///     坐标转换工具ViewModel
/// </summary>
public partial class CoordinateTransformViewModel : ToolViewModelBase
{
    private readonly ICoordinateTransformService _coordinateService;
    private readonly IVectorProcessingService _vectorService;

    [ObservableProperty] private VectorFormat _inputFormat = VectorFormat.Shapefile;

    [ObservableProperty] private VectorFormat _outputFormat = VectorFormat.Shapefile;

    [ObservableProperty] private string _selectedSourceSystem = "WGS 84 (EPSG:4326)";

    [ObservableProperty] private string _selectedTargetSystem = "WGS 84 / Pseudo-Mercator (EPSG:3857)";

    [ObservableProperty] private int _sourceSRID = 4326;

    [ObservableProperty] private int _targetSRID = 3857;

    [ObservableProperty] private int _transformedFeatureCount;

    public CoordinateTransformViewModel(
        IVectorProcessingService vectorService,
        ICoordinateTransformService coordinateService)
    {
        _vectorService = vectorService;
        _coordinateService = coordinateService;

        var systems = coordinateService.GetCommonCoordinateSystems();
        AvailableCoordinateSystems = systems.Keys.ToList();
    }

    public override string ToolName => "坐标系统转换";

    public override string ToolDescription => "在不同坐标参考系统之间转换几何数据";

    public override ToolCategory Category => ToolCategory.CoordinateTools;

    public List<string> AvailableCoordinateSystems { get; }

    protected override async Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        try
        {
            progress.Report(10);

            // 加载几何数据
            var geometries = await _vectorService.LoadGeometryAsync(InputFilePath, InputFormat);
            progress.Report(30);

            // 转换每个几何对象的坐标系统
            for (var i = 0; i < geometries.Count; i++)
            {
                if (geometries[i].Geometry != null)
                {
                    geometries[i].Geometry = _coordinateService.TransformGeometry(
                        geometries[i].Geometry,
                        SourceSRID,
                        TargetSRID);
                    geometries[i].SRID = TargetSRID;
                }

                progress.Report(30 + (int)(60.0 * (i + 1) / geometries.Count));
            }

            TransformedFeatureCount = geometries.Count;
            progress.Report(90);

            // 保存结果
            await _vectorService.SaveGeometryAsync(geometries, OutputFilePath, OutputFormat);
            progress.Report(100);

            return ProcessingResult.CreateSuccess(
                $"成功转换 {geometries.Count} 个要素的坐标系统\n从 EPSG:{SourceSRID} 到 EPSG:{TargetSRID}",
                geometries.Count);
        }
        catch (Exception ex)
        {
            return ProcessingResult.CreateError(ex.Message);
        }
    }

    protected override bool CanExecuteProcessing()
    {
        return base.CanExecuteProcessing() && SourceSRID != TargetSRID;
    }

    partial void OnSelectedSourceSystemChanged(string value)
    {
        var systems = _coordinateService.GetCommonCoordinateSystems();
        if (systems.TryGetValue(value, out var srid)) SourceSRID = srid;
        ProcessCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedTargetSystemChanged(string value)
    {
        var systems = _coordinateService.GetCommonCoordinateSystems();
        if (systems.TryGetValue(value, out var srid)) TargetSRID = srid;
        ProcessCommand.NotifyCanExecuteChanged();
    }

    partial void OnSourceSRIDChanged(int value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }

    partial void OnTargetSRIDChanged(int value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }
}