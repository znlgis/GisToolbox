﻿using GisToolbox.Models;
using GisToolbox.ViewModels.Base;
using GisToolbox.ViewModels.CoordinateTools;
using GisToolbox.ViewModels.RasterTools;
using GisToolbox.ViewModels.VectorTools;
using Microsoft.Extensions.DependencyInjection;

namespace GisToolbox.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private ToolViewModelBase? _selectedTool;
    [ObservableProperty] private string _title = "GisToolbox";

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        ToolCategories =
        [
            new ToolCategoryItem
            {
                Name = "矢量工具",
                Icon = "📐",
                Tools =
                [
                    new ToolMenuItem { Name = "格式转换", ToolType = typeof(FormatConversionViewModel) },
                    new ToolMenuItem { Name = "几何简化", ToolType = typeof(GeometrySimplificationViewModel) },
                    new ToolMenuItem { Name = "缓冲区分析", ToolType = typeof(BufferAnalysisViewModel) },
                    new ToolMenuItem { Name = "叠加分析", ToolType = typeof(OverlayAnalysisViewModel) }
                ]
            },
            new ToolCategoryItem
            {
                Name = "栅格工具",
                Icon = "🗺️",
                Tools =
                [
                    new ToolMenuItem { Name = "格式转换", ToolType = typeof(RasterFormatConversionViewModel) },
                    new ToolMenuItem { Name = "重采样", ToolType = typeof(RasterResampleViewModel) }
                ]
            },
            new ToolCategoryItem
            {
                Name = "坐标工具",
                Icon = "🌐",
                Tools =
                [
                    new ToolMenuItem { Name = "坐标转换", ToolType = typeof(CoordinateTransformViewModel) },
                    new ToolMenuItem { Name = "CSV转几何", ToolType = typeof(CsvToGeometryViewModel) }
                ]
            }
        ];
    }

    public ObservableCollection<ToolCategoryItem> ToolCategories { get; }

    [RelayCommand]
    private void SelectTool(ToolMenuItem toolMenuItem)
    {
        SelectedTool = (ToolViewModelBase)_serviceProvider.GetRequiredService(toolMenuItem.ToolType);
    }
}