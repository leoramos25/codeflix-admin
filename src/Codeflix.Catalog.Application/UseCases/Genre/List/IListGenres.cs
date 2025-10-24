using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.List;

public interface IListGenres : IRequestHandler<ListGenresInput, ListGenresOutput>;
