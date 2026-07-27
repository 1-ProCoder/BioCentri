# Product Requirements — BioCentri (MVP, v1)

> Testable requirements for the first usable BioCentri release. Anything
> outside this document is either already shipped (in `CHANGELOG.md`) or
> scheduled for future phases (in `FEATURE_ROADMAP.md`).

---

## Product Goal

Ship a Windows desktop application that lets a user **select installed
applications**, **toggle protection on or off per app**, and **require a
successful Windows Hello challenge** before any protected app launches — with
a clean, modern UI and **zero network usage** for v1.

---

## MVP Scope

### In scope for v1

- Enumerate user-installed applications.
- Let the user mark an app as **protected**.
- Intercept protected-app launch and request **Windows Hello** authentication
  (face, fingerprint, or **PIN as fallback** when biometric is unavailable).
- Let the user see, add, and remove protected apps in a management view.
- Provide a Settings screen for **default authentication method** and
  **protected-app list management**.
- Ship as a signed native Windows installer (`.msix` or equivalent).

### Out of scope for v1 (see `FEATURE_ROADMAP.md`)

- Usage tracking, time limits, schedules, reports.
- Browser or website protection.
- Cloud sync, accounts, telemetry.
- AI-driven productivity features.
- Enterprise / managed-device scenarios.

---

## Functional Requirements

Numbered, testable, and tied directly to MVP scope.

- **FR-1 — App discovery.** BioCentri lists installed user applications so
  the user can pick one to protect.
  *Acceptance:* every installed user app is shown with a name and icon; no
  protected apps are missed.

- **FR-2 — Protection toggle.** The user can mark an app as protected or
  unprotected.
  *Acceptance:* the protection state is visible in the management view;
  toggling *off* removes enforcement within the same session.

- **FR-3 — Windows Hello authentication.** When a protected app launches,
  BioCentri presents a Windows Hello challenge.
  *Acceptance:* face, fingerprint, and PIN each independently work as a
  credential when the host supports them.

- **FR-4 — Authentication enforcement.** A protected app must not launch
  without a successful Hello response.
  *Acceptance:* launching a protected app with no Hello result leaves the
  app process unstarted. No partial / "sometimes unlocked" states.

- **FR-5 — Protected apps management UI.** The user can view the list of
  protected apps and add or remove entries.
  *Acceptance:* every entry has a clear remove control; removing an app
  takes effect without requiring a reinstall.

- **FR-6 — Settings screen.** BioCentri exposes a Settings screen for the
  default authentication method and protected-app list management.
  *Acceptance:* every setting is reachable from the home view in **two
  clicks or fewer**.

- **FR-7 — Native Windows packaging.** BioCentri is distributed as a signed
  native Windows installer.
  *Acceptance:* install completes on a clean Windows 10 / 11 user profile
  and produces a Start Menu / Settings entry.

---

## Non-Functional Requirements

| Area             | Requirement |
|------------------|-------------|
| **Security**     | Auth-related secrets stored via Windows Credential Manager or DPAPI. No plaintext secrets on disk. Biometric material never leaves the device. |
| **Privacy**      | **Zero** outbound network calls in v1. No telemetry by default. No background pings. No third-party trackers. |
| **Performance**  | Protection overhead on app launch is imperceptible (target ≤ ~50ms p95). Idle CPU usage near zero. |
| **Reliability**  | A protected app either launches after a successful challenge or does not launch. Partial states are bugs. |
| **User Experience** | Calm, modern, native Windows UI. Fully keyboard reachable. Honours Windows accessibility and high-contrast settings. |

---

## User Stories

Written in the standard *"As a user, I want __ so that __"* form. Each story
is sized to fit comfortably inside the MVP.

- As a **user who shares a PC**, I want to lock a specific app behind
  Windows Hello so that other people at this machine can't open it.
- As a **privacy-conscious user**, I want to see exactly which apps are
  protected so that I know what BioCentri is doing for me.
- As a **user without a working fingerprint reader**, I want to fall back
  to PIN so that protection still works on this device.
- As a **user**, I want to remove a protected app from the list so that I
  retain full control when my needs change.
- As a **first-time user**, I want to install BioCentri and protect one app
  in under two minutes so that I can judge whether the product fits.
- As a **user**, I want protection to be reliable so that I never have to
  wonder whether the lock is actually engaged.

---

## MVP Success Criteria

The MVP is **complete** when **all** of the following are true:

1. ✅ A clean Windows 10 / 11 install gets BioCentri running **in under two
   minutes**, end to end.
2. ✅ Five or more apps can be protected at once with **no measurable
   launch delay** attributable to BioCentri.
3. ✅ Windows Hello **face, fingerprint, and PIN** each independently
   unlock a protected app.
4. ✅ BioCentri makes **zero outbound network calls** during a fresh
   protected-app launch.
5. ✅ Removing an app from the management view **immediately disables**
   enforcement without uninstalling BioCentri.
6. ✅ The app passes **Microsoft SmartScreen** (or ships with equivalent
   trust signals) on a clean install.

If any of these fails, we do **not** call it MVP. We fix it or we cut scope.

---

_Last reviewed: project foundation._
