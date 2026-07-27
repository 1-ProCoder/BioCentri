using System.Reflection;
using System.Runtime.CompilerServices;

// Per IMPLEMENTATION_PLAN §5: BioCentri.Tests activates at Milestone 5
// alongside BioCentri.Core. InternalsVisibleTo lets headless xUnit
// tests reach BiometricAuthService, ProcessWatcher, and other internal
// services that normally only the composition root touches.
[assembly: InternalsVisibleTo("BioCentri.Tests")]
