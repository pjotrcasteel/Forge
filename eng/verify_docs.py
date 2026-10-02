from __future__ import annotations

import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"


def fail(message: str) -> None:
    print(f"ERROR: {message}")
    raise SystemExit(1)


def require_file(path: Path) -> str:
    if not path.is_file():
        fail(f"Expected documentation file does not exist: {path.relative_to(ROOT)}")
    return path.read_text(encoding="utf-8-sig")


def require_contains(text: str, value: str, source: str) -> None:
    if value not in text:
        fail(f"{source} must contain: {value}")


def verify_local_assets(html: str, source: str) -> None:
    for attribute, value in re.findall(r'\b(href|src)="([^"]+)"', html):
        if not value.startswith("./") or value in {"./", "./#"}:
            continue

        target = value[2:].split("#", 1)[0].split("?", 1)[0]
        if not target:
            continue

        path = DOCS / target
        if not path.exists():
            fail(f"{source} references missing local {attribute}: {value}")


def verify_javascript(path: Path) -> None:
    result = subprocess.run(
        ["node", "--check", str(path)],
        cwd=ROOT,
        capture_output=True,
        text=True,
        check=False,
    )

    if result.returncode != 0:
        detail = (result.stderr or result.stdout).strip()
        fail(f"JavaScript syntax validation failed for {path.relative_to(ROOT)}: {detail}")


def main() -> None:
    decide = require_file(DOCS / "decide.html")
    decide_css = require_file(DOCS / "decide.css")
    decide_js = require_file(DOCS / "decide.js")
    decide_home_css = require_file(DOCS / "decide-home.css")
    family_js = require_file(DOCS / "script.js")
    sitemap = require_file(DOCS / "sitemap.xml")
    llms = require_file(DOCS / "llms.txt")
    readme = require_file(ROOT / "README.md")

    for anchor in ["space", "propose", "decide", "shadow", "evidence", "packages"]:
        require_contains(decide, f'id="{anchor}"', "docs/decide.html")

    for asset in ["./styles.css", "./story.css", "./decide.css", "./decide.js"]:
        require_contains(decide, asset, "docs/decide.html")

    for concept in [
        "StrategySpace",
        "CompareAsync",
        "DecideWithShadowAsync",
        "Forge.Decide.Testing",
        "Forge.Decide.DependencyInjection",
        "Forge.Decide.OpenTelemetry",
        "Forge decides. Your application executes.",
    ]:
        require_contains(decide, concept, "docs/decide.html")

    require_contains(decide_css, ".decision-board", "docs/decide.css")
    require_contains(decide_css, ".shadow-board", "docs/decide.css")
    require_contains(decide_js, 'const sections=["space","propose","decide","shadow","evidence"]', "docs/decide.js")
    require_contains(decide_home_css, ".decide-card", "docs/decide-home.css")

    require_contains(family_js, "decide:{kicker:", "docs/script.js")
    require_contains(family_js, "Forge.Decide functionality", "docs/script.js")
    require_contains(family_js, "Four focused tools.", "docs/script.js")
    require_contains(family_js, "./decide.html", "docs/script.js")

    require_contains(sitemap, "https://pjotrcasteel.github.io/Forge/decide.html", "docs/sitemap.xml")
    require_contains(llms, "Forge.Decide functionality page", "docs/llms.txt")
    require_contains(llms, "Forge.Decide.OpenTelemetry", "docs/llms.txt")
    require_contains(readme, "Forge.Decide functionality page", "README.md")
    require_contains(readme, "Which valid course of action should become the plan", "README.md")

    verify_local_assets(decide, "docs/decide.html")
    verify_javascript(DOCS / "script.js")
    verify_javascript(DOCS / "decide.js")

    print("Forge documentation and Forge.Decide website validation passed.")


if __name__ == "__main__":
    main()
