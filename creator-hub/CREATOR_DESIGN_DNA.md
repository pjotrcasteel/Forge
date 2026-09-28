# PjotrCasteel Open Source Design DNA

> Status: creator-level standard v1.
>
> Canonical location after creator-hub publication: https://pjotrcasteel.github.io/CREATOR_DESIGN_DNA.md
>
> This staged copy is byte-for-byte sourced from the Forge-era constitution apart from this location note. The Forge copy remains canonical until the creator hub is actually published.

This document defines the **shared product and documentation DNA** for open-source projects created by PjotrCasteel.

It is deliberately **not** a shared theme, CSS framework, component library, or logo system. The goal is to make different projects feel like they come from the same creator without making them look like reskins of one another.

The rule is simple:

> **Share principles, structure, quality and authorship. Do not share personality.**

---

## 1. Product architecture

The creator identity sits above multiple products and product families.

### Standalone products

These should have their own visual metaphor, palette, logo language, hero treatment and interaction model.

- **Causalia** — deterministic simulation testing for concurrent and distributed .NET software.
- **GORM** — graph-oriented data access and graph persistence tooling.

### Forge product family

Forge is a family for strongly typed, explicit transformation and planning primitives.

Current members:

- **Forge.Delta** — semantic object differences.
- **Forge.Sync** — desired-state reconciliation and transition planning.

Reserved family identity:

- **Forge.Parse** — parsing and typed interpretation. When it ships, it belongs inside Forge. It must not be branded as a separate product named ParseForge.

Forge family members should feel more closely related to each other than Forge does to Causalia or GORM. They may share the Forge mark, core typography, structural grid and forged/blueprint visual language, while each package can receive a restrained secondary accent or interaction motif.

### Creator identity

The creator signature is **PjotrCasteel**.

The small **PC** maker mark is the preferred compact signature where a visual signature is useful. It should remain secondary to the product brand.

A product page should say, in effect:

> This is Causalia / Forge / GORM first. It happens to be made by PjotrCasteel.

Not:

> This is a PjotrCasteel-branded shell containing another package.

---

## 2. Shared fingerprints

Every project should share these characteristics.

### Product first

The first screen, README opening and NuGet description must answer:

1. What problem does this solve?
2. What is the smallest useful thing it gives me?
3. Why is that different from ordinary code or existing tooling?
4. Where does its responsibility stop?

Do not lead with architecture, implementation history, package count or creator biography.

### One clear promise

Each project needs a short promise that can survive outside its website.

Examples of the pattern:

- **Causalia:** Explore what can happen.
- **Forge:** Plan what should happen.

The wording can evolve. The principle cannot: one memorable product-level idea before feature detail.

### Evidence before claims

Prefer:

- runnable examples;
- interactive explanations;
- testable guarantees;
- explicit contracts;
- benchmarks with context;
- real limitations;
- reproducible failure or transition traces.

Avoid empty claims such as:

- enterprise-grade;
- blazing fast;
- production ready;
- AI powered;
- revolutionary;
- zero effort.

If a strong claim is useful, show the evidence close to the claim.

### Explicit boundaries

Each project must state what it **does not** own.

This is part of the product, not defensive documentation.

Examples:

- Forge plans; the application executes.
- Causalia simulates controlled semantics; it does not replace real provider integration tests.
- GORM should explain where graph mapping stops and application/domain behavior begins.
- Forge.Parse should explain what it parses and represents versus what the application decides to do with the result.

Boundaries make an engineering tool easier to trust.

### Typed over stringly

Public examples should prefer:

- CLR types;
- enums or dedicated value types where appropriate;
- explicit result models;
- compile-time discoverability;
- narrow APIs.

Avoid `Dictionary<string, object>`, generic extension bags, unexplained magic strings and broad dynamic configuration when a typed model can express the same concept.

### Deterministic explanations

A user should be able to understand why the library produced a result.

Where a project calculates, classifies, plans, parses or simulates something, the public experience should expose enough structure to reason about that result.

---

## 3. Visual identity: family resemblance without clones

### What may be shared

Across standalone products, it is acceptable to share:

