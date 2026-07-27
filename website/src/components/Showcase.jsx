import { motion, AnimatePresence } from 'framer-motion';
import { useEffect, useState, useRef } from 'react';
import { Search, ShieldCheck, Plus, ScanFace, X, Terminal, Monitor, Lock, Shield, Settings, ServerCrash, Cpu } from 'lucide-react';
import { blurIn, staggerParent, perspectiveReveal, viewportOnce } from '../motion';

const apps = [
  { name: 'Chrome',   cat: 'Browser',   color: '#4285F4', letter: 'C' },
  { name: 'Discord',  cat: 'Messaging', color: '#5865F2', letter: 'D' },
  { name: 'Steam',    cat: 'Gaming',    color: '#1b2838', letter: 'S' },
  { name: 'Outlook',  cat: 'Mail',      color: '#0078D4', letter: 'O' },
  { name: 'Spotify',  cat: 'Media',     color: '#1DB954', letter: 'S' },
  { name: 'Obsidian', cat: 'Notes',     color: '#7C3AED', letter: 'O' },
];

const SAMPLE = ['Chr', 'Chro', 'Chrome', 'Chrome '];

function ScanOverlay({ name }) {
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="pointer-events-none absolute inset-0 flex items-center justify-center bg-ink-950/85 backdrop-blur-md z-25"
    >
      <motion.div
        initial={{ scale: 0.88, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        exit={{ scale: 0.88, opacity: 0 }}
        className="flex flex-col items-center gap-3"
      >
        <div className="grid h-12 w-12 place-items-center rounded-full bg-indigo-500/10 ring-1 ring-indigo-400/30 shadow-[0_0_20px_rgba(99,102,241,0.2)]">
          <ScanFace className="h-6 w-6 text-indigo-300 animate-pulse" />
        </div>
        <span className="text-[11px] font-mono text-indigo-300 tracking-wider">injecting_hook // {name.toLowerCase()}…</span>
      </motion.div>
    </motion.div>
  );
}

export default function Showcase() {
  const [typed, setTyped] = useState('');
  const [query, setQuery] = useState('');
  const [toggles, setToggles] = useState({ Chrome: true, Discord: true, Steam: false, Outlook: false, Spotify: false, Obsidian: false });
  const [scanning, setScanning] = useState(null);
  
  // Interactive logs state
  const [logs, setLogs] = useState([
    { id: 1, time: '11:50:02.105', type: 'SYS', msg: 'Kernel driver bc_shield.sys loaded successfully' },
    { id: 2, time: '11:50:02.482', type: 'KEY', msg: 'TPM 2.0 crypt-key bound to local machine' },
    { id: 3, time: '11:50:03.119', type: 'ARM', msg: 'Active biometric hook deployed on chrome.exe' },
    { id: 4, time: '11:50:03.208', type: 'ARM', msg: 'Active biometric hook deployed on discord.exe' },
  ]);

  const logEndRef = useRef(null);

  // Scroll log terminal to bottom on new entries — instant only, no smooth
  // (smooth scrollIntoView can be intercepted by Lenis and scroll the page)
  useEffect(() => {
    if (logEndRef.current) {
      logEndRef.current.scrollIntoView();
    }
  }, [logs]);

  // Typing simulator
  useEffect(() => {
    if (query !== '') return;
    let i = 0;
    const tick = () => {
      setTyped(SAMPLE[i % SAMPLE.length]);
      i += 1;
      return setTimeout(tick, 1400);
    };
    const t = tick();
    return () => clearTimeout(t);
  }, [query]);

  const filtered = query.trim()
    ? apps.filter((a) => a.name.toLowerCase().includes(query.toLowerCase()))
    : apps;

  const protectedCount = Object.values(toggles).filter(Boolean).length;

  const handleToggle = (name) => {
    const next = !toggles[name];
    const time = new Date().toLocaleTimeString('en-US', { hour12: false }) + '.' + String(Math.floor(Math.random() * 900) + 100);
    const id = Date.now();

    if (next) {
      setScanning(name);
      setTimeout(() => {
        setToggles((t) => ({ ...t, [name]: true }));
        setScanning(null);
        setLogs((prev) => [
          ...prev,
          { id, time, type: 'ARM', msg: `Biometric hook armed for ${name.toLowerCase()}.exe [Process: OK]` }
        ]);
      }, 1000);
    } else {
      setToggles((t) => ({ ...t, [name]: false }));
      setLogs((prev) => [
        ...prev,
        { id, time, type: 'WARN', msg: `Bypass armed: disabled shield for ${name.toLowerCase()}.exe` }
      ]);
    }
  };

  return (
    <section id="showcase" className="relative py-24 md:py-36 overflow-hidden">
      {/* Blueprint background details */}
      <div className="absolute inset-0 pointer-events-none select-none opacity-[0.03]">
        <div className="absolute top-10 left-10 text-[9px] font-mono text-white tracking-widest">SHOWCASE // SHIELD_MANAGER_V0.1</div>
        <div className="absolute bottom-10 right-10 text-[9px] font-mono text-white tracking-widest">COORDS // [45.10.99]</div>
      </div>

      <div className="mx-auto max-w-6xl px-6 md:px-8">
        {/* Heading */}
        <motion.div
          variants={staggerParent}
          initial="hidden"
          whileInView="show"
          viewport={viewportOnce}
          className="text-center max-w-2xl mx-auto"
        >
          <motion.div
            variants={blurIn}
            className="inline-flex items-center gap-2 text-[12px] font-medium uppercase tracking-[0.22em] text-indigo-300"
          >
            <span className="h-px w-6 bg-indigo-400/60" /> Interactive preview
          </motion.div>
          <motion.h2
            variants={perspectiveReveal}
            style={{ transformPerspective: 1000 }}
            className="font-display text-4xl md:text-6xl font-extrabold tracking-tightest mt-4 leading-[1.05]"
          >
            The control dashboard, <br/>
            <span className="text-gradient-violet">simulated in-browser.</span>
          </motion.h2>
          <motion.p
            variants={blurIn}
            className="mt-5 text-white/50 text-base md:text-lg leading-relaxed"
          >
            Toggle protection states below to inject simulated security hooks and watch the kernel logs update in real-time.
          </motion.p>
        </motion.div>

        {/* ── Simulated Windows 11 App Shell ── */}
        <motion.div
          variants={blurIn}
          initial="hidden"
          whileInView="show"
          viewport={viewportOnce}
          className="mt-14 mx-auto max-w-4xl rounded-2xl border border-white/[0.08] bg-[#0c0c10]/95 shadow-[0_30px_70px_-15px_rgba(0,0,0,0.8),inset_0_1px_0_rgba(255,255,255,0.05)] overflow-hidden"
        >
          {/* Windows Title Bar */}
          <div className="flex items-center justify-between border-b border-white/[0.06] bg-[#121217] px-4 py-2.5">
            <div className="flex items-center gap-2">
              <span className="grid h-4.5 w-4.5 place-items-center rounded bg-indigo-500/20 text-indigo-300 text-[10px]">
                <Shield className="h-2.5 w-2.5" />
              </span>
              <span className="font-mono text-[11px] tracking-wide text-white/45 select-none">
                BioCentri Protection Console v0.1.0-alpha
              </span>
            </div>
            {/* Windows window buttons */}
            <div className="flex items-center gap-3">
              <span className="h-1.5 w-1.5 rounded-full bg-white/20" />
              <span className="h-1.5 w-1.5 rounded-full bg-white/20" />
              <span className="h-1.5 w-1.5 rounded-full bg-red-400/50" />
            </div>
          </div>

          <div className="grid md:grid-cols-[200px_1fr] min-h-[460px]">
            {/* Sidebar */}
            <div className="hidden md:flex flex-col border-r border-white/[0.06] bg-[#0e0e13]/60 p-4">
              <div className="text-[10px] font-bold uppercase tracking-wider text-white/25 mb-4 px-2">Navigation</div>
              <ul className="space-y-1">
                {[
                  { label: 'Shielded Apps', active: true, icon: Monitor, badge: apps.length },
                  { label: 'Audit Logs', active: false, icon: Terminal },
                  { label: 'System Keys', active: false, icon: Cpu },
                  { label: 'Settings', active: false, icon: Settings },
                ].map((item) => (
                  <li key={item.label}>
                    <button
                      type="button"
                      className={`w-full flex items-center justify-between rounded-lg px-2.5 py-1.5 text-[12px] font-medium transition-colors ${
                        item.active 
                          ? 'bg-white/[0.06] text-white' 
                          : 'text-white/45 hover:bg-white/[0.03] hover:text-white/70'
                      }`}
                    >
                      <span className="flex items-center gap-2">
                        <item.icon className="h-3.5 w-3.5" />
                        {item.label}
                      </span>
                      {item.badge && (
                        <span className="rounded-full bg-white/10 px-1.5 py-0.5 text-[9px] text-white/45 font-mono">{item.badge}</span>
                      )}
                    </button>
                  </li>
                ))}
              </ul>
              
              <div className="mt-auto p-2 rounded-xl bg-white/[0.02] border border-white/[0.04] text-[10px] text-white/40">
                <div className="flex items-center gap-1.5 text-emerald-400 font-semibold mb-1">
                  <span className="h-1 w-1 rounded-full bg-emerald-400 animate-pulse" />
                  SHIELD ACTIVE
                </div>
                Local database is fully synced and encrypted.
              </div>
            </div>

            {/* Main Content Area */}
            <div className="flex flex-col bg-[#0c0c10]">
              {/* Search Toolbar */}
              <div className="flex items-center gap-3 border-b border-white/[0.06] p-4">
                <div className="flex-1 relative flex items-center gap-2 h-9 rounded-lg bg-white/[0.03] ring-1 ring-inset ring-white/[0.06] px-3 text-[12px] focus-within:ring-indigo-500/50 transition-all">
                  <Search className="h-3.5 w-3.5 text-white/35 shrink-0" />
                  <input
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    placeholder=""
                    className="flex-1 bg-transparent outline-none placeholder:text-white/30 text-white/90 min-w-0"
                    aria-label="Search apps"
                  />
                  {!query && typed && (
                    <span className="absolute left-8 font-mono text-[12px] text-white/30 select-none pointer-events-none">
                      {typed}
                      <span className="ml-px inline-block h-3.5 w-px align-middle bg-white/50 animate-caret" />
                    </span>
                  )}
                  {!query && !typed && (
                    <span className="absolute left-8 text-[12px] text-white/30">Search installed apps…</span>
                  )}
                  {query && (
                    <button
                      type="button"
                      onClick={() => setQuery('')}
                      className="text-white/40 hover:text-white transition-colors"
                    >
                      <X className="h-3.5 w-3.5" />
                    </button>
                  )}
                </div>
                <button
                  type="button"
                  className="inline-flex items-center gap-1.5 rounded-lg bg-white/[0.04] px-3 h-9 text-[12px] font-medium text-white/70 ring-1 ring-inset ring-white/[0.06] hover:bg-white/[0.08] transition-colors focus-visible:outline-none"
                >
                  <Plus className="h-3.5 w-3.5" /> <span className="hidden sm:inline">Add custom app</span>
                </button>
              </div>

              {/* App List */}
              <div className="flex-1 overflow-y-auto max-h-[220px] divide-y divide-white/[0.03]">
                <AnimatePresence mode="popLayout" initial={false}>
                  {filtered.map((a) => {
                    const on = !!toggles[a.name];
                    const isScanning = scanning === a.name;
                    return (
                      <motion.div
                        key={a.name}
                        layout
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, y: -10 }}
                        className="relative flex items-center justify-between px-4 py-3"
                      >
                        {isScanning && <ScanOverlay name={a.name} />}
                        
                        <div className="flex items-center gap-3">
                          <span
                            className="grid h-8 w-8 place-items-center rounded-lg ring-1 ring-inset ring-white/[0.08] text-[12px] font-bold text-white shrink-0"
                            style={{ background: `${a.color}25` }}
                          >
                            {a.letter}
                          </span>
                          <div>
                            <div className="text-[13px] font-medium text-white/90 leading-tight">{a.name}</div>
                            <div className="text-[10px] text-white/35 font-mono">{a.cat.toUpperCase()}</div>
                          </div>
                        </div>

                        <button
                          type="button"
                          onClick={() => !isScanning && handleToggle(a.name)}
                          className={`group inline-flex items-center gap-2 rounded-full px-3 py-1 text-[11px] font-semibold transition-colors focus-visible:outline-none ${
                            on
                              ? 'bg-emerald-500/10 text-emerald-300 ring-1 ring-emerald-500/20 hover:bg-emerald-500/15'
                              : 'bg-white/[0.03] text-white/40 ring-1 ring-white/10 hover:bg-white/[0.06] hover:text-white/60'
                          }`}
                          aria-label={`Toggle protection for ${a.name}`}
                        >
                          <span className={`relative inline-flex h-3.5 w-6 items-center rounded-full transition-colors ${
                            on ? 'bg-emerald-500/40' : 'bg-white/[0.08]'
                          }`}>
                            <motion.span
                              animate={{ x: on ? 10 : 2 }}
                              transition={{ type: 'spring', stiffness: 500, damping: 30 }}
                              className="inline-block h-2.5 w-2.5 rounded-full bg-white"
                            />
                          </span>
                          {on ? 'Armed' : 'Bypassed'}
                        </button>
                      </motion.div>
                    );
                  })}
                  {filtered.length === 0 && (
                    <div className="p-8 text-center text-[12px] text-white/30 font-mono">
                      ERR_NO_MATCH // "{query}" not found in system table
                    </div>
                  )}
                </AnimatePresence>
              </div>

              {/* Status footer bar inside shell */}
              <div className="flex items-center justify-between border-t border-white/[0.06] bg-[#0e0e13]/40 px-4 py-2.5">
                <span className="text-[10px] font-mono text-white/35 uppercase tracking-wider">
                  SHIELD STATUS // {protectedCount} ENROLLED
                </span>
                <div className="flex gap-1">
                  {apps.map((a) => (
                    <div
                      key={a.name}
                      className={`h-1 rounded-full transition-all duration-300 ${
                        toggles[a.name] ? 'w-4 bg-emerald-400' : 'w-1 bg-white/15'
                      }`}
                    />
                  ))}
                </div>
              </div>
            </div>
          </div>

          {/* ── Interactive Log Terminal Pane ── */}
          <div className="border-t border-white/[0.08] bg-[#050508] p-4 font-mono">
            <div className="flex items-center gap-2 text-[10px] text-indigo-300/80 uppercase tracking-widest font-bold mb-2.5">
              <Terminal className="h-3.5 w-3.5" />
              <span>Diagnostic Audit Log Stream</span>
            </div>
            <div className="h-[95px] overflow-y-auto space-y-1 text-[11px] leading-relaxed text-white/50 select-text">
              {logs.map((log) => (
                <div key={log.id} className="flex gap-2.5 items-start">
                  <span className="text-white/25 shrink-0 select-none">[{log.time}]</span>
                  <span className={`shrink-0 select-none font-bold text-[10px] rounded px-1 ${
                    log.type === 'WARN' ? 'bg-red-500/10 text-red-400 border border-red-500/20' :
                    log.type === 'ARM' ? 'bg-emerald-500/10 text-emerald-300 border border-emerald-500/20' :
                    'bg-white/5 text-white/50 border border-white/10'
                  }`}>
                    {log.type}
                  </span>
                  <span className="text-white/70">{log.msg}</span>
                </div>
              ))}
              <div ref={logEndRef} />
            </div>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
