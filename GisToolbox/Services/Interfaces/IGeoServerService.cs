using GisToolbox.Models.GeoServer;

namespace GisToolbox.Services.Interfaces;

/// <summary>
///     GeoServer REST API 服务接口
/// </summary>
public interface IGeoServerService
{
    /// <summary>
    ///     当前连接配置
    /// </summary>
    GeoServerConnection? CurrentConnection { get; }

    /// <summary>
    ///     是否已连接
    /// </summary>
    bool IsConnected { get; }

    #region 连接管理

    /// <summary>
    ///     测试连接
    /// </summary>
    Task<(bool Success, string Message, GeoServerInfo? Info)> TestConnectionAsync(GeoServerConnection connection);

    /// <summary>
    ///     连接到 GeoServer
    /// </summary>
    Task<(bool Success, string Message)> ConnectAsync(GeoServerConnection connection);

    /// <summary>
    ///     断开连接
    /// </summary>
    void Disconnect();

    #endregion

    #region 工作空间管理

    /// <summary>
    ///     获取所有工作空间
    /// </summary>
    Task<List<GeoServerWorkspace>> GetWorkspacesAsync();

    /// <summary>
    ///     获取工作空间详情
    /// </summary>
    Task<GeoServerWorkspace?> GetWorkspaceAsync(string workspaceName);

    /// <summary>
    ///     创建工作空间
    /// </summary>
    Task<(bool Success, string Message)> CreateWorkspaceAsync(string workspaceName, bool isolated = false);

    /// <summary>
    ///     更新工作空间
    /// </summary>
    Task<(bool Success, string Message)> UpdateWorkspaceAsync(string workspaceName, GeoServerWorkspace workspace);

    /// <summary>
    ///     删除工作空间
    /// </summary>
    Task<(bool Success, string Message)> DeleteWorkspaceAsync(string workspaceName, bool recurse = false);

    #endregion

    #region 数据存储管理

    /// <summary>
    ///     获取工作空间下的所有数据存储
    /// </summary>
    Task<List<GeoServerDataStore>> GetDataStoresAsync(string workspaceName);

    /// <summary>
    ///     获取数据存储详情
    /// </summary>
    Task<GeoServerDataStore?> GetDataStoreAsync(string workspaceName, string dataStoreName);

    /// <summary>
    ///     创建数据存储
    /// </summary>
    Task<(bool Success, string Message)> CreateDataStoreAsync(string workspaceName, GeoServerDataStore dataStore);

    /// <summary>
    ///     更新数据存储
    /// </summary>
    Task<(bool Success, string Message)> UpdateDataStoreAsync(string workspaceName, string dataStoreName, GeoServerDataStore dataStore);

    /// <summary>
    ///     删除数据存储
    /// </summary>
    Task<(bool Success, string Message)> DeleteDataStoreAsync(string workspaceName, string dataStoreName, bool recurse = false);

    /// <summary>
    ///     上传 Shapefile 创建数据存储
    /// </summary>
    Task<(bool Success, string Message)> UploadShapefileAsync(string workspaceName, string dataStoreName, string shapefilePath);

    #endregion

    #region 图层管理

    /// <summary>
    ///     获取所有图层
    /// </summary>
    Task<List<GeoServerLayer>> GetLayersAsync();

    /// <summary>
    ///     获取工作空间下的图层
    /// </summary>
    Task<List<GeoServerLayer>> GetLayersAsync(string workspaceName);

    /// <summary>
    ///     获取图层详情
    /// </summary>
    Task<GeoServerLayer?> GetLayerAsync(string layerName);

    /// <summary>
    ///     获取工作空间下的图层详情
    /// </summary>
    Task<GeoServerLayer?> GetLayerAsync(string workspaceName, string layerName);

    /// <summary>
    ///     更新图层
    /// </summary>
    Task<(bool Success, string Message)> UpdateLayerAsync(string layerName, GeoServerLayer layer);

    /// <summary>
    ///     删除图层
    /// </summary>
    Task<(bool Success, string Message)> DeleteLayerAsync(string layerName, bool recurse = false);

