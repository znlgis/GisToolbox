using GisToolbox.Models;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.VectorTools;

/// <summary>
///     格式转换工具ViewModel
/// </summary>
public partial class FormatConversionViewModel : ToolViewModelBase
{
    private readonly IVectorProcessingService _vectorService;

    [ObservableProperty] private int _featureCount;

    [ObservableProperty] private VectorFormat _inputFormat = VectorFormat.Shapefile;

    [ObservableProperty] private VectorFormat _outputFormat = VectorFormat.GeoJSON;

    public FormatConversionViewModel(IVectorProcessingService vectorService)
    {
        _vectorService = vectorService;
    }

    public override string ToolName => "矢量格式转换";

    public override string ToolDescription => "支持 Shapefile、GeoJSON、WKT 等格式之间的相互转换";

    public override ToolCategory Category => ToolCategory.VectorTools;

    public List<VectorFormat> AvailableFormats { get; } = Enum.GetValues<VectorFormat>().ToList();

    protected override async Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        var result = await _vectorService.ConvertFormatAsync(
            InputFilePath,
            InputFormat,
            OutputFilePath,
            OutputFormat,
            progress);

        if (result.Success) FeatureCount = result.ProcessedFeatures;

        return result;
    }

    protected override bool CanExecuteProcessing()
    {
        return base.CanExecuteProcessing() && InputFormat != OutputFormat;
    }

    partial void OnInputFormatChanged(VectorFormat value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }

    partial void OnOutputFormatChanged(VectorFormat value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }
}