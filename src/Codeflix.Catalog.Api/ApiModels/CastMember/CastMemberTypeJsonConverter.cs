using System.Text.Json;
using System.Text.Json.Serialization;
using Codeflix.Catalog.Domain.Enum;

namespace Codeflix.Catalog.Api.ApiModels.CastMember;

public class CastMemberTypeJsonConverter : JsonConverter<CastMemberType>
{
    public override CastMemberType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var type = reader.TokenType switch
        {
            JsonTokenType.Number when reader.TryGetInt32(out var value) => (CastMemberType?)value,
            JsonTokenType.String => FromName(reader.GetString()),
            _ => null,
        };
        if (type is null || !Enum.IsDefined(type.Value))
            throw new JsonException("Invalid cast member type");
        return type.Value;
    }

    private static CastMemberType? FromName(string? name) =>
        Enum.GetValues<CastMemberType>()
            .Cast<CastMemberType?>()
            .FirstOrDefault(type =>
                type.ToString()!.Equals(name, StringComparison.OrdinalIgnoreCase)
            );

    public override void Write(
        Utf8JsonWriter writer,
        CastMemberType value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value.ToString());
}
