namespace GisToolbox.Models;

/// <summary>
///     处理结果
/// </summary>
public class ProcessingResult
{
    /// <summary>
    ///     是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     处理的要素数量
    /// </summary>
    public int ProcessedFeatures { get; set; }

    /// <summary>
    ///     输出文件路径
    /// </summary>
    public string? OutputFilePath { get; set; }

    /// <summary>
    ///     处理耗时（毫秒）
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    ///     详细信息
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    public static ProcessingResult CreateSuccess(string message, int processedFeatures = 0)
    {
        return new ProcessingResult
        {
            Success = true,
            Message = message,
            ProcessedFeatures = processedFeatures
        };
    }

    public static ProcessingResult CreateError(string errorMessage)
    {
        return new ProcessingResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Message = "处理失败"
        };
    }
}