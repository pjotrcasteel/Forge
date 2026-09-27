namespace Forge.Sync;

/// <summary>Identifies why a portable plan no longer matches the current state it was planned against.</summary>
public enum ManifestPreconditionFailureReason
{
    ExpectedAbsent,
    ExpectedPresent,
    CurrentStateChanged
}
