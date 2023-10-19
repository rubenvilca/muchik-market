namespace Security.Application.Abstractions.Cryptography;
public interface IPasswordHasher
{
    string HashPassword(string password);
}
