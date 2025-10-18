namespace GisToolbox.Models;

/// <summary>
///     文件选择消息
/// </summary>
public class SelectFileMessage
{
    /// <summary>
    ///     是否为打开文件对话框（false 为保存对话框）
    /// </summary>
    public bool IsOpenDialog { get; set; }

    /// <summary>
    ///     文件路径回调
    /// </summary>
    public Action<string?>? Callback { get; set; }
}