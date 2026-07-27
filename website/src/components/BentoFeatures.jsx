import { motion, useMotionValue, useMotionTemplate, useSpring, AnimatePresence } from 'framer-motion';
import { useEffect, useRef, useState } from 'react';
import {
  ScanFace, AppWindow, ShieldCheck,
  Eye, KeyRound, Lock, Database, Zap, MonitorSmartphone,
  Check, ShieldAlert, Cpu, Fingerprint, Activity
} from 'lucide-react';
import { blurIn, scaleIn, staggerParent, staggerFast, perspectiveReveal, viewportOnce } from '../motion';
import Reticle from './Atmosphere/Reticle';
import HoverBorderTrace from './Atmosphere/HoverBorderTrace';
import HolographicNodes from './Atmosphere/HolographicNodes';

// ─── BentoCard base with mouse spotlight ─────────────────────
function BentoCard({ children, className = '', spotlight = true }) {
  const ref = useRef(null);
  const x = useMotionValue(-500);
  const y = useMotionValue(-500);
  const sx = useSpring(x, { stiffness: 200, damping: 26 });
  const sy = useSpring(y, { stiffness: 200, damping: 26 });
  const bg = useMotionTemplate`radial-gradient(450px circle at ${sx}px ${sy}px, rgba(165,180,252,0.08), transparent 65%)`;

  return (
    <motion.div
      ref={ref}
      variants={blurIn}
      onMouseMove={(e) => {
        const r = ref.current?.getBoundingClientRect();
        if (!r) return;
        x.set(e.clientX - r.left);
        y.set(e.clientY - r.top);
      }}
      onMouseLeave={() => { x.set(-500); y.set(-500); }}
      className={'group relative overflow-hidden rounded-3xl focal border border-white/[0.06] bg-[#09090d]/60 ' + className}
    >
      {spotlight && (
        <motion.div
          aria-hidden="true"
          className="pointer-events-none absolute inset-0 rounded-3xl"
          style={{ background: bg }}
        />
      )}
      <div className="relative h-full">{children}</div>
    </motion.div>
  );
}

// ─── Card 1: Windows Hello (large hero card) ──────────────────
function WindowsHelloCard() {
  const scanTexts = [
    'ANALYZING DEPTH MAP…',
    'EXTRACTING VECTOR MAP…',
    'LIVENESS VALUE: VALID',
    'MATCH CONFIDENCE: 98.4%',
  ];
  const [scanIdx, setScanIdx] = useState(0);
  useEffect(() => {
    const t = setInterval(() => setScanIdx((i) => (i + 1) % scanTexts.length), 2000);
    return () => clearInterval(t);
  }, []);

  return (
    <HoverBorderTrace className="lg:col-span-2 lg:row-span-2" radius={24}>
      <BentoCard className="min-h-[440px] !p-0" spotlight={false}>
        <div className="flex h-full flex-col gap-5 p-7 md:p-8">
          <div className="flex items-center justify-between">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-indigo-500/10 px-2.5 py-1 text-[11px] font-mono font-medium uppercase tracking-wider text-indigo-300 ring-1 ring-inset ring-indigo-500/25">
              <ScanFace className="h-3 w-3" /> Core Biometrics
            </span>
            <span className="text-[11px] text-white/30 font-mono">NODE_01</span>
          </div>
          <h3 className="font-display text-3xl md:text-5xl font-extrabold tracking-tightest leading-[1.05]">
            Protected by Windows Hello <br />
            <span className="text-gradient-violet">at the OS level.</span>
          </h3>
          <p className="text-white/55 text-[14px] md:text-[15px] leading-relaxed max-w-md">
            BioCentri leverages the native Windows Credential Provider. Whichever verification method you enrolled—facial recognition, fingerprint scan, or secure PIN—is invoked instantly.
          </p>

          {/* High-tech scanner visualization panel */}
          <div className="relative mt-auto h-52 rounded-2xl border border-white/[0.06] bg-[#060609] overflow-hidden p-4 flex items-center justify-center">
            {/* Grid background */}
            <div className="absolute inset-0 grid-faint opacity-[0.25]" />
            <div className="absolute inset-0 bg-gradient-to-t from-[#060609] via-transparent to-[#060609]" />
            
            {/* Tech Readouts */}
            <div className="absolute top-3 left-3 flex flex-col gap-1 font-mono text-[9px] text-white/35">
              <span>MESH_STATE: INITIALIZED</span>
              <span>ENGINE: FACE_VEC_PRO_V1</span>
            </div>
            
            <div className="absolute top-3 right-3 flex flex-col gap-1 font-mono text-[9px] text-right text-indigo-300">
              <span>CONFIDENCE: 98.4%</span>
              <span>SECURE_ENCLAVE: ACTIVE</span>
            </div>

            <Reticle size={180} className="opacity-90" />
            <div className="relative z-10 grid h-16 w-16 place-items-center rounded-full bg-indigo-500/10 ring-1 ring-indigo-400/30 shadow-[0_0_20px_rgba(99,102,241,0.25)]">
              <ScanFace className="h-7 w-7 text-indigo-300" />
            </div>

            {/* Live scan readout */}
            <div className="absolute bottom-3 left-0 right-0 flex justify-center">
              <div className="flex items-center gap-2 rounded-md bg-white/[0.03] border border-white/[0.06] px-3 py-1 backdrop-blur-sm">
                <span className="h-1.5 w-1.5 rounded-full bg-indigo-400 animate-pulse" />
                <AnimatePresence mode="wait">
                  <motion.span
                    key={scanIdx}
                    initial={{ opacity: 0, y: 4 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: -4 }}
                    transition={{ duration: 0.25 }}
                    className="text-[9px] text-indigo-300 font-mono tracking-wider font-semibold"
                  >
                    {scanTexts[scanIdx]}
                  </motion.span>
                </AnimatePresence>
              </div>
            </div>
          </div>
        </div>
      </BentoCard>
    </HoverBorderTrace>
  );
}

