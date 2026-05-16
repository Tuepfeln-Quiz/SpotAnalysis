using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class GroupInviteTokenService(IDbContextFactory<AnalysisContext> factory) : IGroupInviteTokenService
{
    private const int MaxCollisionRetries = 5;
    private const int CodeLength = 6;

    // Crockford Base32 Alphabet (ohne I, L, O, U — keine Verwechslungsgefahr)
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);

    public async Task<string> CreateToken(int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
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
                Code = code, GroupId = groupId, CreatedAt = now, ExpiresAt = now + TokenLifetime,
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

    public async Task<GroupInvite?> ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var normalized = Normalize(token);
        if (normalized.Length == 0)
            return null;

        await using var ctx = await factory.CreateDbContextAsync();

        return await ctx.GroupInvites
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.Code == normalized);
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
