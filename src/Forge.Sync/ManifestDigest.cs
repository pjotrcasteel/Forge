using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Forge.Sync;

/// <summary>Computes cross-process canonical SHA-256 identity for portable Sync manifests.</summary>
public static class ManifestDigest
{
    public static string ComputeSha256Hex(SyncManifestDocument manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
        {
            WriteCanonicalManifest(writer, manifest);
        }

        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    public static bool VerifySha256Hex(SyncManifestDocument manifest, string expectedDigest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedDigest);
        var actualBytes = Encoding.ASCII.GetBytes(ComputeSha256Hex(manifest));
        var expectedBytes = Encoding.ASCII.GetBytes(expectedDigest.Trim().ToLowerInvariant());
        return actualBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }

    private static void WriteCanonicalManifest(Utf8JsonWriter writer, SyncManifestDocument manifest)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", manifest.SchemaVersion);
        writer.WriteString("itemType", manifest.ItemType);
        writer.WritePropertyName("operations");
        writer.WriteStartArray();
        foreach (var operation in manifest.Operations
                     .OrderBy(static item => item.Key, StringComparer.Ordinal)
                     .ThenBy(static item => item.Operation))
        {
            writer.WriteStartObject();
            writer.WriteString("operation", operation.Operation.ToString());
            writer.WriteString("key", operation.Key);
            writer.WritePropertyName("current");
            WriteCanonicalElement(writer, operation.Current);
            writer.WritePropertyName("desired");
            WriteCanonicalElement(writer, operation.Desired);
            writer.WritePropertyName("changes");
            writer.WriteStartArray();
            foreach (var change in operation.Changes.OrderBy(static item => item.Path, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("path", change.Path);
                writer.WritePropertyName("before");
                WriteCanonicalElement(writer, change.Before);
                writer.WritePropertyName("after");
                WriteCanonicalElement(writer, change.After);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
    }

    private static void WriteCanonicalElement(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(static item => item.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonicalElement(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteCanonicalElement(writer, item);
                }
                writer.WriteEndArray();
                break;
            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;
            case JsonValueKind.Number:
                writer.WriteRawValue(element.GetRawText(), skipInputValidation: false);
                break;
            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;
            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                writer.WriteNullValue();
                break;
            default:
                writer.WriteRawValue(element.GetRawText(), skipInputValidation: false);
                break;
        }
    }
}
