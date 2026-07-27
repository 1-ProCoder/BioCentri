import { motion } from 'framer-motion';

/**
 * Floating holographic UI nodes — glowing dots linked by dashed connection lines.
 * Sits in the section background.
 *
 * Enhancements:
 *   - 12 nodes forming a rich, cybersecurity-themed constellation.
 *   - Large-target CSS proximity hover scaling (100% performance, no CPU overhead).
 *   - Dynamic linear gradient with indigo, violet, and teal stops.
 */
export default function HolographicNodes({ className = '' }) {
  const nodes = [
    { x: '8%',  y: '14%', size: 5, anim: 'hologram-float-a' },
    { x: '22%', y: '38%', size: 4, anim: 'hologram-float-b' },
    { x: '38%', y: '8%',  size: 6, anim: 'hologram-float-c' },
    { x: '66%', y: '24%', size: 5, anim: 'hologram-float-a' },
    { x: '82%', y: '46%', size: 7, anim: 'hologram-float-b' },
    { x: '92%', y: '14%', size: 4, anim: 'hologram-float-c' },
    // 6 new nodes for a denser, more sophisticated constellation
    { x: '12%', y: '72%', size: 6, anim: 'hologram-float-b' },
    { x: '45%', y: '88%', size: 5, anim: 'hologram-float-a' },
    { x: '58%', y: '68%', size: 4, anim: 'hologram-float-c' },
    { x: '78%', y: '84%', size: 7, anim: 'hologram-float-b' },
    { x: '32%', y: '58%', size: 5, anim: 'hologram-float-c' },
    { x: '89%', y: '72%', size: 6, anim: 'hologram-float-a' },
  ];

  // Hand-tuned connections for clean flow across the screen
  const edges = [
    [0, 2], [0, 1], [1, 3], [2, 4], [3, 4], [4, 5], [1, 2], [3, 5],
    [1, 6], [6, 10], [10, 8], [8, 7], [7, 9], [9, 11], [4, 11], [3, 8], [2, 10]
  ];

  return (
    <div
      aria-hidden="true"
      className={'pointer-events-none absolute inset-0 z-0 select-none ' + className}
    >
      <svg
        className="absolute inset-0 h-full w-full"
        viewBox="0 0 100 100"
        preserveAspectRatio="none"
      >
        <defs>
          <linearGradient id="holo-line-grad" x1="0" y1="0" x2="1" y2="1">
            <stop offset="0%"   stopColor="#818cf8" stopOpacity="0.35" />
            <stop offset="50%"  stopColor="#a78bfa" stopOpacity="0.25" />
            <stop offset="100%" stopColor="#67e8f9" stopOpacity="0.15" />
          </linearGradient>
        </defs>
        {/* Connection lines (dashed, moving) */}
        {edges.map(([a, b], i) => {
          const A = nodes[a];
          const B = nodes[b];
          return (
            <motion.line
              key={`e-${i}`}
              x1={A.x} y1={A.y} x2={B.x} y2={B.y}
              stroke="url(#holo-line-grad)"
              strokeWidth="0.16"
              strokeDasharray="0.8 1.4"
              initial={{ strokeDashoffset: 0 }}
              animate={{ strokeDashoffset: [-6, 0] }}
              transition={{
                duration: 9 + (i % 4),
                ease: 'linear',
                repeat: Infinity,
              }}
            />
          );
        })}
      </svg>

      {/* Nodes themselves */}
      {nodes.map((n, i) => (
        <div
          key={`n-${i}`}
          className="absolute group/node pointer-events-auto cursor-help"
          style={{
            left: n.x,
            top: n.y,
            transform: 'translate(-50%, -50%)',
            width: 48,
            height: 48,
            display: 'grid',
            placeItems: 'center',
          }}
        >
          {/* Node core (scales and glows on hover of this target region) */}
          <div className="relative pointer-events-none transition-all duration-300 ease-out-expo group-hover/node:scale-[1.8]">
            <span
              className="block rounded-full bg-indigo-300 shadow-[0_0_0_1.5px_rgba(165,180,252,0.4),0_0_12px_rgba(165,180,252,0.6)] group-hover/node:bg-cyan-300 group-hover/node:shadow-[0_0_0_2px_rgba(103,232,249,0.5),0_0_16px_rgba(103,232,249,0.85)]"
              style={{ width: n.size, height: n.size }}
            />
            {/* Pulsing ring backdrop */}
            <span
              className={'absolute inset-0 -m-2 rounded-full animate-pulse-glow group-hover/node:bg-cyan-400/20 ' + (i % 3 === 0 ? 'bg-indigo-400/10' : 'bg-transparent')}
            />
            {/* Floater animation layer */}
            <span
              className={'absolute inset-0 rounded-full animate-' + n.anim}
            />
          </div>
        </div>
      ))}
    </div>
  );
}
