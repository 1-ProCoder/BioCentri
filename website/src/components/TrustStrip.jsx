import { motion } from 'framer-motion';
import { ShieldCheck, Fingerprint, Wifi, Lock } from 'lucide-react';
import { staggerFast, scaleIn, viewportOnce } from '../motion';

const signals = [
  { icon: ShieldCheck, label: 'Windows Hello Native',    detail: 'OS-level API' },
  { icon: Lock,        label: 'Zero outbound network',   detail: 'No telemetry' },
  { icon: Fingerprint, label: 'Local biometric storage', detail: 'On-device only' },
  { icon: Wifi,        label: 'Air-gapped capable',      detail: 'Works offline' },
];

// Duplicate for seamless marquee on mobile
const all = [...signals, ...signals, ...signals];

export default function TrustStrip() {
  return (
    <div className="relative overflow-hidden border-y border-white/[0.05]">
      {/* Edge fade masks */}
      <div
        className="pointer-events-none absolute inset-y-0 left-0 w-24 z-10"
        style={{ background: 'linear-gradient(to right, #060608, transparent)' }}
      />
      <div
        className="pointer-events-none absolute inset-y-0 right-0 w-24 z-10"
        style={{ background: 'linear-gradient(to left, #060608, transparent)' }}
      />

      {/* Desktop: static centered row */}
      <motion.div
        variants={staggerFast}
        initial="hidden"
        whileInView="show"
        viewport={viewportOnce}
        className="hidden md:flex items-center justify-center gap-0 py-4"
      >
        {signals.map((s, i) => (
          <motion.div
            key={s.label}
            variants={scaleIn}
            className="flex items-center gap-2.5 px-6 py-1"
          >
            <div className="grid h-7 w-7 place-items-center rounded-lg bg-indigo-400/10 ring-1 ring-inset ring-indigo-400/20 shrink-0">
              <s.icon className="h-3.5 w-3.5 text-indigo-300" />
            </div>
            <div>
              <div className="text-[12px] font-medium text-white/80 leading-none">{s.label}</div>
              <div className="text-[10px] text-white/35 mt-0.5">{s.detail}</div>
            </div>
            {i < signals.length - 1 && (
              <div className="ml-6 h-5 w-px bg-white/[0.07]" />
            )}
          </motion.div>
        ))}
      </motion.div>

      {/* Mobile: continuous marquee */}
      <div className="md:hidden py-4">
        <div className="flex items-center animate-marquee whitespace-nowrap w-max">
          {all.map((s, i) => (
            <div key={i} className="flex items-center gap-2 px-5">
              <div className="grid h-6 w-6 place-items-center rounded-md bg-indigo-400/10 shrink-0">
                <s.icon className="h-3 w-3 text-indigo-300" />
              </div>
              <span className="text-[12px] font-medium text-white/65">{s.label}</span>
              <span className="ml-4 h-3 w-px bg-white/10" />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
