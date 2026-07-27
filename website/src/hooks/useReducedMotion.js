// Centralised reduced-motion helper for the BioCentri site.
// All atmosphere / motion code should consult this hook before
// installing mouse listeners, scroll listeners, or continuous animations.
import { useReducedMotion as framerUseReducedMotion } from 'framer-motion';

export function useReducedMotion() {
  return !!framerUseReducedMotion();
}
