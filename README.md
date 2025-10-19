# GisToolbox

[English](README.en.md) | 简体中文

一个基于 .NET 8 和 Avalonia UI 开发的跨平台 GIS 工具箱应用程序，提供矢量数据处理、栅格数据处理和坐标转换等功能。

![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)

## ✨ 功能特性

### 📐 矢量工具

- **格式转换**：支持 Shapefile、GeoJSON、WKT、WKB、KML、GPX、CSV 等多种矢量格式之间的相互转换
- **几何简化**：使用 Douglas-Peucker 算法简化复杂几何对象，减少数据量
- **缓冲区分析**：为点、线、面要素创建指定距离的缓冲区
- **叠加分析**：支持交集、并集、差集、对称差等空间叠加操作

### 🗺️ 栅格工具（仅 Windows）

- **格式转换**：栅格数据格式转换
- **重采样**：使用多种插值方法对栅格数据进行重采样

### 🌐 坐标工具

- **坐标系统转换**：在不同坐标参考系统（如 WGS84、Web Mercator 等）之间转换几何数据
- **CSV 转几何**：将包含坐标信息的 CSV 文件转换为矢量几何数据

## 🚀 快速开始

### 环境要求

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本
- Windows、Linux 或 macOS 操作系统

### 安装

1. 克隆仓库：
```bash
git clone https://github.com/znlgis/GisToolbox.git
cd GisToolbox
```

2. 还原依赖并构建：
```bash
dotnet restore
dotnet build
```

3. 运行应用：
```bash
dotnet run --project GisToolbox/GisToolbox.csproj
```

### 发布

创建独立可执行文件：

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

## 🏗️ 技术架构

### 核心技术栈

- **UI 框架**：[Avalonia UI](https://avaloniaui.net/) 11.3.7 - 跨平台 XAML UI 框架
- **MVVM 框架**：[CommunityToolkit.Mvvm](https://learn.microsoft.com/zh-cn/dotnet/communitytoolkit/mvvm/) 8.4.0
- **依赖注入**：Microsoft.Extensions.DependencyInjection 9.0.10
- **几何处理**：[NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) 2.6.0
- **坐标转换**：[ProjNET](https://github.com/NetTopologySuite/ProjNet4GeoAPI) 2.1.0
- **数据格式支持**：
  - Shapefile：NetTopologySuite.IO.Esri.Shapefile
  - GeoJSON：NetTopologySuite.IO.GeoJSON
  - JSON：Newtonsoft.Json

### 项目结构

```
GisToolbox/
├── Models/                          # 数据模型
│   ├── Enums.cs                    # 枚举定义（格式、工具分类等）
│   ├── GeometryDataModel.cs        # 几何数据模型
│   ├── RasterDataModel.cs          # 栅格数据模型
│   ├── ProcessingResult.cs         # 处理结果模型
│   ├── ToolMenuItem.cs             # 工具菜单项
│   └── Messages.cs                 # 消息定义
├── Services/                        # 服务层
│   ├── Interfaces/                 # 服务接口
│   │   ├── IVectorProcessingService.cs
│   │   ├── IRasterProcessingService.cs
│   │   └── ICoordinateTransformService.cs
│   └── Implementations/            # 服务实现
│       ├── VectorProcessingService.cs
│       ├── RasterProcessingService.cs
│       └── CoordinateTransformService.cs
├── ViewModels/                      # 视图模型
│   ├── Base/                       # 基础 ViewModel
│   │   ├── ViewModelBase.cs
│   │   └── ToolViewModelBase.cs
│   ├── VectorTools/                # 矢量工具 ViewModels
│   ├── RasterTools/                # 栅格工具 ViewModels
│   ├── CoordinateTools/            # 坐标工具 ViewModels
│   └── MainWindowViewModel.cs
├── Views/                           # 视图
│   ├── VectorTools/
│   ├── RasterTools/
│   ├── CoordinateTools/
│   └── MainWindow.axaml
├── Controls/                        # 自定义控件
│   └── MapPreviewControl.cs
├── Helpers/                         # 辅助类
│   └── FileDialogHelper.cs
└── Program.cs                       # 入口点
```

### 设计模式

- **MVVM（Model-View-ViewModel）**：实现 UI 与业务逻辑分离
- **依赖注入**：松耦合的服务管理
- **服务层模式**：封装业务逻辑和数据处理
- **观察者模式**：使用 CommunityToolkit.Mvvm 实现属性通知和命令绑定

## 📖 使用示例

### 矢量格式转换

1. 在左侧菜单选择"矢量工具" → "格式转换"
2. 选择输入文件（如 Shapefile）
3. 选择输出格式（如 GeoJSON）
4. 设置输出路径
5. 点击"处理"按钮执行转换

### 缓冲区分析

1. 选择"矢量工具" → "缓冲区分析"
2. 加载要素图层
3. 设置缓冲距离（单位：米/千米/度）
4. 指定输出路径
5. 执行分析

### CSV 转几何要素

1. 选择"坐标工具" → "CSV转几何"
2. 加载包含坐标的 CSV 文件
3. 指定 X、Y 列名（如 longitude, latitude）
4. 设置坐标系统（SRID）
5. 选择输出格式并执行转换

## 🔧 开发

### 添加新工具

1. 在相应的 Services/Interfaces 中定义服务接口
2. 在 Services/Implementations 中实现服务
3. 创建 ViewModel（继承自 `ToolViewModelBase`）
4. 创建对应的 AXAML 视图
5. 在 `App.axaml.cs` 中注册服务和 ViewModel
6. 在 `MainWindowViewModel.cs` 的 ToolCategories 中添加菜单项

### 依赖包说明

核心依赖：
```xml
<PackageReference Include="Avalonia" Version="11.3.7" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
<PackageReference Include="NetTopologySuite" Version="2.6.0" />
<PackageReference Include="NetTopologySuite.IO.Esri.Shapefile" Version="1.2.0" />
<PackageReference Include="NetTopologySuite.IO.GeoJSON" Version="4.0.0" />
<PackageReference Include="ProjNET" Version="2.1.0" />
```

## 🤝 贡献

欢迎贡献！请遵循以下步骤：

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📝 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 🙏 致谢

- [Avalonia UI](https://avaloniaui.net/) - 跨平台 UI 框架
- [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) - .NET 空间分析库
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/zh-cn/dotnet/communitytoolkit/mvvm/) - MVVM 工具包

## 📮 联系方式

- 项目主页：[https://github.com/znlgis/GisToolbox](https://github.com/znlgis/GisToolbox)
- 问题反馈：[Issues](https://github.com/znlgis/GisToolbox/issues)

## 🗺️ 路线图

- [ ] 支持更多矢量格式（KML、GPX 等）
- [ ] 增强栅格处理功能
- [ ] 添加空间分析工具（相交、包含等）
- [ ] 地图可视化预览
- [ ] 批处理功能
- [ ] 插件系统
- [ ] 国际化支持

---

⭐ 如果这个项目对你有帮助，欢迎 Star！

