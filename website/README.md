# BioCentri Website

The public marketing site for **BioCentri** — a premium, privacy-first Windows security
product that protects applications with Windows Hello.

## Stack

- **React 18** + **Vite 5**
- **Tailwind CSS 3** (custom `ink` palette, custom keyframes)
- Pure inline SVG iconography (no icon-library dependency)

## Local development

From this `website/` directory:

```bash
npm install
npm run dev
```

Then open <http://localhost:5173>.

## Production build

```bash
npm run build
npm run preview
```

The static site outputs to `dist/` and can be served by any static host
(Cloudflare Pages, Netlify, Vercel, GitHub Pages, S3 + CloudFront, etc.).

## Folder structure

```
website/
├── public/
│   └── favicon.svg
├── src/
│   ├── App.jsx                 # Composition of all sections
│   ├── main.jsx                # Vite/React entry
│   ├── index.css               # Tailwind + utilities
│   ├── hooks/
│   │   └── useReveal.js        # IntersectionObserver-driven reveal
│   └── components/
│       ├── icons.jsx           # Inline SVG icon set
│       ├── Nav.jsx
│       ├── Hero.jsx
│       ├── Problem.jsx
│       ├── Solution.jsx
│       ├── Features.jsx
│       ├── Roadmap.jsx
│       ├── Building.jsx
│       ├── Waitlist.jsx
│       ├── Footer.jsx
│       └── ui/
│           ├── Container.jsx
│           ├── Button.jsx
│           └── SectionHeading.jsx
├── index.html
├── package.json
├── postcss.config.js
├── tailwind.config.js
├── vite.config.js
└── README.md
```

## Notes

- **No backend yet.** The waitlist form is UI-only and fakes a success state.
  When a real signup endpoint is ready, swap the `setTimeout` in
  `Waitlist.jsx` for a `fetch` call.
- **No analytics, no third-party scripts.** The only network calls on first
  load are the Inter font from `fonts.googleapis.com`. Drop that `<link>` in
  `index.html` if you want zero external requests.
- **No dark/light toggle yet.** The site is a single theme (dark) chosen for
  the premium security feel described in the project Bible.
