using GisToolbox.Models;
using GisToolbox.Models.GeoServer;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.GeoServerTools;

/// <summary>
///     GeoServer 工作空间管理 ViewModel
/// </summary>
public partial class WorkspaceManagementViewModel : ToolViewModelBase
{
    private readonly IGeoServerService _geoServerService;

    [ObservableProperty] private ObservableCollection<GeoServerWorkspace> _workspaces = new();
    [ObservableProperty] private GeoServerWorkspace? _selectedWorkspace;
    [ObservableProperty] private string _newWorkspaceName = string.Empty;
    [ObservableProperty] private bool _newWorkspaceIsolated;
    [ObservableProperty] private bool _deleteRecursively;

    public WorkspaceManagementViewModel(IGeoServerService geoServerService)
    {
        _geoServerService = geoServerService;
    }

    public override string ToolName => "工作空间管理";
    public override string ToolDescription => "管理 GeoServer 工作空间，包括创建、查看和删除工作空间";
    public override ToolCategory Category => ToolCategory.WebGis;

    public bool IsConnected => _geoServerService.IsConnected;

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
    private async Task CreateWorkspaceAsync()
    {
        if (string.IsNullOrWhiteSpace(NewWorkspaceName))
        {
            HasError = true;
            ErrorMessage = "请输入工作空间名称";
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
            StatusMessage = "正在创建工作空间...";

            var (success, message) = await _geoServerService.CreateWorkspaceAsync(NewWorkspaceName, NewWorkspaceIsolated);

            if (success)
            {
                StatusMessage = message;
                NewWorkspaceName = string.Empty;
                NewWorkspaceIsolated = false;
                await RefreshWorkspacesAsync();
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
    private async Task DeleteWorkspaceAsync()
    {
        if (SelectedWorkspace == null)
        {
            HasError = true;
            ErrorMessage = "请选择要删除的工作空间";
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
            StatusMessage = "正在删除工作空间...";

            var (success, message) = await _geoServerService.DeleteWorkspaceAsync(SelectedWorkspace.Name, DeleteRecursively);

            if (success)
            {
                StatusMessage = message;
                SelectedWorkspace = null;
                await RefreshWorkspacesAsync();
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
