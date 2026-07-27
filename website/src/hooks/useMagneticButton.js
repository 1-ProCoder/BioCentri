import { useMotionValue, useSpring } from 'framer-motion';
import { useRef, useCallback } from 'react';
import { useReducedMotion } from 'framer-motion';

/**
 * Magnetic button hook — gives elements a subtle attraction to the cursor.
 *
 * @param {number} strength  0–1, how strongly the element follows the cursor (default 0.32)
 * @returns {{ ref, x, y, handlers }} — attach ref to the element, spread handlers, use x/y as motion values
 */
export function useMagneticButton(strength = 0.32) {
  const ref = useRef(null);
  const reducedMotion = useReducedMotion();

  const rawX = useMotionValue(0);
  const rawY = useMotionValue(0);

  const x = useSpring(rawX, { stiffness: 200, damping: 18, mass: 0.8 });
  const y = useSpring(rawY, { stiffness: 200, damping: 18, mass: 0.8 });

  const onMouseMove = useCallback((e) => {
    if (reducedMotion || !ref.current) return;
    const rect = ref.current.getBoundingClientRect();
    const cx = rect.left + rect.width  / 2;
    const cy = rect.top  + rect.height / 2;
    rawX.set((e.clientX - cx) * strength);
    rawY.set((e.clientY - cy) * strength);
  }, [reducedMotion, rawX, rawY, strength]);

  const onMouseLeave = useCallback(() => {
    rawX.set(0);
    rawY.set(0);
  }, [rawX, rawY]);

  return {
    ref,
    x,
    y,
    handlers: { onMouseMove, onMouseLeave },
  };
}
