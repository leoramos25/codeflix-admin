using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.GenreRepository;

[Collection(nameof(GenreRepositoryTestFixture))]
public class GenreRepositoryTest(GenreRepositoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(InsertGenre))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task InsertGenre()
    {
        var context = fixture.CreateDbContext();
        var genre = fixture.GetValidGenre();
        var categories = fixture.GetValidCategories(3);
        await context.Categories.AddRangeAsync(categories, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        categories.ForEach(category => genre.AddCategory(category.Id));
        var repository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(context);

        await repository.Insert(genre, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        var assertDbContext = fixture.CreateDbContext(true);
        var persistedGenre = await assertDbContext.Genres.FindAsync(
            [genre.Id],
            CancellationToken.None
        );
        var genresCategories = await context
            .GenresCategories.Where(genreCategory => genreCategory.GenreId == genre.Id)
            .ToListAsync(CancellationToken.None);

        persistedGenre.Should().NotBeNull();
        persistedGenre.Id.Should().Be(genre.Id);
        persistedGenre.Name.Should().Be(genre.Name);
        persistedGenre.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        genresCategories.Should().HaveCount(categories.Count);
        genresCategories.Select(x => x.CategoryId).Should().BeEquivalentTo(genre.Categories);
    }

    [Fact(DisplayName = nameof(GetGenre))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task GetGenre()
    {
        var context = fixture.CreateDbContext();
        var genre = fixture.GetValidGenre();
        var categories = fixture.GetValidCategories(3);
        await context.Categories.AddRangeAsync(categories, CancellationToken.None);
        await context.Genres.AddAsync(genre, CancellationToken.None);
        var genreCategories = categories.Select(category => new GenresCategories(
            genre.Id,
            category.Id
        ));
        await context.GenresCategories.AddRangeAsync(genreCategories, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        var repository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(
            fixture.CreateDbContext(true)
        );

        var persistedGenre = await repository.Get(genre.Id, CancellationToken.None);

        persistedGenre.Should().NotBeNull();
        persistedGenre.Id.Should().Be(genre.Id);
        persistedGenre.Name.Should().Be(genre.Name);
        persistedGenre.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        persistedGenre.Categories.Should().HaveCount(categories.Count);
        persistedGenre
            .Categories.Should()
            .BeEquivalentTo(categories.Select(category => category.Id));
    }

    [Fact(DisplayName = nameof(GetGenreWithoutCategories))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task GetGenreWithoutCategories()
    {
        var context = fixture.CreateDbContext();
        var genre = fixture.GetValidGenre();
        await context.Genres.AddAsync(genre, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        var repository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(
            fixture.CreateDbContext(true)
        );

        var persistedGenre = await repository.Get(genre.Id, CancellationToken.None);

        persistedGenre.Should().NotBeNull();
        persistedGenre.Id.Should().Be(genre.Id);
        persistedGenre.Name.Should().Be(genre.Name);
        persistedGenre.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        persistedGenre.Categories.Should().HaveCount(0);
        persistedGenre.Categories.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(GetGenreShouldThrowExceptionWhenGenreNotFound))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task GetGenreShouldThrowExceptionWhenGenreNotFound()
    {
        var invalidId = Guid.NewGuid();
        var context = fixture.CreateDbContext();
        var repository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(context);

        var action = () => repository.Get(invalidId, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Genre '{invalidId}' not found");
    }

    [Fact(DisplayName = nameof(DeleteGenre))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task DeleteGenre()
    {
        var context = fixture.CreateDbContext();
        var genre = fixture.GetValidGenre();
        var categories = fixture.GetValidCategories(3);
        await context.Categories.AddRangeAsync(categories, CancellationToken.None);
        await context.Genres.AddAsync(genre, CancellationToken.None);
        var genreCategories = categories.Select(category => new GenresCategories(
            genre.Id,
            category.Id
        ));
        await context.GenresCategories.AddRangeAsync(genreCategories, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        var repository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(context);

        await repository.Delete(genre, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        var assertDbContext = fixture.CreateDbContext(true);
        var persistedGenre = await assertDbContext.Genres.FindAsync(
            [genre.Id],
            CancellationToken.None
        );
        var persistedGenresCategories = await assertDbContext
            .GenresCategories.Where(relation => relation.GenreId == genre.Id)
            .ToListAsync(CancellationToken.None);

        persistedGenre.Should().BeNull();
        persistedGenresCategories.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(UpdateGenre))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task UpdateGenre()
    {
        var context = fixture.CreateDbContext();
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(context);
        var genre = fixture.GetValidGenre();
        var data = new { Name = fixture.GetValidName() };
        var categories = fixture.GetValidCategories(3);
        var genreCategories = categories
            .Select(category => new GenresCategories(genre.Id, category.Id))
            .ToList();
        await context.AddAsync(genre, CancellationToken.None);
        await context.Categories.AddRangeAsync(categories, CancellationToken.None);
        await context.GenresCategories.AddRangeAsync(genreCategories, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        genre.Update(data.Name);
        if (genre.IsActive)
            genre.Deactivate();
        else
            genre.Activate();

        await genreRepository.Update(genre, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        var assertDbContext = fixture.CreateDbContext(true);
        var persistedGenre = await assertDbContext.Genres.FindAsync(
            [genre.Id],
            CancellationToken.None
        );
        var persistedGenresCategories = await assertDbContext
            .GenresCategories.Where(relation => relation.GenreId == genre.Id)
            .ToListAsync(CancellationToken.None);

        persistedGenre.Should().NotBeNull();
        persistedGenre.Name.Should().Be(data.Name);
        persistedGenre.IsActive.Should().Be(genre.IsActive);
        persistedGenre.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        persistedGenresCategories.Should().HaveCount(genreCategories.Count);
    }
}