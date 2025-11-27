using GisToolbox.Models;
using GisToolbox.Models.GeoServer;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels.Base;

namespace GisToolbox.ViewModels.GeoServerTools;

/// <summary>
///     GeoServer 连接管理 ViewModel
/// </summary>
public partial class GeoServerConnectionViewModel : ToolViewModelBase
{
    private readonly IGeoServerService _geoServerService;

    [ObservableProperty] private string _serverUrl = "http://localhost:8080/geoserver";
    [ObservableProperty] private string _username = "admin";
    [ObservableProperty] private string _password = "geoserver";
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _serverVersion = string.Empty;
    [ObservableProperty] private string _buildTimestamp = string.Empty;
    [ObservableProperty] private string _connectionName = "默认连接";

    public GeoServerConnectionViewModel(IGeoServerService geoServerService)
    {
        _geoServerService = geoServerService;
        UpdateConnectionStatus();
    }

    public override string ToolName => "GeoServer 连接";
    public override string ToolDescription => "管理 GeoServer 服务器连接，测试连接状态";
    public override ToolCategory Category => ToolCategory.WebGis;

    private void UpdateConnectionStatus()
    {
        IsConnected = _geoServerService.IsConnected;
        if (_geoServerService.CurrentConnection != null)
        {
            ServerUrl = _geoServerService.CurrentConnection.Url;
            Username = _geoServerService.CurrentConnection.Username;
            ConnectionName = _geoServerService.CurrentConnection.Name;
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            StatusMessage = "正在测试连接...";

            var connection = new GeoServerConnection
            {
                Name = ConnectionName,
                Url = ServerUrl,
                Username = Username,
                Password = Password
            };

            var (success, message, info) = await _geoServerService.TestConnectionAsync(connection);

            if (success)
            {
                StatusMessage = message;
                if (info != null)
                {
                    ServerVersion = info.Version ?? "未知";
                    BuildTimestamp = info.BuildTimestamp ?? "未知";
                }
            }
            else
            {
                HasError = true;
                ErrorMessage = message;
                StatusMessage = "连接测试失败";
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
    private async Task ConnectAsync()
    {
        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            StatusMessage = "正在连接...";

            var connection = new GeoServerConnection
            {
                Name = ConnectionName,
                Url = ServerUrl,
                Username = Username,
                Password = Password
            };

            var (success, message) = await _geoServerService.ConnectAsync(connection);

            if (success)
            {
                IsConnected = true;
                StatusMessage = "已连接到 GeoServer";

                // 获取版本信息
                var (_, _, info) = await _geoServerService.TestConnectionAsync(connection);
                if (info != null)
                {
                    ServerVersion = info.Version ?? "未知";
                    BuildTimestamp = info.BuildTimestamp ?? "未知";
                }
            }
            else
            {
                HasError = true;
                ErrorMessage = message;
                StatusMessage = "连接失败";
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
    private void Disconnect()
    {
        _geoServerService.Disconnect();
        IsConnected = false;
        ServerVersion = string.Empty;
        BuildTimestamp = string.Empty;
        StatusMessage = "已断开连接";
    }

    protected override Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress)
    {
        // 这个工具不使用标准的处理流程
        return Task.FromResult(new ProcessingResult { Success = true });
    }

    protected override bool CanExecuteProcessing()
    {
        return !string.IsNullOrEmpty(ServerUrl) && !string.IsNullOrEmpty(Username) && !IsBusy;
    }
}
