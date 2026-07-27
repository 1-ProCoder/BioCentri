import { useEffect, useRef } from 'react';
import Lenis from 'lenis';

/**
 * Initialises Lenis smooth scroll and connects it to Framer Motion's
 * RAF loop via requestAnimationFrame.
 *
 * Returns the lenis instance for programmatic scrolling (lenis.scrollTo('#section')).
 * Also forces scroll-to-top on first mount to prevent browser scroll restoration.
 */
export function useLenis() {
  const lenisRef = useRef(null);

  useEffect(() => {
    const lenis = new Lenis({
      duration: 1.1,
      easing: (t) => Math.min(1, 1.001 - Math.pow(2, -10 * t)), // expo ease-out
      orientation: 'vertical',
      smoothWheel: true,
      wheelMultiplier: 0.9,
      touchMultiplier: 1.8,
    });

    lenisRef.current = lenis;

    // Prevent native scroll from fighting Lenis by immediately starting the RAF loop
    // and forcing scroll-to-top on the very first frame (after Lenis has measured
    // its virtual scroll position from window.scrollY)
    let fired = false;
    let raf;
    function onRaf(time) {
      lenis.raf(time);
      if (!fired) {
        fired = true;
        // Force scroll to top on the first frame — this runs AFTER Lenis has
        // initialised its virtual position, so it overrides any browser restoration
        lenis.scrollTo(0, { immediate: true });
      }
      raf = requestAnimationFrame(onRaf);
    }
    raf = requestAnimationFrame(onRaf);

    return () => {
      cancelAnimationFrame(raf);
      lenis.destroy();
    };
  }, []);

  return lenisRef;
}
