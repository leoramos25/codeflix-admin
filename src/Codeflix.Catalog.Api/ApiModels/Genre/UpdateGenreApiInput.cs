using Codeflix.Catalog.Application.UseCases.Genre.Update;

namespace Codeflix.Catalog.Api.ApiModels.Genre;

public class UpdateGenreApiInput
{
    public UpdateGenreApiInput(string name, bool? isActive = null, List<Guid>? categories = null)
    {
        Name = name;
        IsActive = isActive;
        Categories = categories;
    }

    public string Name { get; set; }
    public bool? IsActive { get; set; }
    public List<Guid>? Categories { get; set; }

    public UpdateGenreInput ToUpdateGenreInput(Guid id)
    {
        return new UpdateGenreInput(id, Name, IsActive, Categories);
    }
}