// ─── Card 2: Per-app ──────────────────────────────────────────
function PerAppCard() {
  return (
    <HoverBorderTrace radius={24}>
      <BentoCard className="min-h-[220px] !p-0">
        <div className="flex h-full flex-col gap-4 p-6">
          <div className="flex items-center justify-between">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-white/[0.05] px-2.5 py-0.5 text-[11px] font-mono text-white/60 ring-1 ring-white/10">
              <AppWindow className="h-3 w-3" /> Scope
            </span>
            <span className="text-[11px] text-white/30 font-mono">NODE_02</span>
          </div>
          <h3 className="font-display text-xl md:text-2xl font-bold tracking-tightest leading-snug">
            Per-App Shielding.<br />Zero global interference.
          </h3>
          
          {/* Simulated Windows OS Taskbar with Protected Hover */}
          <div className="mt-auto relative rounded-xl border border-white/[0.05] bg-[#0c0c10] p-3 flex items-center justify-center gap-4 overflow-hidden">
            {['C', 'D', 'S', 'O'].map((app, idx) => (
              <div key={app} className="relative group/taskicon">
                <span className={`grid h-8 w-8 place-items-center rounded-lg text-xs font-mono font-bold select-none cursor-pointer ${
                  idx === 1 
                    ? 'bg-emerald-500/10 text-emerald-300 ring-1 ring-emerald-500/35 shadow-[0_0_12px_rgba(52,211,153,0.3)]' 
                    : 'bg-white/[0.03] text-white/40 ring-1 ring-white/[0.08]'
                }`}>
                  {app}
                </span>
                {idx === 1 && (
                  <>
                    {/* Glowing lock badge */}
                    <span className="absolute -top-1.5 -right-1.5 grid h-4 w-4 place-items-center rounded-full bg-emerald-400 text-ink-950 shadow-[0_0_8px_rgba(52,211,153,0.6)]">
                      <Lock className="h-2 w-2" strokeWidth={3} />
                    </span>
                    {/* Simulated Floating Tooltip */}
                    <div className="absolute bottom-10 left-1/2 -translate-x-1/2 rounded-md bg-[#121217] border border-white/10 px-2 py-1 text-[9px] font-mono text-emerald-300 whitespace-nowrap shadow-xl">
                      SHIELD_ACTIVE
                    </div>
                  </>
                )}
              </div>
            ))}
          </div>
        </div>
      </BentoCard>
    </HoverBorderTrace>
  );
}

