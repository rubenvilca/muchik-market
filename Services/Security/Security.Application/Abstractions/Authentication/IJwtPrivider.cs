using Security.Domain.Users;

namespace Security.Application.Abstractions.Authentication;
public interface IJwtProvider
{
    string GenerateToken(User user);
}
