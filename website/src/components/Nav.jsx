import { motion, AnimatePresence, LayoutGroup } from 'framer-motion';
import { Shield, ArrowRight, X, Menu } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import { useMagneticButton } from '../hooks/useMagneticButton';

const links = [
  { href: '#process',  label: 'How it works' },
  { href: '#features', label: 'Features' },
  { href: '#metrics',  label: 'Trust' },
  { href: '#showcase', label: 'Preview' },
];

function NavCTA() {
  const { ref, x, y, handlers } = useMagneticButton(0.22);
  return (
    <motion.div ref={ref} style={{ x, y }} {...handlers} className="flex">
      <motion.a
        whileTap={{ scale: 0.96 }}
        href="#cta"
        className="group relative inline-flex items-center gap-1.5 overflow-hidden rounded-full bg-white px-4 py-2 text-[13px] font-semibold text-ink-950 shadow-[inset_0_1px_0_rgba(255,255,255,0.7),0_6px_20px_-6px_rgba(255,255,255,0.32)] transition-shadow hover:shadow-[inset_0_1px_0_rgba(255,255,255,0.7),0_8px_28px_-5px_rgba(255,255,255,0.40)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400"
      >
        {/* Shimmer sweep */}
        <span
          aria-hidden="true"
          className="pointer-events-none absolute inset-0 -translate-x-full group-hover:translate-x-full transition-transform duration-600 ease-in-out"
          style={{ background: 'linear-gradient(90deg, transparent 0%, rgba(0,0,0,0.07) 50%, transparent 100%)' }}
        />
        Join beta
        <ArrowRight className="h-3.5 w-3.5 transition-transform duration-200 group-hover:translate-x-0.5" />
      </motion.a>
    </motion.div>
  );
}

export default function Nav() {
  const [scrolled, setScrolled]     = useState(false);
  const [hovered, setHovered]       = useState('');
  const [menuOpen, setMenuOpen]     = useState(false);
  const menuRef                     = useRef(null);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 24);
    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  // Close menu on Escape
  useEffect(() => {
    const onKey = (e) => { if (e.key === 'Escape') setMenuOpen(false); };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, []);

  // Lock body scroll while menu open
  useEffect(() => {
    document.body.style.overflow = menuOpen ? 'hidden' : '';
    return () => { document.body.style.overflow = ''; };
  }, [menuOpen]);

  return (
    <>
      <motion.header
        initial={{ y: -28, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        transition={{ duration: 0.55, ease: [0.16, 1, 0.3, 1] }}
        className="fixed inset-x-0 top-4 z-50 flex justify-center px-4"
      >
        <nav
          aria-label="Primary"
          className={
            'flex w-full max-w-5xl items-center justify-between gap-2 rounded-full px-2.5 py-2 transition-all duration-300 ' +
            (scrolled
              ? 'glass-strong shadow-[0_8px_40px_-12px_rgba(0,0,0,0.55),0_0_0_1px_rgba(255,255,255,0.05)] backdrop-blur-2xl'
              : 'bg-black/25 border border-white/[0.06] backdrop-blur-md')
          }
        >
          {/* Logo */}
          <a
            href="#top"
            className="group flex items-center gap-2 rounded-full pl-1.5 pr-3 py-1 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400"
          >
            <span className={`grid h-7 w-7 place-items-center rounded-lg bg-gradient-to-br from-indigo-300 to-violet-400 text-ink-950 shadow-[inset_0_1px_0_rgba(255,255,255,0.5)] transition-all duration-300 group-hover:scale-105 ${scrolled ? 'shadow-[0_0_16px_-4px_rgba(129,140,248,0.5)]' : ''}`}>
              <Shield className="h-3.5 w-3.5" strokeWidth={2.5} />
            </span>
            <span className="font-display text-[15px] font-bold tracking-tight">BioCentri</span>
          </a>

          {/* Desktop links */}
          <LayoutGroup id="nav-pill">
            <ul className="hidden md:flex items-center gap-0.5 text-[13px] text-white/60 relative">
              {links.map((l) => (
                <li key={l.href} className="relative">
                  <a
                    href={l.href}
                    onMouseEnter={() => setHovered(l.href)}
                    onMouseLeave={() => setHovered('')}
                    className="relative inline-flex items-center rounded-full px-3.5 py-1.5 transition-colors hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400"
                  >
                    {hovered === l.href && (
                      <motion.span
                        layoutId="nav-pill"
                        className="absolute inset-0 rounded-full bg-white/[0.07] ring-1 ring-inset ring-white/[0.06]"
                        transition={{ type: 'spring', stiffness: 380, damping: 32 }}
                      />
                    )}
                    <span className="relative">{l.label}</span>
                  </a>
                </li>
              ))}
            </ul>
          </LayoutGroup>

          <div className="flex items-center gap-2">
            <NavCTA />
            {/* Mobile hamburger */}
            <motion.button
              whileTap={{ scale: 0.92 }}
              onClick={() => setMenuOpen(!menuOpen)}
              aria-expanded={menuOpen}
              aria-label={menuOpen ? 'Close menu' : 'Open menu'}
              className="md:hidden grid h-9 w-9 place-items-center rounded-full border border-white/10 bg-white/[0.04] text-white/80 hover:bg-white/[0.08] transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400"
            >
              {menuOpen ? <X className="h-4 w-4" /> : <Menu className="h-4 w-4" />}
            </motion.button>
          </div>
        </nav>
      </motion.header>

      {/* Mobile full-screen menu */}
      <AnimatePresence>
        {menuOpen && (
          <motion.div
            ref={menuRef}
            initial={{ opacity: 0, backdropFilter: 'blur(0px)' }}
            animate={{ opacity: 1, backdropFilter: 'blur(24px)' }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3, ease: [0.16, 1, 0.3, 1] }}
            className="fixed inset-0 z-40 flex flex-col items-center justify-center bg-ink-950/92 md:hidden"
            role="dialog"
            aria-modal="true"
            aria-label="Navigation menu"
          >
            <nav className="flex flex-col items-center gap-2 w-full px-8">
              {links.map((l, i) => (
                <motion.a
                  key={l.href}
                  href={l.href}
                  onClick={() => setMenuOpen(false)}
                  initial={{ opacity: 0, y: 24 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: i * 0.06 + 0.1, duration: 0.45, ease: [0.16, 1, 0.3, 1] }}
                  className="w-full text-center py-4 text-[22px] font-display font-bold text-white/80 hover:text-white border-b border-white/[0.06] last:border-0 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400"
                >
                  {l.label}
                </motion.a>
              ))}
              <motion.a
                href="#cta"
                onClick={() => setMenuOpen(false)}
                initial={{ opacity: 0, y: 16 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.38, duration: 0.45, ease: [0.16, 1, 0.3, 1] }}
                className="mt-8 inline-flex items-center gap-2 rounded-full bg-white px-8 py-4 text-[16px] font-bold text-ink-950 shadow-[0_10px_40px_-10px_rgba(255,255,255,0.4)]"
              >
                Join the beta <ArrowRight className="h-4 w-4" />
              </motion.a>
            </nav>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}
