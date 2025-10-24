using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;

namespace Codeflix.Catalog.Application.Common;

public abstract class PaginatedListInput
{
    protected PaginatedListInput(int page, int perPage, string search, string sort, SearchOrder dir)
    {
        Page = page;
        PerPage = perPage;
        Search = search;
        Sort = sort;
        Dir = dir;
    }

    public int Page { get; set; }
    public int PerPage { get; set; }
    public string Search { get; set; }
    public string Sort { get; set; }
    public SearchOrder Dir { get; set; }

    public SearchInput ToSearchInput()
    {
        return new SearchInput(Page, PerPage, Search, Sort, Dir);
    }
}