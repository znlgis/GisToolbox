using GisToolbox.Models;
using GisToolbox.Models.GeoServer;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.GeoServerTools;

/// <summary>
///     GeoServer 图层管理 ViewModel
/// </summary>
public partial class LayerManagementViewModel : ToolViewModelBase
{
    private readonly IGeoServerService _geoServerService;

    [ObservableProperty] private ObservableCollection<GeoServerWorkspace> _workspaces = new();
    [ObservableProperty] private GeoServerWorkspace? _selectedWorkspace;
    [ObservableProperty] private ObservableCollection<GeoServerLayer> _layers = new();
    [ObservableProperty] private GeoServerLayer? _selectedLayer;
    [ObservableProperty] private GeoServerLayer? _layerDetails;
    [ObservableProperty] private ObservableCollection<GeoServerStyle> _availableStyles = new();
    [ObservableProperty] private GeoServerStyle? _selectedDefaultStyle;
    [ObservableProperty] private bool _deleteRecursively;
    [ObservableProperty] private bool _showAllLayers = true;

    public LayerManagementViewModel(IGeoServerService geoServerService)
    {
        _geoServerService = geoServerService;
    }

    public override string ToolName => "图层管理";
    public override string ToolDescription => "管理 GeoServer 图层，包括查看、配置样式和删除图层";
    public override ToolCategory Category => ToolCategory.WebGis;

    public bool IsConnected => _geoServerService.IsConnected;

    partial void OnSelectedWorkspaceChanged(GeoServerWorkspace? value)
    {
        _ = RefreshLayersAsync();
    }

    partial void OnShowAllLayersChanged(bool value)
    {
        _ = RefreshLayersAsync();
    }

    partial void OnSelectedLayerChanged(GeoServerLayer? value)
    {
        if (value != null)
        {
            _ = LoadLayerDetailsAsync();
        }
        else
        {
            LayerDetails = null;
        }
    }

    [RelayCommand]
    private async Task RefreshWorkspacesAsync()
    {
        if (!_geoServerService.IsConnected)
        {
            HasError = true;
            ErrorMessage = "请先连接到 GeoServer";
            return;
        }

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            StatusMessage = "正在加载工作空间...";

            var workspaces = await _geoServerService.GetWorkspacesAsync();
            Workspaces.Clear();
            foreach (var ws in workspaces)
            {
                Workspaces.Add(ws);
            }

            StatusMessage = $"已加载 {Workspaces.Count} 个工作空间";
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "加载失败";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshLayersAsync()
    {
        if (!_geoServerService.IsConnected)
        {
            HasError = true;
            ErrorMessage = "请先连接到 GeoServer";
            return;
        }

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            StatusMessage = "正在加载图层...";

            List<GeoServerLayer> layers;

            if (ShowAllLayers || SelectedWorkspace == null)
            {
                layers = await _geoServerService.GetLayersAsync();
            }
            else
            {
                layers = await _geoServerService.GetLayersAsync(SelectedWorkspace.Name);
            }

            Layers.Clear();
            foreach (var layer in layers)
            {
                Layers.Add(layer);
            }

            StatusMessage = $"已加载 {Layers.Count} 个图层";
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "加载失败";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadLayerDetailsAsync()
    {
        if (SelectedLayer == null)
        {
            LayerDetails = null;
            return;
        }

        if (!_geoServerService.IsConnected)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "正在加载图层详情...";

            LayerDetails = await _geoServerService.GetLayerAsync(SelectedLayer.Name);

            // 加载可用样式
            var styles = await _geoServerService.GetStylesAsync();
            AvailableStyles.Clear();
            foreach (var style in styles)
            {
                AvailableStyles.Add(style);
            }

            // 设置当前默认样式
            if (LayerDetails?.DefaultStyle != null)
            {
                SelectedDefaultStyle = AvailableStyles.FirstOrDefault(s => s.Name == LayerDetails.DefaultStyle.Name);
            }

            StatusMessage = "图层详情已加载";
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "加载详情失败";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshStylesAsync()
    {
        if (!_geoServerService.IsConnected)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "正在加载样式列表...";

            var styles = await _geoServerService.GetStylesAsync();
            AvailableStyles.Clear();
            foreach (var style in styles)
            {
                AvailableStyles.Add(style);
            }

            StatusMessage = $"已加载 {AvailableStyles.Count} 个样式";
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SetDefaultStyleAsync()
    {
        if (SelectedLayer == null || SelectedDefaultStyle == null)
        {
            HasError = true;
            ErrorMessage = "请选择图层和样式";
            return;
        }

        if (!_geoServerService.IsConnected)
        {
            HasError = true;
            ErrorMessage = "请先连接到 GeoServer";
            return;
        }

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            StatusMessage = "正在设置默认样式...";

            var (success, message) = await _geoServerService.SetLayerDefaultStyleAsync(
                SelectedLayer.Name,
                SelectedDefaultStyle.Name);

            if (success)
            {
                StatusMessage = $"已将图层 '{SelectedLayer.Name}' 的默认样式设置为 '{SelectedDefaultStyle.Name}'";
                await LoadLayerDetailsAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = message;
                StatusMessage = "设置失败";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "发生错误";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteLayerAsync()
    {
        if (SelectedLayer == null)
        {
            HasError = true;
            ErrorMessage = "请选择要删除的图层";
            return;
        }

        if (!_geoServerService.IsConnected)
        {
            HasError = true;
            ErrorMessage = "请先连接到 GeoServer";
            return;
        }

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            StatusMessage = "正在删除图层...";

            var (success, message) = await _geoServerService.DeleteLayerAsync(SelectedLayer.Name, DeleteRecursively);

            if (success)
            {
                StatusMessage = message;
                SelectedLayer = null;
                LayerDetails = null;
                await RefreshLayersAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = message;
                StatusMessage = "删除失败";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "发生错误";
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected override Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        return Task.FromResult(new ProcessingResult { Success = true });
    }

    protected override bool CanExecuteProcessing()
    {
        return _geoServerService.IsConnected && !IsBusy;
    }
}
