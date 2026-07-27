import { motion, useScroll, useTransform, useReducedMotion } from 'framer-motion';

/**
 * Three layered ultra-soft radial auras behind key focal points:
 *   1. Hero application window  (top, deep indigo)
 *   2. Metrics numbers block    (middle, indigo→blue blend)
 *   3. CTA card                 (bottom, emerald→indigo blend)
 *
 * Added scroll-responsive parallax drift and a third deep violet aura near the bottom.
 * Respects prefers-reduced-motion.
 */
export default function AmbientAuras() {
  const { scrollY } = useScroll();
  const reducedMotion = useReducedMotion();

  // Parallax drifts for each aura zone
  const y1 = useTransform(scrollY, [0, 1000], [0, -100]);
  const y2 = useTransform(scrollY, [500, 2000], [0, -80]);
  const y3 = useTransform(scrollY, [1000, 3000], [0, -120]);

  return (
    <div aria-hidden="true" className="pointer-events-none fixed inset-0 z-0 overflow-hidden">
      {/* 1. Hero aura — deep indigo, top-center */}
      <motion.div
        className="absolute left-1/2 -translate-x-1/2 -top-40 h-[640px] w-[1100px] rounded-full opacity-60"
        style={{
          y: reducedMotion ? 0 : y1,
          background: 'radial-gradient(ellipse at center, rgba(129,140,248,0.30) 0%, rgba(99,102,241,0.10) 35%, transparent 70%)',
          filter: 'blur(120px)',
        }}
      />

      {/* 2. Metrics aura — indigo → blue, middle of viewport */}
      <motion.div
        className="absolute left-[58%] -translate-x-1/2 top-[42%] h-[420px] w-[820px] rounded-full opacity-50"
        style={{
          y: reducedMotion ? 0 : y2,
          background: 'radial-gradient(ellipse at center, rgba(99,102,241,0.25) 0%, rgba(59,130,246,0.08) 40%, transparent 70%)',
          filter: 'blur(120px)',
        }}
      />

      {/* 3. CTA aura — emerald + indigo, bottom */}
      <motion.div
        className="absolute left-[35%] -translate-x-1/2 top-[74%] h-[440px] w-[800px] rounded-full opacity-60"
        style={{
          y: reducedMotion ? 0 : y3,
          background: 'radial-gradient(ellipse at center, rgba(52,211,153,0.18) 0%, rgba(129,140,248,0.18) 30%, rgba(99,102,241,0.05) 55%, transparent 75%)',
          filter: 'blur(120px)',
        }}
      />

      {/* 4. Bottom deep violet aura — stabilizes the footer zone */}
      <div
        className="absolute left-1/2 -translate-x-1/2 bottom-[-100px] h-[350px] w-[900px] rounded-full opacity-40"
        style={{
          background: 'radial-gradient(ellipse at center, rgba(124,58,237,0.15) 0%, transparent 70%)',
          filter: 'blur(100px)',
        }}
      />
    </div>
  );
}
