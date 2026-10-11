using System.Text.Json.Serialization;
using Codeflix.Catalog.Application.UseCases.CastMember.Update;
using Codeflix.Catalog.Domain.Enum;

namespace Codeflix.Catalog.Api.ApiModels.CastMember;

public record UpdateCastMemberApiInput(
    string Name,
    [property: JsonConverter(typeof(CastMemberTypeJsonConverter))] CastMemberType Type
)
{
    public UpdateCastMemberInput ToUpdateCastMemberInput(Guid id) => new(id, Name, Type);
}
