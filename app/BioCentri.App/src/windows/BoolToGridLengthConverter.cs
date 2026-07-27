using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BioCentri.App.Windows;

/// <summary>
/// Maps a <see cref="bool"/> to a <see cref="GridLength"/> for the
/// sidebar column width. Hardcoded <c>240</c> expanded / <c>64</c>
/// collapsed — the sidebar is a single component, so a typed converter
/// is more honest than a parameter-laden one and avoids XAML
/// attribute-setter complexity on a plain <see cref="IValueConverter"/>.
/// </summary>
public sealed class BoolToGridLengthConverter : IValueConverter
{
    private const double ExpandedWidth = 240d;
    private const double CollapsedWidth = 64d;

    /// <inheritdoc />
    /// <remarks>true → <c>240</c>, false → <c>64</c>. Anything else → <c>64</c>.</remarks>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => new GridLength(value is bool b && b ? ExpandedWidth : CollapsedWidth);

    /// <inheritdoc />
    /// <remarks>Always returns <c>true</c> when ConvertBack is asked: the
    /// sidebar is one-way bound by width — programmatic editing is not a
    /// supported path.</remarks>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is GridLength g && g.Value >= (ExpandedWidth - CollapsedWidth) / 2d + CollapsedWidth;
}
