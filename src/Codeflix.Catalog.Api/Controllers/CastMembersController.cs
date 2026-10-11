using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Api.ApiModels.CastMember;
using Codeflix.Catalog.Application.UseCases.CastMember.Create;
using Codeflix.Catalog.Application.UseCases.CastMember.Delete;
using Codeflix.Catalog.Application.UseCases.CastMember.Get;
using Codeflix.Catalog.Application.UseCases.CastMember.List;
using Codeflix.Catalog.Application.UseCases.CastMember.Update;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class CastMembersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiOutput<CreateCastMemberOutput>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCastMemberApiInput input,
        CancellationToken cancellationToken
    )
    {
        var output = await mediator.Send(input.ToCreateCastMemberInput(), cancellationToken);
        return CreatedAtAction(
            nameof(Get),
            new { output.Id },
            new ApiOutput<CreateCastMemberOutput>(output)
        );
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiOutput<GetCastMemberOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var output = await mediator.Send(new GetCastMemberInput(id), cancellationToken);
        return Ok(new ApiOutput<GetCastMemberOutput>(output));
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiListOutput<ListCastMembersItemOutput>),
        StatusCodes.Status200OK
    )]
    public async Task<IActionResult> List(
        CancellationToken cancellationToken,
        [FromQuery] int? page = null,
        [FromQuery(Name = "per_page")] int? perPage = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        [FromQuery] SearchOrder? dir = null
    )
    {
        var input = new ListCastMembersInput();
        if (page is not null)
            input.Page = page.Value;
        if (perPage is not null)
            input.PerPage = perPage.Value;
        if (!string.IsNullOrWhiteSpace(search))
            input.Search = search;
        if (!string.IsNullOrWhiteSpace(sort))
            input.Sort = sort;
        if (dir is not null)
            input.Dir = dir.Value;
        var output = await mediator.Send(input, cancellationToken);
        return Ok(new ApiListOutput<ListCastMembersItemOutput>(output));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiOutput<UpdateCastMemberOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCastMemberApiInput input,
        CancellationToken cancellationToken
    )
    {
        var output = await mediator.Send(input.ToUpdateCastMemberInput(id), cancellationToken);
        return Ok(new ApiOutput<UpdateCastMemberOutput>(output));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(new DeleteCastMemberInput(id), cancellationToken);
        return NoContent();
    }
}
