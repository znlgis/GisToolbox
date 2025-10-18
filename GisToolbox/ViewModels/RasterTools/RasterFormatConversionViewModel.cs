using GisToolbox.Models;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.RasterTools;

public partial class RasterFormatConversionViewModel : ToolViewModelBase
{
    private readonly IRasterProcessingService _rasterService;

    [ObservableProperty] private RasterFormat _inputFormat = RasterFormat.PNG;

    [ObservableProperty] private RasterFormat _outputFormat = RasterFormat.GeoTIFF;

    [ObservableProperty] private int _processedCount;

    public RasterFormatConversionViewModel(IRasterProcessingService rasterService)
    {
        _rasterService = rasterService;
    }

    public override string ToolName => "栅格格式转换";

    public override string ToolDescription => "支持 GeoTIFF、PNG、JPEG、BMP 等格式之间的相互转换";

    public override ToolCategory Category => ToolCategory.RasterTools;

    public List<RasterFormat> AvailableFormats { get; } = Enum.GetValues<RasterFormat>().ToList();

    protected override async Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        var result = await _rasterService.ConvertFormatAsync(
            InputFilePath,
            InputFormat,
            OutputFilePath,
            OutputFormat,
            progress);

        if (result.Success)
            ProcessedCount = result.ProcessedFeatures;

        return result;
    }

    protected override bool CanExecuteProcessing()
    {
        return base.CanExecuteProcessing() && InputFormat != OutputFormat;
    }

    partial void OnInputFormatChanged(RasterFormat value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }

    partial void OnOutputFormatChanged(RasterFormat value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }
}