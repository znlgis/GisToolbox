using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using GisToolbox.Services.Implementations;
using GisToolbox.Services.Interfaces;
using GisToolbox.ViewModels;
using GisToolbox.ViewModels.CoordinateTools;
using GisToolbox.ViewModels.RasterTools;
using GisToolbox.ViewModels.VectorTools;
using GisToolbox.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GisToolbox;

public class App : Application
{
    public IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ConfigureServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var mainViewModel = ServiceProvider!.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // 注册服务
        services.AddSingleton<IVectorProcessingService, VectorProcessingService>();
        services.AddSingleton<ICoordinateTransformService, CoordinateTransformService>();

        if (OperatingSystem.IsWindows()) services.AddSingleton<IRasterProcessingService, RasterProcessingService>();

        // 注册 ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<GeometrySimplificationViewModel>();
        services.AddTransient<FormatConversionViewModel>();
        services.AddTransient<BufferAnalysisViewModel>();
        services.AddTransient<OverlayAnalysisViewModel>();
        services.AddTransient<CoordinateTransformViewModel>();
        services.AddTransient<CsvToGeometryViewModel>();

        if (OperatingSystem.IsWindows())
        {
            services.AddTransient<RasterFormatConversionViewModel>();
            services.AddTransient<RasterResampleViewModel>();
        }

        ServiceProvider = services.BuildServiceProvider();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // 移除数据验证插件以避免与 CommunityToolkit 的验证重复
        var pluginsToRemove = BindingPlugins.DataValidators
            .OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        foreach (var plugin in pluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}