using Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.Infra.Data.EF.Models;

public class GenresCategories(Guid genreId, Guid categoryId)
{
    public Guid GenreId { get; set; } = genreId;
    public Guid CategoryId { get; set; } = categoryId;
    public Genre? Genre { get; set; }
    public Category? Category { get; set; }
}
