import { lazy, Suspense, useEffect, useRef } from 'react';
import Nav from './components/Nav';
import Hero from './components/Hero';
import TrustStrip from './components/TrustStrip';
import AtmosphericBackground from './components/Atmosphere/AtmosphericBackground';
import AmbientAuras from './components/Atmosphere/AmbientAuras';
import CustomCursor from './components/CustomCursor';
import { useLenis } from './hooks/useLenis';

// Lazy-load below-the-fold sections for better initial load
const Process      = lazy(() => import('./components/Process'));
const BentoFeatures = lazy(() => import('./components/BentoFeatures'));
const Showcase     = lazy(() => import('./components/Showcase'));
const Metrics      = lazy(() => import('./components/Metrics'));
const CtaBanner    = lazy(() => import('./components/CtaBanner'));
const Footer       = lazy(() => import('./components/Footer'));

// Simple skeleton fallback — matches the dark bg, no layout shift
function SectionSkeleton() {
  return <div className="py-24 md:py-36" aria-hidden="true" />;
}

export default function App() {
  // Initialise Lenis smooth scroll
  useLenis();

  // Force scroll to top on fresh page load (fixes browser scroll restoration)
  const didReset = useRef(false);
  useEffect(() => {
    if (didReset.current) return;
    didReset.current = true;
    history.scrollRestoration = 'manual';
    window.scrollTo(0, 0);
  }, []);

  return (
    <div id="top" className="relative min-h-screen bg-ink-950 text-white cursor-none">
      {/* Fixed atmospheric layers — always behind content */}
      <AtmosphericBackground />
      <AmbientAuras />

      {/* Film grain noise texture */}
      <div
        className="pointer-events-none fixed inset-0 z-0 noise opacity-[0.42] mix-blend-overlay"
        aria-hidden="true"
      />

      {/* Custom cursor — desktop only */}
      <CustomCursor />

      {/* All page content sits above atmosphere */}
      <div className="relative z-10">
        <Nav />
        <main id="main-content">
          <Hero />
          <TrustStrip />
          <Suspense fallback={<SectionSkeleton />}>
            <Process />
          </Suspense>
          <Suspense fallback={<SectionSkeleton />}>
            <BentoFeatures />
          </Suspense>
          <Suspense fallback={<SectionSkeleton />}>
            <Showcase />
          </Suspense>
          <Suspense fallback={<SectionSkeleton />}>
            <Metrics />
          </Suspense>
          <Suspense fallback={<SectionSkeleton />}>
            <CtaBanner />
          </Suspense>
        </main>
        <Suspense fallback={null}>
          <Footer />
        </Suspense>
      </div>
    </div>
  );
}
