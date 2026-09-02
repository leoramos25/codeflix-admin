using Codeflix.Catalog.Application.Common;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.Application.UseCases.Genre.List;

public class ListGenresOutput : PaginatedListOutput<ListGenresItemOutput>
{
    public ListGenresOutput(
        int page,
        int perPage,
        int total,
        IReadOnlyList<ListGenresItemOutput> items
    )
        : base(page, perPage, total, items) { }

    public static ListGenresOutput FromSearchOutput(SearchOutput<DomainEntity.Genre> output) =>
        new(
            output.CurrentPage,
            output.PerPage,
            output.Total,
            output.Items.Select(ListGenresItemOutput.FromGenre).ToList()
        );

    public void FillCategoriesWithName(IReadOnlyCollection<DomainEntity.Category> categories)
    {
        foreach (var item in Items)
        {
            foreach (var categoryOutput in item.Categories)
            {
                categoryOutput.Name = categories?
                .FirstOrDefault(category => category.Id == categoryOutput.Id)
                ?.Name;
            }
        }
    }
}
