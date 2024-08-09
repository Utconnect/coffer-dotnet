using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utconnect.Coffer.Models;
using Utconnect.Coffer.Services.Abstract;
using Utconnect.Coffer.Services.Implementations;
using Utconnect.Common.Configurations;

namespace Utconnect.Coffer;

public static class ConfigureServices
{
    public static void AddCoffer(
        this IServiceCollection services,
        IConfiguration configuration,
        string? configurationPath = null)
    {
        services.AddConfiguration<CofferConfig>(configuration, configurationPath);
        services.AddHttpClient<ICofferService, CofferService>(configureClient =>
        {
            string? configPath = configuration.GetSection(configurationPath ?? nameof(CofferConfig)).Value;
            if (configPath != null)
            {
                configureClient.BaseAddress = new Uri(configPath);
            }
        });
        services.AddTransient<ICofferService, CofferService>();
    }
}