using Security.Application.Abstractions.Authentication;
using Security.Application.Core.Messaging;
using Security.Domain.Abstractions;
using Security.Domain.Services;
using Security.Domain.Users;

namespace Security.Application.Features.Users.Commands.LogInUser;
internal class LogInUserCommandHandler : ICommandHandler<LogInUserCommand, AccessTokenResponse>
{
    private readonly IJwtProvider _jwtService;
    private readonly IPasswordHashChecker _passwordHashChecker;
    private readonly IUserRepository _userRepository;
    public LogInUserCommandHandler(
        IJwtProvider jwtService,
        IUserRepository userRepository,
        IPasswordHashChecker passwordHashChecker)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _passwordHashChecker = passwordHashChecker;
    }
    public async Task<Result<AccessTokenResponse>> Handle(
        LogInUserCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
            return Result.Failure<AccessTokenResponse>(UserErrors.InvalidEmailOrPassword);

        bool passwordValid = user.VerifyPasswordHash(request.Password, _passwordHashChecker);

        if (!passwordValid)
            return Result.Failure<AccessTokenResponse>(UserErrors.InvalidEmailOrPassword);

        var result = _jwtService.GenerateToken(user);

        return new AccessTokenResponse(result);
    }
}
