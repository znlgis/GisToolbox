using CommunityToolkit.Mvvm.Messaging;
using GisToolbox.Models;
using GisToolbox.Models.GeoServer;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.GeoServerTools;

/// <summary>
///     GeoServer 数据存储管理 ViewModel
/// </summary>
public partial class DataStoreManagementViewModel : ToolViewModelBase
{
    private readonly IGeoServerService _geoServerService;

    [ObservableProperty] private ObservableCollection<GeoServerWorkspace> _workspaces = new();
    [ObservableProperty] private GeoServerWorkspace? _selectedWorkspace;
    [ObservableProperty] private ObservableCollection<GeoServerDataStore> _dataStores = new();
    [ObservableProperty] private GeoServerDataStore? _selectedDataStore;
    [ObservableProperty] private string _newDataStoreName = string.Empty;
    [ObservableProperty] private string _newDataStoreType = "Shapefile";
    [ObservableProperty] private string _shapefilePath = string.Empty;
    [ObservableProperty] private bool _deleteRecursively;

    public DataStoreManagementViewModel(IGeoServerService geoServerService)
    {
        _geoServerService = geoServerService;
    }

    public override string ToolName => "数据存储管理";
    public override string ToolDescription => "管理 GeoServer 数据存储，包括创建、查看和删除数据存储";
    public override ToolCategory Category => ToolCategory.WebGis;

    public bool IsConnected => _geoServerService.IsConnected;

    public List<string> DataStoreTypes { get; } = new()
    {
        "Shapefile",
        "PostGIS",
        "GeoPackage",
        "Directory of Shapefiles"
    };

    partial void OnSelectedWorkspaceChanged(GeoServerWorkspace? value)
    {
        if (value != null)
        {
            _ = RefreshDataStoresAsync();
        }
        else
        {
            DataStores.Clear();
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
    private async Task RefreshDataStoresAsync()
    {
        if (SelectedWorkspace == null)
        {
            DataStores.Clear();
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
            StatusMessage = "正在加载数据存储...";

            var dataStores = await _geoServerService.GetDataStoresAsync(SelectedWorkspace.Name);
            DataStores.Clear();
            foreach (var ds in dataStores)
            {
                DataStores.Add(ds);
            }

            StatusMessage = $"已加载 {DataStores.Count} 个数据存储";
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
    private void SelectShapefile()
    {
        WeakReferenceMessenger.Default.Send(new SelectFileMessage
        {
            IsOpenDialog = true,
            Callback = path =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    ShapefilePath = path;
                    // 如果数据存储名称为空，使用文件名作为默认名称
                    if (string.IsNullOrEmpty(NewDataStoreName))
                    {
                        NewDataStoreName = Path.GetFileNameWithoutExtension(path);
                    }
                }
            }
        });
    }

    [RelayCommand]
    private async Task UploadShapefileAsync()
    {
        if (SelectedWorkspace == null)
        {
            HasError = true;
            ErrorMessage = "请选择工作空间";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewDataStoreName))
        {
            HasError = true;
            ErrorMessage = "请输入数据存储名称";
            return;
        }

        if (string.IsNullOrWhiteSpace(ShapefilePath))
        {
            HasError = true;
            ErrorMessage = "请选择 Shapefile 文件";
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
            StatusMessage = "正在上传 Shapefile...";

            var (success, message) = await _geoServerService.UploadShapefileAsync(
                SelectedWorkspace.Name,
                NewDataStoreName,
                ShapefilePath);

            if (success)
            {
                StatusMessage = message;
                NewDataStoreName = string.Empty;
                ShapefilePath = string.Empty;
                await RefreshDataStoresAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = message;
                StatusMessage = "上传失败";
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
    private async Task DeleteDataStoreAsync()
    {
        if (SelectedWorkspace == null || SelectedDataStore == null)
        {
            HasError = true;
            ErrorMessage = "请选择要删除的数据存储";
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
            StatusMessage = "正在删除数据存储...";

            var (success, message) = await _geoServerService.DeleteDataStoreAsync(
                SelectedWorkspace.Name,
                SelectedDataStore.Name,
                DeleteRecursively);

            if (success)
            {
                StatusMessage = message;
                SelectedDataStore = null;
                await RefreshDataStoresAsync();
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
