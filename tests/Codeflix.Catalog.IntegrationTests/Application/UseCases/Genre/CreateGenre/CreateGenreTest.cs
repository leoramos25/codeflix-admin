using Codeflix.Catalog.Domain.Exceptions;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UseCase = Codeflix.Catalog.Application.UseCases.Genre.Create;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.CreateGenre;

[Collection(nameof(CreateGenreTestFixture))]
public class CreateGenreTest(CreateGenreTestFixture fixture)
{
    [Fact(DisplayName = nameof(CreateGenre))]
    [Trait("Integration/Application", "Create Genre - Use Cases")]
    public async Task CreateGenre()
    {
        var input = fixture.GetValidInput();
        var dbContext = fixture.CreateDbContext();
        var unitOfWork = new UnitOfWork(dbContext);
        var genreRepository = new GenreRepository(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.CreateGenre(genreRepository, unitOfWork, categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);
        var assertDbContext = fixture.CreateDbContext(true);
        var genreFromDb = await assertDbContext.Genres.FindAsync(output.Id);

        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.CreatedAt.Should().NotBe(default);
        output.Categories.Should().BeEmpty();
        genreFromDb.Should().NotBeNull();
        output.Name.Should().Be(genreFromDb.Name);
        output.IsActive.Should().Be(genreFromDb.IsActive);
    }

    [Fact(DisplayName = nameof(CreateGenreWithCategoryReletionship))]
    [Trait("Integration/Application", "Create Genre - Use Cases")]
    public async Task CreateGenreWithCategoryReletionship()
    {
        var categories = fixture.GetValidCategories(5);
        var input = fixture.GetValidInput([.. categories.Select(x => x.Id)]);
        var dbContext = fixture.CreateDbContext();
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var unitOfWork = new UnitOfWork(dbContext);
        var genreRepository = new GenreRepository(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.CreateGenre(genreRepository, unitOfWork, categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);
        var assertDbContext = fixture.CreateDbContext(true);
        var genreFromDb = await assertDbContext.Genres.FindAsync(output.Id);
        var genresCategoriesFromDb = await assertDbContext
            .GenresCategories.Where(genreCategory => genreCategory.GenreId == output.Id)
            .ToListAsync();

        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.CreatedAt.Should().NotBe(default);
        output.Categories.Should().NotBeEmpty();
        output.Categories.Should().BeEquivalentTo(categories.Select(x => x.Id));
        genreFromDb.Should().NotBeNull();
        output.Name.Should().Be(genreFromDb.Name);
        output.IsActive.Should().Be(genreFromDb.IsActive);
        output.Categories.Should().HaveCount(genresCategoriesFromDb.Count);
        output.Categories.Should().BeEquivalentTo(genresCategoriesFromDb.Select(x => x.CategoryId));
    }

    [Fact(
        DisplayName = nameof(
            CreateGenreWithInvalidCategoryRelationship
        )
    )]
    [Trait("Integration/Application", "Create Genre - Use Cases")]
    public async Task CreateGenreWithInvalidCategoryRelationship()
    {
        var categories = fixture.GetValidCategories(5);
        var validCategories = categories[2..5];
        var invalidCategories = categories[0..2];
        var input = fixture.GetValidInput([.. categories.Select(x => x.Id)]);
        var dbContext = fixture.CreateDbContext();
        await dbContext.Categories.AddRangeAsync(validCategories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var unitOfWork = new UnitOfWork(dbContext);
        var genreRepository = new GenreRepository(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.CreateGenre(genreRepository, unitOfWork, categoryRepository);

        var act = () => useCase.Handle(input, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RelatedEntityException>()
            .WithMessage(
                $"Related category ids not found {string.Join(", ", invalidCategories.Select(x => x.Id))}"
            );
    }
}
