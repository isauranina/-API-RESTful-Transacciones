using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transacciones.Core.Interfaces.IData;
using Transacciones.Core.Interfaces.IRepositories.Transacciones;
using Transacciones.Core.Interfaces.IServices.Transacciones;
using Transacciones.Core.Services.Transacciones;
using Transacciones.Infrastructure.Persistence;
using Transacciones.Infrastructure.Repositories.Transacciones;

namespace Transacciones.API.Ioc;

public static class IocTransacciones
{
    public static void AddTransaccionesDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar Entity Framework Core
        services.AddDbContext<TransaccionesDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("TransaccionesConnection") 
                ?? throw new InvalidOperationException("Connection string 'TransaccionesConnection' not found."),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(TransaccionesDbContext).Assembly.FullName);
                }));

        // Registrar DbContext wrapper
        services.AddScoped<ITransaccionesDbContext>(provider =>
        {
            var context = provider.GetRequiredService<TransaccionesDbContext>();
            return new TransaccionesDbContextWrapper(context);
        });

        // Registrar Repositorios
        services.AddScoped<ICuentaRepository, CuentaRepository>();
        services.AddScoped<ITransaccionRepository, TransaccionRepository>();

        // Registrar Servicios
        services.AddScoped<ICuentaService, CuentaService>();
        services.AddScoped<ITransaccionService, TransaccionService>();
    }
}


