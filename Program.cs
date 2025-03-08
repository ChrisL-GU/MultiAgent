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


        var kernel = builder.Build();

        var agents = new EssayAgents(kernel);
        
        var chat =
          new AgentGroupChat(agents.GetAgents())
          {
            ExecutionSettings = new AgentGroupChatSettings
            {
                SelectionStrategy = new SequentialSelectionStrategy(),
                TerminationStrategy = new ApprovalTerminationStrategy()
                {
                    Agents = [agents.FinalAgent],
                    MaximumIterations = agents.GetAgents().Length // * 3,
                }
            }
          };
        
        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, TextToAnalyze.PaulGraham));
        chat.IsComplete = false;
        
        await foreach (var chatMessageContent in chat.InvokeAsync())
        {
            Console.WriteLine(
                $"{chatMessageContent.AuthorName?.ToUpperInvariant() ?? "Unspecified"}:" +
                $"{Environment.NewLine}{chatMessageContent.Content}{Environment.NewLine}");
        }
    }
}

internal sealed class ApprovalTerminationStrategy : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[^1].Content?.Contains("perfecto", StringComparison.OrdinalIgnoreCase) ?? false);
}
public class EssayAgents
{
    private readonly ChatCompletionAgent apStylebookAgent;
    private readonly ChatCompletionAgent verificationAgent;
    private readonly ChatCompletionAgent toneConsistencyAgent;

    public EssayAgents(Kernel kernel)
    {
        apStylebookAgent = ApStylebookAgent.CreateAgent(kernel);
        verificationAgent = VerificationAgent.CreateAgent(kernel);
        toneConsistencyAgent = ToneConsistencyAgent.CreateAgent(kernel);
    }
    
    public Agent[] GetAgents() => [apStylebookAgent, toneConsistencyAgent];
    public Agent InitialAgent => apStylebookAgent;
    public Agent FinalAgent => toneConsistencyAgent;
}