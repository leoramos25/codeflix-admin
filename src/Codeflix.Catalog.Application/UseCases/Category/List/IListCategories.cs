using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Category.List;

public interface IListCategories : IRequestHandler<ListCategoriesInput, ListCategoriesOutput>;
