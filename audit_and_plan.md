# BioCentri Website — Full Audit & Transformation Plan

> **Audit completed:** 2026-07-27  
> **Scope:** `/website` folder only — no app, api, assets, docs, or other folders touched.

---

## 1. Current State Audit

### 1.1 Stack & Architecture

| Layer | Current | Assessment |
|---|---|---|
| Framework | React 18 + Vite 5 | ✅ Solid |
| Styling | Tailwind 3.4 + custom CSS utilities | ✅ Good base |
| Motion | Framer Motion 12 | ✅ Good, underutilised |
| Icons | Lucide React | ✅ Keep |
| 3D / Canvas | ❌ None | 🔴 Missing — needed |
| Scroll lib | None (native) | 🟡 Add Lenis |
| Font | Inter + Plus Jakarta Sans (Google Fonts, render-blocking) | 🟡 Keep fonts, add `font-display: optional` |
| Build | Vite, no code splitting | 🟡 Add lazy loading |

### 1.2 Component Inventory

```
src/
  App.jsx               — thin shell, assembles sections
  index.css             — good design token foundation
  motion.js             — minimal, only fadeInUp / staggerParent
  main.jsx              — standard React entry

  components/
    Nav.jsx             — floating pill nav, spring hover
    Hero.jsx            — centered text + mock dashboard window
    Process.jsx         — 4-step accordion + animated preview panel
    BentoFeatures.jsx   — 3-card bento (Windows Hello, Per-App, Privacy)
    Showcase.jsx        — live searchable app list with toggle switches
    Metrics.jsx         — 4 stat counters with sparklines
    CtaBanner.jsx       — email waitlist form
    Footer.jsx          — 4-column footer

  Atmosphere/
    AtmosphericBackground.jsx — topography + iso-grid + laser sweep + cursor glow
    AmbientAuras.jsx    — slow-moving radial blobs
    GlassReflection.jsx — diagonal glare overlay on the hero window
    HolographicNodes.jsx — SVG network of floating dots (hero)
    HoverBorderTrace.jsx — animated gradient border on hover (BentoCards)
    PipelineConnector.jsx — vertical neon line threading Process steps
    Reticle.jsx         — spinning conic ring in Windows Hello card

  ui/
    Button.jsx          — primary / glass / ghost variants
    Container.jsx       — max-w wrapper
    SectionHeading.jsx  — label + h2 + sub pattern
```

### 1.3 Design System Tokens

