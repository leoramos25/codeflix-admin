using System.Net;
using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Api.ApiModels.CastMember;
using Codeflix.Catalog.Application.UseCases.CastMember.Update;
using Codeflix.Catalog.Domain.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.UpdateCastMember;

[Collection(nameof(UpdateCastMemberApiTestFixture))]
public class UpdateCastMemberApiTest(UpdateCastMemberApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(UpdateCastMember))]
    [Trait("EndToEnd/Api", "UpdateCastMember - Endpoints")]
    public async Task UpdateCastMember()
    {
        var castMembers = fixture.GetValidCastMembers();
        var castMember = castMembers[5];
        await fixture.Persistence.InsertList(castMembers);
        var input = fixture.GetValidInput();

        var (response, output) = await fixture.ApiClient.Put<ApiOutput<UpdateCastMemberOutput>>(
            $"/castmembers/{castMember.Id}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(castMember.Id);
        output.Data.Name.Should().Be(input.Name);
        output.Data.Type.Should().Be(input.Type);
        output.Data.CreatedAt.Should().BeSameDateAs(castMember.CreatedAt);
        var dbCastMember = await fixture.Persistence.GetById(castMember.Id);
        dbCastMember.Should().NotBeNull();
        dbCastMember!.Name.Should().Be(input.Name);
        dbCastMember.Type.Should().Be(input.Type);
    }

    [Theory(DisplayName = nameof(UpdateCastMemberWithTypeAsCaseInsensitiveString))]
    [Trait("EndToEnd/Api", "UpdateCastMember - Endpoints")]
    [InlineData("Actor", CastMemberType.Actor)]
    [InlineData("actor", CastMemberType.Actor)]
    [InlineData("DIRECTOR", CastMemberType.Director)]
    [InlineData("dIrEcToR", CastMemberType.Director)]
    public async Task UpdateCastMemberWithTypeAsCaseInsensitiveString(
        string type,
        CastMemberType expectedType
    )
    {
        var castMember = fixture.GetValidCastMember();
        await fixture.Persistence.InsertList([castMember]);
        var name = fixture.GetValidName();

        var (response, output) = await fixture.ApiClient.Put<ApiOutput<UpdateCastMemberOutput>>(
            $"/castmembers/{castMember.Id}",
            new { name, type },
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Type.Should().Be(expectedType);
        var dbCastMember = await fixture.Persistence.GetById(castMember.Id);
        dbCastMember.Should().NotBeNull();
        dbCastMember!.Name.Should().Be(name);
        dbCastMember.Type.Should().Be(expectedType);
    }

    [Theory(DisplayName = nameof(UpdateCastMemberWithTypeAsInteger))]
    [Trait("EndToEnd/Api", "UpdateCastMember - Endpoints")]
    [InlineData(1, CastMemberType.Director)]
    [InlineData(2, CastMemberType.Actor)]
    public async Task UpdateCastMemberWithTypeAsInteger(int type, CastMemberType expectedType)
    {
        var castMember = fixture.GetValidCastMember();
        await fixture.Persistence.InsertList([castMember]);
        var name = fixture.GetValidName();

        var (response, output) = await fixture.ApiClient.Put<ApiOutput<UpdateCastMemberOutput>>(
            $"/castmembers/{castMember.Id}",
            new { name, type },
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Type.Should().Be(expectedType);
        var dbCastMember = await fixture.Persistence.GetById(castMember.Id);
        dbCastMember.Should().NotBeNull();
        dbCastMember!.Name.Should().Be(name);
        dbCastMember.Type.Should().Be(expectedType);
    }

    [Theory(DisplayName = nameof(ThrowWhenTypeIsInvalid))]
    [Trait("EndToEnd/Api", "UpdateCastMember - Endpoints")]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData("1")]
    [InlineData("Producer")]
    [InlineData("")]
    public async Task ThrowWhenTypeIsInvalid(object invalidType)
    {
        var castMember = fixture.GetValidCastMember();
        await fixture.Persistence.InsertList([castMember]);

        var (response, output) = await fixture.ApiClient.Put<ValidationProblemDetails>(
            $"/castmembers/{castMember.Id}",
            new { name = fixture.GetValidName(), type = invalidType },
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        output.Should().NotBeNull();
        output!.Status.Should().Be(StatusCodes.Status400BadRequest);
        output.Errors.Keys.Should().Contain("$.type");
        var dbCastMember = await fixture.Persistence.GetById(castMember.Id);
        dbCastMember!.Name.Should().Be(castMember.Name);
        dbCastMember.Type.Should().Be(castMember.Type);
    }

    [Fact(DisplayName = nameof(ThrowWhenCastMemberNotFound))]
    [Trait("EndToEnd/Api", "UpdateCastMember - Endpoints")]
    public async Task ThrowWhenCastMemberNotFound()
    {
        await fixture.Persistence.InsertList(fixture.GetValidCastMembers());
        var invalidId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient.Put<ProblemDetails>(
            $"/castmembers/{invalidId}",
            fixture.GetValidInput(),
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

    [Theory(DisplayName = nameof(ThrowWhenNameIsInvalid))]
    [Trait("EndToEnd/Api", "UpdateCastMember - Endpoints")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ThrowWhenNameIsInvalid(string invalidName)
    {
        var castMember = fixture.GetValidCastMember();
        await fixture.Persistence.InsertList([castMember]);
        var input = new UpdateCastMemberApiInput(invalidName, fixture.GetRandomCastMemberType());

        var (response, output) = await fixture.ApiClient.Put<ProblemDetails>(
            $"/castmembers/{castMember.Id}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        output.Should().NotBeNull();
        output!.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        output.Title.Should().Be("One or more validation errors occurred");
        output.Type.Should().Be("UnprocessableEntity");
        output.Detail.Should().Be("Name should not be empty or null");
        var dbCastMember = await fixture.Persistence.GetById(castMember.Id);
        dbCastMember!.Name.Should().Be(castMember.Name);
    }
}
