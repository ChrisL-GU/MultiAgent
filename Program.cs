using Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents.Chat;
using MultiAgent.Agents;

namespace MultiAgent;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Settings settings = new();
        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            settings.AzureOpenAi.ChatModelDeployment,
            settings.AzureOpenAi.Endpoint,
            settings.AzureOpenAi.ApiKey);


        var logger = new InMemoryLogger();
        var kernel = builder.Build();

        var apStylebookAgent = ApStylebookAgent.CreateAgent(kernel);
        var verificationAgent = VerificationAgent.CreateAgent(kernel);
        var toneConsistencyAgent = ToneConsistencyAgent.CreateAgent(kernel);
        var historyReducer = new ChatHistoryTruncationReducer(3);
        
        AgentGroupChat chat =
          new(apStylebookAgent, toneConsistencyAgent, verificationAgent)
          {
            ExecutionSettings = new AgentGroupChatSettings
            {
                SelectionStrategy = AgentSelection.CreateSelectionStrategy(kernel, historyReducer, apStylebookAgent, toneConsistencyAgent, verificationAgent),
                TerminationStrategy = ChatTermination.CreateTerminationStrategy(kernel, historyReducer, verificationAgent)
            }
          };
        
        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, TextToAnalyze.GuText));
        chat.IsComplete = false;
        
        await foreach (var chatMessageContent in chat.InvokeAsync())
        {
            Console.WriteLine(
                $"{chatMessageContent.AuthorName?.ToUpperInvariant() ?? "Unspecified"}:" +
                $"{Environment.NewLine}{chatMessageContent.Content}{Environment.NewLine}");
        }
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        var loggerProvider = new InMemoryLoggerProvider();
        services.AddSingleton<ILoggerProvider>(loggerProvider);
        services.AddSingleton(loggerProvider.Logger);
    }
    
    public class InMemoryLoggerProvider : ILoggerProvider
    {
        private readonly InMemoryLogger logger = new();
    
        public ILogger CreateLogger(string categoryName) => logger;
    
        public void Dispose() { }
    
        public InMemoryLogger Logger => logger;
    }
}