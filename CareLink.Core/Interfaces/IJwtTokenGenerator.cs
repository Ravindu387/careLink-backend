using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
