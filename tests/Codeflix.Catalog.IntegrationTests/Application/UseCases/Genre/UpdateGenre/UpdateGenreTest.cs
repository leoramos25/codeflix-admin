using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Exceptions;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UseCase = Codeflix.Catalog.Application.UseCases.Genre.Update;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.UpdateGenre;

[Collection(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTest(UpdateGenreTestFixture fixture)
{
    [Fact(DisplayName = nameof(UpdateGenre))]
    [Trait("Integration/Application", "Update Genre - Use Cases")]
    public async Task UpdateGenre()
    {
        var genre = fixture.GetValidGenre();
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.UpdateGenre(genreRepository, unitOfWork, categoryRepository);
        var input = fixture.GetValidInput(genre.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.Categories.Should().BeEmpty();
        output.IsActive.Should().Be(input.IsActive!.Value);
        var assertDbContext = fixture.CreateDbContext(true);
        var updatedGenre = await assertDbContext.Genres.FindAsync(genre.Id, CancellationToken.None);
        updatedGenre.Should().NotBeNull();
        updatedGenre.Id.Should().Be(input.Id);
        updatedGenre.Name.Should().Be(input.Name);
        updatedGenre.Categories.Should().BeEmpty();
        updatedGenre.IsActive.Should().Be(input.IsActive!.Value);
    }

    [Fact(DisplayName = nameof(UpdateGenreWithCategoryRelationship))]
    [Trait("Integration/Application", "Update Genre - Use Cases")]
    public async Task UpdateGenreWithCategoryRelationship()
    {
        var genre = fixture.GetValidGenre();
        var categories = fixture.GetValidCategories(6);
        var currentCategories = categories.GetRange(0, 3);
        var newCategories = categories.GetRange(2, 3);
        var newCategoryIds = newCategories.Select(x => x.Id).ToList();
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.GenresCategories.AddRangeAsync(
            currentCategories.Select(category => new Catalog.Infra.Data.EF.Models.GenresCategories(
                genre.Id,
                category.Id
            ))
        );
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.UpdateGenre(genreRepository, unitOfWork, categoryRepository);
        var input = fixture.GetValidInput(genre.Id, newCategoryIds);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.Categories.Should().NotBeEmpty();
        output.Categories.Should().BeEquivalentTo(newCategoryIds);
        output.IsActive.Should().Be(input.IsActive!.Value);
        var assertDbContext = fixture.CreateDbContext(true);
        var updatedGenre = await assertDbContext.Genres.FindAsync(genre.Id, CancellationToken.None);
        var genresCategories = await assertDbContext
            .GenresCategories.Where(genreCategory => genreCategory.GenreId == genre.Id)
            .ToListAsync();
        updatedGenre.Should().NotBeNull();
        updatedGenre.Id.Should().Be(input.Id);
        updatedGenre.Name.Should().Be(input.Name);
        genresCategories.Should().NotBeEmpty();
        genresCategories
            .Select(genreCategory => genreCategory.CategoryId)
            .Should()
            .BeEquivalentTo(newCategoryIds);
        updatedGenre.IsActive.Should().Be(input.IsActive!.Value);
    }

    [Fact(DisplayName = nameof(UpdateGenreWithEmptyCategoryRelationship))]
    [Trait("Integration/Application", "Update Genre - Use Cases")]
    public async Task UpdateGenreWithEmptyCategoryRelationship()
    {
        var genre = fixture.GetValidGenre();
        var categories = fixture.GetValidCategories(6);
        var currentCategories = categories.GetRange(0, 3);
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.GenresCategories.AddRangeAsync(
            currentCategories.Select(category => new Catalog.Infra.Data.EF.Models.GenresCategories(
                genre.Id,
                category.Id
            ))
        );
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.UpdateGenre(genreRepository, unitOfWork, categoryRepository);
        var input = fixture.GetValidInput(genre.Id, []);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.Categories.Should().BeEmpty();
        output.IsActive.Should().Be(input.IsActive!.Value);
        var assertDbContext = fixture.CreateDbContext(true);
        var updatedGenre = await assertDbContext.Genres.FindAsync(genre.Id, CancellationToken.None);
        var genresCategories = await assertDbContext
            .GenresCategories.Where(genreCategory => genreCategory.GenreId == genre.Id)
            .ToListAsync();
        updatedGenre.Should().NotBeNull();
        updatedGenre.Id.Should().Be(input.Id);
        updatedGenre.Name.Should().Be(input.Name);
        genresCategories.Should().BeEmpty();
        updatedGenre.IsActive.Should().Be(input.IsActive!.Value);
    }

    [Fact(DisplayName = nameof(UpdateGenreWithoutCategoryRelationship))]
    [Trait("Integration/Application", "Update Genre - Use Cases")]
    public async Task UpdateGenreWithoutCategoryRelationship()
    {
        var genre = fixture.GetValidGenre();
        var categories = fixture.GetValidCategories(6);
        var currentCategories = categories.GetRange(0, 3);
        var currentCategoryIds = currentCategories.Select(x => x.Id).ToList();
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.GenresCategories.AddRangeAsync(
            currentCategories.Select(category => new Catalog.Infra.Data.EF.Models.GenresCategories(
                genre.Id,
                category.Id
            ))
        );
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.UpdateGenre(genreRepository, unitOfWork, categoryRepository);
        var input = fixture.GetValidInput(genre.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.Categories.Should().NotBeEmpty();
        output.Categories.Should().BeEquivalentTo(currentCategoryIds);
        output.IsActive.Should().Be(input.IsActive!.Value);
        var assertDbContext = fixture.CreateDbContext(true);
        var updatedGenre = await assertDbContext.Genres.FindAsync(genre.Id, CancellationToken.None);
        var genresCategories = await assertDbContext
            .GenresCategories.AsNoTracking()
            .Where(genreCategory => genreCategory.GenreId == genre.Id)
            .ToListAsync();
        updatedGenre.Should().NotBeNull();
        updatedGenre.Id.Should().Be(input.Id);
        updatedGenre.Name.Should().Be(input.Name);
        genresCategories.Should().NotBeEmpty();
        genresCategories
            .Select(genreCategory => genreCategory.CategoryId)
            .Should()
            .BeEquivalentTo(currentCategoryIds);
        updatedGenre.IsActive.Should().Be(input.IsActive!.Value);
    }

    [Fact(DisplayName = nameof(UpdateGenreWithIsActiveIsNull))]
    [Trait("Integration/Application", "Update Genre - Use Cases")]
    public async Task UpdateGenreWithIsActiveIsNull()
    {
        var genre = fixture.GetValidGenre();
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.UpdateGenre(genreRepository, unitOfWork, categoryRepository);
        var input = new UseCase.UpdateGenreInput(genre.Id, fixture.GetValidName(), null);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.Categories.Should().BeEmpty();
        output.IsActive.Should().Be(genre.IsActive);
        var assertDbContext = fixture.CreateDbContext(true);
        var updatedGenre = await assertDbContext.Genres.FindAsync(genre.Id, CancellationToken.None);
        updatedGenre.Should().NotBeNull();
        updatedGenre.Id.Should().Be(input.Id);
        updatedGenre.Name.Should().Be(input.Name);
        updatedGenre.Categories.Should().BeEmpty();
        updatedGenre.IsActive.Should().Be(genre.IsActive);
    }

    [Fact(DisplayName = nameof(UpdateGenreWithoutStatusChange))]
    [Trait("Integration/Application", "Update Genre - Use Cases")]
    public async Task UpdateGenreWithoutStatusChange()
    {
        var genre = fixture.GetValidGenre();
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.UpdateGenre(genreRepository, unitOfWork, categoryRepository);
        var input = new UseCase.UpdateGenreInput(genre.Id, fixture.GetValidName(), genre.IsActive);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.Categories.Should().BeEmpty();
        output.IsActive.Should().Be(genre.IsActive);
        var assertDbContext = fixture.CreateDbContext(true);
        var updatedGenre = await assertDbContext.Genres.FindAsync(genre.Id, CancellationToken.None);
        updatedGenre.Should().NotBeNull();
        updatedGenre.Id.Should().Be(input.Id);
        updatedGenre.Name.Should().Be(input.Name);
        updatedGenre.Categories.Should().BeEmpty();
        updatedGenre.IsActive.Should().Be(genre.IsActive);
    }

    [Fact(DisplayName = nameof(UpdateGenreWithInvalidCategoryRelationship))]
    [Trait("Integration/Application", "Update Genre - Use Cases")]
    public async Task UpdateGenreWithInvalidCategoryRelationship()
    {
        var genre = fixture.GetValidGenre();
        var categories = fixture.GetValidCategories(8);
        var currentCategories = categories.GetRange(0, 3);
        var invalidCategories = categories.GetRange(3, 3);
        var invalidCategoryIds = invalidCategories.Select(x => x.Id).ToList();
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.Categories.AddRangeAsync(currentCategories, CancellationToken.None);
        await dbContext.GenresCategories.AddRangeAsync(
            currentCategories.Select(category => new Catalog.Infra.Data.EF.Models.GenresCategories(
                genre.Id,
                category.Id
            ))
        );
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.UpdateGenre(genreRepository, unitOfWork, categoryRepository);
        var input = fixture.GetValidInput(genre.Id, invalidCategoryIds);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<RelatedEntityException>()
            .WithMessage($"Related category ids not found {string.Join(", ", invalidCategoryIds)}");
    }

    [Fact(DisplayName = nameof(UpdateGenreWithInvalidId))]
    [Trait("Integration/Application", "Update Genre - Use Cases")]
    public async Task UpdateGenreWithInvalidId()
    {
        var genre = fixture.GetValidGenre();
        var invalidId = Guid.NewGuid();
        
        var dbContext = fixture.CreateDbContext();
        await dbContext.Genres.AddAsync(genre, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var genreRepository = new GenreRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.UpdateGenre(genreRepository, unitOfWork, categoryRepository);
        var input = fixture.GetValidInput(invalidId);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage( $"Genre '{invalidId}' not found.");
    }
}
