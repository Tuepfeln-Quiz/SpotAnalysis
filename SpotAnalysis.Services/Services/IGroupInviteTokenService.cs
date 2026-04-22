namespace SpotAnalysis.Services.Services;

public interface IGroupInviteTokenService
{
    /// <summary>
    /// Erzeugt einen kurzen, zeitlich begrenzten Einladungscode für eine Gruppe.
    /// Gültigkeit: 15 Minuten.
    /// </summary>
    Task<string> CreateToken(int groupId);

    /// <summary>
    /// Validiert einen Einladungscode und liefert die enthaltene GroupID.
    /// Gibt null zurück, wenn der Code unbekannt oder abgelaufen ist.
    /// </summary>
    Task<int?> ValidateToken(string token);
}
