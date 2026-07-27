# BioCentri.Core — _deferred_

> **Status:** placeholder. Activated at **Milestone 5** to gain headless
> testability of `IHelloService` and other security-sensitive services.

When activated, this project will host:

- `IHelloService` (Windows Hello adapter)
- `HelloOutcome` and reason taxonomy
- `IProtectedAppStore` (file-backed protection list)
- `UserConsentVerifierAdapter` (WinRT interop)
- Pure-C# algorithms with **no WPF reference**

Until M5, all code lives in `BioCentri.App`.

See `app/IMPLEMENTATION_PLAN.md` §6 (Companion NuGet packages) and §11
(Open decisions before Milestone 1) for the rationale.
