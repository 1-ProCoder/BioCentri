using System.Windows.Controls;

namespace BioCentri.App.Components.Surface;

/// <summary>
/// "Hero" card surface used by the dashboard, settings, and status pages
/// to draw the eye to a single feature. Uses the Focal background
/// token (matching website's `.focal` mix) plus the standard elevation
/// shadow and an inner-highlight hairline.
/// </summary>
public partial class FocalCard : Border
{
    public FocalCard()
    {
        InitializeComponent();
    }
}
