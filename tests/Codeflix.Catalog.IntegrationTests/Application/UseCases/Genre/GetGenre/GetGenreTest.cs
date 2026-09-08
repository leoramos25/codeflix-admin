using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Infra.Data.EF.Models;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.Genre.Get;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.GetGenre;

[Collection(nameof(GetGenreTestFixture))]
public class GetGenreTest(GetGenreTestFixture fixture)
{
    [Fact(DisplayName = nameof(GetGenre))]
    [Trait("Integration/Application", "GetGenre - Use Cases")]
    public async Task GetGenre()
    {
        var dbContext = fixture.CreateDbContext();
        var genre = fixture.GetValidGenre();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var actDbContext = fixture.CreateDbContext(true);
        var useCase = new UseCase.GetGenre(
            new GenreRepository(actDbContext),
            new CategoryRepository(actDbContext)
        );
        var input = new UseCase.GetGenreInput(genre.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(genre.Id);
        output.Name.Should().Be(genre.Name);
        output.IsActive.Should().Be(genre.IsActive);
        output.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        output.Categories.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(GetGenreWithCategories))]
    [Trait("Integration/Application", "GetGenre - Use Cases")]
    public async Task GetGenreWithCategories()
    {
        var dbContext = fixture.CreateDbContext();
        var genre = fixture.GetValidGenre();
        var categories = fixture.GetValidCategories(5);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        var genreCategories = categories.Select(c => new GenresCategories(genre.Id, c.Id));
        await dbContext.GenresCategories.AddRangeAsync(genreCategories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var actDbContext = fixture.CreateDbContext(true);
        var useCase = new UseCase.GetGenre(
            new GenreRepository(actDbContext),
            new CategoryRepository(actDbContext)
        );
        var input = new UseCase.GetGenreInput(genre.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(genre.Id);
        output.Name.Should().Be(genre.Name);
        output.IsActive.Should().Be(genre.IsActive);
        output.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        output.Categories.Should().HaveCount(categories.Count);
        output
            .Categories.Select(category => category.Id)
            .Should()
            .BeEquivalentTo(categories.Select(c => c.Id));
        output
            .Categories.ToList()
            .ForEach(categoryOutput =>
            {
                var category = categories.Find(category => category.Id == categoryOutput.Id);
                category.Should().NotBeNull();
                categoryOutput.Name.Should().Be(category.Name);
            });
    }

    [Fact(DisplayName = nameof(ThrowExceptionWhenGenreNotFound))]
    [Trait("Integration/Application", "GetGenre - Use Cases")]
    public async Task ThrowExceptionWhenGenreNotFound()
    {
        var invalidId = Guid.NewGuid();
        var dbContext = fixture.CreateDbContext();
        var useCase = new UseCase.GetGenre(
            new GenreRepository(dbContext),
            new CategoryRepository(dbContext)
        );
        var input = new UseCase.GetGenreInput(invalidId);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Genre '{invalidId}' not found.");
    }
}
