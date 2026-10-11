using System.Text.Json.Serialization;
using Codeflix.Catalog.Application.UseCases.CastMember.Create;
using Codeflix.Catalog.Domain.Enum;

namespace Codeflix.Catalog.Api.ApiModels.CastMember;

public record CreateCastMemberApiInput(
    string Name,
    [property: JsonConverter(typeof(CastMemberTypeJsonConverter))] CastMemberType Type
)
{
    public CreateCastMemberInput ToCreateCastMemberInput() => new(Name, Type);
}
