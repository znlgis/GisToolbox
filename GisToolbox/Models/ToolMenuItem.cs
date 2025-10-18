namespace GisToolbox.Models;

/// <summary>
///     工具分类项
/// </summary>
public record ToolCategoryItem
{
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public ObservableCollection<ToolMenuItem> Tools { get; init; } = [];
}

/// <summary>
///     工具菜单项
/// </summary>
public record ToolMenuItem
{
    public required string Name { get; init; }
    public required Type ToolType { get; init; }
}
