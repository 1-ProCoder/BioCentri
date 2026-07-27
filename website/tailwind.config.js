/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        ink: {
          950: '#060608',
          900: '#0a0a0d',
          850: '#0d0d12',
          800: '#111118',
          700: '#18181f',
          600: '#202028',
          500: '#2a2a35',
          400: '#363642',
        },
      },
      backgroundImage: {
        'gradient-radial': 'radial-gradient(var(--tw-gradient-stops))',
        'gradient-conic': 'conic-gradient(from 180deg at 50% 50%, var(--tw-gradient-stops))',
        'gradient-primary': 'linear-gradient(135deg, #818cf8 0%, #a78bfa 50%, #67e8f9 100%)',
        'gradient-secondary': 'linear-gradient(135deg, #c7d2fe 0%, #818cf8 100%)',
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'sans-serif'],
        display: ['"Plus Jakarta Sans"', 'Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        mono: ['"JetBrains Mono"', '"Fira Code"', 'ui-monospace', 'monospace'],
      },
      transitionTimingFunction: {
        'out-expo':  'cubic-bezier(0.16, 1, 0.3, 1)',
        'in-expo':   'cubic-bezier(0.7, 0, 0.84, 0)',
        'spring':    'cubic-bezier(0.34, 1.56, 0.64, 1)',
        'spring-ease': 'cubic-bezier(0.16, 1, 0.3, 1)',
      },
      letterSpacing: {
        tightest: '-0.045em',
        tighter2: '-0.03em',
      },
      keyframes: {
        /* ─── Entrance ─── */
        'fade-in-up': {
          '0%':   { opacity: '0', transform: 'translateY(24px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        'blur-in': {
          '0%':   { opacity: '0', filter: 'blur(12px)', transform: 'translateY(8px)' },
          '100%': { opacity: '1', filter: 'blur(0px)',  transform: 'translateY(0)' },
        },
        'scale-in': {
          '0%':   { opacity: '0', transform: 'scale(0.88)' },
          '100%': { opacity: '1', transform: 'scale(1)' },
        },

        /* ─── Glow / Pulse ─── */
        'pulse-glow': {
          '0%, 100%': { opacity: '0.55' },
          '50%':      { opacity: '1' },
        },
        'glow-breathe': {
          '0%, 100%': { opacity: '0.6', transform: 'scale(1)' },
          '50%':      { opacity: '1',   transform: 'scale(1.08)' },
        },
        'ring-expand': {
          '0%':   { transform: 'scale(0.85)', opacity: '0.8' },
          '100%': { transform: 'scale(1.5)',  opacity: '0' },
        },

        /* ─── Shimmer ─── */
        'shimmer': {
          '0%':   { backgroundPosition: '-200% 0' },
          '100%': { backgroundPosition: '200% 0' },
        },
        'gradient-shift': {
          '0%':   { backgroundPosition: '0% 50%' },
          '50%':  { backgroundPosition: '100% 50%' },
          '100%': { backgroundPosition: '0% 50%' },
        },

        /* ─── UI atoms ─── */
        'caret': {
          '0%, 50%':   { opacity: '1' },
          '51%, 100%': { opacity: '0' },
        },

        /* ─── Background FX ─── */
        'laser-sweep': {
          '0%':   { transform: 'translateY(-30%)', opacity: '0' },
          '8%':   { opacity: '0.85' },
          '50%':  { transform: 'translateY(0%)',   opacity: '0.85' },
          '92%':  { opacity: '0.85' },
          '100%': { transform: 'translateY(30%)',  opacity: '0' },
        },
        'laser-sweep-h': {
          '0%':   { transform: 'translateX(-20%)', opacity: '0' },
          '10%':  { opacity: '0.4' },
          '50%':  { transform: 'translateX(0%)',   opacity: '0.4' },
          '90%':  { opacity: '0.4' },
          '100%': { transform: 'translateX(20%)',  opacity: '0' },
        },

        /* ─── Holographic ─── */
        'reticle-spin': {
          '0%':   { transform: 'rotate(0deg)' },
          '100%': { transform: 'rotate(360deg)' },
        },
        'hologram-float': {
          '0%, 100%': { transform: 'translate3d(0, 0, 0)' },
          '50%':      { transform: 'translate3d(0, -8px, 0)' },
        },
        'data-node': {
          '0%':   { opacity: '0.2', transform: 'scale(0.85)' },
          '50%':  { opacity: '1',   transform: 'scale(1.2)' },
          '100%': { opacity: '0.2', transform: 'scale(0.85)' },
        },
        'pipeline-descend': {
          '0%':   { transform: 'translateY(-12%)', opacity: '0' },
          '10%':  { opacity: '1' },
          '90%':  { opacity: '1' },
          '100%': { transform: 'translateY(12%)',  opacity: '0' },
        },
        'border-trace': {
          '0%':   { backgroundPosition: '0% 50%' },
          '100%': { backgroundPosition: '200% 50%' },
        },
        'orbit': {
          '0%':   { transform: 'rotate(0deg)   translateX(var(--orbit-r, 52px)) rotate(0deg)' },
          '100%': { transform: 'rotate(360deg) translateX(var(--orbit-r, 52px)) rotate(-360deg)' },
        },
        'float-slow': {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%':      { transform: 'translateY(-12px)' },
        },
        'marquee': {
          '0%':   { transform: 'translateX(0%)' },
          '100%': { transform: 'translateX(-50%)' },
        },
      },
      animation: {
        /* entrances */
        'fade-in-up':   'fade-in-up 0.5s cubic-bezier(0.16, 1, 0.3, 1) both',
        'blur-in':      'blur-in 0.6s cubic-bezier(0.16, 1, 0.3, 1) both',
        'scale-in':     'scale-in 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) both',

        /* glow */
        'pulse-glow':     'pulse-glow 2.6s ease-in-out infinite',
        'glow-breathe':   'glow-breathe 3.5s ease-in-out infinite',
        'ring-expand':    'ring-expand 2s ease-out infinite',

        /* shimmer */
        'shimmer':        'shimmer 3s linear infinite',
        'gradient-shift': 'gradient-shift 6s ease infinite',

        /* UI */
        'caret':          'caret 1s steps(2) infinite',

        /* background */
        'laser-sweep':    'laser-sweep 10s ease-in-out infinite',
        'laser-sweep-h':  'laser-sweep-h 14s ease-in-out infinite 4s',

        /* holographic */
        'reticle-spin':     'reticle-spin 12s linear infinite',
        'reticle-spin-rev': 'reticle-spin 18s linear infinite reverse',
        'reticle-spin-slow':'reticle-spin 28s linear infinite',
        'hologram-float-a': 'hologram-float 6s ease-in-out infinite',
        'hologram-float-b': 'hologram-float 8s ease-in-out infinite 1s',
        'hologram-float-c': 'hologram-float 7s ease-in-out infinite 2s',
        'data-node':        'data-node 2.4s ease-in-out infinite',
        'border-trace':     'border-trace 4s linear infinite',
        'orbit-slow':       'orbit 22s linear infinite',
        'float-slow':       'float-slow 7s ease-in-out infinite',
        'pipeline-descend': 'pipeline-descend 5s ease-in-out infinite',
        'marquee':          'marquee 28s linear infinite',
      },
    },
  },
  plugins: [],
};
