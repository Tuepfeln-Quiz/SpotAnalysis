using Org.BouncyCastle.Asn1.X509;
using SpotAnalysis.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpotAnalysis.Services.DTOs;

public class UserDto
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public List<string> Roles { get; set; } = [];
    public List<GroupDto> AssignedGroups { get; init; } = [];
}
