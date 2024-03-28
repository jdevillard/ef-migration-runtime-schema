using System.Text.Json.Serialization;

namespace JDEV.EFMigrationRuntimeSchema
{
    public record EFJsonOutput(
        [property: JsonPropertyName("migrationFile")] string MigrationFile,
        [property: JsonPropertyName("metadataFile")] string MetadataFile,
        [property: JsonPropertyName("snapshotFile")] string SnapshotFile
            );
}