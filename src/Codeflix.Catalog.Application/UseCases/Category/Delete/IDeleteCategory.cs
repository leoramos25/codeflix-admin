using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Category.Delete;

public interface IDeleteCategory : IRequestHandler<DeleteCategoryInput>;
