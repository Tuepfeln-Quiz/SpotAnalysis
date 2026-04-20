namespace SpotAnalysis.Services.DTOs;

public enum JoinGroupResult
{
    Success,
    AlreadyMember,
    TokenExpired,
    TokenInvalid,
    GroupNotFound,
    UserNotFound
}
