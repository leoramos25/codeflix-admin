using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
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
            .WithMessage($"Genre '{invalidId}' not found.");
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
        categories.ForEach(x => genre.AddCategory(x.Id));
        await context.AddAsync(genre, CancellationToken.None);
        await context.Categories.AddRangeAsync(categories, CancellationToken.None);
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
        persistedGenresCategories.Should().HaveCount(genre.Categories.Count);
    }

    [Fact(DisplayName = nameof(UpdateGenreRemovingRelations))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task UpdateGenreRemovingRelations()
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
        genre.RemoveAllCategories();

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
        persistedGenresCategories.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(UpdateGenreReplacingRelations))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task UpdateGenreReplacingRelations()
    {
        var context = fixture.CreateDbContext();
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(context);
        var genre = fixture.GetValidGenre();
        var data = new { Name = fixture.GetValidName() };
        var categories = fixture.GetValidCategories(3);
        var newCategories = fixture.GetValidCategories(5);
        var genreCategories = categories
            .Select(category => new GenresCategories(genre.Id, category.Id))
            .ToList();
        await context.AddAsync(genre, CancellationToken.None);
        await context.Categories.AddRangeAsync(categories, CancellationToken.None);
        await context.Categories.AddRangeAsync(newCategories, CancellationToken.None);
        await context.GenresCategories.AddRangeAsync(genreCategories, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        genre.Update(data.Name);
        if (genre.IsActive)
            genre.Deactivate();
        else
            genre.Activate();
        genre.RemoveAllCategories();
        newCategories.ForEach(category => genre.AddCategory(category.Id));

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
        persistedGenresCategories.Should().HaveCount(newCategories.Count);
        persistedGenresCategories.ForEach(relation =>
        {
            var expectedCategory = newCategories.FirstOrDefault(c => c.Id == relation.CategoryId);
            expectedCategory.Should().NotBeNull();
        });
    }

    [Fact(DisplayName = nameof(SearchGenresAndTotal))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task SearchGenresAndTotal()
    {
        var context = fixture.CreateDbContext();
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(context);
        var genres = fixture.GetValidGenres();
        await context.Genres.AddRangeAsync(genres, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);

        var result = await genreRepository.Search(searchInput, CancellationToken.None);

        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(searchInput.Page);
        result.PerPage.Should().Be(searchInput.PerPage);
        result.Total.Should().Be(result.Total);
        foreach (var item in result.Items)
        {
            var expectedGenre = genres.FirstOrDefault(g => g.Id == item.Id);
            expectedGenre.Should().NotBeNull();
            expectedGenre.Name.Should().Be(item.Name);
            expectedGenre.IsActive.Should().Be(item.IsActive);
            expectedGenre.CreatedAt.Should().BeSameDateAs(item.CreatedAt);
        }
    }

    [Fact(DisplayName = nameof(SearchGenresAndTotalWithRelations))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task SearchGenresAndTotalWithRelations()
    {
        var context = fixture.CreateDbContext();
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(
            fixture.CreateDbContext(true)
        );
        var genres = fixture.GetValidGenres();
        var random = new Random();
        foreach (var genre in genres)
        {
            var categories = fixture.GetValidCategories(random.Next(1, 10));
            if (categories.Count > 0)
            {
                categories.ForEach(category => genre.AddCategory(category.Id));
                var genreCategories = categories
                    .Select(category => new GenresCategories(genre.Id, category.Id))
                    .ToList();
                await context.Genres.AddRangeAsync(genres, CancellationToken.None);
                await context.GenresCategories.AddRangeAsync(
                    genreCategories,
                    CancellationToken.None
                );
            }
        }

        await context.Genres.AddRangeAsync(genres, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);

        var result = await genreRepository.Search(searchInput, CancellationToken.None);

        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(searchInput.Page);
        result.PerPage.Should().Be(searchInput.PerPage);
        result.Total.Should().Be(result.Total);
        foreach (var item in result.Items)
        {
            var expectedGenre = genres.FirstOrDefault(g => g.Id == item.Id);
            expectedGenre.Should().NotBeNull();
            expectedGenre.Name.Should().Be(item.Name);
            expectedGenre.IsActive.Should().Be(item.IsActive);
            expectedGenre.CreatedAt.Should().BeSameDateAs(item.CreatedAt);
            expectedGenre.Categories.Should().HaveCount(item.Categories.Count);
            expectedGenre.Categories.Should().BeEquivalentTo(item.Categories);
        }
    }

    [Fact(DisplayName = nameof(SearchGenresShouldReturnEmpty))]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task SearchGenresShouldReturnEmpty()
    {
        var context = fixture.CreateDbContext();
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(context);
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);

        var result = await genreRepository.Search(searchInput, CancellationToken.None);

        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(searchInput.Page);
        result.PerPage.Should().Be(searchInput.PerPage);
        result.Total.Should().Be(result.Total);
        result.Items.Should().HaveCount(0);
        result.Items.Should().BeEmpty();
    }

    [Theory(DisplayName = nameof(SearchGenresPaginated))]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    [Trait("Integration/Infra.Data", "GenreRepository - Repositories")]
    public async Task SearchGenresPaginated(
        int quantityToGenerate,
        int page,
        int perPage,
        int expectedItems
    )
    {
        var context = fixture.CreateDbContext();
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(
            fixture.CreateDbContext(true)
        );
        var genres = fixture.GetValidGenres(quantityToGenerate);
        var random = new Random();
        foreach (var genre in genres)
        {
            var categories = fixture.GetValidCategories(random.Next(1, 10));
            if (categories.Count > 0)
            {
                categories.ForEach(category => genre.AddCategory(category.Id));
                var genreCategories = categories
                    .Select(category => new GenresCategories(genre.Id, category.Id))
                    .ToList();
                await context.Genres.AddRangeAsync(genres, CancellationToken.None);
                await context.GenresCategories.AddRangeAsync(
                    genreCategories,
                    CancellationToken.None
                );
            }
        }

        await context.Genres.AddRangeAsync(genres, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        var searchInput = new SearchInput(page, perPage, "", "", SearchOrder.Asc);

        var result = await genreRepository.Search(searchInput, CancellationToken.None);

        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(searchInput.Page);
        result.PerPage.Should().Be(searchInput.PerPage);
        result.Total.Should().Be(result.Total);
        result.Items.Should().HaveCount(expectedItems);
        foreach (var item in result.Items)
        {
            var expectedGenre = genres.FirstOrDefault(g => g.Id == item.Id);
            expectedGenre.Should().NotBeNull();
            expectedGenre.Name.Should().Be(item.Name);
            expectedGenre.IsActive.Should().Be(item.IsActive);
            expectedGenre.CreatedAt.Should().BeSameDateAs(item.CreatedAt);
            expectedGenre.Categories.Should().HaveCount(item.Categories.Count);
            expectedGenre.Categories.Should().BeEquivalentTo(item.Categories);
        }
    }

    [Theory(DisplayName = nameof(SearchGenresByText))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    [InlineData("Action", 1, 5, 1, 1)]
    [InlineData("Horror", 1, 5, 3, 3)]
    [InlineData("Horror", 2, 5, 0, 3)]
    [InlineData("Sci-Fi", 1, 5, 4, 4)]
    [InlineData("Sci-Fi", 1, 2, 2, 4)]
    [InlineData("Sci-Fi", 2, 2, 2, 4)]
    [InlineData("Sci-Fi", 2, 3, 1, 4)]
    [InlineData("Other", 1, 3, 0, 0)]
    [InlineData("Robot", 1, 5, 2, 2)]
    public async Task SearchGenresByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityReturn,
        int expectTotalItems
    )
    {
        var context = fixture.CreateDbContext();
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(
            fixture.CreateDbContext(true)
        );
        var genres = fixture.GetValidGenresWithNames([
            "Action",
            "Horror",
            "Horror - Robots",
            "Horror - Based On Real Facts",
            "Drama",
            "Sci-Fi IA",
            "Sci-Fi Robots",
            "Sci-Fi Space",
            "Sci-Fi Future",
        ]);
        var random = new Random();
        foreach (var genre in genres)
        {
            var categories = fixture.GetValidCategories(random.Next(1, 10));
            if (categories.Count > 0)
            {
                categories.ForEach(category => genre.AddCategory(category.Id));
                var genreCategories = categories
                    .Select(category => new GenresCategories(genre.Id, category.Id))
                    .ToList();
                await context.Genres.AddRangeAsync(genres, CancellationToken.None);
                await context.GenresCategories.AddRangeAsync(
                    genreCategories,
                    CancellationToken.None
                );
            }
        }

        await context.Genres.AddRangeAsync(genres, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        var searchInput = new SearchInput(page, perPage, search, "", SearchOrder.Asc);

        var result = await genreRepository.Search(searchInput, CancellationToken.None);

        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(searchInput.Page);
        result.PerPage.Should().Be(searchInput.PerPage);
        result.Total.Should().Be(expectTotalItems);
        result.Items.Should().HaveCount(expectedQuantityReturn);
        foreach (var item in result.Items)
        {
            var expectedGenre = genres.FirstOrDefault(g => g.Id == item.Id);
            expectedGenre.Should().NotBeNull();
            expectedGenre.Name.Should().Be(item.Name);
            expectedGenre.IsActive.Should().Be(item.IsActive);
            expectedGenre.CreatedAt.Should().BeSameDateAs(item.CreatedAt);
            expectedGenre.Categories.Should().HaveCount(item.Categories.Count);
            expectedGenre.Categories.Should().BeEquivalentTo(item.Categories);
        }
    }

    [Theory(DisplayName = nameof(SearchGenresByText))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    public async Task SearchGenresOrdered(string orderBy, string order)
    {
        var context = fixture.CreateDbContext();
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(
            fixture.CreateDbContext(true)
        );
        var genres = fixture.GetValidGenresWithNames([
            "Action",
            "Horror",
            "Horror - Robots",
            "Horror - Based On Real Facts",
            "Drama",
            "Sci-Fi IA",
            "Sci-Fi Robots",
            "Sci-Fi Space",
            "Sci-Fi Future",
        ]);
        var random = new Random();
        foreach (var genre in genres)
        {
            var categories = fixture.GetValidCategories(random.Next(1, 10));
            if (categories.Count > 0)
            {
                categories.ForEach(category => genre.AddCategory(category.Id));
                var genreCategories = categories
                    .Select(category => new GenresCategories(genre.Id, category.Id))
                    .ToList();
                await context.Genres.AddRangeAsync(genres, CancellationToken.None);
                await context.GenresCategories.AddRangeAsync(
                    genreCategories,
                    CancellationToken.None
                );
            }
        }
        await context.Genres.AddRangeAsync(genres, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        var searchOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var searchInput = new SearchInput(1, 20, "", orderBy, searchOrder);

        var result = await genreRepository.Search(searchInput, CancellationToken.None);

        var expectedList = fixture.GetOrderedGenres(genres, orderBy, searchOrder);
        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(searchInput.Page);
        result.PerPage.Should().Be(searchInput.PerPage);
        result.Total.Should().Be(genres.Count);
        for (var index = 0; index < expectedList.Count; index++)
        {
            var expectedItem = expectedList[index];
            var outputItem = result.Items[index];
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }
}