    #endregion

    #region 要素类型管理

    /// <summary>
    ///     获取数据存储下的所有要素类型
    /// </summary>
    Task<List<GeoServerFeatureType>> GetFeatureTypesAsync(string workspaceName, string dataStoreName);

    /// <summary>
    ///     获取要素类型详情
    /// </summary>
    Task<GeoServerFeatureType?> GetFeatureTypeAsync(string workspaceName, string dataStoreName, string featureTypeName);

    /// <summary>
    ///     发布要素类型（创建图层）
    /// </summary>
    Task<(bool Success, string Message)> PublishFeatureTypeAsync(string workspaceName, string dataStoreName, GeoServerFeatureType featureType);

    /// <summary>
    ///     删除要素类型
    /// </summary>
    Task<(bool Success, string Message)> DeleteFeatureTypeAsync(string workspaceName, string dataStoreName, string featureTypeName, bool recurse = false);

    #endregion

    #region 样式管理

    /// <summary>
    ///     获取所有样式
    /// </summary>
    Task<List<GeoServerStyle>> GetStylesAsync();

    /// <summary>
    ///     获取工作空间下的样式
    /// </summary>
    Task<List<GeoServerStyle>> GetStylesAsync(string workspaceName);

    /// <summary>
    ///     获取样式详情
    /// </summary>
    Task<GeoServerStyle?> GetStyleAsync(string styleName);

    /// <summary>
    ///     获取样式 SLD 内容
    /// </summary>
    Task<string?> GetStyleSldAsync(string styleName);

    /// <summary>
    ///     创建样式
    /// </summary>
    Task<(bool Success, string Message)> CreateStyleAsync(string styleName, string sldContent, string? workspaceName = null);

    /// <summary>
    ///     更新样式
    /// </summary>
    Task<(bool Success, string Message)> UpdateStyleAsync(string styleName, string sldContent);

    /// <summary>
    ///     删除样式
    /// </summary>
    Task<(bool Success, string Message)> DeleteStyleAsync(string styleName, bool purge = false);

    /// <summary>
    ///     为图层设置默认样式
    /// </summary>
    Task<(bool Success, string Message)> SetLayerDefaultStyleAsync(string layerName, string styleName);

    #endregion

    #region 栅格存储管理

    /// <summary>
    ///     获取工作空间下的所有栅格存储
    /// </summary>
    Task<List<GeoServerCoverageStore>> GetCoverageStoresAsync(string workspaceName);

    /// <summary>
    ///     获取栅格存储详情
    /// </summary>
    Task<GeoServerCoverageStore?> GetCoverageStoreAsync(string workspaceName, string coverageStoreName);

    /// <summary>
    ///     创建栅格存储
    /// </summary>
    Task<(bool Success, string Message)> CreateCoverageStoreAsync(string workspaceName, GeoServerCoverageStore coverageStore);

    /// <summary>
    ///     删除栅格存储
    /// </summary>
    Task<(bool Success, string Message)> DeleteCoverageStoreAsync(string workspaceName, string coverageStoreName, bool recurse = false);

    #endregion

    #region 图层组管理

    /// <summary>
    ///     获取所有图层组
    /// </summary>
    Task<List<GeoServerLayerGroup>> GetLayerGroupsAsync();

    /// <summary>
    ///     获取工作空间下的图层组
    /// </summary>
    Task<List<GeoServerLayerGroup>> GetLayerGroupsAsync(string workspaceName);

    /// <summary>
    ///     获取图层组详情
    /// </summary>
    Task<GeoServerLayerGroup?> GetLayerGroupAsync(string layerGroupName);

    /// <summary>
    ///     创建图层组
    /// </summary>
    Task<(bool Success, string Message)> CreateLayerGroupAsync(GeoServerLayerGroup layerGroup, string? workspaceName = null);

    /// <summary>
    ///     删除图层组
    /// </summary>
    Task<(bool Success, string Message)> DeleteLayerGroupAsync(string layerGroupName);

    #endregion
}
