import { motion, useMotionValue, useMotionTemplate, useReducedMotion } from 'framer-motion';
import { useEffect } from 'react';

/**
 * Page-wide atmospheric backdrop.
 * - Topography ridges (fingerprint / face-scan feel)
 * - Isometric cyber grid
 * - Two laser sweeps (vertical + diagonal)
 * - Cursor-tracking indigo/cyan glow with wider radius
 */
export default function AtmosphericBackground() {
  const reduceMotion = useReducedMotion();
  const mx = useMotionValue(-9999);
  const my = useMotionValue(-9999);

  // Primary indigo halo
  const cursorBg = useMotionTemplate`radial-gradient(
    520px circle at ${mx}px ${my}px,
    rgba(129,140,248,0.09),
    rgba(103,232,249,0.04),
    transparent 68%
  )`;

  // Secondary, smaller, higher opacity
  const cursorBg2 = useMotionTemplate`radial-gradient(
    180px circle at ${mx}px ${my}px,
    rgba(165,180,252,0.05),
    transparent 100%
  )`;

  useEffect(() => {
    if (reduceMotion) return;
    let raf = 0;
    const onMove = (e) => {
      cancelAnimationFrame(raf);
      raf = requestAnimationFrame(() => {
        mx.set(e.clientX);
        my.set(e.clientY);
      });
    };
    window.addEventListener('mousemove', onMove, { passive: true });
    return () => {
      window.removeEventListener('mousemove', onMove);
      cancelAnimationFrame(raf);
    };
  }, [reduceMotion, mx, my]);

  return (
    <div
      aria-hidden="true"
      className="pointer-events-none fixed inset-0 z-0 overflow-hidden"
    >
      {/* 1. Topography ridges — strongest at top, fades with depth */}
      <div
        className="absolute inset-x-0 top-0 h-[160vh] topography opacity-100"
        style={{
          WebkitMaskImage:
            'radial-gradient(ellipse 80% 55% at 50% 0%, #000 0%, rgba(0,0,0,0.5) 40%, transparent 80%)',
          maskImage:
            'radial-gradient(ellipse 80% 55% at 50% 0%, #000 0%, rgba(0,0,0,0.5) 40%, transparent 80%)',
        }}
      />

      {/* 2. Isometric cyber grid — mid-page band */}
      <div
        className="absolute inset-0 grid-iso opacity-[0.045]"
        style={{
          WebkitMaskImage:
            'linear-gradient(to bottom, transparent 0%, rgba(0,0,0,0.65) 28%, transparent 65%)',
          maskImage:
            'linear-gradient(to bottom, transparent 0%, rgba(0,0,0,0.65) 28%, transparent 65%)',
        }}
      />

      {/* 3. Vertical laser sweep */}
      <div
        className="absolute inset-x-0 -top-12 bottom-0 overflow-hidden"
        style={{
          WebkitMaskImage: 'linear-gradient(to bottom, transparent, black 25%, black 75%, transparent)',
          maskImage:       'linear-gradient(to bottom, transparent, black 25%, black 75%, transparent)',
        }}
      >
        <div
          className="absolute inset-x-0 animate-laser-sweep h-48"
          style={{
            background:
              'linear-gradient(180deg, transparent 0%, rgba(129,140,248,0.20) 42%, rgba(129,140,248,0.50) 50%, rgba(129,140,248,0.20) 58%, transparent 100%)',
            filter: 'blur(1px)',
          }}
        />
      </div>

      {/* 4. Diagonal secondary sweep — very subtle */}
      <div
        className="absolute inset-0 overflow-hidden opacity-40"
        style={{
          WebkitMaskImage: 'linear-gradient(to right, transparent, black 20%, black 80%, transparent)',
          maskImage:       'linear-gradient(to right, transparent, black 20%, black 80%, transparent)',
        }}
      >
        <div
          className="absolute inset-y-0 animate-laser-sweep-h w-64"
          style={{
            left: '35%',
            background:
              'linear-gradient(90deg, transparent 0%, rgba(103,232,249,0.15) 45%, rgba(103,232,249,0.30) 50%, rgba(103,232,249,0.15) 55%, transparent 100%)',
            filter: 'blur(2px)',
          }}
        />
      </div>

      {/* 5. Cursor glow — primary halo */}
      {!reduceMotion && (
        <motion.div
          className="absolute inset-0"
          style={{ background: cursorBg, mixBlendMode: 'screen' }}
        />
      )}

      {/* 6. Cursor glow — secondary tight halo */}
      {!reduceMotion && (
        <motion.div
          className="absolute inset-0"
          style={{ background: cursorBg2, mixBlendMode: 'screen' }}
        />
      )}
    </div>
  );
}
