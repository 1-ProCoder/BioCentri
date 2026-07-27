import { motion, useInView, useMotionValue, useTransform, animate } from 'framer-motion';
import { useEffect, useRef, useState } from 'react';
import { blurIn, staggerParent, staggerFast, perspectiveReveal, viewportOnce } from '../motion';

const PATHS = {
  zero:     'M0 28 L18 22 L36 30 L54 24 L72 32 L90 22 L108 30 L126 18 L144 26 L162 16 L180 22 L198 14 L220 22',
  latency:  'M0 32 L20 26 L40 14 L60 22 L80 10 L100 18 L120 8 L140 12 L160 6 L180 10 L200 8 L220 6',
  biometric:'M0 20 L220 20',
  minutes:  'M0 32 L40 30 L80 26 L120 18 L160 12 L200 8 L220 6',
};

const NODES = {
  zero:      [40, 110, 180],
  latency:   [40, 100, 180],
  biometric: [60, 110, 160],
  minutes:   [60, 130, 195],
};

const NODE_Y = { zero: 26, latency: 10, biometric: 20, minutes: 8 };

function Sparkline({ id, color }) {
  const d = PATHS[id];
  const nodes = NODES[id];
  const nodeY = NODE_Y[id];
  return (
    <svg viewBox="0 0 220 40" className="h-14 w-full" aria-hidden="true">
      <defs>
        <linearGradient id={`grad-${id}`} x1="0" y1="0" x2="1" y2="0">
          <stop offset="0%"   stopColor={color} stopOpacity="0.12" />
          <stop offset="50%"  stopColor={color} stopOpacity="0.65" />
          <stop offset="100%" stopColor={color} stopOpacity="1" />
        </linearGradient>
        <filter id={`glow-${id}`}>
          <feGaussianBlur stdDeviation="1.5" result="blur" />
          <feMerge><feMergeNode in="blur" /><feMergeNode in="SourceGraphic" /></feMerge>
        </filter>
      </defs>
      {/* Fill under line */}
      <motion.path
        d={d + ` L220 40 L0 40 Z`}
        fill={`url(#grad-${id})`}
        fillOpacity="0.12"
        initial={{ pathLength: 0, opacity: 0 }}
        whileInView={{ pathLength: 1, opacity: 1 }}
        viewport={{ once: true, margin: '-40px' }}
        transition={{ duration: 1.4, ease: [0.16, 1, 0.3, 1] }}
      />
      {/* Line */}
      <motion.path
        d={d}
        fill="none"
        stroke={`url(#grad-${id})`}
        strokeWidth="1.8"
        strokeLinecap="round"
        filter={`url(#glow-${id})`}
        initial={{ pathLength: 0, opacity: 0.3 }}
        whileInView={{ pathLength: 1, opacity: 1 }}
        viewport={{ once: true, margin: '-40px' }}
        transition={{ duration: 1.6, ease: [0.16, 1, 0.3, 1] }}
      />
      {/* Pulsing nodes */}
      {nodes.map((cx, i) => (
        <motion.circle
          key={i}
          cx={cx} cy={nodeY} r="2.5"
          fill={color}
          initial={{ opacity: 0.15, scale: 0.7 }}
          whileInView={{ opacity: [0.15, 1, 0.15], scale: [0.7, 1.3, 0.7] }}
          viewport={{ once: true, margin: '-40px' }}
          transition={{ duration: 2.4, ease: 'easeInOut', repeat: Infinity, delay: i * 0.4 }}
        />
      ))}
      {/* Head dot */}
      <motion.circle
        cx="220" cy={nodeY} r="3"
        fill={color}
        filter={`url(#glow-${id})`}
        initial={{ opacity: 0 }}
        whileInView={{ opacity: 1 }}
        viewport={{ once: true, margin: '-40px' }}
        transition={{ delay: 1.6, duration: 0.4 }}
      />
      <motion.circle
        cx="220" cy={nodeY} r="8"
        fill="none" stroke={color} strokeWidth="1"
        initial={{ opacity: 0, scale: 0.4 }}
        whileInView={{ opacity: [0, 0.7, 0], scale: [0.4, 1.6, 1.6] }}
        viewport={{ once: true, margin: '-40px' }}
        transition={{ delay: 1.8, duration: 1.8, repeat: Infinity, repeatDelay: 0.8 }}
      />
    </svg>
  );
}

