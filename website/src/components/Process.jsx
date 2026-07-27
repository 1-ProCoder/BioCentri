import { motion, AnimatePresence } from 'framer-motion';
import { useState } from 'react';
import {
  ScanFace, ToggleRight, ShieldCheck, ListChecks,
  Check, Lock, Zap,
} from 'lucide-react';
import {
  blurIn, staggerParent, staggerFast, slideInLeft, slideInRight,
  perspectiveReveal, viewportOnce,
} from '../motion';
import PipelineConnector from './Atmosphere/PipelineConnector';
import Reticle from './Atmosphere/Reticle';

const steps = [
  {
    n: '01', label: 'Choose application', icon: ListChecks,
    body: 'Pick any installed app — browsers, messengers, games, anything that benefits from a lock in front of it.',
    color: 'indigo',
  },
  {
    n: '02', label: 'Enable protection', icon: ToggleRight,
    body: 'Flip a single toggle. BioCentri registers the app and arms Windows Hello in front of it.',
    color: 'indigo',
  },
  {
    n: '03', label: 'Authenticate', icon: ScanFace,
    body: 'Next time the app launches, the native Windows Hello prompt appears. Face, fingerprint, or PIN.',
    color: 'violet',
  },
  {
    n: '04', label: 'Access granted', icon: ShieldCheck,
    body: 'On success, control hands back to your app. Nothing ever left your machine.',
    color: 'emerald',
  },
];

// Step 01 preview — app picker grid
function PreviewChoose() {
  const apps = [
    { name: 'Chrome',  emoji: '◎', ready: true },
    { name: 'Discord', emoji: '◆', ready: true },
    { name: 'Steam',   emoji: '▲', ready: true },
    { name: 'Outlook', emoji: '✉', ready: false },
  ];
  return (
    <div className="space-y-2.5">
      <div className="text-[11px] uppercase tracking-[0.18em] text-white/35 mb-4">Installed applications</div>
      {apps.map((a, i) => (
        <motion.div
          key={a.name}
          initial={{ opacity: 0, x: 16 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ delay: i * 0.07, duration: 0.4, ease: [0.16, 1, 0.3, 1] }}
          className="flex items-center justify-between rounded-xl border border-white/[0.05] bg-white/[0.02] px-3.5 py-3"
        >
          <div className="flex items-center gap-3">
            <span className="grid h-9 w-9 place-items-center rounded-lg bg-white/[0.06] text-white/70 ring-1 ring-inset ring-white/[0.08] text-[13px]">
              {a.emoji}
            </span>
            <div>
              <div className="text-[13px] font-medium text-white/90">{a.name}</div>
              <div className="text-[11px] text-white/35">{a.ready ? 'Ready to protect' : 'Not enrolled'}</div>
            </div>
          </div>
          <span className={
            'inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-medium ' +
            (a.ready
              ? 'bg-indigo-400/10 text-indigo-200 ring-1 ring-inset ring-indigo-400/20'
              : 'bg-white/[0.04] text-white/40 ring-1 ring-inset ring-white/10')
          }>
            {a.ready ? 'Available' : 'Skipped'}
          </span>
        </motion.div>
      ))}
    </div>
  );
}

// Step 02 preview — toggle with ripple
function PreviewEnable() {
  const [on, setOn] = useState(false);
  return (
    <div className="space-y-4">
      <div className="text-[11px] uppercase tracking-[0.18em] text-white/35 mb-4">Chrome — Protection</div>
      {[
        { label: 'Windows Hello biometrics', key: 'hello' },
        { label: 'Fallback PIN',              key: 'pin' },
        { label: 'Launch hook active',        key: 'hook' },
      ].map((row, i) => (
        <div key={row.key} className="flex items-center justify-between rounded-xl border border-white/[0.05] bg-white/[0.02] px-3.5 py-3">
          <span className="text-[13px] text-white/85">{row.label}</span>
          <button
            onClick={() => i === 0 && setOn(!on)}
            aria-label={`Toggle ${row.label}`}
            className={
              'relative inline-flex h-5 w-9 items-center rounded-full transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400 ' +
              (on || i > 0 ? 'bg-emerald-400/30' : 'bg-white/[0.08]')
            }
          >
            <span className={'inline-block h-4 w-4 transform rounded-full bg-white shadow transition-transform duration-300 ' + (on || i > 0 ? 'translate-x-4' : 'translate-x-0.5')} />
          </button>
        </div>
      ))}
      <motion.div
        animate={{ opacity: on ? 1 : 0, y: on ? 0 : 6 }}
        className="rounded-xl border border-emerald-400/25 bg-emerald-400/10 px-3.5 py-3 text-[12px] text-emerald-300 flex items-center gap-2"
      >
        <Check className="h-3.5 w-3.5" />
        Protection armed. Next launch will require authentication.
      </motion.div>
    </div>
  );
}

