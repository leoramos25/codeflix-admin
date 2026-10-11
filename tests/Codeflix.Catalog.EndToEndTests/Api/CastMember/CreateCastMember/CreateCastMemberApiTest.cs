using System.Net;
using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Api.ApiModels.CastMember;
using Codeflix.Catalog.Application.UseCases.CastMember.Create;
using Codeflix.Catalog.Domain.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.CreateCastMember;

[Collection(nameof(CreateCastMemberApiTestFixture))]
public class CreateCastMemberApiTest(CreateCastMemberApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(CreateCastMember))]
    [Trait("EndToEnd/Api", "CreateCastMember - Endpoints")]
    public async Task CreateCastMember()
    {
        var input = fixture.GetValidInput();

        var (response, output) = await fixture.ApiClient.Post<ApiOutput<CreateCastMemberOutput>>(
            "/castmembers",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        output.Should().NotBeNull();
        output!.Data.Id.Should().NotBeEmpty();
        output.Data.Name.Should().Be(input.Name);
        output.Data.Type.Should().Be(input.Type);
        output.Data.CreatedAt.Should().NotBeSameDateAs(default);

        var dbCastMember = await fixture.Persistence.GetById(output.Data.Id);
        dbCastMember.Should().NotBeNull();
        dbCastMember!.Name.Should().Be(input.Name);
        dbCastMember.Type.Should().Be(input.Type);
    }

    [Theory(DisplayName = nameof(CreateCastMemberWithTypeAsCaseInsensitiveString))]
    [Trait("EndToEnd/Api", "CreateCastMember - Endpoints")]
    [InlineData("Actor", CastMemberType.Actor)]
    [InlineData("actor", CastMemberType.Actor)]
    [InlineData("DIRECTOR", CastMemberType.Director)]
    [InlineData("dIrEcToR", CastMemberType.Director)]
    public async Task CreateCastMemberWithTypeAsCaseInsensitiveString(
        string type,
        CastMemberType expectedType
    )
    {
        var name = fixture.GetValidName();

        var (response, output) = await fixture.ApiClient.Post<ApiOutput<CreateCastMemberOutput>>(
            "/castmembers",
            new { name, type },
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        output.Should().NotBeNull();
        output!.Data.Type.Should().Be(expectedType);
        var dbCastMember = await fixture.Persistence.GetById(output.Data.Id);
        dbCastMember.Should().NotBeNull();
        dbCastMember!.Name.Should().Be(name);
        dbCastMember.Type.Should().Be(expectedType);
    }

    [Theory(DisplayName = nameof(CreateCastMemberWithTypeAsInteger))]
    [Trait("EndToEnd/Api", "CreateCastMember - Endpoints")]
    [InlineData(1, CastMemberType.Director)]
    [InlineData(2, CastMemberType.Actor)]
    public async Task CreateCastMemberWithTypeAsInteger(int type, CastMemberType expectedType)
    {
        var name = fixture.GetValidName();

        var (response, output) = await fixture.ApiClient.Post<ApiOutput<CreateCastMemberOutput>>(
            "/castmembers",
            new { name, type },
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        output.Should().NotBeNull();
        output!.Data.Type.Should().Be(expectedType);
        var dbCastMember = await fixture.Persistence.GetById(output.Data.Id);
        dbCastMember.Should().NotBeNull();
        dbCastMember!.Name.Should().Be(name);
        dbCastMember.Type.Should().Be(expectedType);
    }

    [Theory(DisplayName = nameof(ThrowWhenTypeIsInvalid))]
    [Trait("EndToEnd/Api", "CreateCastMember - Endpoints")]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData("1")]
    [InlineData("Producer")]
    [InlineData("")]
    public async Task ThrowWhenTypeIsInvalid(object invalidType)
    {
        var (response, output) = await fixture.ApiClient.Post<ValidationProblemDetails>(
            "/castmembers",
            new { name = fixture.GetValidName(), type = invalidType },
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        output.Should().NotBeNull();
        output!.Status.Should().Be(StatusCodes.Status400BadRequest);
        output.Errors.Keys.Should().Contain("$.type");
    }

    [Theory(DisplayName = nameof(ThrowWhenNameIsInvalid))]
    [Trait("EndToEnd/Api", "CreateCastMember - Endpoints")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ThrowWhenNameIsInvalid(string invalidName)
    {
        var input = new CreateCastMemberApiInput(invalidName, fixture.GetRandomCastMemberType());

        var (response, output) = await fixture.ApiClient.Post<ProblemDetails>(
            "/castmembers",
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
    }
}
