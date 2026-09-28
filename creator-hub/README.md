# PjotrCasteel creator hub — deployment staging

This directory contains the complete source for the future canonical creator hub:

> https://pjotrcasteel.github.io/

It is staged inside the Forge repository only because the special GitHub Pages repository `pjotrcasteel/pjotrcasteel.github.io` does not exist yet and the connected GitHub integration cannot create repositories.

## Publish

1. Create a **public** repository named exactly `pjotrcasteel.github.io`.
2. Copy the **contents** of this directory to the root of that repository.
3. Keep `.github/workflows/pages.yml` at that exact path.
4. Push to `main`.
5. GitHub Actions will publish the root site to `https://pjotrcasteel.github.io/`.

No build step, package manager or external web framework is required.

## Contents

- `index.html` — creator-level front door and problem-based project router.
- `styles.css` — neutral creator identity; product accents remain local to product cards.
- `script.js` — accessible problem-to-product switcher plus restrained reveal behavior.
- `mark.svg` — creator-level PC mark.
- `social-preview.svg` — creator-hub share art; a PNG can replace it during final public-surface polish.
- `CREATOR_DESIGN_DNA.md` — creator-level design/documentation constitution.
- `llms.txt` — machine-readable product map.
- `robots.txt`, `sitemap.xml`, `site.webmanifest`, `.nojekyll` — release-surface metadata.
- `.github/workflows/pages.yml` — Pages deployment workflow.

## Architecture

The hub intentionally exposes three top-level products/families:

```text
PjotrCasteel
├── Causalia          standalone
├── Forge             family
│   ├── Forge.Delta
│   ├── Forge.Sync
│   └── Forge.Parse   planned
└── GORM              standalone
```

Forge.Parse is not a fourth standalone creator brand.

## After publication

Once this directory is moved into `pjotrcasteel/pjotrcasteel.github.io`:

- the Design DNA in that repository becomes canonical;
- `Forge/docs/CREATOR_DESIGN_DNA.md` should be replaced by a short pointer to the creator-hub copy;
- this staging directory should be removed from Forge;
- repository-level About/topics/social-preview polish can be handled when Point 1 is revisited.
