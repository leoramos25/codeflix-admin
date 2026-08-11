using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Application.UseCases.Genre.Delete;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Models;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.DeleteGenre;

[Collection(nameof(DeleteGenreTestFixture))]
public class DeleteGenreTest(DeleteGenreTestFixture fixture)
{
    [Fact(DisplayName = nameof(DeleteGenre))]
    [Trait("Integration/Application", "Delete Genre - Use Cases")]
    public async Task DeleteGenre()
    {
        var genre = fixture.GetValidGenre();
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var input = new DeleteGenreInput(genre.Id);
        var useCase = new Catalog.Application.UseCases.Genre.Delete.DeleteGenre(
            genreRepository,
            unitOfWork
        );

        await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = fixture.CreateDbContext(true);
        var deletedGenre = await assertDbContext.Genres.FindAsync(genre.Id, CancellationToken.None);
        deletedGenre.Should().BeNull();
    }

    [Fact(DisplayName = nameof(DeleteGenreWithRelationship))]
    [Trait("Integration/Application", "Delete Genre - Use Cases")]
    public async Task DeleteGenreWithRelationship()
    {
        var genre = fixture.GetValidGenre();
        var categories = fixture.GetValidCategories(5);
        var genreCategories = categories.Select(category => new GenresCategories(
            genre.Id,
            category.Id
        ));
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.GenresCategories.AddRangeAsync(genreCategories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var input = new DeleteGenreInput(genre.Id);
        var useCase = new Catalog.Application.UseCases.Genre.Delete.DeleteGenre(
            genreRepository,
            unitOfWork
        );

        await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = fixture.CreateDbContext(true);
        var deletedGenre = await assertDbContext.Genres.FindAsync(genre.Id, CancellationToken.None);
        var deletedGenresCategories = await assertDbContext
            .GenresCategories.Where(genreCategory => genreCategory.GenreId == genre.Id)
            .ToListAsync(CancellationToken.None);
        deletedGenre.Should().BeNull();
        deletedGenresCategories.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(DeleteGenreWithInvalidId))]
    [Trait("Integration/Application", "Delete Genre - Use Cases")]
    public async Task DeleteGenreWithInvalidId()
    {
        var genre = fixture.GetValidGenre();
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var input = new DeleteGenreInput(Guid.NewGuid());
        var useCase = new Catalog.Application.UseCases.Genre.Delete.DeleteGenre(
            genreRepository,
            unitOfWork
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>($"Genre '{input.Id}' not found.");
    }
}
