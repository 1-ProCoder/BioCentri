using System.Windows.Controls;

namespace BioCentri.App.Components.Auth;

/// <summary>
/// Top-of-shell overlay that shows "Verifying identity…" while
/// <see cref="IBiometricAuthService.AuthenticateAsync"/> runs.
/// Visual layout and animations live in the XAML file.
/// </summary>
public partial class AuthenticationOverlay : UserControl
{
    public AuthenticationOverlay()
    {
        InitializeComponent();
    }
}
