using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Security.Application.Abstractions.Authentication;
using Security.Application.Abstractions.Clock;
using Security.Application.Abstractions.Cryptography;
using Security.Domain.Abstractions;
using Security.Domain.Services;
using Security.Domain.Users;
using Security.Infrastructure.Authentication;
using Security.Infrastructure.Clock;
using Security.Infrastructure.Cryptography;
using Security.Infrastructure.Persistence;
using Security.Infrastructure.Persistence.Repositories;

namespace Security.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddSecurityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        services
        .AddSecurityServicePrivider()
        .AddSecurityPersistence(configuration)
        .AddSecurityAuthentication(configuration);

        return services;
    }

    private static IServiceCollection AddSecurityPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SecurityDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SecurityDbContext>());
        return services;
    }

    private static IServiceCollection AddSecurityServicePrivider(this IServiceCollection services)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        return services;
    }

    private static IServiceCollection AddSecurityAuthentication(this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<IPasswordHashChecker, PasswordHasher>();
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddSingleton(Options.Create(jwtSettings));


        _ = services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwtSettings.Issuer,
          ValidAudience = jwtSettings.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(jwtSettings.Secret)
           )
      });
        return services;
    }
}
