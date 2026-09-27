from __future__ import annotations

import sys
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path


def fail(message: str) -> None:
    print(f"ERROR: {message}")
    raise SystemExit(1)


def package_file(directory: Path, package_id: str, version: str, suffix: str) -> Path:
    path = directory / f"{package_id}.{version}.{suffix}"
    if not path.exists():
        fail(f"Expected package does not exist: {path}")
    return path


def local_name(element: ET.Element) -> str:
    return element.tag.rsplit("}", 1)[-1]


def child_text(element: ET.Element, name: str) -> str | None:
    for child in element:
        if local_name(child) == name:
            return child.text
    return None


def metadata(nuspec: ET.Element) -> ET.Element:
    for element in nuspec.iter():
        if local_name(element) == "metadata":
            return element
    fail("Nuspec has no metadata element")


def read_nuspec(archive: zipfile.ZipFile, package_name: str) -> ET.Element:
    names = archive.namelist()
    nuspec_names = [name for name in names if name.endswith(".nuspec")]
    if len(nuspec_names) != 1:
        fail(f"{package_name} must contain exactly one nuspec")
    return ET.fromstring(archive.read(nuspec_names[0]))


def dependency_map(nuspec: ET.Element) -> dict[str, str]:
    result: dict[str, str] = {}
    for element in nuspec.iter():
        if local_name(element) != "dependency":
            continue
        package_id = element.attrib.get("id")
        if package_id is None:
            continue
        result[package_id] = element.attrib.get("version", "")
    return result


def verify_metadata(nuspec: ET.Element, package_id: str, version: str) -> None:
    data = metadata(nuspec)

    if child_text(data, "id") != package_id:
        fail(f"{package_id} nuspec has an unexpected package id")
    if child_text(data, "version") != version:
        fail(f"{package_id} nuspec has an unexpected version")
    if not (child_text(data, "description") or "").strip():
        fail(f"{package_id} must have a package description")
    if child_text(data, "readme") != "README.md":
        fail(f"{package_id} must declare README.md as its package README")
    if (child_text(data, "requireLicenseAcceptance") or "").lower() != "false":
        fail(f"{package_id} must not require license acceptance")

    license_elements = [element for element in data if local_name(element) == "license"]
    if len(license_elements) != 1:
        fail(f"{package_id} must contain exactly one license declaration")
    license_element = license_elements[0]
    if license_element.attrib.get("type") != "expression" or (license_element.text or "").strip() != "MIT":
        fail(f"{package_id} must use the MIT SPDX license expression")

    tags = set((child_text(data, "tags") or "").replace(";", " ").split())
    if "source-generator" not in tags:
        fail(f"{package_id} package tags must include source-generator")


def verify_main_package(
    path: Path,
    package_id: str,
    version: str,
    runtime_assembly: str,
    generator_assembly: str,
    readme_heading: str,
) -> ET.Element:
    with zipfile.ZipFile(path) as archive:
        names = set(archive.namelist())
        required = {
            "README.md",
            "CHANGELOG.md",
            "LICENSE",
            f"lib/net10.0/{runtime_assembly}.dll",
            f"lib/net10.0/{runtime_assembly}.xml",
            f"analyzers/dotnet/cs/{generator_assembly}.dll",
        }
        missing = required - names
        if missing:
            fail(f"{path.name} is missing: {', '.join(sorted(missing))}")

        if any(
            name.startswith("lib/") and Path(name).name == f"{generator_assembly}.dll"
            for name in names
        ):
            fail(f"{path.name} exposes its generator as a runtime library")
        if any(
            name.startswith("analyzers/") and Path(name).name == f"{runtime_assembly}.dll"
            for name in names
        ):
            fail(f"{path.name} exposes its runtime assembly as an analyzer")
        if any("Microsoft.CodeAnalysis" in name and name.endswith(".dll") for name in names):
            fail(f"{path.name} must not embed Roslyn assemblies")
        if any(name.endswith(".cs") for name in names):
            fail(f"{path.name} unexpectedly contains C# source files")

        readme = archive.read("README.md").decode("utf-8-sig")
        if not readme.startswith(readme_heading):
            fail(f"{path.name} does not contain its package-specific README")

        nuspec = read_nuspec(archive, path.name)
        verify_metadata(nuspec, package_id, version)
        return nuspec


def verify_symbol_package(path: Path, runtime_assembly: str) -> None:
    with zipfile.ZipFile(path) as archive:
        names = set(archive.namelist())
        expected_pdb = f"lib/net10.0/{runtime_assembly}.pdb"
        if expected_pdb not in names:
            fail(f"{path.name} is missing {expected_pdb}")
        read_nuspec(archive, path.name)


def main() -> None:
    if len(sys.argv) != 3:
        fail("Usage: verify_packages.py <package-directory> <version>")

    directory = Path(sys.argv[1])
    version = sys.argv[2]

    delta = package_file(directory, "Forge.Delta", version, "nupkg")
    delta_nuspec = verify_main_package(
        delta,
        "Forge.Delta",
        version,
        "Forge.Delta",
        "Forge.Delta.Generators",
        "# Forge.Delta",
    )
    delta_dependencies = dependency_map(delta_nuspec)
    if delta_dependencies:
        fail(f"Forge.Delta must have no package dependencies, found: {', '.join(sorted(delta_dependencies))}")

    sync = package_file(directory, "Forge.Sync", version, "nupkg")
    sync_nuspec = verify_main_package(
        sync,
        "Forge.Sync",
        version,
        "Forge.Sync",
        "Forge.Sync.Generators",
        "# Forge.Sync",
    )
    sync_dependencies = dependency_map(sync_nuspec)
    if set(sync_dependencies) != {"Forge.Delta"}:
        fail("Forge.Sync must depend only on Forge.Delta")
    if version not in sync_dependencies["Forge.Delta"]:
        fail("Forge.Sync must depend on the matching Forge.Delta package version")

    verify_symbol_package(
        package_file(directory, "Forge.Delta", version, "snupkg"),
        "Forge.Delta",
    )
    verify_symbol_package(
        package_file(directory, "Forge.Sync", version, "snupkg"),
        "Forge.Sync",
    )

    print("Package metadata, layout, dependency, README, XML-doc, and symbol validation passed.")


if __name__ == "__main__":
    main()
