using System;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

public class LoginResultDto
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public bool LockedOut { get; set; }
    public bool RequiresTwoFactor { get; set; }
}