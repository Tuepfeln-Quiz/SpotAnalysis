using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class GroupInviteTokenService : IGroupInviteTokenService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);
    private const int MaxCollisionRetries = 5;
    private const int CodeLength = 6;

    // Crockford Base32 Alphabet (ohne I, L, O, U — keine Verwechslungsgefahr)
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    private readonly IDbContextFactory<AnalysisContext> _factory;

    public GroupInviteTokenService(IDbContextFactory<AnalysisContext> factory)
    {
        _factory = factory;
    }

    public async Task<string> CreateToken(int groupId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        var now = DateTime.UtcNow;

        for (var attempt = 0; attempt < MaxCollisionRetries; attempt++)
        {
            var code = GenerateCode();

            if (await ctx.GroupInvites.AnyAsync(i => i.Code == code))
            {
                continue;
            }

            var invite = new GroupInvite
            {
                Code = code,
                GroupID = groupId,
                CreatedAt = now,
                ExpiresAt = now + TokenLifetime,
            };
            ctx.GroupInvites.Add(invite);

            try
            {
                await ctx.SaveChangesAsync();
                return code;
            }
            catch (DbUpdateException)
            {
                // Race auf Unique-Index: Entity abhängen und neuen Code versuchen
                ctx.Entry(invite).State = EntityState.Detached;
            }
        }

        throw new InvalidOperationException(
            $"Konnte nach {MaxCollisionRetries} Versuchen keinen eindeutigen Einladungscode erzeugen.");
    }

    public async Task<int?> ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var normalized = Normalize(token);
        if (normalized.Length == 0)
        {
            return null;
        }

        await using var ctx = await _factory.CreateDbContextAsync();

        var invite = await ctx.GroupInvites
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.Code == normalized);

        if (invite is null)
        {
            return null;
        }

        if (invite.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        return invite.GroupID;
    }

    private static string Normalize(string input)
    {
        return input.Trim().ToUpperInvariant();
    }

    private static string GenerateCode()
    {
        Span<byte> bytes = stackalloc byte[CodeLength];
        RandomNumberGenerator.Fill(bytes);

        Span<char> chars = stackalloc char[CodeLength];
        for (var i = 0; i < CodeLength; i++)
        {
            chars[i] = Alphabet[bytes[i] & 0x1F];
        }
        return new string(chars);
    }
}
