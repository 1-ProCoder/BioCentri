using BioCentri.App.Routing;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.Features.Rules;

/// <summary>
/// Rules view-model. M2 placeholder per IMPLEMENTATION_PLAN §7.
/// Automation rules land in Milestone 4; this view-model only
/// exposes its title and subtitle today so the route renders.
/// </summary>
public sealed partial class RulesViewModel : ObservableObject
{
    public string Title => RouteTable.Get(Route.Rules).Title;
    public string Subtitle => RouteTable.Get(Route.Rules).Subtitle;
}
