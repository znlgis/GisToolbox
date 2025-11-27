namespace GisToolbox.Models.GeoServer;

/// <summary>
///     GeoServer 连接配置
/// </summary>
public class GeoServerConnection : ObservableObject
{
    private string _name = string.Empty;
    private string _url = "http://localhost:8080/geoserver";
    private string _username = "admin";
    private string _password = "geoserver";
    private bool _isConnected;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public string RestApiUrl => Url.TrimEnd('/') + "/rest";
}

/// <summary>
///     工作空间
/// </summary>
public class GeoServerWorkspace
{
    public string Name { get; set; } = string.Empty;
    public string? Href { get; set; }
    public bool Isolated { get; set; }
}

/// <summary>
///     工作空间列表响应
/// </summary>
public class WorkspacesResponse
{
    public WorkspacesWrapper? Workspaces { get; set; }
}

public class WorkspacesWrapper
{
    public List<GeoServerWorkspace>? Workspace { get; set; }
}

/// <summary>
///     单个工作空间响应
/// </summary>
public class WorkspaceResponse
{
    public GeoServerWorkspace? Workspace { get; set; }
}

/// <summary>
///     数据存储
/// </summary>
public class GeoServerDataStore
{
    public string Name { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string? Type { get; set; }
    public bool? Enabled { get; set; }
    public GeoServerWorkspace? Workspace { get; set; }
    public Dictionary<string, string>? ConnectionParameters { get; set; }
    public FeatureTypesLink? FeatureTypes { get; set; }
}

public class FeatureTypesLink
{
    public string? Href { get; set; }
}

/// <summary>
///     数据存储列表响应
/// </summary>
public class DataStoresResponse
{
    public DataStoresWrapper? DataStores { get; set; }
}

public class DataStoresWrapper
{
    public List<GeoServerDataStore>? DataStore { get; set; }
}

/// <summary>
///     单个数据存储响应
/// </summary>
public class DataStoreResponse
{
    public GeoServerDataStore? DataStore { get; set; }
}

/// <summary>
///     图层
/// </summary>
public class GeoServerLayer
{
    public string Name { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string? Type { get; set; }
    public GeoServerLayerResource? Resource { get; set; }
    public GeoServerStyle? DefaultStyle { get; set; }
    public GeoServerStylesWrapper? Styles { get; set; }
    public bool? Queryable { get; set; }
    public bool? Opaque { get; set; }
}

public class GeoServerLayerResource
{
    public string? Class { get; set; }
    public string? Name { get; set; }
    public string? Href { get; set; }
}

public class GeoServerStylesWrapper
{
    public List<GeoServerStyle>? Style { get; set; }
}

/// <summary>
///     图层列表响应
/// </summary>
public class LayersResponse
{
    public LayersWrapper? Layers { get; set; }
}

public class LayersWrapper
{
    public List<GeoServerLayer>? Layer { get; set; }
}

/// <summary>
///     单个图层响应
/// </summary>
public class LayerResponse
{
    public GeoServerLayer? Layer { get; set; }
}

/// <summary>
///     样式
/// </summary>
public class GeoServerStyle
{
    public string Name { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string? Format { get; set; }
    public string? LanguageVersion { get; set; }
    public string? Filename { get; set; }
    public GeoServerWorkspace? Workspace { get; set; }
}

/// <summary>
///     样式列表响应
/// </summary>
public class StylesResponse
{
    public StylesWrapper? Styles { get; set; }
}

public class StylesWrapper
{
    public List<GeoServerStyle>? Style { get; set; }
}

/// <summary>
///     单个样式响应
/// </summary>
public class StyleResponse
{
    public GeoServerStyle? Style { get; set; }
}

/// <summary>
///     要素类型
/// </summary>
public class GeoServerFeatureType
{
    public string Name { get; set; } = string.Empty;
    public string? NativeName { get; set; }
    public string? Href { get; set; }
    public string? Title { get; set; }
    public string? Abstract { get; set; }
    public string? Srs { get; set; }
    public BoundingBox? NativeBoundingBox { get; set; }
    public BoundingBox? LatLonBoundingBox { get; set; }
    public bool? Enabled { get; set; }
    public GeoServerDataStore? Store { get; set; }
}

public class BoundingBox
{
    public double MinX { get; set; }
    public double MinY { get; set; }
    public double MaxX { get; set; }
    public double MaxY { get; set; }
    public string? Crs { get; set; }
}

/// <summary>
///     要素类型列表响应
/// </summary>
public class FeatureTypesResponse
{
    public FeatureTypesWrapper? FeatureTypes { get; set; }
}

public class FeatureTypesWrapper
{
    public List<GeoServerFeatureType>? FeatureType { get; set; }
}

/// <summary>
///     单个要素类型响应
/// </summary>
public class FeatureTypeResponse
{
    public GeoServerFeatureType? FeatureType { get; set; }
}

/// <summary>
///     栅格存储
/// </summary>
public class GeoServerCoverageStore
{
    public string Name { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string? Type { get; set; }
    public string? Url { get; set; }
    public bool? Enabled { get; set; }
    public GeoServerWorkspace? Workspace { get; set; }
}

/// <summary>
///     栅格存储列表响应
/// </summary>
public class CoverageStoresResponse
{
    public CoverageStoresWrapper? CoverageStores { get; set; }
}

public class CoverageStoresWrapper
{
    public List<GeoServerCoverageStore>? CoverageStore { get; set; }
}

/// <summary>
///     单个栅格存储响应
/// </summary>
public class CoverageStoreResponse
{
    public GeoServerCoverageStore? CoverageStore { get; set; }
}

/// <summary>
///     图层组
/// </summary>
public class GeoServerLayerGroup
{
    public string Name { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string? Title { get; set; }
    public string? Mode { get; set; }
    public GeoServerWorkspace? Workspace { get; set; }
    public LayerGroupLayers? PublishedInfo { get; set; }
    public LayerGroupStyles? Styles { get; set; }
    public BoundingBox? Bounds { get; set; }
}

public class LayerGroupLayers
{
    public List<LayerGroupPublished>? Published { get; set; }
}

public class LayerGroupPublished
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Href { get; set; }
}

public class LayerGroupStyles
{
    public List<GeoServerStyle>? Style { get; set; }
}

/// <summary>
///     图层组列表响应
/// </summary>
public class LayerGroupsResponse
{
    public LayerGroupsWrapper? LayerGroups { get; set; }
}

public class LayerGroupsWrapper
{
    public List<GeoServerLayerGroup>? LayerGroup { get; set; }
}

/// <summary>
///     单个图层组响应
/// </summary>
public class LayerGroupResponse
{
    public GeoServerLayerGroup? LayerGroup { get; set; }
}

/// <summary>
///     GeoServer 服务器信息
/// </summary>
public class GeoServerInfo
{
    public string? Version { get; set; }
    public string? BuildTimestamp { get; set; }
    public string? GitRevision { get; set; }
}

/// <summary>
///     GeoServer 关于信息响应
/// </summary>
public class AboutVersionResponse
{
    public AboutVersion? About { get; set; }
}

public class AboutVersion
{
    public AboutResource? Resource { get; set; }
}

public class AboutResource
{
    public string? Name { get; set; }
    public List<AboutEntry>? Entry { get; set; }
}

public class AboutEntry
{
    public string? Key { get; set; }
    public string? Value { get; set; }
}
