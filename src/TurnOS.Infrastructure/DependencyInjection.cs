using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TurnOS.Application.Interfaces;
using TurnOS.Application.Services;
using TurnOS.Domain.Interfaces;
using TurnOS.Infrastructure.Data;
using TurnOS.Infrastructure.Repositories;
using TurnOS.Infrastructure.Services;

namespace TurnOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("DefaultConnection") ?? "";
        if (connStr.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(connStr));
        else
            services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connStr));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // Infrastructure services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBusinessService, BusinessService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IAppointmentService, AppointmentService>();

        return services;
    }
}
