// ─────────────────────────────────────────────────────────────
// BioCentri Motion Vocabulary
// Premium easing: [0.16, 1, 0.3, 1] = out-expo (fast start, gentle land)
// Spring hover: stiffness 280, damping 20 (snappy, zero bounce)
// Spring magnetic: stiffness 200, damping 18 (elastic, warm)
// ─────────────────────────────────────────────────────────────

// ─── Stagger parents ─────────────────────────────────────────
export const staggerParent = {
  hidden: {},
  show: {
    transition: { staggerChildren: 0.10, delayChildren: 0.05 },
  },
};

export const staggerFast = {
  hidden: {},
  show: {
    transition: { staggerChildren: 0.055, delayChildren: 0.02 },
  },
};

export const staggerSlow = {
  hidden: {},
  show: {
    transition: { staggerChildren: 0.14, delayChildren: 0.08 },
  },
};

// ─── Base reveals ────────────────────────────────────────────

/** Classic upward fade — use sparingly, prefer richer variants */
export const fadeInUp = {
  hidden: { opacity: 0, y: 22 },
  show: {
    opacity: 1, y: 0,
    transition: { duration: 0.52, ease: [0.16, 1, 0.3, 1] },
  },
};

/** Blur + lift — Raycast-style, premium feel */
export const blurIn = {
  hidden: { opacity: 0, filter: 'blur(10px)', y: 10 },
  show: {
    opacity: 1, filter: 'blur(0px)', y: 0,
    transition: { duration: 0.6, ease: [0.16, 1, 0.3, 1] },
  },
};

/** Scale entrance — great for badges, icons, tags */
export const scaleIn = {
  hidden: { opacity: 0, scale: 0.86 },
  show: {
    opacity: 1, scale: 1,
    transition: { duration: 0.44, ease: [0.34, 1.56, 0.64, 1] },
  },
};

/** Clip-path reveal — cinematic word/line entrance */
export const clipReveal = {
  hidden: { clipPath: 'inset(100% 0 0 0)', opacity: 0 },
  show: {
    clipPath: 'inset(0% 0 0 0)', opacity: 1,
    transition: { duration: 0.7, ease: [0.16, 1, 0.3, 1] },
  },
};

/** Perspective entrance — rotateX, heading-level drama */
export const perspectiveReveal = {
  hidden: { opacity: 0, rotateX: 14, y: 30, transformOrigin: 'top center' },
  show: {
    opacity: 1, rotateX: 0, y: 0,
    transition: { duration: 0.7, ease: [0.16, 1, 0.3, 1] },
  },
};

/** Slide from left */
export const slideInLeft = {
  hidden: { opacity: 0, x: -40 },
  show: {
    opacity: 1, x: 0,
    transition: { duration: 0.55, ease: [0.16, 1, 0.3, 1] },
  },
};

/** Slide from right */
export const slideInRight = {
  hidden: { opacity: 0, x: 40 },
  show: {
    opacity: 1, x: 0,
    transition: { duration: 0.55, ease: [0.16, 1, 0.3, 1] },
  },
};

// ─── Hover / interaction ─────────────────────────────────────

export const cardHover = {
  rest:  { y: 0,  scale: 1 },
  hover: { y: -7, scale: 1.004,
    transition: { type: 'spring', stiffness: 280, damping: 20 } },
};

export const pressTap = { scale: 0.96 };

/** Spring config for magnetic buttons */
export const magneticSpring = { type: 'spring', stiffness: 200, damping: 18 };

// ─── Viewport preset ─────────────────────────────────────────
export const viewportOnce = { once: true, margin: '-60px' };
