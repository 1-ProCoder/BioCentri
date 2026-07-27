import { motion, useMotionValue, useSpring } from 'framer-motion';
import { useEffect, useRef, useState } from 'react';
import { useReducedMotion } from 'framer-motion';

/**
 * Custom cursor — dot + ring, desktop only (pointer:fine).
 * Ring expands over interactive elements.
 * Replaced via CSS cursor:none on .cursor-none root.
 */
export default function CustomCursor() {
  const reducedMotion = useReducedMotion();
  const [visible, setVisible] = useState(false);
  const [hovering, setHovering] = useState(false);

  const mx = useMotionValue(-100);
  const my = useMotionValue(-100);

  // Dot: instant
  const dotX = useSpring(mx, { stiffness: 800, damping: 35, mass: 0.4 });
  const dotY = useSpring(my, { stiffness: 800, damping: 35, mass: 0.4 });

  // Ring: slightly lagged for depth
  const ringX = useSpring(mx, { stiffness: 180, damping: 22, mass: 0.8 });
  const ringY = useSpring(my, { stiffness: 180, damping: 22, mass: 0.8 });

  useEffect(() => {
    if (reducedMotion) return;

    // Only show on pointer:fine devices
    if (!window.matchMedia('(pointer: fine)').matches) return;

    const onMove = (e) => {
      mx.set(e.clientX);
      my.set(e.clientY);
      setVisible(true);
    };
    const onLeave  = () => setVisible(false);
    const onEnter  = () => setVisible(true);

    window.addEventListener('mousemove', onMove,  { passive: true });
    window.addEventListener('mouseleave', onLeave);
    window.addEventListener('mouseenter', onEnter);

    const onHoverStart = () => setHovering(true);
    const onHoverEnd   = () => setHovering(false);

    const interactives = document.querySelectorAll(
      'a, button, [role="button"], input, label, [tabindex]'
    );
    interactives.forEach((el) => {
      el.addEventListener('mouseenter', onHoverStart);
      el.addEventListener('mouseleave', onHoverEnd);
    });

    return () => {
      window.removeEventListener('mousemove', onMove);
      window.removeEventListener('mouseleave', onLeave);
      window.removeEventListener('mouseenter', onEnter);
      interactives.forEach((el) => {
        el.removeEventListener('mouseenter', onHoverStart);
        el.removeEventListener('mouseleave', onHoverEnd);
      });
    };
  }, [reducedMotion, mx, my]);

  if (reducedMotion) return null;

  return (
    <div aria-hidden="true" className="pointer-events-none fixed inset-0 z-[9999]">
      {/* Ring */}
      <motion.div
        style={{ x: ringX, y: ringY, translateX: '-50%', translateY: '-50%' }}
        animate={{
          width:   hovering ? 40 : 28,
          height:  hovering ? 40 : 28,
          opacity: visible  ? 1  : 0,
          borderColor: hovering
            ? 'rgba(165,180,252,0.8)'
            : 'rgba(165,180,252,0.45)',
        }}
        transition={{ type: 'spring', stiffness: 300, damping: 22 }}
        className="fixed top-0 left-0 rounded-full border"
      />
      {/* Dot */}
      <motion.div
        style={{ x: dotX, y: dotY, translateX: '-50%', translateY: '-50%' }}
        animate={{ opacity: visible ? 1 : 0 }}
        className="fixed top-0 left-0 h-1.5 w-1.5 rounded-full bg-indigo-300"
      />
    </div>
  );
}
