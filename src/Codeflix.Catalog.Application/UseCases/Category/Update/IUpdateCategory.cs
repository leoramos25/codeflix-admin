using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Category.Update;

public interface IUpdateCategory : IRequestHandler<UpdateCategoryInput, UpdateCategoryOutput>;