// ─── Card 3: Privacy ──────────────────────────────────────────
function PrivacyCard() {
  return (
    <HoverBorderTrace radius={24}>
      <BentoCard className="min-h-[220px] !p-0" spotlight={false}>
        <div className="flex h-full flex-col gap-4 p-6">
          <div className="flex items-center justify-between">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-400/10 px-2.5 py-0.5 text-[11px] font-mono text-emerald-300 ring-1 ring-emerald-400/20">
              <ShieldCheck className="h-3 w-3" /> Sovereignty
            </span>
            <span className="text-[11px] text-white/30 font-mono">NODE_03</span>
          </div>
          <h3 className="font-display text-xl md:text-2xl font-bold tracking-tightest leading-snug">
            Severed Telemetry.<br />
            <span className="text-gradient">Fully air-gapped logic.</span>
          </h3>

          {/* Dynamic flow chart illustration */}
          <div className="mt-auto flex items-center justify-between rounded-xl border border-white/[0.05] bg-[#0c0c10] px-4 py-3 relative overflow-hidden">
            <div className="flex flex-col gap-0.5">
              <span className="text-[9px] font-mono text-white/35">LOCAL CORE</span>
              <span className="text-[11px] font-mono text-emerald-300 font-bold">100% OFF-GRID</span>
            </div>
            
            <div className="flex items-center gap-2">
              <span className="h-2 w-2 rounded-full bg-emerald-400 animate-pulse" />
              <div className="h-px w-8 bg-dashed border-t border-white/10" />
              <ShieldAlert className="h-4 w-4 text-red-400/80" />
              <div className="h-px w-8 bg-dashed border-t border-white/10" />
              <span className="text-[10px] font-mono text-red-400/70 line-through">CLOUD</span>
            </div>
          </div>
        </div>
      </BentoCard>
    </HoverBorderTrace>
  );
}

// ─── Card 4: Speed ────────────────────────────────────────────
function SpeedCard() {
  return (
    <HoverBorderTrace radius={24}>
      <BentoCard className="min-h-[220px] !p-0">
        <div className="flex h-full flex-col gap-4 p-6">
          <div className="flex items-center justify-between">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-indigo-500/10 px-2.5 py-0.5 text-[11px] font-mono text-indigo-300 ring-1 ring-indigo-500/20">
              <Activity className="h-3 w-3" /> Performance
            </span>
            <span className="text-[11px] text-white/30 font-mono">NODE_04</span>
          </div>
          <h3 className="font-display text-xl md:text-2xl font-bold tracking-tightest leading-snug">
            Latency under p95.<br />
            <span className="text-white/40">Launch impact: zero.</span>
          </h3>

          {/* Interactive SVG coordinate spline chart */}
          <div className="mt-auto relative rounded-xl border border-white/[0.05] bg-[#050508] h-18 overflow-hidden p-1.5">
            <svg className="absolute inset-0 h-full w-full opacity-60" viewBox="0 0 100 30" preserveAspectRatio="none">
              {/* Coordinates Grid */}
              <line x1="0" y1="10" x2="100" y2="10" stroke="rgba(255,255,255,0.03)" strokeWidth="0.5" />
              <line x1="0" y1="20" x2="100" y2="20" stroke="rgba(255,255,255,0.03)" strokeWidth="0.5" />
              <line x1="25" y1="0" x2="25" y2="30" stroke="rgba(255,255,255,0.03)" strokeWidth="0.5" />
              <line x1="50" y1="0" x2="50" y2="30" stroke="rgba(255,255,255,0.03)" strokeWidth="0.5" />
              <line x1="75" y1="0" x2="75" y2="30" stroke="rgba(255,255,255,0.03)" strokeWidth="0.5" />
              
              {/* Performance spline */}
              <motion.path
                d="M 0,25 Q 25,12 50,8 T 100,5"
                fill="none"
                stroke="#6366f1"
                strokeWidth="1.5"
                initial={{ pathLength: 0 }}
                animate={{ pathLength: 1 }}
                transition={{ duration: 1.5, ease: 'easeOut' }}
              />
              {/* Gradient glow fill */}
              <path d="M 0,25 Q 25,12 50,8 T 100,5 L 100,30 L 0,30 Z" fill="url(#speed-grad)" opacity="0.1" />
              <defs>
                <linearGradient id="speed-grad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#6366f1" />
                  <stop offset="100%" stopColor="transparent" />
                </linearGradient>
              </defs>
            </svg>
            <div className="absolute top-2 left-2 text-[8px] font-mono text-white/35">LATENCY: 42ms (p95)</div>
            <div className="absolute bottom-2 right-2 text-[8px] font-mono text-indigo-400">BENCHMARK: PASS</div>
          </div>
        </div>
      </BentoCard>
    </HoverBorderTrace>
  );
}

