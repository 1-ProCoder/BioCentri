import { useEffect, useRef, useCallback } from 'react';
import { useReducedMotion } from 'framer-motion';

/**
 * BiometricOrb — Canvas 2D animated biometric visualisation.
 *
 * Renders:
 *   - Outer breathing glow
 *   - Three concentric rotating rings with conic sweep segments
 *   - Inner iris pattern (radial lines)
 *   - Horizontal scan sweep line
 *   - Floating encrypted data particles
 *   - Mouse-proximity spring reaction
 *
 * Pauses drawing when off-screen (Intersection Observer).
 * Respects prefers-reduced-motion.
 */
export default function BiometricOrb({ size = 420, className = '' }) {
  const canvasRef = useRef(null);
  const rafRef    = useRef(null);
  const mouseRef  = useRef({ x: 0, y: 0 });
  const activeRef = useRef(true);
  const reducedMotion = useReducedMotion();

  const draw = useCallback((ctx, W, t) => {
    ctx.clearRect(0, 0, W, W);
    const cx = W / 2;
    const cy = W / 2;

    // ── Mouse spring offset (gentle tilt feel) ──
    const mx = (mouseRef.current.x - cx) * 0.018;
    const my = (mouseRef.current.y - cy) * 0.018;
    const tx = cx + mx;
    const ty = cy + my;

    // ── 1. Outer atmosphere glow ──
    const breathe = 1 + Math.sin(t * 0.6) * 0.04;
    const outerR = W * 0.43 * breathe;
    const glow = ctx.createRadialGradient(tx, ty, 0, tx, ty, outerR);
    glow.addColorStop(0,   'rgba(129,140,248,0.06)');
    glow.addColorStop(0.5, 'rgba(129,140,248,0.03)');
    glow.addColorStop(1,   'rgba(0,0,0,0)');
    ctx.beginPath();
    ctx.arc(tx, ty, outerR, 0, Math.PI * 2);
    ctx.fillStyle = glow;
    ctx.fill();

    // ── 2. Iris radial lines ──
    const irisR = W * 0.18;
    const lineCount = 48;
    ctx.save();
    ctx.translate(tx, ty);
    ctx.rotate(t * 0.08);
    for (let i = 0; i < lineCount; i++) {
      const angle = (i / lineCount) * Math.PI * 2;
      const pulse = 0.6 + Math.sin(t * 1.2 + i * 0.4) * 0.4;
      ctx.beginPath();
      ctx.moveTo(
        Math.cos(angle) * irisR * 0.35,
        Math.sin(angle) * irisR * 0.35
      );
      ctx.lineTo(
        Math.cos(angle) * irisR,
        Math.sin(angle) * irisR
      );
      ctx.strokeStyle = `rgba(165,180,252,${0.12 * pulse})`;
      ctx.lineWidth = 0.8;
      ctx.stroke();
    }
    ctx.restore();

    // ── 3. Core circle ──
    const coreR = W * 0.10;
    const coreGlow = ctx.createRadialGradient(tx, ty, 0, tx, ty, coreR);
    coreGlow.addColorStop(0,   'rgba(200,210,255,0.90)');
    coreGlow.addColorStop(0.4, 'rgba(129,140,248,0.55)');
    coreGlow.addColorStop(1,   'rgba(99,102,241,0.0)');
    ctx.beginPath();
    ctx.arc(tx, ty, coreR, 0, Math.PI * 2);
    ctx.fillStyle = coreGlow;
    ctx.fill();

    // ── 4. Ring 1 — primary rotation ──
    const r1 = W * 0.30;
    ctx.save();
    ctx.translate(tx, ty);
    ctx.rotate(t * 0.4);
    ctx.beginPath();
    ctx.arc(0, 0, r1, 0, Math.PI * 2);
    ctx.strokeStyle = 'rgba(129,140,248,0.12)';
    ctx.lineWidth = 1;
    ctx.stroke();
    // sweep arc
    ctx.beginPath();
    ctx.arc(0, 0, r1, -0.2, 0.9);
    ctx.strokeStyle = 'rgba(165,180,252,0.7)';
    ctx.lineWidth = 1.5;
    ctx.stroke();
    // tick marks
    for (let i = 0; i < 32; i++) {
      const a = (i / 32) * Math.PI * 2;
      const len = i % 4 === 0 ? 5 : 2.5;
      ctx.beginPath();
      ctx.moveTo(Math.cos(a) * (r1 - len), Math.sin(a) * (r1 - len));
      ctx.lineTo(Math.cos(a) * (r1 + len), Math.sin(a) * (r1 + len));
      ctx.strokeStyle = `rgba(165,180,252,${i % 4 === 0 ? 0.45 : 0.18})`;
      ctx.lineWidth = 0.8;
      ctx.stroke();
    }
    ctx.restore();

    // ── 5. Ring 2 — counter-rotation ──
    const r2 = W * 0.38;
    ctx.save();
    ctx.translate(tx, ty);
    ctx.rotate(-t * 0.22);
    ctx.beginPath();
    ctx.arc(0, 0, r2, 0, Math.PI * 2);
    ctx.strokeStyle = 'rgba(103,232,249,0.08)';
    ctx.lineWidth = 1;
    ctx.setLineDash([4, 6]);
    ctx.stroke();
    ctx.setLineDash([]);
    // cyan sweep
    ctx.beginPath();
    ctx.arc(0, 0, r2, 1.2, 2.4);
    ctx.strokeStyle = 'rgba(103,232,249,0.5)';
    ctx.lineWidth = 1.2;
    ctx.stroke();
    ctx.restore();

    // ── 6. Ring 3 — outer slow ──
    const r3 = W * 0.44;
    ctx.save();
    ctx.translate(tx, ty);
    ctx.rotate(t * 0.1);
    ctx.beginPath();
    ctx.arc(0, 0, r3, 0, Math.PI * 2);
    ctx.strokeStyle = 'rgba(129,140,248,0.05)';
    ctx.lineWidth = 1;
    ctx.setLineDash([2, 10]);
    ctx.stroke();
    ctx.setLineDash([]);
    ctx.restore();

    // ── 7. Scan line ──
    const scanY = cy + Math.sin(t * 0.7) * (W * 0.28);
    const scanGrad = ctx.createLinearGradient(cx - r1, scanY, cx + r1, scanY);
    scanGrad.addColorStop(0,   'rgba(129,140,248,0)');
    scanGrad.addColorStop(0.3, 'rgba(129,140,248,0.5)');
    scanGrad.addColorStop(0.5, 'rgba(200,210,255,0.8)');
    scanGrad.addColorStop(0.7, 'rgba(129,140,248,0.5)');
    scanGrad.addColorStop(1,   'rgba(129,140,248,0)');
    ctx.save();
    ctx.beginPath();
    ctx.arc(tx, ty, r1 - 2, 0, Math.PI * 2);
    ctx.clip();
    ctx.beginPath();
    ctx.moveTo(cx - r1, scanY);
    ctx.lineTo(cx + r1, scanY);
    ctx.strokeStyle = scanGrad;
    ctx.lineWidth = 1.5;
    ctx.stroke();
    // scan glow below line
    const scanGlow = ctx.createLinearGradient(cx, scanY, cx, scanY + 18);
    scanGlow.addColorStop(0, 'rgba(129,140,248,0.14)');
    scanGlow.addColorStop(1, 'rgba(129,140,248,0)');
    ctx.fillStyle = scanGlow;
    ctx.fillRect(cx - r1, scanY, r1 * 2, 18);
    ctx.restore();

    // ── 8. Floating data particles ──
    const particles = [
      { angle: 0.8,  orbit: r2, size: 3, speed: 0.35, color: 'rgba(165,180,252,0.8)' },
      { angle: 2.1,  orbit: r2, size: 2, speed: 0.28, color: 'rgba(103,232,249,0.7)' },
      { angle: 3.9,  orbit: r2, size: 2.5, speed: 0.32, color: 'rgba(165,180,252,0.6)' },
      { angle: 5.0,  orbit: r1, size: 2, speed: 0.5, color: 'rgba(200,210,255,0.9)' },
      { angle: 1.3,  orbit: r1, size: 1.5, speed: 0.45, color: 'rgba(103,232,249,0.6)' },
    ];
    particles.forEach((p) => {
      const a = p.angle + t * p.speed;
      const px = tx + Math.cos(a) * p.orbit;
      const py = ty + Math.sin(a) * p.orbit;
      // glow halo
      const halo = ctx.createRadialGradient(px, py, 0, px, py, p.size * 4);
      halo.addColorStop(0, p.color.replace(')', ', 0.3)').replace('rgba', 'rgba'));
      halo.addColorStop(1, 'rgba(0,0,0,0)');
      ctx.beginPath();
      ctx.arc(px, py, p.size * 4, 0, Math.PI * 2);
      ctx.fillStyle = halo;
      ctx.fill();
      // core dot
      ctx.beginPath();
      ctx.arc(px, py, p.size, 0, Math.PI * 2);
      ctx.fillStyle = p.color;
      ctx.fill();
    });

    // ── 9. Corner data readout ──
    const matchPct = 94 + Math.sin(t * 0.3) * 4;
    ctx.font = '10px "JetBrains Mono", monospace';
    ctx.fillStyle = 'rgba(165,180,252,0.5)';
    ctx.fillText(`MATCH  ${matchPct.toFixed(1)}%`, tx - W * 0.36, ty + W * 0.36);
    ctx.fillText(`LATENCY  42ms`, tx + W * 0.06, ty + W * 0.36);
    ctx.fillText(`LIVENESS  OK`, tx - W * 0.36, ty - W * 0.33);
  }, []);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    const W = size * window.devicePixelRatio;
    canvas.width  = W;
    canvas.height = W;
    ctx.scale(window.devicePixelRatio, window.devicePixelRatio);

    // IntersectionObserver — pause when off-screen
    const obs = new IntersectionObserver(([entry]) => {
      activeRef.current = entry.isIntersecting;
    }, { threshold: 0.1 });
    obs.observe(canvas);

    // Mouse tracking
    const onMouse = (e) => {
      const rect = canvas.getBoundingClientRect();
      mouseRef.current = {
        x: (e.clientX - rect.left) * (size / rect.width),
        y: (e.clientY - rect.top)  * (size / rect.height),
      };
    };
    window.addEventListener('mousemove', onMouse, { passive: true });

    let t = 0;
    let last = performance.now();

    const loop = (now) => {
      rafRef.current = requestAnimationFrame(loop);
      if (!activeRef.current) return;
      const dt = Math.min((now - last) / 1000, 0.05);
      last = now;
      if (!reducedMotion) t += dt;
      draw(ctx, size, t);
    };
    rafRef.current = requestAnimationFrame(loop);

    return () => {
      cancelAnimationFrame(rafRef.current);
      obs.disconnect();
      window.removeEventListener('mousemove', onMouse);
    };
  }, [size, reducedMotion, draw]);

  return (
    <canvas
      ref={canvasRef}
      aria-hidden="true"
      className={className}
      style={{ width: size, height: size }}
    />
  );
}
