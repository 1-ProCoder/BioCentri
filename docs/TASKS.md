# Tasks — BioCentri

> The short, immediate list of what is being worked on *right now*. The
> larger roadmap lives in `FEATURE_ROADMAP.md`; the steady state of *what
> must be true* lives in `PROJECT_BIBLE.md` and `PRODUCT_REQUIREMENTS.md`.

---

## Current Focus

- [ ] **TASK-001 — Finalize BioCentri product definition.**
  *Owner:* founder. *Depends on:* `PROJECT_BIBLE.md`, `PRODUCT_REQUIREMENTS.md`,
  `FEATURE_ROADMAP.md`. *Acceptance:* the three docs above are reviewed and
  signed off as the v1 product definition; they can be shown to a new
  contributor without rewrites.

---

## Next Tasks

Ordered by what unblocks the MVP next.

- [ ] **TASK-002 — Design website.** Marketing pages and a waitlist only —
  no product surface, no code dependencies.
- [ ] **TASK-003 — Create branding.** Logo, palette, typography, and a one-
  page brand reference, captured in `assets/`.
- [ ] **TASK-004 — Plan MVP architecture.** Pick the Windows-native stack
  for the desktop app, and capture the decision in writing
  (`docs/DECISIONS.md` once created).
- [ ] **TASK-005 — Build first prototype.** App discovery + protection
  toggle working end-to-end; auth interception is **not** in scope for
  this prototype.
- [ ] **TASK-006 — Test authentication system.** End-to-end Windows Hello
  coverage (face, fingerprint, PIN) on a clean test profile, with both
  success and failure paths.

---

## Backlog

Held until after MVP ships — captured so we don't re-derive the idea later.

- Usage tracking (Phase 2).
- Time limits and schedules (Phase 2).
- Weekly protected-app reports (Phase 2).
- Browser extension (Phase 3).
- Website protection parity (Phase 3).
- AI productivity assistant, narrowly scoped (Phase 3).
- Cloud sync for protected apps across devices (post-MVP revisit).

---

## Completed

- [x] Created the BioCentri repository scaffold: `app/`, `website/`, `api/`,
  `extension/`, `docs/`, `assets/`.
- [x] Authored the documentation foundation: `PROJECT_BIBLE.md`,
  `PRODUCT_REQUIREMENTS.md`, `FEATURE_ROADMAP.md`, `TASKS.md`, and
  `CHANGELOG.md`.

---

_Last reviewed: project foundation._