**Colors:** `ink-950` (#060606) through `ink-400` (#3d3d49) — good dark scale.  
**Accent:** Indigo (`#818cf8`) + Violet (`#c4b5fd`) + Emerald (`#34d399`) — solid trio.  
**Typography:** Display = Plus Jakarta Sans, Body = Inter. Tracking: `-0.045em` (tightest).  
**Motion easing:** `[0.16, 1, 0.3, 1]` (out-expo) — consistent.

---

## 2. Critical Issues Found

### 🔴 RED — Blocking Quality

| # | Issue | Location | Impact |
|---|---|---|---|
| R1 | **Hero is static and generic** — center text + floating window is the most common SaaS layout ever. No visual hook, no cinematic moment. | `Hero.jsx` | Entire first impression |
| R2 | **All section headings follow identical pattern** — pill label + `fadeInUp` h2 + subtext. Repeated 6× with zero variation. Feels template-generated. | All sections | Memorability = 0 |
| R3 | **`fadeInUp` is the only reveal used everywhere** — literally the "AI generated" animation the brief warns against. Every section enters the same way. | All sections | Interaction quality |
| R4 | **No scroll-driven storytelling** — sections are stacked independently with no visual connection. Nothing links one section to the next. | App.jsx / all sections | Scroll feel |
| R5 | **No 3D or canvas element** — the product (biometric, Windows Hello) demands a memorable visual centrepiece. A face-scan orb, neural grid, or biometric ring would immediately communicate the product category. | Hero.jsx | First 5 seconds |
| R6 | **BentoFeatures has only 3 cards, two are near-identical text blocks** — PerAppCard and PrivacyCard are copy-paste variations with no visual differentiation. | `BentoFeatures.jsx` | Section quality |
| R7 | **No Lenis / smooth scroll** — native browser scroll is choppy on Windows, where this product's target users live. | App.jsx | Scroll feel |
| R8 | **No magnetic button effect** — brief requests it, CTAs feel default. | `Button.jsx`, `Nav.jsx` | Premium feel |
| R9 | **Fonts loaded via render-blocking `<link>` stylesheet** — hurts LCP. Need `font-display: optional` or subsetting. | `index.html` | Performance |
| R10 | **No structured data (JSON-LD)** — not a product or SoftwareApplication schema. | `index.html` | SEO |

### 🟡 YELLOW — Significant Improvements

| # | Issue | Location |
|---|---|---|
| Y1 | `motion.js` has only 4 exports — no clip-path reveals, no blur transitions, no perspective variants | `motion.js` |
| Y2 | `SectionHeading.jsx` exists but is never used (all sections inline their headings) | `ui/SectionHeading.jsx` |
| Y3 | `Container.jsx` is 3 lines long but barely used | `ui/Container.jsx` |
| Y4 | Hero dashboard illustration is pure HTML/CSS with no interaction — feels dead on first view | `Hero.jsx` |
| Y5 | Process section: active step shows small preview panel on the right — the preview is underwhelming (just rows of pills) | `Process.jsx` |
| Y6 | Metrics section: stat numbers are impressive but the sparklines are barely visible | `Metrics.jsx` |
| Y7 | CtaBanner: the glass card looks fine but has no memorable visual — it's just a form in a box | `CtaBanner.jsx` |
| Y8 | Nav: no mobile menu at all — hamburger / drawer missing | `Nav.jsx` |
| Y9 | No custom cursor | Missing entirely |
| Y10 | No page transition / loading state | App.jsx |
| Y11 | Showcase section: good concept (live toggles) but positioned awkwardly after Features — storytelling order doesn't flow | App.jsx |
| Y12 | AmbientAuras and AtmosphericBackground are both fixed/absolute but layered with potential z-index conflicts | Atmosphere/ |

### 🟢 KEEP / ENHANCE

- The **design token system** (`ink-*` colors, indigo/violet/emerald trio) is excellent. Keep exactly.
- The **easing curve** `[0.16, 1, 0.3, 1]` is premium. Keep.
- The **Nav hover pill** with `LayoutGroup` is clever. Keep, enhance magnetism.
- The **Showcase interactive toggles** — best section conceptually. Enhance, don't replace.
- The **Metrics sparklines** — great idea, need to be more dramatic.
- The **BentoCard mouse-following spotlight** — keep, enhance radius and color.
- `useReducedMotion` hook — keep, apply more consistently.

---

## 3. Transformation Plan

### 3.1 New Section Order (Storytelling Improvement)

```
Current:  Nav → Hero → Process → BentoFeatures → Showcase → Metrics → CTA → Footer
New:      Nav → Hero → [Trust strip] → Process → BentoFeatures → Showcase → Metrics → CTA → Footer
```

A new **TrustStrip** component (logo/badge bar, "Windows Hello certified · Local-first · No telemetry") bridges Hero → Process and immediately answers the silent objection after the hero.

### 3.2 File Change Map

Every file in `/website/src` will be modified or replaced. Here is the exact change per file:

---

#### `index.html` — SEO + Performance
- Add JSON-LD `SoftwareApplication` structured data
- Add `twitter:site` and `twitter:creator`
- Add `<link rel="preload">` for critical fonts with `font-display: optional`
- Add `<meta name="color-scheme" content="dark">`
- Add `<link rel="apple-touch-icon">`
- Improve description copy

---

#### `tailwind.config.js` — Design System Expansion
- Add new keyframes: `float-slow`, `gradient-shift`, `glow-pulse`, `clip-reveal`, `border-glow`
- Add new animation shortcuts: `float-slow`, `gradient-shift`, `glow`  
- Add `backgroundImage` tokens for common gradients
- Add `blur` token extensions
- Add `transitionTimingFunction` for `spring-ease`

---

#### `src/index.css` — CSS Utilities Expansion
- Add `.text-gradient-cyan` (cyan→indigo for variance)
- Add `.cursor-dot` and `.cursor-ring` styles for custom cursor
- Add `.clip-reveal` keyframe utility
- Add `.section-divider` — a subtle hairline with radial fade
- Add `@keyframes gradient-border` for animated border shimmer
- Improve `.glass` and `.glass-strong` with better shadow layering
- Add `.card-lift` — combined hover transform + shadow
- Add `.prose-dim` for consistent body text across sections

---

#### `src/motion.js` — Motion Vocabulary Expansion
- Add `clipReveal` — clip-path from bottom, premium reveal
- Add `blurIn` — opacity + blur transition (like Raycast)
- Add `scaleIn` — opacity + scale from 0.92
- Add `slideInLeft` / `slideInRight` — for alternating layouts
- Add `perspectiveReveal` — rotateX entrance
- Add `magneticSpring` — spring config for magnetic buttons
- Add `staggerFast` / `staggerSlow` — more stagger options
- Refine `viewportOnce` margin to `-60px`

---

#### `src/hooks/useReducedMotion.js` — No change needed

---

#### `src/hooks/useMagneticButton.js` — **NEW**
- Custom hook for magnetic button effect
- Takes ref + strength (default 0.35)
- Returns `{ x, y, handlers }` using Framer `useMotionValue` + spring
- Respects `prefers-reduced-motion`

---

#### `src/hooks/useLenis.js` — **NEW**
- Initialises Lenis smooth scroll instance
- Connects to Framer Motion `useScroll` via RAF loop
- Returns lenis instance for programmatic scrolling

---

#### `src/components/Atmosphere/AtmosphericBackground.jsx` — Enhanced
- Keep topography, iso-grid, laser sweep, cursor glow
- Add a second slower laser sweep in opposing diagonal (subtle depth)
- Increase cursor glow radius to 520px, add cyan secondary halo
- Add a very subtle `grid-faint` layer that fades in on scroll (scroll-driven opacity)

---

#### `src/components/Atmosphere/AmbientAuras.jsx` — Enhanced
- Make aura positions scroll-responsive (subtle parallax drift)
- Add a third aura: deep violet, centered near bottom of page

---

#### `src/components/Atmosphere/BiometricOrb.jsx` — **NEW**
- Canvas-based animated biometric orb for the hero
- Uses HTML5 Canvas (no Three.js — keeps bundle lean while delivering visual impact)
- Renders: rotating iris ring, scan line sweep, pulsing core, floating data particles
- Spring-physics reaction to mouse proximity
- Respects reduced motion (static fallback)

---

#### `src/components/Atmosphere/GlassReflection.jsx` — Minor tweak
- Make the diagonal glare follow mouse angle (currently static at 105deg)

---

#### `src/components/Atmosphere/HolographicNodes.jsx` — Enhanced
- Add more nodes (8 → 12)
- Add subtle scale animation on individual nodes tied to a cursor proximity check
- Improve line gradient (add teal stop)

---

#### `src/components/Atmosphere/HoverBorderTrace.jsx` — Enhanced
- Improve gradient to include violet-to-cyan spectrum
- Slightly increase animation speed for perceived responsiveness

---

#### `src/components/Atmosphere/PipelineConnector.jsx` — Enhanced
- Make the neon line width animated (pulses on active step)
- Add glow filter on the active segment

---

#### `src/components/Atmosphere/Reticle.jsx` — Enhanced
- Add a third inner ring (finer detail)
- Add scanning dot that orbits the outer ring (replaces inner rotate)

---

#### `src/components/ui/Button.jsx` — Magnetic + Enhanced
- Integrate `useMagneticButton` hook
- Add glow shadow on primary variant hover
- Add shimmer sweep animation on primary button
- Improve glass variant: increase backdrop-blur, refine border

---

#### `src/components/ui/Container.jsx` — Slight tweak
- Keep as-is, ensure it's used consistently

---

#### `src/components/ui/SectionHeading.jsx` — Actually used now
- Refactor to accept `eyebrow`, `title`, `sub`, `align` props
- Used by ALL sections going forward (DRY, consistent)
- Internally uses the new `clipReveal` motion variant

---

#### `src/components/Nav.jsx` — Major Enhancement
- Add mobile menu (hamburger → full-screen overlay drawer)
- Add subtle logo glow on scroll
- Improve scrolled state to frosted glass pill (stronger backdrop)
- Keep `LayoutGroup` hover pill but add spring magnetism
- Add `aria-expanded` for mobile menu accessibility
- Add keyboard trap in mobile menu

---

#### `src/components/Hero.jsx` — **Complete Rethink** 🔴

**New hero concept: "The Scanner"**

- Left half: cinematic vertical typography — headline breaks across 3 lines with staggered per-word reveals using `clipReveal`
- Right half: the new `BiometricOrb` canvas centrepiece — a living, scanning biometric visualization
- Headline: splits into two visual "weights" — thin + bold — mixing Inter light and Plus Jakarta Sans ExtraBold
- Badge: now floats and pulses like a status indicator
- CTAs: magnetic buttons — the primary one has a shimmer sweep
- Scroll indicator: a thin animated line that says "Scroll" — fades out after first scroll event
- Background: adds a subtle radial `topography` around the orb
- Parallax: orb parallaxes faster than text (depth separation)
- Mobile: orb becomes a centered element below text, smaller

**Before/After mental model:**  
Before: SaaS hero with centered text + screenshot  
After: Dark luxury product reveal — biometric scanner + cinematic type

---

#### `src/components/TrustStrip.jsx` — **NEW**

A minimal horizontal band between Hero and Process:
- 4 trust signals in a horizontal scrolling marquee on mobile, flex on desktop
- "Windows Hello Native" · "No cloud telemetry" · "Local biometric storage" · "Private beta · Windows 11"
- Each item has a micro icon
- Subtle top/bottom hairlines
- Stagger entrance
- No heading — just signals

---

#### `src/components/Process.jsx` — Enhanced

- Replace generic `fadeInUp` with `perspectiveReveal` for heading
- Steps: Instead of accordion buttons, make them **horizontally traversable** on desktop — steps listed vertically on left, but the preview panel on the right becomes a cinematic visualization (not just rows of pills)
- Step 01 preview: animated app selector grid
- Step 02 preview: animated toggle with ripple
- Step 03 preview: Face-scan reticle from `Reticle.jsx` as the preview (full height)
- Step 04 preview: Access granted — green checkmark explodes with particles
- Add connecting animation between steps (active step glows brighter, line pulses)
- Keep the `PipelineConnector`

---

#### `src/components/BentoFeatures.jsx` — Enhanced + Expanded

- Expand to 5 cards in a more irregular asymmetric bento grid (not 4-col uniform)
- WindowsHello card (2×2): Replace the static reticle area with the `Reticle` component at larger size + add animated scan text cycling through "Analyzing depth map", "Comparing feature vectors", "Liveness check passed"
- PerApp card (2×1): Add animated app list preview that cycles items
- Privacy card (2×1): Add a visual — glowing shield with "0 bytes transmitted" counter
- Add two new smaller cards (1×1 each):
  - **Speed card**: animated `<50ms` counter with a velocity ring
  - **Compatibility card**: "Windows 11 · Hello API" with Windows logo treatment
- Mouse spotlight stays on all cards
- `HoverBorderTrace` on all cards

---

#### `src/components/Showcase.jsx` — Enhanced + Repositioned

- Move to after Process (before BentoFeatures) — it shows the interface before features are listed, better narrative
- Actually stays after BentoFeatures per the final order — but gets a stronger visual treatment
- Add a "now scanning" animation when a toggle is turned ON
- Add a staggered list entrance (each app row staggers in from left)
- Add app icons (emoji-based, or colored letter avatars with proper background colors per app)
- The search input gets a proper focus ring animation
- Add a "protection active" summary bar at the bottom that updates live

---

#### `src/components/Metrics.jsx` — Enhanced

- Add a subtle horizontal hairline section separator before the stats
- Replace the 4-column grid with a more dramatic layout: 2 large stats on left + 2 medium on right
- Make sparklines larger and more prominent (double the height)
- Add a glowing "endpoint" dot on each sparkline that pulses
- Improve CountUp: add comma formatting for large numbers
- Add a subtle radial glow under each stat number

---

#### `src/components/CtaBanner.jsx` — **Visual Overhaul**

- Add the `BiometricOrb` (small, 40% scale) floating in the top-right corner of the card as a decorative visual
- Add animated radial rings expanding outward from center
- Refine the form: email input becomes a proper floating-label input
- The submit button uses the magnetic effect
- Add a subtle particle/star field in the background of the card
- After submit: confetti-like particle burst

---

#### `src/components/Footer.jsx` — Polish

- Add a decorative topographic line above the footer
- Improve social link hover states (scale + glow on icon)
- Add a subtle "back to top" button (appears on scroll, fixed bottom-right)
- Add "Made with care. Built in public." line
- Improve status indicator (operational) — add a tooltip

---

#### `src/App.jsx` — Enhanced

- Initialise Lenis via `useLenis` hook
- Add `CustomCursor` component (desktop only, `pointer-fine` media query)
- Add lazy loading via `React.lazy` for below-fold sections
- Add `<Suspense>` boundaries
- Insert `TrustStrip` between Hero and Process
- Reorder: Hero → TrustStrip → Process → BentoFeatures → Showcase → Metrics → CTA → Footer

---

#### `src/components/CustomCursor.jsx` — **NEW**

- Dot + ring cursor following mouse with spring physics
- Ring expands on hoverable elements
- Ring morphs to text "View" on images, "Click" on CTAs
- Uses `pointer-fine` media query — desktop only, no mobile interference
- Respects `prefers-reduced-motion`

---

### 3.3 Libraries to Install

| Package | Reason | Size Impact |
|---|---|---|
| `@studio-freight/lenis` | Premium smooth scroll — native feel on Windows | ~7KB gzipped |

> **No Three.js.** The `BiometricOrb` will be Canvas 2D — same visual quality for this use case, 400KB+ lighter bundle. Three.js is genuinely overkill for a rotating ring + scan line. Canvas is the right tool here.

---

### 3.4 Motion Vocabulary Summary

| Variant | Use case | Description |
|---|---|---|
| `clipReveal` | Hero headline, section headings | `clip-path` from `inset(100% 0 0 0)` → `inset(0% 0 0 0)` |
| `blurIn` | Feature cards, body copy | `opacity: 0, filter: blur(8px)` → `opacity: 1, filter: blur(0)` |
| `scaleIn` | Badges, tags | `opacity: 0, scale: 0.88` → `opacity: 1, scale: 1` |
| `perspectiveReveal` | Section headings | `rotateX(12deg), opacity: 0` → `rotateX(0), opacity: 1` |
| `slideInLeft/Right` | Alternating content | translate X ±40px + opacity |
| `magneticSpring` | Buttons, nav links | `useSpring` position following cursor proximity |
| `staggerFast` | Nav links, tags | 0.05s stagger |
| `staggerSlow` | Section content | 0.12s stagger |

---

### 3.5 Section-by-Section Visual Direction

#### Hero — "The Scanner"
```
┌─────────────────────────────────────────────────────┐
│  [status badge]                                     │
│                                                     │
│  Protect your           [BIOMETRIC ORB]             │
│  apps with              [Living canvas —            │
│  your face.             rotating iris ring,         │
│                         scan sweep, particles]      │
│  [CTA ↗]  [How it works]                           │
│                                                     │
│  ────── scroll ──────                               │
└─────────────────────────────────────────────────────┘
```

#### TrustStrip
```
──── Windows Hello Native · No Cloud · Local Storage · Windows 11 ────
```

#### Process — "Four Steps"
```
Left: numbered steps (vertical list, active glows)
Right: contextual visual changing per step
  Step 1: app picker grid
  Step 2: toggle + ripple  
  Step 3: Reticle (face scan live)
  Step 4: checkmark + particles
```

#### BentoFeatures — Asymmetric Grid
```
┌──────────────┬──────┬──────┐
│ Windows Hello│ Per  │Speed │
│ (2×2 hero)  │ App  │      │
│              ├──────┴──────┤
│              │   Privacy   │
├──────────────┴─────────────┤
│       Compatibility        │
└────────────────────────────┘
```

#### Showcase — Live UI Demo
Interactive app list with real toggle interactions + scan animation

#### Metrics — Trust Numbers
4 stats with dramatic typography, prominent sparklines

#### CTA — "Be one of the first 200"
Form with BiometricOrb decorative, particle background

---

### 3.6 Performance Strategy

- Lazy load `Process`, `BentoFeatures`, `Showcase`, `Metrics`, `CtaBanner` (below fold)
- `BiometricOrb` canvas pauses drawing when off-screen (Intersection Observer)
- All `whileInView` already set to `once: true` — keep this
- Fonts: add `&display=optional` to Google Fonts URL
- Images: only `og.png` and `favicon.svg` — already optimised
- Code split via `React.lazy` saves ~30% of initial bundle

---

### 3.7 Accessibility Plan

- All `aria-hidden="true"` on decorative elements — keep
- Mobile menu: `aria-expanded`, `aria-label`, focus trap, `Escape` to close
- Custom cursor: never replaces cursor, only augments — accessibility unaffected
- Reduced motion: all animations defer to `useReducedMotion` / CSS media query
- Color contrast: all text on dark backgrounds passes WCAG AA (white/70 = ~4.5:1 on ink-950)
- Focus states: add explicit `focus-visible:ring-2 ring-indigo-400` to all interactive elements
- Semantic HTML: verify all sections have proper heading hierarchy

---

## 4. Implementation Order

1. **Install Lenis** (`npm install @studio-freight/lenis`)
2. **Expand design tokens** (`tailwind.config.js`, `index.css`, `motion.js`)
3. **Create hooks** (`useMagneticButton.js`, `useLenis.js`)
4. **Create new Atmosphere components** (`BiometricOrb.jsx`)
5. **Enhance existing Atmosphere** (AmbientAuras, GlassReflection, HolographicNodes, Reticle)
6. **Update UI components** (Button magnetic, SectionHeading, Container)
7. **Create CustomCursor.jsx**
8. **Rework Hero.jsx** (full rewrite)
9. **Create TrustStrip.jsx**
10. **Enhance Nav.jsx** (mobile menu)
11. **Enhance Process.jsx**
12. **Enhance BentoFeatures.jsx**
13. **Enhance Showcase.jsx**
14. **Enhance Metrics.jsx**
15. **Enhance CtaBanner.jsx**
16. **Polish Footer.jsx**
17. **Update App.jsx** (Lenis, lazy loading, new section order)
18. **Update index.html** (SEO, JSON-LD, font optimisation)
19. **Production build + audit**

---

## 5. What Does NOT Change

- The `ink-*` color palette — it's correct
- The indigo/violet/emerald accent trio — it's correct  
- The `[0.16, 1, 0.3, 1]` easing — it's premium
- Plus Jakarta Sans + Inter font pairing — it's correct
- The `viewportOnce` scroll pattern — keep
- The overall dark luxury direction — it's right
- The product's copywriting — it's already strong

---

> **This plan is complete. Implementation begins immediately after approval.**
