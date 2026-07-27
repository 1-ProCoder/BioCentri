import { motion } from 'framer-motion';
import { pressTap } from '../../motion';
import { useMagneticButton } from '../../hooks/useMagneticButton';

const base =
  'group relative inline-flex items-center justify-center gap-2 rounded-full text-sm font-semibold transition-colors overflow-hidden focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400 focus-visible:ring-offset-2 focus-visible:ring-offset-ink-950';

const variants = {
  primary:
    'bg-white text-ink-950 shadow-[inset_0_1px_0_rgba(255,255,255,0.7),0_10px_32px_-10px_rgba(255,255,255,0.28)] hover:shadow-[inset_0_1px_0_rgba(255,255,255,0.7),0_10px_40px_-8px_rgba(255,255,255,0.36),0_0_24px_-4px_rgba(255,255,255,0.3)]',
  glass:
    'border border-white/12 bg-white/[0.055] text-white shadow-[inset_0_1px_0_rgba(255,255,255,0.1)] backdrop-blur-xl hover:bg-white/[0.09] hover:border-white/[0.18] hover:shadow-[inset_0_1px_0_rgba(255,255,255,0.12),0_8px_24px_-8px_rgba(0,0,0,0.3)]',
  ghost:
    'text-white/70 hover:text-white',
  indigo:
    'bg-indigo-500/90 text-white shadow-[inset_0_1px_0_rgba(255,255,255,0.2),0_10px_32px_-10px_rgba(129,140,248,0.6)] hover:bg-indigo-400/90 hover:shadow-[inset_0_1px_0_rgba(255,255,255,0.2),0_10px_40px_-8px_rgba(129,140,248,0.7),0_0_24px_-4px_rgba(129,140,248,0.4)]',
};

const sizes = {
  sm: 'px-3.5 py-1.5 text-[13px]',
  md: 'px-5    py-3   text-sm',
  lg: 'px-7    py-4   text-[15px]',
};

export default function Button({
  variant = 'primary',
  size = 'md',
  as: Tag = 'a',
  href,
  children,
  className = '',
  Icon,
  iconPosition = 'right',
  magnetic = true,
  onClick,
  ...rest
}) {
  const { ref, x, y, handlers } = useMagneticButton(magnetic ? 0.28 : 0);

  return (
    <motion.div
      ref={ref}
      style={{ x, y }}
      whileTap={pressTap}
      {...handlers}
      className="inline-flex"
    >
      <Tag
        href={href}
        onClick={onClick}
        className={[base, variants[variant], sizes[size], className].join(' ')}
        {...rest}
      >
        {/* Shimmer sweep — primary only */}
        {variant === 'primary' && (
          <span
            aria-hidden="true"
            className="pointer-events-none absolute inset-0 -translate-x-full group-hover:translate-x-full transition-transform duration-700 ease-in-out"
            style={{
              background: 'linear-gradient(90deg, transparent 0%, rgba(255,255,255,0.18) 50%, transparent 100%)',
            }}
          />
        )}
        {Icon && iconPosition === 'left' && <Icon className="h-4 w-4 shrink-0" />}
        <span>{children}</span>
        {Icon && iconPosition === 'right' && (
          <Icon className="h-4 w-4 shrink-0 transition-transform duration-200 group-hover:translate-x-0.5" />
        )}
      </Tag>
    </motion.div>
  );
}
