using System.Net;
using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Application.UseCases.CastMember.Get;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.GetCastMember;

[Collection(nameof(GetCastMemberApiTestFixture))]
public class GetCastMemberApiTest(GetCastMemberApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(GetCastMember))]
    [Trait("EndToEnd/Api", "GetCastMember - Endpoints")]
    public async Task GetCastMember()
    {
        var castMembers = fixture.GetValidCastMembers();
        var castMember = castMembers[5];
        await fixture.Persistence.InsertList(castMembers);

        var (response, output) = await fixture.ApiClient.Get<ApiOutput<GetCastMemberOutput>>(
            $"/castmembers/{castMember.Id}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(castMember.Id);
        output.Data.Name.Should().Be(castMember.Name);
        output.Data.Type.Should().Be(castMember.Type);
        output.Data.CreatedAt.Should().BeSameDateAs(castMember.CreatedAt);
    }

    [Fact(DisplayName = nameof(ThrowWhenCastMemberNotFound))]
    [Trait("EndToEnd/Api", "GetCastMember - Endpoints")]
    public async Task ThrowWhenCastMemberNotFound()
    {
        await fixture.Persistence.InsertList(fixture.GetValidCastMembers());
        var invalidId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient.Get<ProblemDetails>(
            $"/castmembers/{invalidId}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be(StatusCodes.Status404NotFound);
        output.Title.Should().Be("Not found");
        output.Type.Should().Be("NotFound");
        output.Detail.Should().Be($"CastMember {invalidId} not found.");
    }
}
