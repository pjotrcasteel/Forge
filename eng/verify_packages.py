from __future__ import annotations

import sys
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path

PROJECT_URL = "https://pjotrcasteel.github.io/Forge/"
REPOSITORY_URL = "https://github.com/pjotrcasteel/Forge"


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
    nuspec_names = [name for name in archive.namelist() if name.endswith(".nuspec")]
    if len(nuspec_names) != 1:
        fail(f"{package_name} must contain exactly one nuspec")
    return ET.fromstring(archive.read(nuspec_names[0]))


def dependency_map(nuspec: ET.Element) -> dict[str, str]:
    result: dict[str, str] = {}
    for element in nuspec.iter():
        if local_name(element) != "dependency":
            continue
        package_id = element.attrib.get("id")
        if package_id is not None:
            result[package_id] = element.attrib.get("version", "")
    return result


def verify_metadata(nuspec: ET.Element, package_id: str, version: str, require_source_generator_tag: bool) -> None:
    data = metadata(nuspec)
    if child_text(data, "id") != package_id:
        fail(f"{package_id} nuspec has an unexpected package id")
    if child_text(data, "version") != version:
        fail(f"{package_id} nuspec has an unexpected version")
    if not (child_text(data, "description") or "").strip():
        fail(f"{package_id} must have a package description")
    if child_text(data, "readme") != "README.md":
        fail(f"{package_id} must declare README.md as its package README")
    if child_text(data, "projectUrl") != PROJECT_URL:
        fail(f"{package_id} must use the canonical Forge project URL")
    if child_text(data, "icon") != "forge-icon.png":
        fail(f"{package_id} must declare forge-icon.png as its package icon")
    repository_elements = [element for element in data if local_name(element) == "repository"]
    if len(repository_elements) != 1:
        fail(f"{package_id} must contain exactly one repository declaration")
    repository = repository_elements[0]
    if repository.attrib.get("type") != "git" or repository.attrib.get("url") != REPOSITORY_URL:
        fail(f"{package_id} must point repository metadata at {REPOSITORY_URL}")
    require_license_acceptance = child_text(data, "requireLicenseAcceptance")
    if require_license_acceptance is not None and require_license_acceptance.lower() != "false":
        fail(f"{package_id} must not require license acceptance")
    license_elements = [element for element in data if local_name(element) == "license"]
    if len(license_elements) != 1:
        fail(f"{package_id} must contain exactly one license declaration")
    license_element = license_elements[0]
    if license_element.attrib.get("type") != "expression" or (license_element.text or "").strip() != "MIT":
        fail(f"{package_id} must use the MIT SPDX license expression")
    tags = set((child_text(data, "tags") or "").replace(";", " ").split())
    if require_source_generator_tag and "source-generator" not in tags:
        fail(f"{package_id} package tags must include source-generator")


def verify_generated_package(path: Path, package_id: str, version: str, runtime_assembly: str, generator_assembly: str, readme_heading: str) -> ET.Element:
    with zipfile.ZipFile(path) as archive:
        names = set(archive.namelist())
        required = {"README.md", "CHANGELOG.md", "LICENSE", "forge-icon.png", f"lib/net10.0/{runtime_assembly}.dll", f"lib/net10.0/{runtime_assembly}.xml", f"analyzers/dotnet/cs/{generator_assembly}.dll"}
        missing = required - names
        if missing:
            fail(f"{path.name} is missing: {', '.join(sorted(missing))}")
        if any(name.startswith("lib/") and Path(name).name == f"{generator_assembly}.dll" for name in names):
            fail(f"{path.name} exposes its generator as a runtime library")
        if any("Microsoft.CodeAnalysis" in name and name.endswith(".dll") for name in names):
            fail(f"{path.name} must not embed Roslyn assemblies")
        if any(name.endswith(".cs") for name in names):
            fail(f"{path.name} unexpectedly contains C# source files")
        readme = archive.read("README.md").decode("utf-8-sig")
        if not readme.startswith(readme_heading):
            fail(f"{path.name} does not contain its package-specific README")
        nuspec = read_nuspec(archive, path.name)
        verify_metadata(nuspec, package_id, version, True)
        return nuspec


def verify_runtime_package(path: Path, package_id: str, version: str, runtime_assembly: str, readme_heading: str) -> ET.Element:
    with zipfile.ZipFile(path) as archive:
        names = set(archive.namelist())
        required = {"README.md", "CHANGELOG.md", "LICENSE", "forge-icon.png", f"lib/net10.0/{runtime_assembly}.dll", f"lib/net10.0/{runtime_assembly}.xml"}
        missing = required - names
        if missing:
            fail(f"{path.name} is missing: {', '.join(sorted(missing))}")
        if any(name.startswith("analyzers/") for name in names):
            fail(f"{path.name} must not contain analyzer assemblies")
        if any(name.endswith(".cs") for name in names):
            fail(f"{path.name} unexpectedly contains C# source files")
        readme = archive.read("README.md").decode("utf-8-sig")
        if not readme.startswith(readme_heading):
            fail(f"{path.name} does not contain its package-specific README")
        nuspec = read_nuspec(archive, path.name)
        verify_metadata(nuspec, package_id, version, False)
        return nuspec


