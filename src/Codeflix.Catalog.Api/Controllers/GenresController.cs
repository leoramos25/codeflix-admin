using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Api.ApiModels.Genre;
using Codeflix.Catalog.Application.UseCases.Genre.Create;
using Codeflix.Catalog.Application.UseCases.Genre.Delete;
using Codeflix.Catalog.Application.UseCases.Genre.Get;
using Codeflix.Catalog.Application.UseCases.Genre.List;
using Codeflix.Catalog.Application.UseCases.Genre.Update;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class GenresController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiOutput<CreateGenreOutput>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGenreInput input,
        CancellationToken cancellationToken
    )
    {
        var output = await mediator.Send(input, cancellationToken);
        return CreatedAtAction(
            nameof(Get),
            new { output.Id },
            new ApiOutput<CreateGenreOutput>(output)
        );
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiOutput<GetGenreOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var output = await mediator.Send(new GetGenreInput(id), cancellationToken);
        return Ok(new ApiOutput<GetGenreOutput>(output));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(new DeleteGenreInput(id), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiOutput<UpdateGenreOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateGenreApiInput input,
        CancellationToken cancellationToken
    )
    {
        var output = await mediator.Send(input.ToUpdateGenreInput(id), cancellationToken);
        return Ok(new ApiOutput<UpdateGenreOutput>(output));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiListOutput<ListGenresItemOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        CancellationToken cancellationToken,
        [FromQuery] int? page = null,
        [FromQuery(Name = "per_page")] int? perPage = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        [FromQuery] SearchOrder? dir = null
    )
    {
        var input = new ListGenresInput();
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
        return Ok(new ApiListOutput<ListGenresItemOutput>(output));
    }
}
