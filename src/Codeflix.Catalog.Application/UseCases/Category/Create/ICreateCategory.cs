using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Category.Create;

public interface ICreateCategory : IRequestHandler<CreateCategoryInput, CreateCategoryOutput>;
