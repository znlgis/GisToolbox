using GisToolbox.Models;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.VectorTools;

/// <summary>
///     几何简化工具ViewModel
/// </summary>
public partial class GeometrySimplificationViewModel : ToolViewModelBase
{
    private readonly IVectorProcessingService _vectorService;

    [ObservableProperty] private VectorFormat _inputFormat = VectorFormat.Shapefile;

    [ObservableProperty] private int _originalFeatureCount;

    [ObservableProperty] private VectorFormat _outputFormat = VectorFormat.Shapefile;

    [ObservableProperty] private int _simplifiedFeatureCount;

    [ObservableProperty] private double _tolerance = 0.001;

    public GeometrySimplificationViewModel(IVectorProcessingService vectorService)
    {
        _vectorService = vectorService;
    }

    public override string ToolName => "几何简化";

    public override string ToolDescription => "使用 Douglas-Peucker 算法简化几何对象，减少顶点数量";

    public override ToolCategory Category => ToolCategory.VectorTools;

    protected override async Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        try
        {
            progress.Report(10);

            // 加载几何数据
            var geometries = await _vectorService.LoadGeometryAsync(InputFilePath, InputFormat);
            OriginalFeatureCount = geometries.Count;

            progress.Report(30);

            // 简化每个几何对象
            for (var i = 0; i < geometries.Count; i++)
            {
                if (geometries[i].Geometry != null)
                    geometries[i].Geometry = _vectorService.SimplifyGeometry(geometries[i].Geometry, Tolerance);
                
                progress.Report(30 + (int)(60.0 * (i + 1) / geometries.Count));
            }

            SimplifiedFeatureCount = geometries.Count;
            progress.Report(90);

            // 保存结果
            await _vectorService.SaveGeometryAsync(geometries, OutputFilePath, OutputFormat);

            progress.Report(100);

            return ProcessingResult.CreateSuccess(
                $"成功简化 {geometries.Count} 个要素，容差: {Tolerance}",
                geometries.Count);
        }
        catch (Exception ex)
        {
            return ProcessingResult.CreateError(ex.Message);
        }
    }

    protected override bool CanExecuteProcessing()
    {
        return base.CanExecuteProcessing() && Tolerance > 0;
    }

    partial void OnToleranceChanged(double value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }
}