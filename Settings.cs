namespace MultiAgent;

using System.Reflection;
using Microsoft.Extensions.Configuration;

public class Settings
{
    private readonly IConfigurationRoot configRoot;

    private AzureOpenAiSettings? azureOpenAi;

    public Settings()
    {
        configRoot =
            new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .Build();
    }

    public AzureOpenAiSettings AzureOpenAi => azureOpenAi ??= GetSettings<AzureOpenAiSettings>();

    public TSettings GetSettings<TSettings>()
    {
        return configRoot.GetRequiredSection(typeof(TSettings).Name).Get<TSettings>()!;
    }

    public class AzureOpenAiSettings
    {
        public string ChatModelDeployment { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}