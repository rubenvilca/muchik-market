using Security.Domain.Abstractions;
using Security.Domain.Services;

namespace Security.Domain.Users;
public sealed class User : Entity<UserId>
{
    private string _passwordHash;
    private User(UserId id, string email, string passwordHash)
    : base(id)
    {
        Email = email;
        _passwordHash = passwordHash;
    }
    public string Email { get; private set; }

    private User() { }

    public static Result<User> Create(string email, string passwordHash)
    {
        var user = new User(UserId.New(), email, passwordHash);
        return user;
    }

    public bool VerifyPasswordHash(string password, IPasswordHashChecker passwordHashChecker)
            => !string.IsNullOrWhiteSpace(password) && passwordHashChecker.HashesMatch(_passwordHash, password);
}
