import { motion } from 'framer-motion';
import { Shield, ArrowUpRight, ChevronUp } from 'lucide-react';
import { blurIn, staggerParent, viewportOnce } from '../motion';
import { useEffect, useState } from 'react';

const cols = [
  {
    title: 'Product',
    links: [
      { label: 'How it works', href: '#process' },
      { label: 'Features',     href: '#features' },
      { label: 'Live preview', href: '#showcase' },
      { label: 'Trust',        href: '#metrics' },
    ],
  },
  {
    title: 'Company',
    links: [
      { label: 'GitHub',    href: 'https://github.com/biocentri', external: true },
      { label: 'Roadmap',   href: 'https://github.com/biocentri', external: true },
      { label: 'Changelog', href: 'https://github.com/biocentri', external: true },
      { label: 'Contact',   href: 'mailto:hello@biocentri.com' },
    ],
  },
  {
    title: 'Legal',
    links: [
      { label: 'Privacy',  href: '#' },
      { label: 'Security', href: '#' },
      { label: 'Terms',    href: '#' },
    ],
  },
];

function BackToTop() {
  const [visible, setVisible] = useState(false);
  useEffect(() => {
    const onScroll = () => setVisible(window.scrollY > 600);
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  return (
    <motion.button
      onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
      animate={{ opacity: visible ? 1 : 0, y: visible ? 0 : 10 }}
      transition={{ duration: 0.3 }}
      aria-label="Back to top"
      className="fixed bottom-6 right-6 z-40 grid h-10 w-10 place-items-center rounded-full border border-white/10 bg-ink-900/80 text-white/60 backdrop-blur-md shadow-[0_4px_20px_-4px_rgba(0,0,0,0.5)] hover:bg-white/[0.08] hover:text-white transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400"
    >
      <ChevronUp className="h-4 w-4" />
    </motion.button>
  );
}

export default function Footer() {
  return (
    <>
      <footer className="relative pt-16 md:pt-20 pb-10">
        {/* Topographic line above footer */}
        <div className="absolute top-0 left-0 right-0 section-divider" />

        <div className="mx-auto max-w-6xl px-6 md:px-8">
          <motion.div
            variants={staggerParent}
            initial="hidden"
            whileInView="show"
            viewport={viewportOnce}
            className="grid grid-cols-1 md:grid-cols-[1.5fr_1fr_1fr_1fr] gap-10 md:gap-12"
          >
            {/* Brand column */}
            <motion.div variants={blurIn} className="flex flex-col gap-5 max-w-xs">
              <a
                href="#top"
                className="inline-flex items-center gap-2 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400 rounded-lg"
              >
                <span className="grid h-7 w-7 place-items-center rounded-lg bg-gradient-to-br from-indigo-300 to-violet-400 text-ink-950 shadow-[inset_0_1px_0_rgba(255,255,255,0.5)]">
                  <Shield className="h-3.5 w-3.5" strokeWidth={2.5} />
                </span>
                <span className="font-display text-[15px] font-bold tracking-tight">BioCentri</span>
              </a>
              <p className="text-[13px] leading-relaxed text-white/45">
                A privacy-focused, local-first Windows application that protects
                individual apps with Windows Hello biometrics.
              </p>
              <div className="flex flex-wrap items-center gap-2">
                {[
                  { label: 'GitHub',   href: 'https://github.com/biocentri' },
                  { label: 'X',        href: 'https://twitter.com/biocentri' },
                  { label: 'LinkedIn', href: 'https://linkedin.com/company/biocentri' },
                ].map((s) => (
                  <a
                    key={s.label}
                    href={s.href}
                    target="_blank"
                    rel="noopener noreferrer"
                    aria-label={s.label}
                    className="group inline-flex items-center gap-1 rounded-full border border-white/[0.07] bg-white/[0.02] px-3 h-8 text-[12px] font-medium text-white/55 hover:bg-white/[0.07] hover:text-white hover:shadow-[0_0_16px_-4px_rgba(129,140,248,0.3)] hover:scale-105 transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400"
                  >
                    {s.label}
                    <ArrowUpRight className="h-3 w-3 opacity-0 -translate-y-0.5 translate-x-0.5 transition-all group-hover:opacity-100 group-hover:translate-y-0 group-hover:translate-x-0" />
                  </a>
                ))}
              </div>
            </motion.div>

            {/* Nav columns */}
            {cols.map((c) => (
              <motion.div key={c.title} variants={blurIn}>
                <div className="text-[11px] font-medium uppercase tracking-[0.18em] text-white/30 mb-4">
                  {c.title}
                </div>
                <ul className="space-y-2.5">
                  {c.links.map((l) => (
                    <li key={l.label}>
                      <a
                        href={l.href}
                        target={l.external ? '_blank' : undefined}
                        rel={l.external ? 'noopener noreferrer' : undefined}
                        className="group inline-flex items-center gap-1 text-[13px] text-white/55 hover:text-white transition-colors focus-visible:outline-none focus-visible:underline"
                      >
                        {l.label}
                        {l.external && (
                          <ArrowUpRight className="h-3 w-3 opacity-0 group-hover:opacity-60 transition-opacity" />
                        )}
                      </a>
                    </li>
                  ))}
                </ul>
              </motion.div>
            ))}
          </motion.div>

          {/* Status + copyright */}
          <motion.div
            variants={blurIn}
            initial="hidden"
            whileInView="show"
            viewport={viewportOnce}
            className="mt-14 flex flex-col md:flex-row md:items-center md:justify-between gap-4 border-t border-white/[0.05] pt-6"
          >
            <div className="flex items-center gap-2" title="All systems operational">
              <span className="relative flex h-2 w-2">
                <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-60" />
                <span className="relative inline-flex h-2 w-2 rounded-full bg-emerald-400" />
              </span>
              <span className="text-[12px] uppercase tracking-[0.16em] text-emerald-300/80">
                All systems operational
              </span>
            </div>
            <div className="flex flex-wrap items-center gap-x-6 gap-y-2 text-[12px] text-white/30">
              <span>© {new Date().getFullYear()} BioCentri. Built in public.</span>
              <span>Made with care for Windows 11.</span>
              <span className="font-mono-num tabular-nums">v0.1 · alpha</span>
            </div>
          </motion.div>
        </div>
      </footer>

      <BackToTop />
    </>
  );
}
