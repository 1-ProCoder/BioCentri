import { motion, useScroll, useTransform, useReducedMotion } from 'framer-motion';
import { useEffect, useRef, useState } from 'react';
import { ArrowRight, BookOpen, ShieldCheck, Fingerprint } from 'lucide-react';
import Button from './ui/Button';
import BiometricOrb from './Atmosphere/BiometricOrb';
import { staggerFast, clipReveal, blurIn, scaleIn, viewportOnce } from '../motion';

// Words that animate in with clip-path, staggered
function AnimatedWord({ children, delay = 0, className = '' }) {
  return (
    <motion.span
      className={'inline-block overflow-hidden ' + className}
      initial="hidden"
      animate="show"
      variants={{
        hidden: {},
        show: { transition: { delayChildren: delay } },
      }}
    >
      <motion.span
        className="inline-block"
        variants={{
          hidden: { y: '105%', opacity: 0 },
          show: {
            y: '0%', opacity: 1,
            transition: { duration: 0.75, ease: [0.16, 1, 0.3, 1] },
          },
        }}
      >
        {children}
      </motion.span>
    </motion.span>
  );
}

// Status badge with pulsing dot
function StatusBadge() {
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.5, delay: 0.1, ease: [0.16, 1, 0.3, 1] }}
      className="inline-flex items-center gap-2.5 rounded-full border border-white/10 bg-white/[0.04] px-4 py-1.5 text-[12px] font-medium text-white/70 shadow-[inset_0_1px_0_rgba(255,255,255,0.07)] backdrop-blur-sm"
    >
      <span className="relative flex h-1.5 w-1.5">
        <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-indigo-400 opacity-75" />
        <span className="relative inline-flex h-1.5 w-1.5 rounded-full bg-indigo-400" />
      </span>
      <span>Private beta · Windows 11</span>
      <span className="hidden sm:inline text-white/30">·</span>
      <span className="hidden sm:inline-flex items-center gap-1 text-white/50">
        <ShieldCheck className="h-3 w-3" />
        Windows Hello
      </span>
    </motion.div>
  );
}

// Floating trust chips below the CTA
function TrustChips() {
  const chips = [
    { icon: ShieldCheck,   label: 'No cloud' },
    { icon: Fingerprint,   label: 'On-device' },
  ];
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ delay: 1.1, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
      className="flex items-center gap-x-5 gap-y-1.5 flex-wrap text-[12px] uppercase tracking-[0.18em] text-white/35"
    >
      {chips.map(({ icon: Icon, label }) => (
        <span key={label} className="inline-flex items-center gap-1.5">
          <Icon className="h-3 w-3" />
          {label}
        </span>
      ))}
      <span className="text-white/20">·</span>
      <span>One email. No newsletter.</span>
    </motion.div>
  );
}