- disciplined typography;
- generous spacing;
- strong information hierarchy;
- technical/machine-readable microcopy;
- compact mono labels;
- subtle creator signature treatment;
- restrained motion;
- high-contrast code presentation;
- similar standards for cards, focus states and responsive behavior.

These are quality fingerprints, not a theme.

### What must be product-specific

Standalone products should define their own:

- primary palette;
- logo geometry;
- hero metaphor;
- decorative system;
- interactive demo concept;
- iconography;
- motion character;
- diagram language.

A visitor should be able to place screenshots of Causalia, Forge and GORM beside one another and immediately see three different products.

They should still feel like they were designed by the same person.

### Current product metaphors

**Causalia**

- Metaphor: causal paths, controlled experiments, failure traces, replay.
- Character: analytical, exploratory, precise.
- Visual language: dark failure lab, mint/blue signals, controlled warning accents.
- Interaction: change a failure scenario and inspect deterministic evidence.

**Forge**

- Metaphor: blueprint, fabrication, transformation, explicit transition plans.
- Character: structural, precise, engineered.
- Visual language: dark technical drawing surface, forged orange, steel/blue accents.
- Interaction: change current/desired state and inspect the resulting plan.

**GORM**

- Direction: connected data, graph topology, paths and persistence.
- It should not reuse the Causalia trace aesthetic or Forge blueprint aesthetic.
- Its identity should be designed when its public surface is built.

### Forge sub-products

Forge.Delta, Forge.Sync and Forge.Parse belong to one family. Do not build separate standalone visual brands for them.

Use the Forge parent identity, then distinguish the sub-product through:

- symbol;
- secondary accent;
- example domain;
- interaction;
- package-specific copy.

The user should always understand that `Forge.Parse` is part of Forge.

---

## 4. README contract

READMEs across the family should feel structurally related without becoming templates filled with different nouns.

The default sequence is:

1. **Name + one-sentence promise**
2. **The problem**
3. **When to use it**
4. **Five-minute start**
5. **First meaningful example**
6. **How to choose the right capability/package**
7. **Core concepts**
8. **Boundaries / non-goals**
9. **Links to deeper documentation**
10. **License / creator attribution where useful**

Rules:

- Optimize the first screen for comprehension.
- Show useful code early.
- Keep the root README shorter than the complete documentation.
- Do not turn GitHub into a generated API manual.
- Package-specific NuGet READMEs may be narrower than the repository README.
- Examples in documentation should compile or be protected by a verification mechanism whenever practical.

---

## 5. Website contract

Every major standalone product should eventually have a small static product website.

A website must earn its existence by doing something the README cannot do as well.

At minimum:

- explain the product in under a minute;
- contain one product-specific interactive explanation;
- show one clear path to installation;
- link to NuGet and source;
- expose important boundaries;
- work on mobile;
- respect reduced-motion preferences;
- have meaningful metadata for search and sharing;
- expose machine-readable context such as `llms.txt` when useful.

Avoid:

- generic SaaS dashboards;
- fake customer logos;
- fake testimonials;
- invented adoption metrics;
- decorative animations that obstruct reading;
- massive client frameworks for a static documentation problem.

Static HTML/CSS/JS is a feature when it keeps the site fast, inspectable and durable.

---

## 6. Interactive demonstration rule

Each project should have **one signature interaction** that teaches the core idea.

The interaction is not a toy added for visual polish. It should compress the mental model.

Good examples:

- Causalia: choose a production failure and inspect the deterministic path/replay.
- Forge: change a state-transition problem and inspect the explicit plan.
- GORM: likely change nodes/edges/query shape and inspect the graph/SQL mapping.
- Forge.Parse: likely change an input grammar/sample and inspect typed parse output or diagnostics.

The demo must clearly label whether it is illustrative or actually executing the library.

Never imply browser JavaScript is running the .NET package when it is not.

---

## 7. Writing voice

The shared voice is:

- technical;
- calm;
- concise;
- confident without hype;
- explicit about trade-offs;
- interested in why something works, not only what button to press.

Prefer:

> Forge calculates the plan and stops before side effects begin.

Over:

> Forge revolutionizes orchestration with an enterprise-grade next-generation engine.

