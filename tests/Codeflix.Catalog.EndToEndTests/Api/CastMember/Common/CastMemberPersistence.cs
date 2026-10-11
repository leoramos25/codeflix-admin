using Codeflix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.Common;

public class CastMemberPersistence(CodeflixCatalogDbContext context)
{
    public async Task InsertList(
        List<DomainEntity.CastMember> castMembers,
        CancellationToken cancellationToken = default
    )
    {
        await context.CastMembers.AddRangeAsync(castMembers, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<DomainEntity.CastMember?> GetById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .CastMembers.AsNoTracking()
            .FirstOrDefaultAsync(castMember => castMember.Id == id, cancellationToken);
    }
}
