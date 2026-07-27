# Feature Roadmap — BioCentri

> A directional plan of what BioCentri will ship, ordered by value. Each
> phase has a clear goal, a small feature set, and the reason it earns its
> place.

Dates are deliberately left off until the team is comfortable with the rate
of MVP turnover. Phases describe *what*, not *when*.

---

## Phase 1 — MVP

**Goal:** Ship a reliable, polished Windows desktop app that protects
selected applications with Windows Hello authentication — and does nothing
else.

**Features:**

- Enumerate installed user applications.
- Per-app protection toggle (on / off).
- Windows Hello challenge at launch (face, fingerprint, PIN fallback).
- Protected-apps management UI (list, add, remove).
- Settings screen: default auth method, app list management.
- Native Windows installer (signed, SmartScreen-clean).

**Why it matters:** This is the product. Everything else is an expansion of
this promise — if v1 isn't trustworthy, nothing later will be either.

---

## Phase 2 — Productivity & Digital Wellbeing

**Goal:** Move BioCentri from *"set it and forget it"* to a tool the user
opens daily, by giving them honest visibility into how their protected apps
are used and calm tools to act on what they see.

**Features:**

- Local-only **usage tracking** for protected apps.
- Per-app **time limits** with configurable enforcement behaviour.
- **Schedules** — for example *"require authentication outside 9–5"* or
  *"block after bedtime"*.
- **Weekly reports** summarising protected-app usage, fully on-device.

**Why it matters:** Adds daily-surface value beyond security, grows the
moat (the more protected apps a user has, the higher the cost to leave),
and gives us a feedback loop for the long-term trust story.

---

## Phase 3 — Expansion

**Goal:** Extend protection and assistance beyond the desktop app surface,
without diluting the product's identity.

**Features:**

- Browser extension for **site-level protection**, integrated with the
  existing protected-apps data model.
- **Website protection parity** with the desktop app (using the same
  Windows Hello credential model where the device supports it).
- **AI productivity assistant** — narrowly scoped: contextual help *inside*
  BioCentri itself, never a general-purpose chatbot bolted on.
- **Integrations** with productivity tools the user already trusts, only
  with explicit opt-in.

**Why it matters:** Compounds the value of the protected-apps graph,
opens a second distribution surface (the extension), and lets BioCentri be
useful to the user in the browser — where attention actually lives.

---

## Out of roadmap (for now)

These are deliberately excluded. If any of them is added later, it must
get its own phase and its own rationale.

- Antivirus / malware detection.
- Enterprise SSO / IdP / directory sync.
- VPN / network security features.
- System optimisation / "PC tune-up" tools.
- Cross-platform ports (macOS, Linux, mobile).

---

_Last reviewed: project foundation._
