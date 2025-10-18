using GisToolbox.Models;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.VectorTools;

/// <summary>
///     缓冲区分析工具ViewModel
/// </summary>
public partial class BufferAnalysisViewModel : ToolViewModelBase
{
    private readonly IVectorProcessingService _vectorService;

    [ObservableProperty] private double _bufferDistance = 100.0;

    [ObservableProperty] private string _distanceUnit = "米";

    [ObservableProperty] private VectorFormat _inputFormat = VectorFormat.Shapefile;

    [ObservableProperty] private VectorFormat _outputFormat = VectorFormat.Shapefile;

    public BufferAnalysisViewModel(IVectorProcessingService vectorService)
    {
        _vectorService = vectorService;
    }

    public override string ToolName => "缓冲区分析";

    public override string ToolDescription => "为几何对象创建指定距离的缓冲区";

    public override ToolCategory Category => ToolCategory.Analysis;

    public List<string> DistanceUnits { get; } = new() { "米", "千米", "度" };

    protected override async Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        try
        {
            progress.Report(10);

            // 加载几何数据
            var geometries = await _vectorService.LoadGeometryAsync(InputFilePath, InputFormat);
            progress.Report(30);

            // 创建缓冲区
            for (var i = 0; i < geometries.Count; i++)
            {
                if (geometries[i].Geometry != null)
                    geometries[i].Geometry = _vectorService.BufferGeometry(geometries[i].Geometry, BufferDistance);
                
                progress.Report(30 + (int)(60.0 * (i + 1) / geometries.Count));
            }

            progress.Report(90);

            // 保存结果
            await _vectorService.SaveGeometryAsync(geometries, OutputFilePath, OutputFormat);
            progress.Report(100);

            return ProcessingResult.CreateSuccess(
                $"成功创建 {geometries.Count} 个缓冲区，距离: {BufferDistance} {DistanceUnit}",
                geometries.Count);
        }
        catch (Exception ex)
        {
            return ProcessingResult.CreateError(ex.Message);
        }
    }

    protected override bool CanExecuteProcessing()
    {
        return base.CanExecuteProcessing() && BufferDistance > 0;
    }

    partial void OnBufferDistanceChanged(double value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }
}