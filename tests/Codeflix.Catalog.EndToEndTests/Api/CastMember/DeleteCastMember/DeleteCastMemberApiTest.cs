using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.DeleteCastMember;

[Collection(nameof(DeleteCastMemberApiTestFixture))]
public class DeleteCastMemberApiTest(DeleteCastMemberApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(DeleteCastMember))]
    [Trait("EndToEnd/Api", "DeleteCastMember - Endpoints")]
    public async Task DeleteCastMember()
    {
        var castMembers = fixture.GetValidCastMembers();
        var castMember = castMembers[5];
        await fixture.Persistence.InsertList(castMembers);

        var (response, output) = await fixture.ApiClient.Delete<object>(
            $"/castmembers/{castMember.Id}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        output.Should().BeNull();
        var dbCastMember = await fixture.Persistence.GetById(castMember.Id);
        dbCastMember.Should().BeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenCastMemberNotFound))]
    [Trait("EndToEnd/Api", "DeleteCastMember - Endpoints")]
    public async Task ThrowWhenCastMemberNotFound()
    {
        await fixture.Persistence.InsertList(fixture.GetValidCastMembers());
        var invalidId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient.Delete<ProblemDetails>(
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
