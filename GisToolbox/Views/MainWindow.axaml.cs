using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using GisToolbox.Helpers;
using GisToolbox.Models;

namespace GisToolbox.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 注册文件对话框消息处理
        WeakReferenceMessenger.Default.Register<SelectFileMessage>(this, async (r, m) =>
        {
            string? filePath = null;

            if (m.IsOpenDialog)
                filePath = await FileDialogHelper.OpenFileAsync(this);
            else
                filePath = await FileDialogHelper.SaveFileAsync(this);

            m.Callback?.Invoke(filePath);
        });
    }
}