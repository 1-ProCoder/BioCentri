import { motion } from 'framer-motion';
import { useId } from 'react';

/**
 * Wraps children and overlays an animated gradient border that sweeps
 * around the perimeter on hover.
 *
 * Enhancements:
 *  - High-end violet-cyan-indigo spectrum gradient.
 *  - Faster sweep duration (3s) for responsive feel.
 */
export default function HoverBorderTrace({ children, className = '', radius = 24 }) {
  const uid = useId().replace(/:/g, '');
  const id = `trace-${uid}`;

  return (
    <div className={'group/trace relative isolate ' + className}>
      {/* Trace overlay */}
      <svg
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 h-full w-full opacity-0 transition-opacity duration-300 group-hover/trace:opacity-100"
      >
        <defs>
          <linearGradient id={`${id}-grad`} x1="0" y1="0" x2="1" y2="0">
            <stop offset="0%"   stopColor="#818cf8" stopOpacity="0" />
            <stop offset="15%"  stopColor="#818cf8" stopOpacity="1" />
            <stop offset="45%"  stopColor="#a78bfa" stopOpacity="1" />
            <stop offset="70%"  stopColor="#67e8f9" stopOpacity="1" />
            <stop offset="90%"  stopColor="#818cf8" stopOpacity="1" />
            <stop offset="100%" stopColor="#818cf8" stopOpacity="0" />
          </linearGradient>
        </defs>
        <motion.rect
          x="0" y="0" width="100%" height="100%"
          rx={radius} ry={radius}
          fill="none"
          stroke={`url(#${id}-grad)`}
          strokeWidth="1.5"
          strokeLinecap="round"
          pathLength={1}
          initial={{ strokeDasharray: '0.2 0.8', strokeDashoffset: 0 }}
          animate={{
            strokeDasharray: ['0.2 0.8', '0.2 0.8'],
            strokeDashoffset: [-1, 1],
          }}
          transition={{
            duration: 3,
            ease: 'linear',
            repeat: Infinity,
          }}
        />
      </svg>

      {/* Children inherit normal border; the trace sits on top */}
      <div className="relative">{children}</div>
    </div>
  );
}
