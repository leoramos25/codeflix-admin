using Codeflix.Catalog.Application.Common;

namespace Codeflix.Catalog.Api.ApiModels;

public class ApiListOutput<TItemData> : ApiOutput<IReadOnlyCollection<TItemData>>
    where TItemData : class
{
    public ApiMetaOutput Meta { get; private set; }

    public ApiListOutput(int currentPage, int perPage, int total, IReadOnlyList<TItemData> data)
        : base(data)
    {
        Meta = new ApiMetaOutput(currentPage, perPage, total);
    }

    public ApiListOutput(PaginatedListOutput<TItemData> paginatedListOutput)
        : base(paginatedListOutput.Items)
    {
        Meta = new ApiMetaOutput(
            paginatedListOutput.Page,
            paginatedListOutput.PerPage,
            paginatedListOutput.Total
        );
    }
}
