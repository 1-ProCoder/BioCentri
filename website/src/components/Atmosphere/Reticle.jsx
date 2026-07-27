import { motion } from 'framer-motion';

/**
 * Reticle — three concentric rotating conic rings.
 * Ring 1: indigo, fast  | Ring 2: cyan, reverse  | Ring 3: emerald, slow
 */
export default function Reticle({ size = 160, className = '' }) {
  const s = size;
  const half = s / 2;

  return (
    <div
      aria-hidden="true"
      className={'pointer-events-none relative ' + className}
      style={{ width: s, height: s }}
    >
      {/* Ring 1 — indigo, primary rotation */}
      <div
        className="absolute inset-0 rounded-full reticle-ring animate-reticle-spin"
        style={{ mask: `radial-gradient(transparent ${half * 0.68}px, black ${half * 0.69}px, black ${half * 0.84}px, transparent ${half * 0.85}px)` }}
      />
      {/* Ring 2 — cyan, counter-rotation */}
      <div
        className="absolute inset-0 rounded-full reticle-ring-rev animate-reticle-spin-rev"
        style={{ mask: `radial-gradient(transparent ${half * 0.88}px, black ${half * 0.89}px, black ${half * 0.97}px, transparent ${half * 0.98}px)` }}
      />
      {/* Ring 3 — emerald, very slow */}
      <div
        className="absolute inset-0 rounded-full reticle-ring-slow animate-reticle-spin-slow"
        style={{ mask: `radial-gradient(transparent ${half * 0.53}px, black ${half * 0.54}px, black ${half * 0.64}px, transparent ${half * 0.65}px)` }}
      />
      {/* Ring 4 — inner fine detail ring, very slow */}
      <div
        className="absolute inset-0 rounded-full reticle-ring animate-reticle-spin-slow"
        style={{ 
          mask: `radial-gradient(transparent ${half * 0.42}px, black ${half * 0.43}px, black ${half * 0.50}px, transparent ${half * 0.51}px)`,
          opacity: 0.6
        }}
      />

      {/* Cross-hair ticks at cardinal points */}
      {[0, 90, 180, 270].map((deg) => (
        <div
          key={deg}
          className="absolute"
          style={{
            top: '50%', left: '50%',
            width: 8, height: 1.5,
            background: 'rgba(165,180,252,0.7)',
            transformOrigin: '0 50%',
            transform: `rotate(${deg}deg) translateX(${half * 0.72}px) translateY(-50%)`,
          }}
        />
      ))}

      {/* Scanning dot orbiting the outer ring */}
      <motion.div
        className="absolute top-1/2 left-1/2 h-2 w-2 -translate-x-1/2 -translate-y-1/2"
        animate={{ rotate: 360 }}
        transition={{ duration: 5, ease: 'linear', repeat: Infinity }}
        style={{ transformOrigin: '50% 50%' }}
      >
        <div
          className="absolute"
          style={{
            width: 4, height: 4,
            borderRadius: '50%',
            background: 'rgba(103,232,249,0.95)',
            boxShadow: '0 0 10px rgba(103,232,249,0.9), 0 0 20px rgba(103,232,249,0.4)',
            top: `${-half * 0.88}px`,
            left: '-2px',
          }}
        />
      </motion.div>
    </div>
  );
}
