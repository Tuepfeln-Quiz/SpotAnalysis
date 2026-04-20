namespace SpotAnalysis.Services.Services;

public interface IGroupInviteTokenService
{
    /// <summary>
    /// Erzeugt einen signierten, zeitlich begrenzten Einladungstoken für eine Gruppe.
    /// Gültigkeit: 15 Minuten.
    /// </summary>
    string CreateToken(int groupId);

    /// <summary>
    /// Validiert einen Einladungstoken und liefert die enthaltene GroupID.
    /// Gibt null zurück, wenn der Token abgelaufen, manipuliert oder ungültig ist.
    /// </summary>
    int? ValidateToken(string token);
}
