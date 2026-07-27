import { motion, AnimatePresence } from 'framer-motion';
import { useState, useEffect, useRef } from 'react';
import { ArrowRight, Check, Fingerprint } from 'lucide-react';
import { blurIn, staggerParent, perspectiveReveal, viewportOnce } from '../motion';
import { useMagneticButton } from '../hooks/useMagneticButton';

const STORAGE_KEY = 'biocentri-waitlist-email';

// Particle burst on submit
function SuccessParticles({ trigger }) {
  const [particles, setParticles] = useState([]);
  useEffect(() => {
    if (!trigger) return;
    const pts = Array.from({ length: 20 }, (_, i) => ({
      id: i,
      angle: (i / 20) * Math.PI * 2,
      dist: 60 + Math.random() * 60,
      size: 2 + Math.random() * 3,
      color: Math.random() > 0.5 ? '#818cf8' : '#34d399',
    }));
    setParticles(pts);
    const t = setTimeout(() => setParticles([]), 1200);
    return () => clearTimeout(t);
  }, [trigger]);

  return (
    <div className="pointer-events-none absolute inset-0 overflow-hidden rounded-3xl">
      {particles.map((p) => (
        <motion.div
          key={p.id}
          initial={{ x: '50%', y: '50%', opacity: 1, scale: 1 }}
          animate={{
            x: `calc(50% + ${Math.cos(p.angle) * p.dist}px)`,
            y: `calc(50% + ${Math.sin(p.angle) * p.dist}px)`,
            opacity: 0, scale: 0,
          }}
          transition={{ duration: 0.9, ease: [0.16, 1, 0.3, 1] }}
          className="absolute rounded-full"
          style={{ width: p.size, height: p.size, background: p.color }}
        />
      ))}
    </div>
  );
}

function SubmitButton({ submitted, onClick }) {
  const { ref, x, y, handlers } = useMagneticButton(0.3);
  return (
    <motion.div ref={ref} style={{ x, y }} {...handlers}>
      <motion.button
        type="submit"
        onClick={onClick}
        whileTap={{ scale: 0.96 }}
        disabled={submitted}
        className={
          'group relative inline-flex items-center justify-center gap-2 overflow-hidden rounded-full px-6 h-12 text-[14px] font-semibold transition-all whitespace-nowrap focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400 ' +
          (submitted
            ? 'bg-emerald-400/15 text-emerald-200 ring-1 ring-inset ring-emerald-400/25 cursor-default'
            : 'bg-white text-ink-950 shadow-[inset_0_1px_0_rgba(255,255,255,0.7),0_10px_32px_-8px_rgba(255,255,255,0.35)] hover:shadow-[inset_0_1px_0_rgba(255,255,255,0.7),0_12px_40px_-6px_rgba(255,255,255,0.42)]')
        }
      >
        {/* Shimmer */}
        {!submitted && (
          <span
            aria-hidden="true"
            className="pointer-events-none absolute inset-0 -translate-x-full group-hover:translate-x-full transition-transform duration-700 ease-in-out"
            style={{ background: 'linear-gradient(90deg, transparent 0%, rgba(0,0,0,0.06) 50%, transparent 100%)' }}
          />
        )}
        <AnimatePresence mode="wait">
          {submitted ? (
            <motion.span
              key="done"
              initial={{ opacity: 0, scale: 0.8 }}
              animate={{ opacity: 1, scale: 1 }}
              className="flex items-center gap-2"
            >
              <Check className="h-4 w-4" /> You're on the list
            </motion.span>
          ) : (
            <motion.span
              key="cta"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className="flex items-center gap-2"
            >
              Request beta access
              <ArrowRight className="h-4 w-4 transition-transform duration-200 group-hover:translate-x-0.5" />
            </motion.span>
          )}
        </AnimatePresence>
      </motion.button>
    </motion.div>
  );
}

