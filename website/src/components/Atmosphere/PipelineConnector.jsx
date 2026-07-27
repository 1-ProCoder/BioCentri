import { motion } from 'framer-motion';

/**
 * Vertical neon "pipeline" threading the Process 01-04 step buttons.
 *
 * Enhancements:
 *   - Spring-driven active segment glow (a vertical laser pill) that physicalises the active step.
 *   - Drop-shadow glow filter on the active segment and dot.
 *   - Smooth transitions for step dots.
 */
export default function PipelineConnector({ count = 4, active = 0 }) {
  // step dot positions as percentages of column height
  const slots = Array.from({ length: count }, (_, i) =>
    ((i + 0.5) / count) * 100,
  );

  const activeY = slots[active];

  return (
    <div
      aria-hidden="true"
      className="pointer-events-none absolute left-[26px] top-0 bottom-0 w-px"
    >
      {/* 1. Static gradient line */}
      <div
        className="absolute inset-0 w-px"
        style={{
          background:
            'linear-gradient(180deg, rgba(129,140,248,0) 0%, rgba(129,140,248,0.3) 14%, rgba(165,180,252,0.4) 50%, rgba(129,140,248,0.3) 86%, rgba(129,140,248,0) 100%)',
        }}
      />

      {/* 2. Animated travelling scan segment (slow passive sweep) */}
      <motion.div
        className="absolute left-0 right-0 h-24 animate-pipeline-descend"
        style={{
          background:
            'linear-gradient(180deg, transparent, rgba(165,180,252,0.0) 20%, rgba(165,180,252,0.7) 50%, rgba(165,180,252,0.0) 80%, transparent)',
          filter: 'blur(0.5px)',
        }}
      />

      {/* 3. Dynamic active step glow segment (follows active step) */}
      <motion.div
        className="absolute left-1/2 -translate-x-1/2 w-0.5 rounded-full"
        initial={false}
        animate={{
          top: `calc(${activeY}% - 32px)`,
          height: 64,
          background: active === 3 
            ? 'linear-gradient(180deg, rgba(129,140,248,0.2), #34d399, rgba(129,140,248,0.2))' 
            : 'linear-gradient(180deg, rgba(129,140,248,0.2), #818cf8, rgba(129,140,248,0.2))',
        }}
        transition={{
          type: 'spring',
          stiffness: 100,
          damping: 18,
        }}
        style={{
          filter: 'drop-shadow(0 0 4px rgba(129, 140, 248, 0.75))',
        }}
      />

      {/* 4. Per-step dots */}
      {slots.map((pct, i) => {
        const isActive = i === active;
        const isPast = i < active;
        return (
          <div
            key={i}
            className="absolute left-1/2 -translate-x-1/2 -translate-y-1/2"
            style={{ top: `${pct}%` }}
          >
            <motion.span
              animate={{
                scale: isActive ? 1.35 : 1,
              }}
              transition={{ type: 'spring', stiffness: 300, damping: 20 }}
              className={
                'block h-2.5 w-2.5 rounded-full ring-2 transition-colors ' +
                (isActive
                  ? 'bg-emerald-300 ring-emerald-300/30 shadow-[0_0_12px_rgba(52,211,153,0.7)]'
                  : isPast
                  ? 'bg-indigo-300/80 ring-indigo-300/20'
                  : 'bg-white/30 ring-white/10')
              }
            />
          </div>
        );
      })}
    </div>
  );
}
