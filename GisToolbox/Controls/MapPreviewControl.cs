using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using NetTopologySuite.Geometries;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;
using NtsEnvelope = NetTopologySuite.Geometries.Envelope;
using Point = Avalonia.Point;

namespace GisToolbox.Controls;

/// <summary>
///     地图预览控件（简化版）
/// </summary>
public class MapPreviewControl : Control
{
    public static readonly StyledProperty<List<NtsGeometry>?> GeometriesProperty =
        AvaloniaProperty.Register<MapPreviewControl, List<NtsGeometry>?>(nameof(Geometries));

    private NtsEnvelope? _viewExtent;

    static MapPreviewControl()
    {
        AffectsRender<MapPreviewControl>(GeometriesProperty);
    }

    public List<NtsGeometry>? Geometries
    {
        get => GetValue(GeometriesProperty);
        set => SetValue(GeometriesProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // 绘制背景
        context.DrawRectangle(Brushes.White, null, new Rect(Bounds.Size));

        if (Geometries == null || !Geometries.Any())
        {
            // 绘制占位文本
            var formattedText = new FormattedText(
                "无预览数据 - 地图渲染功能正在开发中",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                14,
                Brushes.Gray);

            context.DrawText(formattedText, new Point(
                (Bounds.Width - formattedText.Width) / 2,
                (Bounds.Height - formattedText.Height) / 2));
            return;
        }

        // 计算边界
        if (_viewExtent == null) CalculateViewExtent();

        // TODO: 实现基于 Skia 的几何渲染
        // 当前版本显示几何统计信息
        var info = $"已加载 {Geometries.Count} 个几何对象";
        var infoText = new FormattedText(
            info,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            12,
            Brushes.Black);

        context.DrawText(infoText, new Point(10, 10));
    }

    private void CalculateViewExtent()
    {
        if (Geometries == null || !Geometries.Any())
            return;

        var factory = new GeometryFactory();
        var collection = factory.CreateGeometryCollection(Geometries.ToArray());
        _viewExtent = collection.EnvelopeInternal;

        // 添加边距
        if (_viewExtent != null && !_viewExtent.IsNull)
        {
            var margin = Math.Max(_viewExtent.Width, _viewExtent.Height) * 0.1;
            _viewExtent.ExpandBy(margin);
        }
    }
}