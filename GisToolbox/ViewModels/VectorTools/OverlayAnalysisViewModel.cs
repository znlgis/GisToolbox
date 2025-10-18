using CommunityToolkit.Mvvm.Messaging;
using GisToolbox.Models;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.VectorTools;

/// <summary>
///     叠加分析工具ViewModel
/// </summary>
public partial class OverlayAnalysisViewModel : ToolViewModelBase
{
    private readonly IVectorProcessingService _vectorService;

    [ObservableProperty] private VectorFormat _inputFormat = VectorFormat.Shapefile;

    [ObservableProperty] private VectorFormat _outputFormat = VectorFormat.Shapefile;

    [ObservableProperty] private int _resultFeatureCount;

    [ObservableProperty] private string _secondInputFilePath = string.Empty;

    [ObservableProperty] private OverlayOperation _selectedOperation = OverlayOperation.Intersection;

    public OverlayAnalysisViewModel(IVectorProcessingService vectorService)
    {
        _vectorService = vectorService;
    }

    public override string ToolName => "叠加分析";

    public override string ToolDescription => "对两个几何图层进行空间叠加分析（交集、并集、差集、对称差）";

    public override ToolCategory Category => ToolCategory.Analysis;

    public List<OverlayOperation> AvailableOperations { get; } = Enum.GetValues<OverlayOperation>().ToList();

    [RelayCommand]
    private void SelectSecondInputFile()
    {
        // 发送消息请求打开文件对话框
        WeakReferenceMessenger.Default.Send(new SelectFileMessage
        {
            IsOpenDialog = true,
            Callback = path =>
            {
                if (!string.IsNullOrEmpty(path)) SecondInputFilePath = path;
            }
        });
    }

    protected override async Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        try
        {
            progress.Report(10);

            // 加载第一个图层
            var geometries1 = await _vectorService.LoadGeometryAsync(InputFilePath, InputFormat);
            progress.Report(25);

            // 加载第二个图层
            var geometries2 = await _vectorService.LoadGeometryAsync(SecondInputFilePath, InputFormat);
            progress.Report(40);

            // 执行叠加分析
            var resultGeometries = new List<GeometryDataModel>();
            var totalOperations = geometries1.Count * geometries2.Count;
            var currentOperation = 0;

            foreach (var geom1 in geometries1)
            foreach (var geom2 in geometries2)
            {
                if (geom1.Geometry == null || geom2.Geometry == null)
                    continue;

                var resultGeom = SelectedOperation switch
                {
                    OverlayOperation.Intersection => _vectorService.Intersection(geom1.Geometry, geom2.Geometry),
                    OverlayOperation.Union => _vectorService.Union(geom1.Geometry, geom2.Geometry),
                    OverlayOperation.Difference => _vectorService.Difference(geom1.Geometry, geom2.Geometry),
                    OverlayOperation.SymmetricDifference => _vectorService.SymmetricDifference(geom1.Geometry,
                        geom2.Geometry),
                    _ => null
                };

                if (resultGeom != null && !resultGeom.IsEmpty)
                {
                    var resultModel = new GeometryDataModel
                    {
                        Geometry = resultGeom,
                        Properties = new Dictionary<string, object>(geom1.Properties)
                    };

                    // 合并属性
                    foreach (var (key, value) in geom2.Properties)
                        resultModel.Properties[$"layer2_{key}"] = value;

                    resultGeometries.Add(resultModel);
                }

                currentOperation++;
                progress.Report(40 + (int)(50.0 * currentOperation / totalOperations));
            }

            ResultFeatureCount = resultGeometries.Count;
            progress.Report(90);

            // 保存结果
            await _vectorService.SaveGeometryAsync(resultGeometries, OutputFilePath, OutputFormat);
            progress.Report(100);

            return ProcessingResult.CreateSuccess(
                $"叠加分析完成，生成 {resultGeometries.Count} 个要素",
                resultGeometries.Count);
        }
        catch (Exception ex)
        {
            return ProcessingResult.CreateError(ex.Message);
        }
    }

    protected override bool CanExecuteProcessing()
    {
        return base.CanExecuteProcessing() && !string.IsNullOrEmpty(SecondInputFilePath);
    }

    partial void OnSecondInputFilePathChanged(string value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }
}

/// <summary>
///     叠加操作类型
/// </summary>
public enum OverlayOperation
{
    Intersection,
    Union,
    Difference,
    SymmetricDifference
}