using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using GisToolbox.Models.GeoServer;
using GisToolbox.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GisToolbox.Services.Implementations;

/// <summary>
///     GeoServer REST API 服务实现
/// </summary>
public class GeoServerService : IGeoServerService
{
    private HttpClient? _httpClient;
    private readonly JsonSerializerSettings _jsonSettings;

    public GeoServerService()
    {
        _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    public GeoServerConnection? CurrentConnection { get; private set; }
    public bool IsConnected => CurrentConnection?.IsConnected ?? false;

    #region 连接管理

    public async Task<(bool Success, string Message, GeoServerInfo? Info)> TestConnectionAsync(GeoServerConnection connection)
    {
        try
        {
            using var client = CreateHttpClient(connection);
            var response = await client.GetAsync("about/version.json");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var info = new GeoServerInfo();

                try
                {
                    var aboutResponse = JsonConvert.DeserializeObject<AboutVersionResponse>(content, _jsonSettings);
                    if (aboutResponse?.About?.Resource?.Entry != null)
                    {
                        foreach (var entry in aboutResponse.About.Resource.Entry)
                        {
                            switch (entry.Key?.ToLower())
                            {
                                case "version":
                                    info.Version = entry.Value;
                                    break;
                                case "build-timestamp":
                                case "build timestamp":
                                    info.BuildTimestamp = entry.Value;
                                    break;
                                case "git-revision":
                                case "git revision":
                                    info.GitRevision = entry.Value;
                                    break;
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // 忽略 JSON 解析错误，只要能连接就算成功
                    // 某些 GeoServer 版本可能返回不同格式的响应
                }

                return (true, "连接成功", info);
            }

            return (false, $"连接失败: HTTP {(int)response.StatusCode} {response.ReasonPhrase}", null);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"连接失败: {ex.Message}", null);
        }
        catch (Exception ex)
        {
            return (false, $"连接错误: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> ConnectAsync(GeoServerConnection connection)
    {
        var (success, message, _) = await TestConnectionAsync(connection);

        if (success)
        {
            CurrentConnection = connection;
            CurrentConnection.IsConnected = true;
            _httpClient = CreateHttpClient(connection);
        }

        return (success, message);
    }

    public void Disconnect()
    {
        if (CurrentConnection != null)
        {
            CurrentConnection.IsConnected = false;
        }
        CurrentConnection = null;
        _httpClient?.Dispose();
        _httpClient = null;
    }

    private HttpClient CreateHttpClient(GeoServerConnection connection)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(connection.RestApiUrl + "/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{connection.Username}:{connection.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }

    private HttpClient GetClient()
    {
        if (_httpClient == null || CurrentConnection == null)
            throw new InvalidOperationException("未连接到 GeoServer");
        return _httpClient;
    }

    #endregion

    #region 工作空间管理

    public async Task<List<GeoServerWorkspace>> GetWorkspacesAsync()
    {
        var client = GetClient();
        var response = await client.GetAsync("workspaces.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerWorkspace>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<WorkspacesResponse>(content, _jsonSettings);

        return result?.Workspaces?.Workspace ?? new List<GeoServerWorkspace>();
    }

    public async Task<GeoServerWorkspace?> GetWorkspaceAsync(string workspaceName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}.json");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<WorkspaceResponse>(content, _jsonSettings);

        return result?.Workspace;
    }

    public async Task<(bool Success, string Message)> CreateWorkspaceAsync(string workspaceName, bool isolated = false)
    {
        var client = GetClient();

        var workspace = new
        {
            workspace = new
            {
                name = workspaceName,
                isolated
            }
        };

        var json = JsonConvert.SerializeObject(workspace, _jsonSettings);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("workspaces", httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"工作空间 '{workspaceName}' 创建成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"创建工作空间失败: {error}");
    }

    public async Task<(bool Success, string Message)> UpdateWorkspaceAsync(string workspaceName, GeoServerWorkspace workspace)
    {
        var client = GetClient();

        var payload = new { workspace };
        var json = JsonConvert.SerializeObject(payload, _jsonSettings);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}", httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"工作空间 '{workspaceName}' 更新成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"更新工作空间失败: {error}");
    }

    public async Task<(bool Success, string Message)> DeleteWorkspaceAsync(string workspaceName, bool recurse = false)
    {
        var client = GetClient();

        var url = $"workspaces/{Uri.EscapeDataString(workspaceName)}?recurse={recurse.ToString().ToLower()}";
        var response = await client.DeleteAsync(url);

        if (response.IsSuccessStatusCode)
            return (true, $"工作空间 '{workspaceName}' 删除成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"删除工作空间失败: {error}");
    }

    #endregion

    #region 数据存储管理

    public async Task<List<GeoServerDataStore>> GetDataStoresAsync(string workspaceName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerDataStore>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<DataStoresResponse>(content, _jsonSettings);

        return result?.DataStores?.DataStore ?? new List<GeoServerDataStore>();
    }

    public async Task<GeoServerDataStore?> GetDataStoreAsync(string workspaceName, string dataStoreName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores/{Uri.EscapeDataString(dataStoreName)}.json");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<DataStoreResponse>(content, _jsonSettings);

        return result?.DataStore;
    }

    public async Task<(bool Success, string Message)> CreateDataStoreAsync(string workspaceName, GeoServerDataStore dataStore)
    {
        var client = GetClient();

        var payload = new { dataStore };
        var json = JsonConvert.SerializeObject(payload, _jsonSettings);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores", httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"数据存储 '{dataStore.Name}' 创建成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"创建数据存储失败: {error}");
    }

    public async Task<(bool Success, string Message)> UpdateDataStoreAsync(string workspaceName, string dataStoreName, GeoServerDataStore dataStore)
    {
        var client = GetClient();

        var payload = new { dataStore };
        var json = JsonConvert.SerializeObject(payload, _jsonSettings);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores/{Uri.EscapeDataString(dataStoreName)}", httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"数据存储 '{dataStoreName}' 更新成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"更新数据存储失败: {error}");
    }

    public async Task<(bool Success, string Message)> DeleteDataStoreAsync(string workspaceName, string dataStoreName, bool recurse = false)
    {
        var client = GetClient();

        var url = $"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores/{Uri.EscapeDataString(dataStoreName)}?recurse={recurse.ToString().ToLower()}";
        var response = await client.DeleteAsync(url);

        if (response.IsSuccessStatusCode)
            return (true, $"数据存储 '{dataStoreName}' 删除成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"删除数据存储失败: {error}");
    }

    public async Task<(bool Success, string Message)> UploadShapefileAsync(string workspaceName, string dataStoreName, string shapefilePath)
    {
        if (!File.Exists(shapefilePath))
            return (false, $"文件不存在: {shapefilePath}");

        // 验证文件扩展名
        var extension = Path.GetExtension(shapefilePath).ToLower();
        if (extension != ".zip" && extension != ".shp")
            return (false, "仅支持 .zip 或 .shp 文件格式");

        var client = GetClient();

        // 上传 ZIP 文件包含 Shapefile
        var fileBytes = await File.ReadAllBytesAsync(shapefilePath);
        var httpContent = new ByteArrayContent(fileBytes);

        // 根据文件扩展名设置 Content-Type
        var contentType = extension switch
        {
            ".zip" => "application/zip",
            ".shp" => "application/x-shp",
            _ => "application/octet-stream"
        };
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var url = $"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores/{Uri.EscapeDataString(dataStoreName)}/file.shp";
        var response = await client.PutAsync(url, httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"Shapefile 上传成功并创建数据存储 '{dataStoreName}'");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"上传 Shapefile 失败: {error}");
    }

    #endregion

    #region 图层管理

    public async Task<List<GeoServerLayer>> GetLayersAsync()
    {
        var client = GetClient();
        var response = await client.GetAsync("layers.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerLayer>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<LayersResponse>(content, _jsonSettings);

        return result?.Layers?.Layer ?? new List<GeoServerLayer>();
    }

    public async Task<List<GeoServerLayer>> GetLayersAsync(string workspaceName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/layers.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerLayer>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<LayersResponse>(content, _jsonSettings);

        return result?.Layers?.Layer ?? new List<GeoServerLayer>();
    }

    public async Task<GeoServerLayer?> GetLayerAsync(string layerName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"layers/{Uri.EscapeDataString(layerName)}.json");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<LayerResponse>(content, _jsonSettings);

        return result?.Layer;
    }

    public async Task<GeoServerLayer?> GetLayerAsync(string workspaceName, string layerName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/layers/{Uri.EscapeDataString(layerName)}.json");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<LayerResponse>(content, _jsonSettings);

        return result?.Layer;
    }

    public async Task<(bool Success, string Message)> UpdateLayerAsync(string layerName, GeoServerLayer layer)
    {
        var client = GetClient();

        var payload = new { layer };
        var json = JsonConvert.SerializeObject(payload, _jsonSettings);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"layers/{Uri.EscapeDataString(layerName)}", httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"图层 '{layerName}' 更新成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"更新图层失败: {error}");
    }

    public async Task<(bool Success, string Message)> DeleteLayerAsync(string layerName, bool recurse = false)
    {
        var client = GetClient();

        var url = $"layers/{Uri.EscapeDataString(layerName)}?recurse={recurse.ToString().ToLower()}";
        var response = await client.DeleteAsync(url);

        if (response.IsSuccessStatusCode)
            return (true, $"图层 '{layerName}' 删除成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"删除图层失败: {error}");
    }

    #endregion

    #region 要素类型管理

    public async Task<List<GeoServerFeatureType>> GetFeatureTypesAsync(string workspaceName, string dataStoreName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores/{Uri.EscapeDataString(dataStoreName)}/featuretypes.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerFeatureType>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FeatureTypesResponse>(content, _jsonSettings);

        return result?.FeatureTypes?.FeatureType ?? new List<GeoServerFeatureType>();
    }

    public async Task<GeoServerFeatureType?> GetFeatureTypeAsync(string workspaceName, string dataStoreName, string featureTypeName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores/{Uri.EscapeDataString(dataStoreName)}/featuretypes/{Uri.EscapeDataString(featureTypeName)}.json");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FeatureTypeResponse>(content, _jsonSettings);

        return result?.FeatureType;
    }

    public async Task<(bool Success, string Message)> PublishFeatureTypeAsync(string workspaceName, string dataStoreName, GeoServerFeatureType featureType)
    {
        var client = GetClient();

        var payload = new { featureType };
        var json = JsonConvert.SerializeObject(payload, _jsonSettings);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores/{Uri.EscapeDataString(dataStoreName)}/featuretypes", httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"要素类型 '{featureType.Name}' 发布成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"发布要素类型失败: {error}");
    }

    public async Task<(bool Success, string Message)> DeleteFeatureTypeAsync(string workspaceName, string dataStoreName, string featureTypeName, bool recurse = false)
    {
        var client = GetClient();

        var url = $"workspaces/{Uri.EscapeDataString(workspaceName)}/datastores/{Uri.EscapeDataString(dataStoreName)}/featuretypes/{Uri.EscapeDataString(featureTypeName)}?recurse={recurse.ToString().ToLower()}";
        var response = await client.DeleteAsync(url);

        if (response.IsSuccessStatusCode)
            return (true, $"要素类型 '{featureTypeName}' 删除成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"删除要素类型失败: {error}");
    }

    #endregion

    #region 样式管理

    public async Task<List<GeoServerStyle>> GetStylesAsync()
    {
        var client = GetClient();
        var response = await client.GetAsync("styles.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerStyle>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<StylesResponse>(content, _jsonSettings);

        return result?.Styles?.Style ?? new List<GeoServerStyle>();
    }

    public async Task<List<GeoServerStyle>> GetStylesAsync(string workspaceName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/styles.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerStyle>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<StylesResponse>(content, _jsonSettings);

        return result?.Styles?.Style ?? new List<GeoServerStyle>();
    }

    public async Task<GeoServerStyle?> GetStyleAsync(string styleName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"styles/{Uri.EscapeDataString(styleName)}.json");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<StyleResponse>(content, _jsonSettings);

        return result?.Style;
    }

    public async Task<string?> GetStyleSldAsync(string styleName)
    {
        var client = GetClient();

        // 创建一个新的请求来获取 SLD
        using var request = new HttpRequestMessage(HttpMethod.Get, $"styles/{Uri.EscapeDataString(styleName)}.sld");
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.ogc.sld+xml"));

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<(bool Success, string Message)> CreateStyleAsync(string styleName, string sldContent, string? workspaceName = null)
    {
        var client = GetClient();

        // 首先创建样式元数据
        var styleMetadata = new
        {
            style = new
            {
                name = styleName,
                filename = $"{styleName}.sld"
            }
        };

        var metadataJson = JsonConvert.SerializeObject(styleMetadata, _jsonSettings);
        var metadataContent = new StringContent(metadataJson, Encoding.UTF8, "application/json");

        var baseUrl = workspaceName != null
            ? $"workspaces/{Uri.EscapeDataString(workspaceName)}/styles"
            : "styles";

        var createResponse = await client.PostAsync(baseUrl, metadataContent);

        if (!createResponse.IsSuccessStatusCode)
        {
            var error = await createResponse.Content.ReadAsStringAsync();
            return (false, $"创建样式元数据失败: {error}");
        }

        // 然后上传 SLD 内容
        var sldHttpContent = new StringContent(sldContent, Encoding.UTF8, "application/vnd.ogc.sld+xml");

        var uploadUrl = workspaceName != null
            ? $"workspaces/{Uri.EscapeDataString(workspaceName)}/styles/{Uri.EscapeDataString(styleName)}"
            : $"styles/{Uri.EscapeDataString(styleName)}";

        var uploadResponse = await client.PutAsync(uploadUrl, sldHttpContent);

        if (uploadResponse.IsSuccessStatusCode)
            return (true, $"样式 '{styleName}' 创建成功");

        var uploadError = await uploadResponse.Content.ReadAsStringAsync();
        return (false, $"上传 SLD 内容失败: {uploadError}");
    }

    public async Task<(bool Success, string Message)> UpdateStyleAsync(string styleName, string sldContent)
    {
        var client = GetClient();

        var httpContent = new StringContent(sldContent, Encoding.UTF8, "application/vnd.ogc.sld+xml");

        var response = await client.PutAsync($"styles/{Uri.EscapeDataString(styleName)}", httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"样式 '{styleName}' 更新成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"更新样式失败: {error}");
    }

    public async Task<(bool Success, string Message)> DeleteStyleAsync(string styleName, bool purge = false)
    {
        var client = GetClient();

        var url = $"styles/{Uri.EscapeDataString(styleName)}?purge={purge.ToString().ToLower()}";
        var response = await client.DeleteAsync(url);

        if (response.IsSuccessStatusCode)
            return (true, $"样式 '{styleName}' 删除成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"删除样式失败: {error}");
    }

    public async Task<(bool Success, string Message)> SetLayerDefaultStyleAsync(string layerName, string styleName)
    {
        var layer = await GetLayerAsync(layerName);
        if (layer == null)
            return (false, $"图层 '{layerName}' 不存在");

        layer.DefaultStyle = new GeoServerStyle { Name = styleName };

        return await UpdateLayerAsync(layerName, layer);
    }

    #endregion

    #region 栅格存储管理

    public async Task<List<GeoServerCoverageStore>> GetCoverageStoresAsync(string workspaceName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/coveragestores.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerCoverageStore>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<CoverageStoresResponse>(content, _jsonSettings);

        return result?.CoverageStores?.CoverageStore ?? new List<GeoServerCoverageStore>();
    }

    public async Task<GeoServerCoverageStore?> GetCoverageStoreAsync(string workspaceName, string coverageStoreName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/coveragestores/{Uri.EscapeDataString(coverageStoreName)}.json");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<CoverageStoreResponse>(content, _jsonSettings);

        return result?.CoverageStore;
    }

    public async Task<(bool Success, string Message)> CreateCoverageStoreAsync(string workspaceName, GeoServerCoverageStore coverageStore)
    {
        var client = GetClient();

        var payload = new { coverageStore };
        var json = JsonConvert.SerializeObject(payload, _jsonSettings);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/coveragestores", httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"栅格存储 '{coverageStore.Name}' 创建成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"创建栅格存储失败: {error}");
    }

    public async Task<(bool Success, string Message)> DeleteCoverageStoreAsync(string workspaceName, string coverageStoreName, bool recurse = false)
    {
        var client = GetClient();

        var url = $"workspaces/{Uri.EscapeDataString(workspaceName)}/coveragestores/{Uri.EscapeDataString(coverageStoreName)}?recurse={recurse.ToString().ToLower()}";
        var response = await client.DeleteAsync(url);

        if (response.IsSuccessStatusCode)
            return (true, $"栅格存储 '{coverageStoreName}' 删除成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"删除栅格存储失败: {error}");
    }

    #endregion

    #region 图层组管理

    public async Task<List<GeoServerLayerGroup>> GetLayerGroupsAsync()
    {
        var client = GetClient();
        var response = await client.GetAsync("layergroups.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerLayerGroup>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<LayerGroupsResponse>(content, _jsonSettings);

        return result?.LayerGroups?.LayerGroup ?? new List<GeoServerLayerGroup>();
    }

    public async Task<List<GeoServerLayerGroup>> GetLayerGroupsAsync(string workspaceName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"workspaces/{Uri.EscapeDataString(workspaceName)}/layergroups.json");

        if (!response.IsSuccessStatusCode)
            return new List<GeoServerLayerGroup>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<LayerGroupsResponse>(content, _jsonSettings);

        return result?.LayerGroups?.LayerGroup ?? new List<GeoServerLayerGroup>();
    }

    public async Task<GeoServerLayerGroup?> GetLayerGroupAsync(string layerGroupName)
    {
        var client = GetClient();
        var response = await client.GetAsync($"layergroups/{Uri.EscapeDataString(layerGroupName)}.json");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<LayerGroupResponse>(content, _jsonSettings);

        return result?.LayerGroup;
    }

    public async Task<(bool Success, string Message)> CreateLayerGroupAsync(GeoServerLayerGroup layerGroup, string? workspaceName = null)
    {
        var client = GetClient();

        var payload = new { layerGroup };
        var json = JsonConvert.SerializeObject(payload, _jsonSettings);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var url = workspaceName != null
            ? $"workspaces/{Uri.EscapeDataString(workspaceName)}/layergroups"
            : "layergroups";

        var response = await client.PostAsync(url, httpContent);

        if (response.IsSuccessStatusCode)
            return (true, $"图层组 '{layerGroup.Name}' 创建成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"创建图层组失败: {error}");
    }

    public async Task<(bool Success, string Message)> DeleteLayerGroupAsync(string layerGroupName)
    {
        var client = GetClient();

        var response = await client.DeleteAsync($"layergroups/{Uri.EscapeDataString(layerGroupName)}");

        if (response.IsSuccessStatusCode)
            return (true, $"图层组 '{layerGroupName}' 删除成功");

        var error = await response.Content.ReadAsStringAsync();
        return (false, $"删除图层组失败: {error}");
    }

    #endregion
}