// Scroll indicator
function ScrollIndicator() {
  const [visible, setVisible] = useState(true);
  useEffect(() => {
    const onScroll = () => { if (window.scrollY > 60) setVisible(false); };
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  return (
    <motion.div
      animate={{ opacity: visible ? 1 : 0 }}
      transition={{ duration: 0.4 }}
      className="absolute bottom-10 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 pointer-events-none"
    >
      <span className="text-[10px] uppercase tracking-[0.22em] text-white/30">Scroll</span>
      <div className="relative h-8 w-px overflow-hidden">
        <motion.div
          animate={{ y: ['-100%', '200%'] }}
          transition={{ duration: 1.4, ease: 'easeInOut', repeat: Infinity }}
          className="absolute inset-x-0 h-4 bg-gradient-to-b from-transparent via-indigo-400/70 to-transparent"
        />
      </div>
    </motion.div>
  );
}

export default function Hero() {
  const reducedMotion = useReducedMotion();
  const { scrollY } = useScroll();

  // Orb parallaxes faster than text for depth
  const orbY = useTransform(scrollY, [0, 700], [0, -90]);
  // Text parallaxes slower
  const textY = useTransform(scrollY, [0, 700], [0, -30]);

  return (
    <section className="relative min-h-screen overflow-hidden pt-28 pb-16 md:pt-36 md:pb-24 flex items-center">
      {/* Hairline grid, masked radially */}
      <div
        className="pointer-events-none absolute inset-0 grid-faint opacity-50"
        style={{
          maskImage: 'radial-gradient(ellipse 65% 60% at 50% 30%, black, transparent 80%)',
          WebkitMaskImage: 'radial-gradient(ellipse 65% 60% at 50% 30%, black, transparent 80%)',
        }}
      />
      {/* Central top glow */}
      <div
        className="pointer-events-none absolute left-1/2 top-0 -translate-x-1/2 -translate-y-1/4 h-[520px] w-[760px] rounded-full opacity-[0.08]"
        style={{ background: 'radial-gradient(circle at 50% 50%, #818cf8 0%, transparent 65%)' }}
      />

      <div className="relative mx-auto w-full max-w-7xl px-6 md:px-10 lg:px-12">
        <div className="grid lg:grid-cols-[1fr_1fr] gap-12 lg:gap-6 items-center">

          {/* ── Left: Typography ── */}
          <motion.div
            style={{ y: reducedMotion ? 0 : textY }}
            className="flex flex-col items-start max-w-2xl"
          >
            <StatusBadge />

            {/* Headline — word-level clip reveals */}
            <h1 className="font-display mt-8 text-[52px] leading-[1.02] font-extrabold tracking-tightest md:text-[68px] lg:text-[76px] lg:leading-[0.96]">
              <span className="block overflow-hidden">
                <AnimatedWord delay={0.18} className="text-white">Protect&nbsp;</AnimatedWord>
                <AnimatedWord delay={0.26} className="text-white">your&nbsp;</AnimatedWord>
                <AnimatedWord delay={0.34} className="text-white">apps</AnimatedWord>
              </span>
              <span className="block overflow-hidden mt-1">
                <AnimatedWord delay={0.44} className="text-gradient-violet">with&nbsp;</AnimatedWord>
                <AnimatedWord delay={0.52} className="text-gradient-violet">your&nbsp;</AnimatedWord>
                <AnimatedWord delay={0.60} className="text-gradient-violet">face.</AnimatedWord>
              </span>
            </h1>

            {/* Subtext */}
            <motion.p
              initial={{ opacity: 0, y: 18 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.82, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
              className="mt-7 max-w-md text-base md:text-lg leading-relaxed text-white/55"
            >
              BioCentri puts Windows Hello in front of the apps that hold
              the parts of your life you don't want to share.
              <span className="text-white/80"> Privacy-first. Local-first. No cloud.</span>
            </motion.p>

            {/* CTAs */}
            <motion.div
              initial={{ opacity: 0, y: 16 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.96, duration: 0.55, ease: [0.16, 1, 0.3, 1] }}
              className="mt-9 flex flex-wrap items-center gap-3"
            >
              <Button href="#cta" Icon={ArrowRight} iconPosition="right" size="lg">
                Join the beta
              </Button>
              <Button href="#process" variant="glass" Icon={BookOpen} iconPosition="right" size="lg">
                How it works
              </Button>
            </motion.div>

            <div className="mt-6">
              <TrustChips />
            </div>
          </motion.div>

          {/* ── Right: BiometricOrb ── */}
          <motion.div
            style={{ y: reducedMotion ? 0 : orbY }}
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.3, duration: 1.0, ease: [0.16, 1, 0.3, 1] }}
            className="relative flex items-center justify-center order-first lg:order-last"
          >
            {/* Glow behind orb */}
            <div
              className="absolute h-[420px] w-[420px] rounded-full opacity-25"
              style={{
                background: 'radial-gradient(circle at 50% 50%, rgba(129,140,248,0.6) 0%, rgba(103,232,249,0.3) 40%, transparent 70%)',
                filter: 'blur(55px)',
              }}
            />

            {/* The orb itself */}
            <div className="relative z-10">
              <BiometricOrb
                size={420}
                className="hidden lg:block"
              />
              <BiometricOrb
                size={280}
                className="block lg:hidden"
              />
            </div>

            {/* Floating UI chip: verified */}
            <motion.div
              initial={{ opacity: 0, x: 20, y: -10 }}
              animate={{ opacity: 1, x: 0, y: 0 }}
              transition={{ delay: 1.2, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
              className="absolute top-4 right-0 lg:right-4 z-20 flex items-center gap-2 rounded-full border border-emerald-400/25 bg-emerald-400/10 px-3 py-1.5 text-[12px] font-medium text-emerald-300 backdrop-blur-sm shadow-[0_0_20px_-5px_rgba(52,211,153,0.3)]"
            >
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-400 animate-pulse-glow" />
              Liveness verified
            </motion.div>

            {/* Floating UI chip: match score */}
            <motion.div
              initial={{ opacity: 0, x: -20, y: 10 }}
              animate={{ opacity: 1, x: 0, y: 0 }}
              transition={{ delay: 1.35, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
              className="absolute bottom-8 left-0 lg:left-4 z-20 flex items-center gap-2 rounded-full border border-indigo-400/25 bg-indigo-400/10 px-3 py-1.5 text-[12px] font-medium text-indigo-200 backdrop-blur-sm shadow-[0_0_20px_-5px_rgba(129,140,248,0.3)]"
            >
              <Fingerprint className="h-3 w-3" />
              Face match · 98.4%
            </motion.div>
          </motion.div>
        </div>
      </div>

      <ScrollIndicator />
    </section>
  );
}
