from pathlib import Path

ROOT = Path(__file__).resolve().parent

REQUIRED = [
    "index.html",
    "styles.css",
    "script.js",
    "mark.svg",
    "social-preview.svg",
    "CREATOR_DESIGN_DNA.md",
    "llms.txt",
    "robots.txt",
    "sitemap.xml",
    "site.webmanifest",
    ".nojekyll",
]

for relative in REQUIRED:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing creator-hub file: {relative}")

index = (ROOT / "index.html").read_text(encoding="utf-8")
dna = (ROOT / "CREATOR_DESIGN_DNA.md").read_text(encoding="utf-8")
llms = (ROOT / "llms.txt").read_text(encoding="utf-8")
sitemap = (ROOT / "sitemap.xml").read_text(encoding="utf-8")

for value in ("Causalia", "Forge", "GORM"):
    if value not in index:
        raise SystemExit(f"Creator hub must expose top-level product/family: {value}")

for value in ("Forge.Delta", "Forge.Sync", "Forge.Parse"):
    if value not in index or value not in llms:
        raise SystemExit(f"Forge family member missing from creator surfaces: {value}")

combined = "\n".join((index, dna, llms))
if "ParseForge" in combined:
    raise SystemExit("Use Forge.Parse. ParseForge must not appear in creator-hub surfaces.")

if "https://pjotrcasteel.github.io/" not in index:
    raise SystemExit("Creator hub canonical URL is missing.")

if "https://pjotrcasteel.github.io/Causalia/" not in index:
    raise SystemExit("Causalia canonical website is missing.")

if "https://pjotrcasteel.github.io/Forge/" not in index:
    raise SystemExit("Forge canonical website is missing.")

if "https://pjotrcasteel.github.io/" not in sitemap:
    raise SystemExit("Creator hub sitemap does not contain the root canonical URL.")

print("Creator hub validated: products, Forge family, canonical URLs and required files.")
