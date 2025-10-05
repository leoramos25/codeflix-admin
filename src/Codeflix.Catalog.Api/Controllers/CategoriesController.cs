using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Api.ApiModels.Category;
using Codeflix.Catalog.Application.UseCases.Category.Create;
using Codeflix.Catalog.Application.UseCases.Category.Delete;
using Codeflix.Catalog.Application.UseCases.Category.Get;
using Codeflix.Catalog.Application.UseCases.Category.List;
using Codeflix.Catalog.Application.UseCases.Category.Update;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiOutput<CreateCategoryOutput>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryInput input,
        CancellationToken cancellationToken
    )
    {
        var output = await mediator.Send(input, cancellationToken);
        return CreatedAtAction(
            nameof(Get),
            new { output.Id },
            new ApiOutput<CreateCategoryOutput>(output)
        );
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiOutput<GetCategoryOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var output = await mediator.Send(new GetCategoryInput(id), cancellationToken);
        return Ok(new ApiOutput<GetCategoryOutput>(output));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(new DeleteCategoryInput(id), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiOutput<UpdateCategoryOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCategoryApiInput input,
        CancellationToken cancellationToken
    )
    {
        var output = await mediator.Send(input.ToUpdateCategoryInput(id), cancellationToken);
        return Ok(new ApiOutput<UpdateCategoryOutput>(output));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiListOutput<ListCategoriesOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        CancellationToken cancellationToken,
        [FromQuery] int? page = null,
        [FromQuery(Name = "per_page")] int? perPage = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        [FromQuery] SearchOrder? dir = null
    )
    {
        var input = new ListCategoriesInput();
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
        return Ok(new ApiListOutput<ListCategoriesItemOutput>(output));
    }
}