def verify_symbol_package(path: Path, runtime_assembly: str) -> None:
    with zipfile.ZipFile(path) as archive:
        names = set(archive.namelist())
        expected_pdb = f"lib/net10.0/{runtime_assembly}.pdb"
        if expected_pdb not in names:
            fail(f"{path.name} is missing {expected_pdb}")
        read_nuspec(archive, path.name)


def require_matching_dependency(dependencies: dict[str, str], package_id: str, version: str) -> None:
    if package_id not in dependencies or version not in dependencies[package_id]:
        fail(f"Expected dependency on {package_id} {version}, found: {dependencies}")


def verify_single_forge_dependency(directory: Path, package_id: str, version: str) -> None:
    nuspec = verify_runtime_package(package_file(directory, package_id, version, "nupkg"), package_id, version, package_id, f"# {package_id}")
    dependencies = dependency_map(nuspec)
    if set(dependencies) != {"Forge.Decide"}:
        fail(f"{package_id} must depend only on Forge.Decide")
    require_matching_dependency(dependencies, "Forge.Decide", version)


def main() -> None:
    if len(sys.argv) != 3:
        fail("Usage: verify_packages.py <package-directory> <version>")
    directory = Path(sys.argv[1])
    version = sys.argv[2]

    delta_nuspec = verify_generated_package(package_file(directory, "Forge.Delta", version, "nupkg"), "Forge.Delta", version, "Forge.Delta", "Forge.Delta.Generators", "# Forge.Delta")
    if dependency_map(delta_nuspec):
        fail("Forge.Delta must have no package dependencies")

    sync_nuspec = verify_generated_package(package_file(directory, "Forge.Sync", version, "nupkg"), "Forge.Sync", version, "Forge.Sync", "Forge.Sync.Generators", "# Forge.Sync")
    sync_dependencies = dependency_map(sync_nuspec)
    if set(sync_dependencies) != {"Forge.Delta"}:
        fail("Forge.Sync must depend only on Forge.Delta")
    require_matching_dependency(sync_dependencies, "Forge.Delta", version)

    parse_nuspec = verify_runtime_package(package_file(directory, "Forge.Parse", version, "nupkg"), "Forge.Parse", version, "Forge.Parse", "# Forge.Parse")
    if dependency_map(parse_nuspec):
        fail("Forge.Parse must have no package dependencies")

    reqnroll_nuspec = verify_runtime_package(package_file(directory, "Forge.Parse.Reqnroll", version, "nupkg"), "Forge.Parse.Reqnroll", version, "Forge.Parse.Reqnroll", "# Forge.Parse.Reqnroll")
    reqnroll_dependencies = dependency_map(reqnroll_nuspec)
    require_matching_dependency(reqnroll_dependencies, "Forge.Parse", version)
    if "Reqnroll" not in reqnroll_dependencies:
        fail("Forge.Parse.Reqnroll must depend on Reqnroll")

    decide_nuspec = verify_runtime_package(package_file(directory, "Forge.Decide", version, "nupkg"), "Forge.Decide", version, "Forge.Decide", "# Forge.Decide")
    if dependency_map(decide_nuspec):
        fail("Forge.Decide must have no package dependencies")

    verify_single_forge_dependency(directory, "Forge.Decide.Testing", version)
    verify_single_forge_dependency(directory, "Forge.Decide.OpenTelemetry", version)

    decide_di_nuspec = verify_runtime_package(package_file(directory, "Forge.Decide.DependencyInjection", version, "nupkg"), "Forge.Decide.DependencyInjection", version, "Forge.Decide.DependencyInjection", "# Forge.Decide.DependencyInjection")
    decide_di_dependencies = dependency_map(decide_di_nuspec)
    if set(decide_di_dependencies) != {"Forge.Decide", "Microsoft.Extensions.DependencyInjection.Abstractions"}:
        fail("Forge.Decide.DependencyInjection must depend only on Forge.Decide and Microsoft.Extensions.DependencyInjection.Abstractions")
    require_matching_dependency(decide_di_dependencies, "Forge.Decide", version)

    for package_id in ["Forge.Delta", "Forge.Sync", "Forge.Parse", "Forge.Parse.Reqnroll", "Forge.Decide", "Forge.Decide.Testing", "Forge.Decide.DependencyInjection", "Forge.Decide.OpenTelemetry"]:
        verify_symbol_package(package_file(directory, package_id, version, "snupkg"), package_id)

    print("Forge package metadata, layout, dependency, README, XML-doc, and symbol validation passed.")


if __name__ == "__main__":
    main()