using Microsoft.EntityFrameworkCore;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Models.Interfaces;

namespace Reservae.Repository;

public class SpaceRepository(ApplicationDbContext context)
    : BaseRepository<Space>(context), ISpaceRepository
{
    public async Task<PagedResponseDto<Space>> SearchAsync(
        string term,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(term);

        var normalizedTerm = term.Trim();
        var containsPattern = $"%{EscapeLikePattern(normalizedTerm)}%";

        await using var transaction = await Context.Database
            .BeginTransactionAsync(cancellationToken);
        await Context.Database.ExecuteSqlRawAsync(
            "SET LOCAL pg_trgm.word_similarity_threshold = 0.2",
            cancellationToken);

        var rankedQuery = DbSet
            .AsNoTracking()
            .Where(space =>
                space.IsActive &&
                (EF.Functions.TrigramsAreWordSimilar(normalizedTerm, space.Title) ||
                 EF.Functions.TrigramsAreWordSimilar(normalizedTerm, space.Address) ||
                 EF.Functions.TrigramsAreWordSimilar(normalizedTerm, space.Description) ||
                 EF.Functions.TrigramsAreSimilar(normalizedTerm, space.Title) ||
                 EF.Functions.TrigramsAreSimilar(normalizedTerm, space.Address) ||
                 EF.Functions.TrigramsAreSimilar(normalizedTerm, space.Description) ||
                 EF.Functions.ILike(space.Title, containsPattern, "\\") ||
                 EF.Functions.ILike(space.Address, containsPattern, "\\") ||
                 EF.Functions.ILike(space.Description, containsPattern, "\\")))
            .Select(space => new
            {
                Space = space,
                TitleSimilarity = EF.Functions.TrigramsWordSimilarity(normalizedTerm, space.Title),
                AddressSimilarity = EF.Functions.TrigramsWordSimilarity(normalizedTerm, space.Address),
                DescriptionSimilarity = EF.Functions.TrigramsWordSimilarity(normalizedTerm, space.Description)
            });

        var totalCount = await rankedQuery.CountAsync(cancellationToken);
        var items = await rankedQuery
            .OrderByDescending(result =>
                result.TitleSimilarity * 3 +
                result.AddressSimilarity * 2 +
                result.DescriptionSimilarity)
            .ThenBy(result => result.Space.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(result => result.Space)
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PagedResponseDto<Space>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private static string EscapeLikePattern(string value)
        => value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
}
