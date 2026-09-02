using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.Infra.Data.EF.Models;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.Genre.List;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.ListGenres;

[Collection(nameof(ListGenresTestFixture))]
public class ListGenresTest(ListGenresTestFixture fixture)
{
    [Fact(DisplayName = nameof(ListGenres))]
    [Trait("Integration/Application", "List Genres - Use Cases")]
    public async Task ListGenres()
    {
        var genres = fixture.GetValidGenres();
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddRangeAsync(genres, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListGenres(genreRepository, categoryRepository);
        var input = new UseCase.ListGenresInput(1, 20, "", "", SearchOrder.Asc);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(genres.Count);
        output.Items.Should().HaveCount(genres.Count);
        output
            .Items.ToList()
            .ForEach(item =>
            {
                var genre = genres.FirstOrDefault(genre => genre.Id == item.Id);
                genre.Should().NotBeNull();
                genre.Name.Should().Be(item.Name);
                genre.IsActive.Should().Be(item.IsActive);
                genre.Categories.Should().BeEquivalentTo(item.Categories);
                genre.CreatedAt.Should().BeSameDateAs(item.CreatedAt);
            });
    }

    [Fact(DisplayName = nameof(ListGenresReturnsEmptyWhenPersistenceIsEmpty))]
    [Trait("Integration/Application", "List Genres - Use Cases")]
    public async Task ListGenresReturnsEmptyWhenPersistenceIsEmpty()
    {
        var dbContext = fixture.CreateDbContext();
        var genreRepository = new GenreRepository(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListGenres(genreRepository, categoryRepository);
        var input = new UseCase.ListGenresInput(1, 20, "", "", SearchOrder.Asc);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(ListGenresWithCategoryRelationship))]
    [Trait("Integration/Application", "List Genres - Use Cases")]
    public async Task ListGenresWithCategoryRelationship()
    {
        var genres = fixture.GetValidGenres(10);
        var categories = fixture.GetValidCategories(10);
        var random = new Random();
        genres.ForEach(genre =>
        {
            var relationsCount = random.Next(0, 3);
            for (int i = 0; i < relationsCount; i++)
            {
                var selectedCategory = categories[random.Next(0, categories.Count - 1)];
                if (!genre.Categories.Contains(selectedCategory.Id))
                    genre.AddCategory(selectedCategory.Id);
            }
        });
        var dbContext = fixture.CreateDbContext();
        var genresCategories = new List<GenresCategories>();
        genres.ForEach(genre =>
        {
            var categories = genre.Categories.ToList();

            genresCategories.AddRange(
                categories.Select(category => new GenresCategories(genre.Id, category))
            );
        });
        await dbContext.Genres.AddRangeAsync(genres, CancellationToken.None);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.GenresCategories.AddRangeAsync(genresCategories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListGenres(genreRepository, categoryRepository);
        var input = new UseCase.ListGenresInput(1, 20, "", "", SearchOrder.Asc);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(genres.Count);
        output.Items.Should().HaveCount(genres.Count);
        output
            .Items.ToList()
            .ForEach(item =>
            {
                var genre = genres.FirstOrDefault(genre => genre.Id == item.Id);
                genre.Should().NotBeNull();
                genre.Name.Should().Be(item.Name);
                genre.IsActive.Should().Be(item.IsActive);
                genre
                    .Categories.Should()
                    .BeEquivalentTo(item.Categories.Select(category => category.Id));
                genre.CreatedAt.Should().BeSameDateAs(item.CreatedAt);
                var categoryIds = genresCategories
                    .Where(genreCategory => genreCategory.GenreId == genre.Id)
                    .Select(genreCategory => genreCategory.CategoryId);
                genre.Categories.Should().BeEquivalentTo(categoryIds);
                item.Categories.ToList()
                    .ForEach(categoryItem =>
                    {
                        var category = categories.Find(cat => cat.Id == categoryItem.Id);
                        category.Should().NotBeNull();
                        category.Name.Should().Be(categoryItem.Name);
                    });
            });
    }

    [Theory(DisplayName = nameof(LsitGenresPaginated))]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    [Trait("Integration/Application", "List Genres - Use Cases")]
    public async Task LsitGenresPaginated(
        int quantityToGenerate,
        int page,
        int perPage,
        int expectedItems
    )
    {
        var genres = fixture.GetValidGenres(quantityToGenerate);
        var categories = fixture.GetValidCategories(10);
        var random = new Random();
        genres.ForEach(genre =>
        {
            var relationsCount = random.Next(0, 3);
            for (int i = 0; i < relationsCount; i++)
            {
                var selectedCategory = categories[random.Next(0, categories.Count - 1)];
                if (!genre.Categories.Contains(selectedCategory.Id))
                    genre.AddCategory(selectedCategory.Id);
            }
        });
        var dbContext = fixture.CreateDbContext();
        var genresCategories = new List<GenresCategories>();
        genres.ForEach(genre =>
        {
            var categories = genre.Categories.ToList();

            genresCategories.AddRange(
                categories.Select(category => new GenresCategories(genre.Id, category))
            );
        });
        await dbContext.Genres.AddRangeAsync(genres, CancellationToken.None);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.GenresCategories.AddRangeAsync(genresCategories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListGenres(genreRepository, categoryRepository);
        var input = new UseCase.ListGenresInput(page, perPage, "", "", SearchOrder.Asc);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(genres.Count);
        output.Items.Should().HaveCount(expectedItems);
        output
            .Items.ToList()
            .ForEach(item =>
            {
                var genre = genres.FirstOrDefault(genre => genre.Id == item.Id);
                genre.Should().NotBeNull();
                genre.Name.Should().Be(item.Name);
                genre.IsActive.Should().Be(item.IsActive);
                genre
                    .Categories.Should()
                    .BeEquivalentTo(item.Categories.Select(category => category.Id));
                genre.CreatedAt.Should().BeSameDateAs(item.CreatedAt);
                var categoryIds = genresCategories
                    .Where(genreCategory => genreCategory.GenreId == genre.Id)
                    .Select(genreCategory => genreCategory.CategoryId);
                genre.Categories.Should().BeEquivalentTo(categoryIds);
                item.Categories.ToList()
                    .ForEach(categoryItem =>
                    {
                        var category = categories.Find(cat => cat.Id == categoryItem.Id);
                        category.Should().NotBeNull();
                        category.Name.Should().Be(categoryItem.Name);
                    });
            });
    }

    [Theory(DisplayName = nameof(ListGenresByText))]
    [InlineData("Action", 1, 5, 1, 1)]
    [InlineData("Horror", 1, 5, 3, 3)]
    [InlineData("Horror", 2, 5, 0, 3)]
    [InlineData("Sci-Fi", 1, 5, 4, 4)]
    [InlineData("Sci-Fi", 1, 2, 2, 4)]
    [InlineData("Sci-Fi", 2, 2, 2, 4)]
    [InlineData("Sci-Fi", 2, 3, 1, 4)]
    [InlineData("Other", 1, 3, 0, 0)]
    [InlineData("Robot", 1, 5, 2, 2)]
    [Trait("Integration/Application", "List Genres - Use Cases")]
    public async Task ListGenresByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityReturn,
        int expectTotalItems
    )
    {
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
        var categories = fixture.GetValidCategories(10);
        var random = new Random();
        genres.ForEach(genre =>
        {
            var relationsCount = random.Next(0, 3);
            for (int i = 0; i < relationsCount; i++)
            {
                var selectedCategory = categories[random.Next(0, categories.Count - 1)];
                if (!genre.Categories.Contains(selectedCategory.Id))
                    genre.AddCategory(selectedCategory.Id);
            }
        });
        var dbContext = fixture.CreateDbContext();
        var genresCategories = new List<GenresCategories>();
        genres.ForEach(genre =>
        {
            var categories = genre.Categories.ToList();

            genresCategories.AddRange(
                categories.Select(category => new GenresCategories(genre.Id, category))
            );
        });
        await dbContext.Genres.AddRangeAsync(genres, CancellationToken.None);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.GenresCategories.AddRangeAsync(genresCategories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListGenres(genreRepository, categoryRepository);
        var input = new UseCase.ListGenresInput(page, perPage, search, "", SearchOrder.Asc);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(expectTotalItems);
        output.Items.Should().HaveCount(expectedQuantityReturn);
        output
            .Items.ToList()
            .ForEach(item =>
            {
                var genre = genres.FirstOrDefault(genre => genre.Id == item.Id);
                genre.Should().NotBeNull();
                genre.Name.Should().Be(item.Name);
                genre.IsActive.Should().Be(item.IsActive);
                genre
                    .Categories.Should()
                    .BeEquivalentTo(item.Categories.Select(category => category.Id));
                genre.CreatedAt.Should().BeSameDateAs(item.CreatedAt);
                var categoryIds = genresCategories
                    .Where(genreCategory => genreCategory.GenreId == genre.Id)
                    .Select(genreCategory => genreCategory.CategoryId);
                genre.Categories.Should().BeEquivalentTo(categoryIds);
                item.Categories.ToList()
                    .ForEach(categoryItem =>
                    {
                        var category = categories.Find(cat => cat.Id == categoryItem.Id);
                        category.Should().NotBeNull();
                        category.Name.Should().Be(categoryItem.Name);
                    });
            });
    }

    [Theory(DisplayName = nameof(ListGenresOrdered))]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [Trait("Integration/Application", "List Genres - Use Cases")]
    public async Task ListGenresOrdered(string orderBy, string order)
    {
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
        var categories = fixture.GetValidCategories(10);
        var random = new Random();
        genres.ForEach(genre =>
        {
            var relationsCount = random.Next(0, 3);
            for (int i = 0; i < relationsCount; i++)
            {
                var selectedCategory = categories[random.Next(0, categories.Count - 1)];
                if (!genre.Categories.Contains(selectedCategory.Id))
                    genre.AddCategory(selectedCategory.Id);
            }
        });
        var dbContext = fixture.CreateDbContext();
        var genresCategories = new List<GenresCategories>();
        genres.ForEach(genre =>
        {
            var categories = genre.Categories.ToList();

            genresCategories.AddRange(
                categories.Select(category => new GenresCategories(genre.Id, category))
            );
        });
        await dbContext.Genres.AddRangeAsync(genres, CancellationToken.None);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.GenresCategories.AddRangeAsync(genresCategories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListGenres(genreRepository, categoryRepository);
        var searchOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new UseCase.ListGenresInput(1, 20, "", orderBy, searchOrder);

        var output = await useCase.Handle(input, CancellationToken.None);

        var expectedList = fixture.GetOrderedGenres(genres, orderBy, searchOrder);
        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(genres.Count);
        for (var index = 0; index < expectedList.Count; index++)
        {
            var expectedItem = expectedList[index];
            var outputItem = output.Items[index];
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }
}