function CountUp({ to, decimals = 0, prefix = '', suffix = '' }) {
  const ref = useRef(null);
  const inView = useInView(ref, { once: true, margin: '-40px' });
  const value = useMotionValue(0);
  const display = useTransform(value, (v) => `${prefix}${v.toFixed(decimals)}${suffix}`);
  const [text, setText] = useState(`${prefix}0${decimals > 0 ? '.0'.repeat(decimals) : ''}${suffix}`);

  useEffect(() => {
    const unsub = display.on('change', setText);
    return unsub;
  }, [display]);

  useEffect(() => {
    if (!inView) return;
    const ctrl = animate(value, to, { duration: 1.6, ease: [0.16, 1, 0.3, 1] });
    return () => ctrl.stop();
  }, [inView, to, value]);

  return <span ref={ref}>{text}</span>;
}

const metrics = [
  {
    n: '01', value: 0, prefix: '', suffix: '', decimals: 0,
    label: 'Outbound network calls', sub: 'during fresh app launches.',
    spark: 'zero', tone: 'text-emerald-300', color: '#34d399',
    big: true,
  },
  {
    n: '02', value: 50, prefix: '<', suffix: 'ms', decimals: 0,
    label: 'Added launch latency', sub: 'target p95 on a 4-core device.',
    spark: 'latency', tone: 'text-indigo-200', color: '#a5b4fc',
    big: true,
  },
  {
    n: '03', value: 100, prefix: '', suffix: '%', decimals: 0,
    label: 'Biometric data on-device', sub: 'encrypted at rest, never leaves.',
    spark: 'biometric', tone: 'text-white', color: 'rgba(255,255,255,0.6)',
  },
  {
    n: '04', value: 2, prefix: '<', suffix: ' min', decimals: 0,
    label: 'Install to first locked app', sub: 'from download to working lock.',
    spark: 'minutes', tone: 'text-white', color: 'rgba(255,255,255,0.6)',
  },
];

export default function Metrics() {
  return (
    <section id="metrics" className="relative py-24 md:py-36">
      <div className="mx-auto max-w-6xl px-6 md:px-8">
        {/* Heading */}
        <motion.div
          variants={staggerParent}
          initial="hidden"
          whileInView="show"
          viewport={viewportOnce}
          className="max-w-3xl"
        >
          <motion.div
            variants={blurIn}
            className="inline-flex items-center gap-2 text-[12px] font-medium uppercase tracking-[0.22em] text-indigo-300"
          >
            <span className="h-px w-6 bg-indigo-400/60" /> Trust
          </motion.div>
          <motion.h2
            variants={perspectiveReveal}
            style={{ transformPerspective: 1000 }}
            className="font-display text-4xl md:text-6xl font-extrabold tracking-tightest mt-4 leading-[1.05]"
          >
            <span className="text-white">Performance you </span>
            <span className="text-gradient">won't notice.</span>
          </motion.h2>
          <motion.p
            variants={blurIn}
            className="mt-5 text-white/50 text-base md:text-lg leading-relaxed max-w-2xl"
          >
            BioCentri is designed to be invisible until it isn't.
            These are the numbers we hold ourselves to.
          </motion.p>
        </motion.div>

        {/* Stats grid */}
        <div className="mt-16 border-t border-white/[0.06] pt-12">
          <motion.div
            variants={staggerFast}
            initial="hidden"
            whileInView="show"
            viewport={viewportOnce}
            className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-x-10 gap-y-14"
          >
            {metrics.map((m) => (
              <motion.div key={m.n} variants={blurIn} className="flex flex-col gap-4">
                {/* Number */}
                <div className="relative">
                  <div className={
                    'font-display font-extrabold tracking-tightest leading-none font-mono-num tabular-nums ' +
                    m.tone + (m.big ? ' text-[76px] md:text-[88px]' : ' text-[60px] md:text-[72px]')
                  }>
                    <CountUp to={m.value} decimals={m.decimals} prefix={m.prefix} suffix={m.suffix} />
                  </div>
                  {/* Subtle glow under number */}
                  <div
                    className="absolute -bottom-2 left-0 h-8 w-3/4 opacity-20 blur-lg rounded-full"
                    style={{ background: m.color }}
                  />
                </div>
                {/* Labels */}
                <div className="space-y-1">
                  <div className="font-display text-[15px] font-semibold text-white/90">{m.label}</div>
                  <div className="text-[13px] text-white/40 leading-relaxed">{m.sub}</div>
                </div>
                {/* Sparkline */}
                <Sparkline id={m.spark} color={m.color} />
              </motion.div>
            ))}
          </motion.div>
        </div>

        <motion.p
          variants={blurIn}
          initial="hidden"
          whileInView="show"
          viewport={viewportOnce}
          className="mt-10 text-[12px] text-white/30 max-w-2xl"
        >
          Targets based on BioCentri MVP requirements. Real measurements will be published with v1.
        </motion.p>
      </div>
    </section>
  );
}
