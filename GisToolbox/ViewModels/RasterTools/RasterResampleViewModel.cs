using GisToolbox.Models;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.RasterTools;

public partial class RasterResampleViewModel : ToolViewModelBase
{
    private readonly IRasterProcessingService _rasterService;

    [ObservableProperty] private RasterFormat _inputFormat = RasterFormat.PNG;

    [ObservableProperty] private bool _maintainAspectRatio = true;

    [ObservableProperty] private int _newHeight = 768;

    [ObservableProperty] private int _newWidth = 1024;

    [ObservableProperty] private int _originalHeight;

    [ObservableProperty] private int _originalWidth;

    [ObservableProperty] private ResampleMethod _selectedMethod = ResampleMethod.Bilinear;

    public RasterResampleViewModel(IRasterProcessingService rasterService)
    {
        _rasterService = rasterService;
    }

    public override string ToolName => "栅格重采样";

    public override string ToolDescription => "改变栅格图像的分辨率，支持最近邻、双线性、三次卷积插值";

    public override ToolCategory Category => ToolCategory.RasterTools;

    public List<ResampleMethod> AvailableMethods { get; } = Enum.GetValues<ResampleMethod>().ToList();

    protected override async Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        var result = await _rasterService.ResampleRasterAsync(
            InputFilePath,
            InputFormat,
            OutputFilePath,
            NewWidth,
            NewHeight,
            SelectedMethod,
            progress);

        return result;
    }

    protected override bool CanExecuteProcessing()
    {
        return base.CanExecuteProcessing() && NewWidth > 0 && NewHeight > 0;
    }

    protected override async void OnInputFileChanged()
    {
        base.OnInputFileChanged();

        if (!string.IsNullOrEmpty(InputFilePath))
            try
            {
                var raster = await _rasterService.LoadRasterAsync(InputFilePath, InputFormat);
                OriginalWidth = raster.Width;
                OriginalHeight = raster.Height;
                NewWidth = raster.Width;
                NewHeight = raster.Height;
            }
            catch
            {
                OriginalWidth = 0;
                OriginalHeight = 0;
            }
    }

    partial void OnNewWidthChanged(int value)
    {
        if (MaintainAspectRatio && OriginalWidth > 0 && OriginalHeight > 0)
            NewHeight = (int)(value * OriginalHeight / (double)OriginalWidth);
        ProcessCommand.NotifyCanExecuteChanged();
    }

    partial void OnNewHeightChanged(int value)
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }
}