Prefer:

> This model can reproduce the failure semantics; keep real provider integration tests.

Over:

> Causalia guarantees your distributed system is correct.

Use plain English before specialist terminology. Then introduce the precise term.

---

## 8. Documentation layers

A mature project should provide information in layers.

### Layer 1 — discovery

- NuGet description
- repository description
- README opening
- website hero
- social preview

All should communicate the same product promise using surface-appropriate wording.

### Layer 2 — adoption

- five-minute start;
- package choice;
- first real example;
- migration/adoption tooling where applicable.

### Layer 3 — engineering contract

- design semantics;
- API contracts;
- failure modes;
- performance characteristics;
- compatibility;
- packaging;
- release policy.

### Layer 4 — machine-readable context

Where useful:

- `llms.txt`;
- structured CLI output;
- deterministic examples;
- stable headings/anchors.

Agent-readable documentation does not replace human documentation. It is another surface of the same product contract.

---

## 9. Open-source trust signals

Prefer trust through inspectability.

Recommended signals:

- MIT license where appropriate;
- clear repository and project URLs in NuGet metadata;
- Source Link / symbols;
- package validation;
- release notes;
- CI status;
- tests;
- benchmarks with methodology;
- explicit compatibility policy;
- documented limitations;
- package icons and consistent metadata;
- public source for examples.

Do not manufacture social proof.

One real benchmark, one passing package consumer, or one reproduced failure is more useful than a row of vague trust badges.

---

## 10. Accessibility and performance baseline

Public sites should:

- remain usable with JavaScript disabled wherever the interaction is not essential;
- use semantic HTML;
- expose keyboard focus states;
- avoid conveying meaning through color alone;
- respect `prefers-reduced-motion`;
- retain readable contrast;
- work at narrow mobile widths;
- avoid unnecessary third-party scripts;
- avoid loading large frameworks for static content;
- keep primary content available without a loading sequence.

Performance is part of polish.

---

## 11. Creator signature

The signature should be subtle and consistent.

Preferred footer treatment:

- small `PC` maker mark;
- `AN OPEN-SOURCE PROJECT BY`;
- `PjotrCasteel` linking to the creator profile.

Rules:

- never make the creator mark larger than the product mark;
- do not place the creator name ahead of the product in page titles;
- keep the signature visually adapted to the product palette;
- do not make all products share the same footer implementation or CSS.

The signature should answer authorship, not compete for attention.

---

## 12. Things we deliberately do not standardize

Do **not** create a shared creator-level:

- CSS framework;
- JavaScript component library;
- mandatory dark theme;
- universal color palette;
- universal hero layout;
- universal logo container;
- shared runtime NuGet dependency;
- `PjotrCasteel.Core` package;
- generic website template that every product must inherit.

A shared implementation creates visual convergence and technical coupling.

The DNA should remain portable as principles.

---

## 13. Release-surface consistency

Before a public release, the project should verify that its surfaces agree on:

- package name;
- version;
- product description;
- project URL;
- repository URL;
- package icon;
- README;
- website install command;
- documentation links;
- `llms.txt` where present.

Where practical, automate these checks in CI.

This rule becomes more important as the product family grows.

---

## 14. Decision test for future work

Before adding a design or documentation convention, ask:

1. Does this make the product easier to understand?
2. Does it provide evidence or merely decoration?
3. Is this a creator-level principle or product-specific personality?
4. Would applying it to every project make the projects look too similar?
5. Can it be validated or kept from silently drifting?

If a convention improves clarity and quality across products, add it to the DNA.

If it mainly expresses the product metaphor, keep it local to that product.

---

## 15. Current family map

```text
PjotrCasteel
│
├── Causalia
│   └── deterministic simulation testing
│
├── Forge
│   ├── Forge.Delta
│   ├── Forge.Sync
│   └── Forge.Parse   (family identity; publish when ready)
│
└── GORM
    └── graph-oriented persistence / ORM
```

The family relationship should be visible enough for discovery, but packages must remain independently useful unless there is a real technical reason for a dependency.

The creator identity connects the projects.

It does not turn them into one framework.
