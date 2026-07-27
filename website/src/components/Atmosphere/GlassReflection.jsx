import { motion, useMotionValue, useMotionTemplate, useTransform, useSpring, useReducedMotion } from 'framer-motion';
import { useRef } from 'react';

/**
 * Diagonal glare overlay — sits over a surface to give it a "real glass" feel.
 * Enhancements:
 *   - The glare shifts dynamically using 3D rotation based on mouse coordinates.
 *   - Mouse-aware specular highlight.
 *   - Respects reduced motion.
 */
export default function GlassReflection({ className = '', mouseAware = true }) {
  const ref = useRef(null);
  const reduceMotion = useReducedMotion();
  const mx = useMotionValue(50);
  const my = useMotionValue(50);

  // Specular highlight radial gradient
  const spec = useMotionTemplate`radial-gradient(240px circle at ${mx}% ${my}%, rgba(255,255,255,0.12), transparent 60%)`;

  // Dynamic glare shift values (rotation and offset) to simulate light hitting glass from different angles
  const glareX = useTransform(mx, [0, 100], [-15, 15]);
  const glareY = useTransform(my, [0, 100], [-15, 15]);
  const springGlareX = useSpring(glareX, { stiffness: 120, damping: 22 });
  const springGlareY = useSpring(glareY, { stiffness: 120, damping: 22 });

  return (
    <div
      ref={ref}
      onMouseMove={(e) => {
        if (!mouseAware || reduceMotion) return;
        const r = ref.current?.getBoundingClientRect();
        if (!r) return;
        mx.set(((e.clientX - r.left) / r.width) * 100);
        my.set(((e.clientY - r.top) / r.height) * 100);
      }}
      onMouseLeave={() => {
        mx.set(50);
        my.set(50);
      }}
      aria-hidden="true"
      className={'pointer-events-none absolute inset-0 overflow-hidden rounded-[inherit] ' + className}
      style={{ perspective: 1000 }}
    >
      {/* 3D dynamic diagonal glare layer */}
      <motion.div
        className="absolute inset-[-20%] glare opacity-80"
        style={{
          x: reduceMotion ? 0 : springGlareX,
          y: reduceMotion ? 0 : springGlareY,
          rotate: 15,
        }}
      />

      {/* Mouse-aware specular light */}
      {mouseAware && !reduceMotion && (
        <motion.div
          className="absolute inset-0"
          style={{ background: spec, mixBlendMode: 'screen' }}
        />
      )}
    </div>
  );
}