export default function CtaBanner() {
  const [email, setEmail]       = useState('');
  const [submitted, setSubmitted] = useState(false);
  const [burst, setBurst]       = useState(false);

  useEffect(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved) { setEmail(saved); setSubmitted(true); }
    } catch { /* private mode */ }
  }, []);

  const onSubmit = (e) => {
    e?.preventDefault();
    if (!email.includes('@') || submitted) return;
    setSubmitted(true);
    setBurst(true);
    setTimeout(() => setBurst(false), 1500);
    try { localStorage.setItem(STORAGE_KEY, email.trim().toLowerCase()); } catch { /* skip */ }
  };

  return (
    <section id="cta" className="relative py-24 md:py-36">
      <div className="mx-auto max-w-5xl px-6 md:px-8">
        <motion.div
          variants={staggerParent}
          initial="hidden"
          whileInView="show"
          viewport={viewportOnce}
          className="relative overflow-hidden rounded-3xl glass-strong p-10 md:p-16"
        >
          {/* Particle burst */}
          <SuccessParticles trigger={burst} />

          {/* Decorative rings */}
          <div aria-hidden="true" className="pointer-events-none absolute inset-0 overflow-hidden rounded-3xl">
            {/* Top center glow */}
            <div
              className="absolute -top-40 left-1/2 -translate-x-1/2 h-80 w-[700px] opacity-40"
              style={{ background: 'radial-gradient(ellipse at center, rgba(129,140,248,0.22), transparent 65%)', filter: 'blur(50px)' }}
            />
            {/* Expanding rings */}
            <div className="absolute top-1/2 right-16 -translate-y-1/2 w-64 h-64 opacity-30">
              <div className="absolute inset-0 rounded-full border border-indigo-400/25 animate-ring-expand" />
              <div className="absolute inset-8 rounded-full border border-violet-400/20 animate-ring-expand" style={{ animationDelay: '0.8s' }} />
              <div className="absolute inset-16 rounded-full border border-indigo-400/30 animate-ring-expand" style={{ animationDelay: '1.6s' }} />
            </div>
          </div>

          <div className="relative max-w-xl">
            <motion.div
              variants={blurIn}
              className="inline-flex items-center gap-2 text-[12px] font-medium uppercase tracking-[0.22em] text-indigo-300"
            >
              <span className="h-px w-6 bg-indigo-400/60" /> Join the beta
            </motion.div>
            <motion.h2
              variants={perspectiveReveal}
              style={{ transformPerspective: 1000 }}
              className="font-display text-4xl md:text-6xl font-extrabold tracking-tightest mt-4 leading-[1.05]"
            >
              <span className="text-white">Be one of the </span>
              <span className="text-gradient-violet">first 200.</span>
            </motion.h2>
            <motion.p
              variants={blurIn}
              className="mt-5 text-white/60 text-base md:text-lg leading-relaxed"
            >
              BioCentri is in private beta for Windows 11. We open the next batch
              selectively to keep the experience tight.
              Drop your email — we'll reach out when a slot opens.
            </motion.p>

            <motion.form
              variants={blurIn}
              onSubmit={onSubmit}
              className="mt-8 flex flex-col sm:flex-row gap-3"
            >
              <label className="sr-only" htmlFor="cta-email">Email address</label>
              <div className="flex-1 flex items-center h-12 rounded-full bg-black/40 border border-white/10 px-4 ring-1 ring-inset ring-white/[0.04] shadow-[inset_0_1px_0_rgba(255,255,255,0.06)] focus-within:ring-indigo-400/35 focus-within:border-indigo-400/25 transition-all">
                <input
                  id="cta-email"
                  name="email"
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@yourdomain.com"
                  disabled={submitted}
                  className="flex-1 bg-transparent outline-none text-[14px] text-white/90 placeholder:text-white/30 disabled:opacity-60"
                />
              </div>
              <SubmitButton submitted={submitted} onClick={onSubmit} />
            </motion.form>

            <motion.ul
              variants={blurIn}
              className="mt-6 flex flex-wrap items-center gap-x-5 gap-y-2 text-[13px] text-white/50"
            >
              {[
                'One email when a slot opens',
                'No newsletter',
                'No tracking',
              ].map((item) => (
                <li key={item} className="inline-flex items-center gap-1.5">
                  <Check className="h-3.5 w-3.5 text-emerald-300 shrink-0" /> {item}
                </li>
              ))}
            </motion.ul>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