// Step 03 preview — face scan reticle
function PreviewAuth() {
  return (
    <div className="flex flex-col items-center gap-4">
      <div className="text-[11px] uppercase tracking-[0.18em] text-white/35 self-start">Windows Hello</div>
      <div className="relative flex items-center justify-center h-52 w-full rounded-2xl border border-white/[0.05] bg-white/[0.02] overflow-hidden">
        <div className="absolute inset-0 opacity-30"
          style={{ background: 'radial-gradient(ellipse at center, rgba(129,140,248,0.2), transparent 70%)' }} />
        <Reticle size={200} className="opacity-95" />
        <div className="absolute z-10 grid h-20 w-20 place-items-center rounded-full bg-indigo-400/15 ring-1 ring-inset ring-indigo-400/30">
          <ScanFace className="h-9 w-9 text-indigo-200" />
        </div>
      </div>
      <div className="grid grid-cols-3 gap-2 w-full">
        {['Camera', 'User match', 'Liveness'].map((l) => (
          <div key={l} className="flex flex-col items-center gap-1 rounded-lg border border-emerald-400/20 bg-emerald-400/05 px-2 py-2 text-[11px] text-emerald-300">
            <Check className="h-3 w-3" />
            {l}
          </div>
        ))}
      </div>
    </div>
  );
}

