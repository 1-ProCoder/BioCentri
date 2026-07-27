# BioCentri.Tests — _deferred_

> **Status:** placeholder. Activated at **Milestone 5** alongside
> `BioCentri.Core` to enable xUnit coverage of the auth surface.

The eventual project will use:

- `xUnit` for test runner.
- `FluentAssertions` for assertions.
- Headless patterns: injecting fakes for `IDispatcher`,
  `IHelloService` (we provide a `FakeHelloService` that returns
  pre-canned outcomes), `IProtectedAppStore` (in-memory).

Every `FR-*` in `docs/PRODUCT_REQUIREMENTS.md` gets at least one test
once `BioCentri.Tests` activates. Earlier milestones rely on visual /
manual verification for the foundation.
