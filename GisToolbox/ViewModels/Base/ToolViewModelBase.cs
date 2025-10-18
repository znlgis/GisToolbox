using CommunityToolkit.Mvvm.Messaging;
using GisToolbox.Models;

namespace GisToolbox.ViewModels.Base;

/// <summary>
///     工具ViewModel基类
/// </summary>
public abstract partial class ToolViewModelBase : ObservableObject
{
    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private bool _hasError;

    [ObservableProperty] private string _inputFilePath = string.Empty;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private string _outputFilePath = string.Empty;

    [ObservableProperty] private int _progressValue;

    [ObservableProperty] private string _statusMessage = "就绪";

    /// <summary>
    ///     工具名称
    /// </summary>
    public abstract string ToolName { get; }

    /// <summary>
    ///     工具描述
    /// </summary>
    public abstract string ToolDescription { get; }

    /// <summary>
    ///     工具分类
    /// </summary>
    public abstract ToolCategory Category { get; }

    [RelayCommand]
    private void SelectInputFile()
    {
        // 发送消息请求打开文件对话框
        WeakReferenceMessenger.Default.Send(new SelectFileMessage
        {
            IsOpenDialog = true,
            Callback = path =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    InputFilePath = path;
                    OnInputFileChanged();
                }
            }
        });
    }

    [RelayCommand]
    private void SelectOutputFile()
    {
        // 发送消息请求保存文件对话框
        WeakReferenceMessenger.Default.Send(new SelectFileMessage
        {
            IsOpenDialog = false,
            Callback = path =>
            {
                if (!string.IsNullOrEmpty(path))
                    OutputFilePath = path;
            }
        });
    }

    [RelayCommand(CanExecute = nameof(CanExecuteProcessing))]
    private async Task ProcessAsync()
    {
        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            ProgressValue = 0;
            StatusMessage = "处理中...";

            var progress = new Progress<int>(value => ProgressValue = value);
            var result = await ExecuteProcessingAsync(progress);

            (StatusMessage, HasError, ErrorMessage, ProgressValue) = result.Success
                ? (result.Message, false, string.Empty, 100)
                : ("处理失败", true, result.ErrorMessage ?? "处理失败", ProgressValue);
        }
        catch (Exception ex)
        {
            (HasError, ErrorMessage, StatusMessage) = (true, ex.Message, "发生错误");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    ///     执行具体的处理逻辑（由子类实现）
    /// </summary>
    protected abstract Task<ProcessingResult> ExecuteProcessingAsync(IProgress<int> progress);

    /// <summary>
    ///     检查是否可以执行处理
    /// </summary>
    protected virtual bool CanExecuteProcessing()
    {
        return !string.IsNullOrEmpty(InputFilePath) &&
               !string.IsNullOrEmpty(OutputFilePath) &&
               !IsBusy;
    }

    /// <summary>
    ///     输入文件改变时的处理（子类可重写）
    /// </summary>
    protected virtual void OnInputFileChanged()
    {
        ProcessCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void Reset()
    {
        InputFilePath = string.Empty;
        OutputFilePath = string.Empty;
        ProgressValue = 0;
        StatusMessage = "就绪";
        HasError = false;
        ErrorMessage = string.Empty;
        OnReset();
    }

    /// <summary>
    ///     重置时的额外处理（子类可重写）
    /// </summary>
    protected virtual void OnReset()
    {
    }
}