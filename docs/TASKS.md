# Tasks — BioCentri

> The short, immediate list of what is being worked on *right now*. The
> larger roadmap lives in `FEATURE_ROADMAP.md`; the steady state of *what
> must be true* lives in `PROJECT_BIBLE.md` and `PRODUCT_REQUIREMENTS.md`.

---

## Current Focus

**v1.0.0 launch readiness.** Phase 1 MVP code is complete. The remaining
gates are packaging and distribution:

- [ ] **TASK-007 — WiX installer compilation.** Run `heat.exe` + `candle.exe`
  + `light.exe` against `app/BioCentri.Setup/BioCentri.Setup.*`. Requires
  WiX Toolset 3.14+ on the build machine.
- [ ] **TASK-008 — Code signing.** Procure an EV certificate (or Azure
  Trusted Signing) and sign `BioCentri.msi`.
- [ ] **TASK-009 — SmartScreen seeding.** Submit the signed `.msi` to
  Microsoft for initial reputation.
- [ ] **TASK-010 — Tray icon activation.** Re-add `H.NotifyIcon.Wpf` 2.x
  NuGet when the offline cache syncs; uncomment the activation block in
  `App.xaml.cs`. The `TrayIconViewModel` is already registered in DI.

---

## Completed

- [x] **TASK-001 — Product definition.** `PROJECT_BIBLE.md`,
  `PRODUCT_REQUIREMENTS.md`, `FEATURE_ROADMAP.md` authored and reviewed.
- [x] **TASK-002 — Website.** React/Vite/Tailwind landing page with hero,
  features, showcase, waitlist, and SEO metadata. Builds 0/0.
- [x] **TASK-003 — Branding.** Logo, indigo/violet palette, Plus Jakarta
  Sans + Inter typography. Applied consistently across website and WPF app.
- [x] **TASK-004 — MVP architecture.** WPF on .NET 8 with Wpf.Ui,
  CommunityToolkit.Mvvm, M.E.DI. Documented in `docs/DECISIONS.md`.
- [x] **TASK-005 — First prototype → full MVP.** M1–M7 shipped:
  app discovery (registry), protection toggle, Windows Hello auth,
  process monitoring (WMI), app-locking enforcement, 7 feature pages,
  toast/dialog system, dark theme, high-contrast hook, reduced-motion
  toggle, accessibility labels.
- [x] **TASK-006 — Auth testing.** 7 xUnit tests covering Hello outcomes,
  coalescing, dedupe, and cancel-overlay. Manual QA script at
  `docs/MANUAL_QA.md`.

---

## Backlog (Phase 2 — Productivity & Digital Wellbeing)

Held until after Phase 1 ships.

- Usage tracking for protected apps.
- Per-app time limits with configurable enforcement.
- Schedules (e.g. "require auth outside 9–5").
- Weekly protected-app usage reports, fully on-device.

---

_Last reviewed: v1.0.0 — Phase 1 MVP code complete._
