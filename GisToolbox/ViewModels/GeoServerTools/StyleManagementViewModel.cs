using CommunityToolkit.Mvvm.Messaging;
using GisToolbox.Models;
using GisToolbox.Models.GeoServer;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.GeoServerTools;

/// <summary>
///     GeoServer 样式管理 ViewModel
/// </summary>
public partial class StyleManagementViewModel : ToolViewModelBase
{
    private readonly IGeoServerService _geoServerService;

    /// <summary>
    ///     示例 SLD 样式模板
    /// </summary>
    private const string SampleSldTemplate = """
        <?xml version="1.0" encoding="UTF-8"?>
        <StyledLayerDescriptor version="1.0.0" 
            xsi:schemaLocation="http://www.opengis.net/sld StyledLayerDescriptor.xsd" 
            xmlns="http://www.opengis.net/sld" 
            xmlns:ogc="http://www.opengis.net/ogc" 
            xmlns:xlink="http://www.w3.org/1999/xlink" 
            xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
          <NamedLayer>
            <Name>sample_style</Name>
            <UserStyle>
              <Title>Sample Style</Title>
              <FeatureTypeStyle>
                <Rule>
                  <Name>rule1</Name>
                  <Title>Blue Fill</Title>
                  <PolygonSymbolizer>
                    <Fill>
                      <CssParameter name="fill">#0000FF</CssParameter>
                      <CssParameter name="fill-opacity">0.5</CssParameter>
                    </Fill>
                    <Stroke>
                      <CssParameter name="stroke">#000000</CssParameter>
                      <CssParameter name="stroke-width">1</CssParameter>
                    </Stroke>
                  </PolygonSymbolizer>
                </Rule>
              </FeatureTypeStyle>
            </UserStyle>
          </NamedLayer>
        </StyledLayerDescriptor>
        """;

    [ObservableProperty] private ObservableCollection<GeoServerWorkspace> _workspaces = new();
    [ObservableProperty] private GeoServerWorkspace? _selectedWorkspace;
    [ObservableProperty] private ObservableCollection<GeoServerStyle> _styles = new();
    [ObservableProperty] private GeoServerStyle? _selectedStyle;
    [ObservableProperty] private string _sldContent = string.Empty;
    [ObservableProperty] private string _newStyleName = string.Empty;
    [ObservableProperty] private string _newSldContent = string.Empty;
    [ObservableProperty] private bool _showAllStyles = true;
    [ObservableProperty] private bool _purgeOnDelete;

    public StyleManagementViewModel(IGeoServerService geoServerService)
    {
        _geoServerService = geoServerService;
    }

    public override string ToolName => "样式管理";
    public override string ToolDescription => "管理 GeoServer 样式（SLD），包括创建、编辑和删除样式";
    public override ToolCategory Category => ToolCategory.WebGis;

    public bool IsConnected => _geoServerService.IsConnected;

    partial void OnSelectedWorkspaceChanged(GeoServerWorkspace? value)
    {
        _ = RefreshStylesAsync();
    }

    partial void OnShowAllStylesChanged(bool value)
    {
        _ = RefreshStylesAsync();
    }

    partial void OnSelectedStyleChanged(GeoServerStyle? value)
    {
        if (value != null)
        {
            _ = LoadStyleContentAsync();
        }
        else
        {
            SldContent = string.Empty;
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
    private async Task RefreshStylesAsync()
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
            StatusMessage = "正在加载样式...";

            List<GeoServerStyle> styles;

            if (ShowAllStyles || SelectedWorkspace == null)
            {
                styles = await _geoServerService.GetStylesAsync();
            }
            else
            {
                styles = await _geoServerService.GetStylesAsync(SelectedWorkspace.Name);
            }

            Styles.Clear();
            foreach (var style in styles)
            {
                Styles.Add(style);
            }

            StatusMessage = $"已加载 {Styles.Count} 个样式";
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
    private async Task LoadStyleContentAsync()
    {
        if (SelectedStyle == null)
        {
            SldContent = string.Empty;
            return;
        }

        if (!_geoServerService.IsConnected)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "正在加载样式内容...";

            var content = await _geoServerService.GetStyleSldAsync(SelectedStyle.Name);
            SldContent = content ?? string.Empty;

            StatusMessage = "样式内容已加载";
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "加载样式内容失败";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateStyleAsync()
    {
        if (string.IsNullOrWhiteSpace(NewStyleName))
        {
            HasError = true;
            ErrorMessage = "请输入样式名称";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewSldContent))
        {
            HasError = true;
            ErrorMessage = "请输入 SLD 样式内容";
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
            StatusMessage = "正在创建样式...";

            var (success, message) = await _geoServerService.CreateStyleAsync(
                NewStyleName,
                NewSldContent,
                ShowAllStyles ? null : SelectedWorkspace?.Name);

            if (success)
            {
                StatusMessage = message;
                NewStyleName = string.Empty;
                NewSldContent = string.Empty;
                await RefreshStylesAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = message;
                StatusMessage = "创建失败";
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
    private async Task UpdateStyleAsync()
    {
        if (SelectedStyle == null)
        {
            HasError = true;
            ErrorMessage = "请选择要更新的样式";
            return;
        }

        if (string.IsNullOrWhiteSpace(SldContent))
        {
            HasError = true;
            ErrorMessage = "样式内容不能为空";
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
            StatusMessage = "正在更新样式...";

            var (success, message) = await _geoServerService.UpdateStyleAsync(SelectedStyle.Name, SldContent);

            if (success)
            {
                StatusMessage = message;
            }
            else
            {
                HasError = true;
                ErrorMessage = message;
                StatusMessage = "更新失败";
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
    private async Task DeleteStyleAsync()
    {
        if (SelectedStyle == null)
        {
            HasError = true;
            ErrorMessage = "请选择要删除的样式";
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
            StatusMessage = "正在删除样式...";

            var (success, message) = await _geoServerService.DeleteStyleAsync(SelectedStyle.Name, PurgeOnDelete);

            if (success)
            {
                StatusMessage = message;
                SelectedStyle = null;
                SldContent = string.Empty;
                await RefreshStylesAsync();
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

    [RelayCommand]
    private void LoadSampleSld()
    {
        NewSldContent = SampleSldTemplate;
    }

    [RelayCommand]
    private void SelectSldFile()
    {
        WeakReferenceMessenger.Default.Send(new SelectFileMessage
        {
            IsOpenDialog = true,
            Callback = async path =>
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try
                    {
                        NewSldContent = await File.ReadAllTextAsync(path);
                        if (string.IsNullOrEmpty(NewStyleName))
                        {
                            NewStyleName = Path.GetFileNameWithoutExtension(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        HasError = true;
                        ErrorMessage = $"读取文件失败: {ex.Message}";
                    }
                }
            }
        });
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
