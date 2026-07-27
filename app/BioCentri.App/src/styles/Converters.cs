using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BioCentri.App.Styles;

/// <summary>
/// WPF value-converter palette used by every page that needs UI-thread
/// binding-to-Visibility conversions (M4 protected-apps page + picker
/// + future Settings/Activity/Dashboard card toggles). All four are
/// registered as <c>StaticResource</c> in <c>App.xaml</c> under short
/// keys (e.g. <c>BoolToVisibility</c>) — the class names add the
/// <c>Converter</c> suffix while the resource keys do not, mirroring
/// the WPF <c>BooleanToVisibilityConverter</c> convention.
///
/// Keep these stateless, allocation-free, and culture-invariant.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        => (value is true) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}

public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        => (value is true) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility v && v != Visibility.Visible;
}

/// <summary>Visible when the string is non-null and non-empty after trim.</summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("StringToVisibility is one-way.");
}

/// <summary>
/// Visible based on collection count. Parameter semantics:
///   * <c>"zero"</c>    — Visible when count == 0
///   * <c>"nonzero"</c> — Visible when count  &gt; 0
///   * <c>null</c> / unset — Visible when count &gt; 0 (matches the
///     common "show when items exist" UX).
/// </summary>
public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var count = value is int n ? n : 0;
        var mode = (parameter as string)?.ToLowerInvariant();
        return mode switch
        {
            "zero"    => count == 0    ? Visibility.Visible : Visibility.Collapsed,
            "nonzero" => count != 0    ? Visibility.Visible : Visibility.Collapsed,
            _         => count >  0    ? Visibility.Visible : Visibility.Collapsed,
        };
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("CountToVisibility is one-way.");
}