// ─── Card 5: Compatibility ───────────────────────────────────
function CompatCard() {
  return (
    <HoverBorderTrace radius={24}>
      <BentoCard className="min-h-[220px] !p-0">
        <div className="flex h-full flex-col gap-4 p-6">
          <div className="flex items-center justify-between">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-white/[0.05] px-2.5 py-0.5 text-[11px] font-mono text-white/60 ring-1 ring-white/10">
              <MonitorSmartphone className="h-3 w-3" /> Credentials
            </span>
            <span className="text-[11px] text-white/30 font-mono">NODE_05</span>
          </div>
          <h3 className="font-display text-xl md:text-2xl font-bold tracking-tightest leading-snug">
            Native Integration.<br />
            <span className="text-white/40">Plug into Windows 11.</span>
          </h3>

          {/* Interactive hardware authentication visual */}
          <div className="mt-auto grid grid-cols-3 gap-2.5">
            {[
              { icon: ScanFace, label: 'FaceID', active: true },
              { icon: Fingerprint, label: 'TouchID', active: true },
              { icon: KeyRound, label: 'PIN', active: false }
            ].map((item) => (
              <div 
                key={item.label} 
                className={`rounded-lg border p-2 flex flex-col items-center justify-center gap-1.5 transition-all cursor-crosshair ${
                  item.active 
                    ? 'border-indigo-500/20 bg-indigo-500/5 text-indigo-300 shadow-[0_0_10px_rgba(99,102,241,0.1)]' 
                    : 'border-white/[0.05] bg-white/[0.01] text-white/30'
                }`}
              >
                <item.icon className="h-4 w-4" />
                <span className="text-[8px] font-mono font-bold tracking-wider">{item.label.toUpperCase()}</span>
              </div>
            ))}
          </div>
        </div>
      </BentoCard>
    </HoverBorderTrace>
  );
}

export default function BentoFeatures() {
  return (
    <section id="features" className="relative py-24 md:py-36 overflow-hidden">
      <HolographicNodes className="opacity-30" />
      
      {/* Decorative layout border lines to break typical section structures */}
      <div className="absolute top-0 left-12 right-12 h-px bg-gradient-to-r from-transparent via-white/[0.05] to-transparent" />
      <div className="absolute bottom-0 left-12 right-12 h-px bg-gradient-to-r from-transparent via-white/[0.05] to-transparent" />

      <div className="mx-auto max-w-6xl px-6 md:px-8 relative z-10">
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
            <span className="h-px w-6 bg-indigo-400/60" /> Capabilities
          </motion.div>
          <motion.h2
            variants={perspectiveReveal}
            style={{ transformPerspective: 1000 }}
            className="font-display text-4xl md:text-6xl font-extrabold tracking-tightest mt-4 leading-[1.05]"
          >
            What ships in the <span className="text-gradient-violet">first release.</span>
          </motion.h2>
          <motion.p
            variants={blurIn}
            className="mt-5 text-white/50 text-base md:text-lg leading-relaxed max-w-2xl"
          >
            Three opinionated capabilities, doing one thing deliberately. No scope creep.
            No analytics. No cloud dependency you didn't sign up for.
          </motion.p>
        </motion.div>

        {/* Asymmetric bento grid */}
        <motion.div
          variants={staggerFast}
          initial="hidden"
          whileInView="show"
          viewport={viewportOnce}
          className="mt-14 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4"
        >
          {/* Big hero card — spans 2×2 */}
          <div className="lg:col-span-2 lg:row-span-2">
            <WindowsHelloCard />
          </div>
          {/* Right column */}
          <PerAppCard />
          <SpeedCard />
          <PrivacyCard />
          <CompatCard />
        </motion.div>

        {/* Roadmap footnote */}
        <motion.div
          variants={blurIn}
          initial="hidden"
          whileInView="show"
          viewport={viewportOnce}
          className="mt-8 flex flex-wrap items-center gap-x-4 gap-y-2 text-[12px] text-white/35"
        >
          <span className="uppercase tracking-[0.18em] font-mono text-[10px]">ROADMAP_QUEUE //</span>
          {[
            { icon: Eye,      label: 'Productivity insights' },
            { icon: KeyRound, label: 'Browser extension' },
            { icon: Lock,     label: 'AI assistant (opt-in)' },
            { icon: Database, label: 'Cloud sync (opt-in)' },
          ].map(({ icon: Icon, label }, i) => (
            <div key={label} className="inline-flex items-center gap-1.5">
              {i > 0 && <span className="text-white/15 mr-1.5">·</span>}
              <Icon className="h-3 w-3" /> 
              <span className="font-mono text-[11px]">{label}</span>
            </div>
          ))}
        </motion.div>
      </div>
    </section>
  );
}
