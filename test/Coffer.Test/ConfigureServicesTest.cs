using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Utconnect.Coffer;
using Utconnect.Coffer.Models;
using Utconnect.Coffer.Services.Abstract;
using Utconnect.Coffer.Services.Implementations;

namespace Coffer.Test;

public class ConfigureServicesTests
{
    [Fact]
    public void AddCoffer_ShouldRegisterCofferConfig()
    {
        // Arrange
        ServiceCollection services = [];
        IConfiguration? configuration = Substitute.For<IConfiguration>();
        IConfigurationSection? configurationSection = Substitute.For<IConfigurationSection>();
        configuration.GetSection(nameof(CofferConfig)).Returns(configurationSection);

        // Act
        services.AddCoffer(configuration);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<CofferConfig>? options = serviceProvider.GetService<IOptions<CofferConfig>>();
        Assert.NotNull(options);
        Assert.Same(configurationSection, configuration.GetSection(nameof(CofferConfig)));
    }

    [Fact]
    public void AddCoffer_ShouldRegisterICofferService()
    {
        // Arrange
        ServiceCollection services = [];
        IConfiguration? configuration = Substitute.For<IConfiguration>();
        IConfigurationSection? configurationSection = Substitute.For<IConfigurationSection>();
        configurationSection.Value.Returns("https://example.com");
        configuration.GetSection(nameof(CofferConfig)).Returns(configurationSection);

        // Act
        services.AddCoffer(configuration);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ICofferService? cofferService = serviceProvider.GetService<ICofferService>();
        Assert.NotNull(cofferService);
        Assert.IsType<CofferService>(cofferService);
    }

    [Fact]
    public void AddCoffer_ShouldConfigureHttpClient()
    {
        // Arrange
        ServiceCollection services = [];
        IConfiguration? configuration = Substitute.For<IConfiguration>();
        IConfigurationSection? configurationSection = Substitute.For<IConfigurationSection>();
        configurationSection.Value.Returns("https://example.com");
        configuration.GetSection(nameof(CofferConfig)).Returns(configurationSection);

        // Act
        services.AddCoffer(configuration);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IHttpClientFactory? httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        Assert.NotNull(httpClientFactory);

        HttpClient cofferServiceClient = httpClientFactory.CreateClient(nameof(ICofferService));
        Assert.NotNull(cofferServiceClient);
        Assert.Equal(new Uri("https://example.com"), cofferServiceClient.BaseAddress);
    }

    [Fact]
    public void AddCoffer_WithCustomConfigurationPath_ShouldUseProvidedSection()
    {
        // Arrange
        ServiceCollection services = [];
        IConfiguration? configuration = Substitute.For<IConfiguration>();
        const string customConfigSection = "CustomCofferConfig";
        IConfigurationSection? configurationSection = Substitute.For<IConfigurationSection>();
        configuration.GetSection(customConfigSection).Returns(configurationSection);

        // Act
        services.AddCoffer(configuration, customConfigSection);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<CofferConfig>? options = serviceProvider.GetService<IOptions<CofferConfig>>();
        Assert.NotNull(options);
        Assert.Same(configurationSection, configuration.GetSection(customConfigSection));
    }
}