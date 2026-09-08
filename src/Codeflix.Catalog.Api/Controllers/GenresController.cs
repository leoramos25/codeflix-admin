using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Application.UseCases.Genre.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class GenresController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiOutput<GetGenreOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var output = await mediator.Send(new GetGenreInput(id), cancellationToken);
        return Ok(new ApiOutput<GetGenreOutput>(output));
    }
}