// Step 04 preview — access granted
function PreviewGranted() {
  return (
    <div className="flex flex-col items-center justify-center gap-6 py-4">
      <div className="text-[11px] uppercase tracking-[0.18em] text-white/35 self-start">Session</div>
      <motion.div
        initial={{ scale: 0.7, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        transition={{ type: 'spring', stiffness: 260, damping: 18 }}
        className="relative grid h-24 w-24 place-items-center"
      >
        <div className="absolute inset-0 rounded-full bg-emerald-400/10 animate-ring-expand" style={{ animationDelay: '0s' }} />
        <div className="absolute inset-0 rounded-full bg-emerald-400/08 animate-ring-expand" style={{ animationDelay: '0.7s' }} />
        <div className="grid h-24 w-24 place-items-center rounded-full bg-emerald-400/15 ring-1 ring-inset ring-emerald-400/30">
          <ShieldCheck className="h-10 w-10 text-emerald-300" />
        </div>
      </motion.div>
      <div className="w-full space-y-2">
        {[
          { k: 'App unlocked',   v: 'OK',   tone: 'emerald' },
          { k: 'Telemetry',      v: 'None', tone: 'dim' },
          { k: 'Network calls',  v: 'None', tone: 'dim' },
        ].map((r) => (
          <div key={r.k} className="flex items-center justify-between rounded-xl border border-white/[0.05] bg-white/[0.02] px-3.5 py-2.5">
            <span className="text-[13px] text-white/80">{r.k}</span>
            <span className={
              'text-[11px] font-medium rounded-full px-2 py-0.5 ' +
              (r.tone === 'emerald' ? 'bg-emerald-400/10 text-emerald-300 ring-1 ring-inset ring-emerald-400/25' : 'text-white/35')
            }>{r.v}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

const PREVIEWS = [PreviewChoose, PreviewEnable, PreviewAuth, PreviewGranted];

export default function Process() {
  const [active, setActive] = useState(0);
  const Preview = PREVIEWS[active];

  return (
    <section id="process" className="relative py-24 md:py-36">
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
            <span className="h-px w-6 bg-indigo-400/60" /> How it works
          </motion.div>
          <motion.h2
            variants={perspectiveReveal}
            style={{ transformPerspective: 1000 }}
            className="font-display text-4xl md:text-6xl font-extrabold tracking-tightest mt-4 leading-[1.05]"
          >
            Four steps.{' '}
            <span className="text-gradient">Nothing leaves your machine.</span>
          </motion.h2>
          <motion.p
            variants={blurIn}
            className="mt-5 text-white/50 text-base md:text-lg leading-relaxed max-w-2xl"
          >
            BioCentri is intentionally simple. Pick an app, arm protection, and from then on,
            your face becomes the key.
          </motion.p>
        </motion.div>

        {/* Steps + Preview */}
        <motion.div
          variants={staggerParent}
          initial="hidden"
          whileInView="show"
          viewport={viewportOnce}
          className="mt-14 grid lg:grid-cols-[1fr_1.4fr] gap-6 lg:gap-12 items-start"
        >
          {/* Step list */}
          <motion.div variants={slideInLeft} className="relative">
            <PipelineConnector count={steps.length} active={active} />
            <div className="space-y-2.5 relative">
              {steps.map((step, i) => {
                const isActive = active === i;
                const Icon = step.icon;
                return (
                  <motion.button
                    key={step.n}
                    onClick={() => setActive(i)}
                    whileHover={{ x: isActive ? 0 : 3 }}
                    transition={{ type: 'spring', stiffness: 300, damping: 22 }}
                    className={
                      'group w-full text-left rounded-2xl p-5 transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400 ' +
                      (isActive
                        ? 'glass-strong ring-1 ring-inset ring-white/10'
                        : 'glass hover:bg-white/[0.04]')
                    }
                    aria-pressed={isActive}
                  >
                    <div className="flex items-start gap-4">
                      <div className={
                        'grid h-10 w-10 shrink-0 place-items-center rounded-xl transition-all duration-200 ' +
                        (isActive
                          ? 'bg-indigo-400/15 ring-1 ring-inset ring-indigo-400/30 shadow-[0_0_16px_-4px_rgba(129,140,248,0.5)]'
                          : 'bg-white/[0.04] ring-1 ring-inset ring-white/10')
                      }>
                        <Icon className={'h-5 w-5 transition-colors ' + (isActive ? 'text-indigo-200' : 'text-white/50')} />
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-3">
                          <span className="font-display text-[11px] font-semibold text-white/35 font-mono-num tabular-nums">{step.n}</span>
                          <span className="font-display text-base md:text-[17px] font-semibold truncate">{step.label}</span>
                          {isActive && (
                            <span className="ml-auto shrink-0 inline-flex items-center gap-1.5 rounded-full bg-emerald-400/10 px-2 py-0.5 text-[10px] font-medium uppercase tracking-wider text-emerald-300 ring-1 ring-inset ring-emerald-400/25">
                              <span className="h-1.5 w-1.5 rounded-full bg-emerald-400 animate-pulse-glow" />
                              Active
                            </span>
                          )}
                        </div>
                        <p className={'mt-1.5 text-[13px] md:text-[14px] leading-relaxed ' + (isActive ? 'text-white/65' : 'text-white/40')}>
                          {step.body}
                        </p>
                      </div>
                    </div>
                  </motion.button>
                );
              })}
            </div>
          </motion.div>

          {/* Preview panel */}
          <motion.div variants={slideInRight} className="relative">
            <AnimatePresence mode="wait">
              <motion.div
                key={active}
                initial={{ opacity: 0, y: 20, filter: 'blur(6px)' }}
                animate={{ opacity: 1, y: 0, filter: 'blur(0px)' }}
                exit={{ opacity: 0, y: -12, filter: 'blur(4px)' }}
                transition={{ duration: 0.4, ease: [0.16, 1, 0.3, 1] }}
                className="rounded-3xl glass-strong p-7 md:p-8"
              >
                <div className="flex items-center justify-between mb-6">
                  <div className="text-[11px] uppercase tracking-wider text-white/35">Step preview</div>
                  <div className="rounded-full bg-white/[0.04] px-3 py-1 text-[11px] text-white/40 ring-1 ring-inset ring-white/10 font-mono-num tabular-nums">
                    {steps[active].n} / 04
                  </div>
                </div>
                <Preview />
                <div className="mt-6 rounded-xl border border-white/[0.05] bg-white/[0.02] p-3.5 text-[12px] text-white/40 flex items-start gap-2">
                  <Lock className="h-3.5 w-3.5 text-indigo-300 mt-0.5 shrink-0" />
                  Illustrative only — the live product invokes the native Windows Hello prompt.
                </div>
              </motion.div>
            </AnimatePresence>
          </motion.div>
        </motion.div>
      </div>
    </section>
  );
}
