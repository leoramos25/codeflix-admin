using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Category.Get;

public interface IGetCategory : IRequestHandler<GetCategoryInput, GetCategoryOutput>;
