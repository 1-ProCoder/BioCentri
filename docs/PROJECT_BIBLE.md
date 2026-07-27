# Project Bible — BioCentri

> The single source of truth for *what BioCentri is*, *who it serves*, and the
> principles every feature decision must respect. Slow-moving; only edited when
> the product's identity meaningfully changes.

---

## Vision

BioCentri exists because Windows users have lost direct, granular control
over the apps on their own machines. The built-in options are coarse, the
third-party app lockers are bundled with unrelated feature bloat, and the
security tools that touch this space usually ask for more trust than they
earn back.

Our vision is a Windows desktop experience where **the user — not the OS
vendor, not an antivirus vendor — is the source of truth for who can use
which app, when, and under what proof of identity.**

---

## Mission

To give Windows users a calm, premium tool that locks selected apps behind
biometric-grade authentication, and over time becomes the user's trusted hub
for app privacy, access control, and digital wellbeing.

We help:

- People who share a device with family or roommates.
- Privacy-conscious individuals who want fine-grained app access control.
- Professionals who want a focused, biometric lock before sensitive tools.
- Anyone who wants Windows Hello to do more than unlock the device at sign-in.

---

## Product Overview

BioCentri is a Windows desktop application. In version 1, the user picks an
installed app, switches protection on, and from then on BioCentri requires
**Windows Hello** — face, fingerprint, or PIN fallback — before that app
will launch. Everything runs locally. Nothing about a user's protected apps
ever leaves the device without an explicit, opt-in action.

Future phases extend this foundation into productivity, digital wellbeing, and
platform-level protection. The MVP does none of that.

---

## Target Users

BioCentri is built for **individual Windows users**, not enterprises:

- **Privacy-led individuals** who want explicit, per-app access rules.
- **Shared-PC households** where multiple people use one machine.
- **Focused professionals** who want a fast, biometric lock before banking,
  communications, or admin tools.
- **Parents** who want kids to authenticate before specific apps without
  standing up a full parental-control suite.
- **Productivity-aware users** who, in later phases, will value the digital
  wellbeing layer.

Explicitly *not* the primary audience in v1: enterprise IT, regulated
industries, schools, and developers building security toolchains. These may
be valid future segments; they are not the design centre today.

---

## Problems We Solve

- **There is no friendly, focused app-locker for Windows.** Parental
  controls and enterprise suites are heavy. Password-based app locks are
  awkward. Antivirus suites bundle locking with unrelated features.
- **Windows Hello is underused.** Most users experience it only at sign-in.
  BioCentri makes it useful throughout a normal day.
- **Shared PCs lack a graceful "this is mine" boundary.** Roommates,
  family, and visiting guests can open anything by default.
- **Privacy-minded users distrust cloud-coupled security tools.** BioCentri
  is local-first, so its trust model is small enough to audit.

---

## Core Principles

1. **Privacy first.** We collect nothing by default. Anything we collect
   must be opt-in, surfaced, and easy to delete.
2. **Security before features.** A new feature that weakens the security
   posture is not a feature — it is a regression. We defer it.
3. **Simple, focused experience.** A small surface done extremely well beats
   a large surface done adequately. We resist feature creep.
4. **Premium quality.** Every interaction should feel solid, calm, and
   intentional. Rough edges are not shipped.
5. **Local-first.** User data lives on the user's machine. Cloud-backed
   features are explicit and reversible.
6. **User control.** The user is the source of truth. Defaults respect them,
   and overrides are obvious.
7. **Honest scope.** When something is out of scope, we say so — loudly and
   in writing. We do not blur categories to look bigger than we are.

---

## Non-Goals

BioCentri is **not**:

- ❌ **Not antivirus software.** No malware scanning, detection, or
  quarantine.
- ❌ **Not a data collection platform.** No behavioural analytics, no
  third-party trackers, no advertising data flows.
- ❌ **Not a replacement for Windows Security or Microsoft Defender.** We
  sit alongside them and assume they are installed.
- ❌ **Not enterprise SSO / IdP.** No directory sync, no SAML, no SCIM.
- ❌ **Not a VPN, firewall, or network security product.**
- ❌ **Not a system optimiser, cleaner, or "PC tune-up" tool.**
- ❌ **Not a parental control suite.** We do one thing well; parental
  control vendors can do the rest.

---

## Product Philosophy

When a question comes up, we apply three tests:

1. **Does this increase the user's control?**
2. **Does this preserve or strengthen their privacy?**
3. **Does this make BioCentri more trustworthy as a product?**

If the answer to any of these is *no* and there is no compensating reason,
the answer is **no, not yet**.

We optimise for **long-term trust** over short-term growth. We ship fewer
features, polished, rather than many features, rough. We write things down
so that future versions of the team — including future AI coding sessions —
have a steady answer to *"should we?"*.

---

_Last reviewed: project foundation._
