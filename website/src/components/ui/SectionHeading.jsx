import { motion } from 'framer-motion';
import { blurIn, perspectiveReveal, staggerParent, viewportOnce } from '../../motion';

/**
 * Reusable SectionHeading component using the premium motion vocabulary.
 */
export default function SectionHeading({
  eyebrow,
  title,
  description,
  align = 'left',
  className = '',
}) {
  const isCenter = align === 'center';
  const alignClass = isCenter ? 'mx-auto text-center items-center' : 'text-left items-start';

  return (
    <motion.div
      variants={staggerParent}
      initial="hidden"
      whileInView="show"
      viewport={viewportOnce}
      className={`flex flex-col max-w-3xl ${alignClass} ${className}`}
    >
      {eyebrow && (
        <motion.div
          variants={blurIn}
          className="inline-flex items-center gap-2 text-[12px] font-medium uppercase tracking-[0.22em] text-indigo-300"
        >
          {!isCenter && <span className="h-px w-6 bg-indigo-400/60" />}
          {eyebrow}
          {isCenter && <span className="h-px w-6 bg-indigo-400/60" />}
        </motion.div>
      )}

      <motion.h2
        variants={perspectiveReveal}
        style={{ transformPerspective: 1000 }}
        className="font-display text-4xl md:text-6xl font-extrabold tracking-tightest mt-4 leading-[1.05] text-white"
      >
        {title}
      </motion.h2>

      {description && (
        <motion.p
          variants={blurIn}
          className="mt-5 text-white/50 text-base md:text-lg leading-relaxed max-w-2xl"
        >
          {description}
        </motion.p>
      )}
    </motion.div>
  );
}
