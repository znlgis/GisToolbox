# GisToolbox

English | [简体中文](README.md)

A cross-platform GIS toolbox application developed with .NET 8 and Avalonia UI, providing vector data processing, raster data processing, and coordinate transformation capabilities.

![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)

## ✨ Features

### 📐 Vector Tools

- **Format Conversion**: Support conversion between multiple vector formats including Shapefile, GeoJSON, WKT, WKB, KML, GPX, CSV
- **Geometry Simplification**: Simplify complex geometries using Douglas-Peucker algorithm to reduce data size
- **Buffer Analysis**: Create buffers at specified distances for point, line, and polygon features
- **Overlay Analysis**: Support spatial overlay operations including intersection, union, difference, and symmetric difference

### 🗺️ Raster Tools (Windows Only)

- **Format Conversion**: Convert between raster data formats
- **Resampling**: Resample raster data using various interpolation methods

### 🌐 Coordinate Tools

- **Coordinate System Transformation**: Transform geometry data between different coordinate reference systems (e.g., WGS84, Web Mercator)
- **CSV to Geometry**: Convert CSV files containing coordinate information to vector geometry data

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- Windows, Linux, or macOS operating system

### Installation

1. Clone the repository:
```bash
git clone https://github.com/znlgis/GisToolbox.git
cd GisToolbox
```

2. Restore dependencies and build:
```bash
dotnet restore
dotnet build
```

3. Run the application:
```bash
dotnet run --project GisToolbox/GisToolbox.csproj
```

### Publishing

Create standalone executable:

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

## 🏗️ Technical Architecture

### Core Technology Stack

- **UI Framework**: [Avalonia UI](https://avaloniaui.net/) 11.3.7 - Cross-platform XAML UI framework
- **MVVM Framework**: [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) 8.4.0
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection 9.0.10
- **Geometry Processing**: [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) 2.6.0
- **Coordinate Transformation**: [ProjNET](https://github.com/NetTopologySuite/ProjNet4GeoAPI) 2.1.0
- **Data Format Support**:
  - Shapefile: NetTopologySuite.IO.Esri.Shapefile
  - GeoJSON: NetTopologySuite.IO.GeoJSON
  - JSON: Newtonsoft.Json

### Project Structure

```
GisToolbox/
├── Models/                          # Data models
│   ├── Enums.cs                    # Enum definitions (formats, tool categories)
│   ├── GeometryDataModel.cs        # Geometry data model
│   ├── RasterDataModel.cs          # Raster data model
│   ├── ProcessingResult.cs         # Processing result model
│   ├── ToolMenuItem.cs             # Tool menu item
│   └── Messages.cs                 # Message definitions
├── Services/                        # Service layer
│   ├── Interfaces/                 # Service interfaces
│   │   ├── IVectorProcessingService.cs
│   │   ├── IRasterProcessingService.cs
│   │   └── ICoordinateTransformService.cs
│   └── Implementations/            # Service implementations
│       ├── VectorProcessingService.cs
│       ├── RasterProcessingService.cs
│       └── CoordinateTransformService.cs
├── ViewModels/                      # View models
│   ├── Base/                       # Base ViewModels
│   │   ├── ViewModelBase.cs
│   │   └── ToolViewModelBase.cs
│   ├── VectorTools/                # Vector tool ViewModels
│   ├── RasterTools/                # Raster tool ViewModels
│   ├── CoordinateTools/            # Coordinate tool ViewModels
│   └── MainWindowViewModel.cs
├── Views/                           # Views
│   ├── VectorTools/
│   ├── RasterTools/
│   ├── CoordinateTools/
│   └── MainWindow.axaml
├── Controls/                        # Custom controls
│   └── MapPreviewControl.cs
├── Helpers/                         # Helper classes
│   └── FileDialogHelper.cs
└── Program.cs                       # Entry point
```

### Design Patterns

- **MVVM (Model-View-ViewModel)**: Separation of UI and business logic
- **Dependency Injection**: Loosely coupled service management
- **Service Layer Pattern**: Encapsulation of business logic and data processing
- **Observer Pattern**: Property notification and command binding using CommunityToolkit.Mvvm

## 📖 Usage Examples

### Vector Format Conversion

1. Select "Vector Tools" → "Format Conversion" from the left menu
2. Choose input file (e.g., Shapefile)
3. Select output format (e.g., GeoJSON)
4. Set output path
5. Click "Process" button to execute conversion

### Buffer Analysis

1. Select "Vector Tools" → "Buffer Analysis"
2. Load feature layer
3. Set buffer distance (units: meters/kilometers/degrees)
4. Specify output path
5. Execute analysis

### CSV to Geometry Features

1. Select "Coordinate Tools" → "CSV to Geometry"
2. Load CSV file containing coordinates
3. Specify X, Y column names (e.g., longitude, latitude)
4. Set coordinate system (SRID)
5. Choose output format and execute conversion

## 🔧 Development

### Adding New Tools

1. Define service interface in Services/Interfaces
2. Implement service in Services/Implementations
3. Create ViewModel (inherit from `ToolViewModelBase`)
4. Create corresponding AXAML view
5. Register service and ViewModel in `App.axaml.cs`
6. Add menu item in ToolCategories in `MainWindowViewModel.cs`

### Package Dependencies

Core dependencies:
```xml
<PackageReference Include="Avalonia" Version="11.3.7" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
<PackageReference Include="NetTopologySuite" Version="2.6.0" />
<PackageReference Include="NetTopologySuite.IO.Esri.Shapefile" Version="1.2.0" />
<PackageReference Include="NetTopologySuite.IO.GeoJSON" Version="4.0.0" />
<PackageReference Include="ProjNET" Version="2.1.0" />
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

## 🙏 Acknowledgments

- [Avalonia UI](https://avaloniaui.net/) - Cross-platform UI framework
- [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) - .NET spatial analysis library
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) - MVVM toolkit

## 📮 Contact

- Project Homepage: [https://github.com/znlgis/GisToolbox](https://github.com/znlgis/GisToolbox)
- Issue Tracking: [Issues](https://github.com/znlgis/GisToolbox/issues)

## 🗺️ Roadmap

- [ ] Support more vector formats (KML, GPX, etc.)
- [ ] Enhanced raster processing capabilities
- [ ] Additional spatial analysis tools (intersects, contains, etc.)
- [ ] Map visualization preview
- [ ] Batch processing functionality
- [ ] Plugin system
- [ ] Internationalization support

---

⭐ If this project helps you, please consider giving it a star!
