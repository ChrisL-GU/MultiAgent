using Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents.Chat;
using MultiAgent.Agents;

namespace MultiAgent;

using System.Text.Json;
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
        var chatResponse = new List<ChatResponse>();
        
        await foreach (var chatMessageContent in chat.InvokeAsync())
        {
            var response = chatMessageContent.Content ?? "";
            var detectionContent = response.GetBetween("<essay_analysis>", "</essay_analysis>").Trim();
            var jsonContent = response.GetBetween("json```", "```").Trim();
            chatResponse.Add(new (
                chatMessageContent.AuthorName ?? "Unknown",
                detectionContent,
                JsonSerializer.Deserialize<FeedbackResponse>(jsonContent)?.Feedback ?? []));
        }

        Console.WriteLine(chatResponse.Count);
    }
}

internal sealed class ApprovalTerminationStrategy : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(
        Agent agent,
        IReadOnlyList<ChatMessageContent> history,
        CancellationToken cancellationToken) =>
        // always give approval, nothing to verify
        Task.FromResult(true);
}
public class EssayAgents
{
    private readonly ChatCompletionAgent apStylebookAgent;
    private readonly ChatCompletionAgent verificationAgent;
    private readonly ChatCompletionAgent toneConsistencyAgent;
    private readonly ChatCompletionAgent biasDetectionAgent;

    public EssayAgents(Kernel kernel)
    {
        apStylebookAgent = ApStylebookAgent.CreateAgent(kernel);
        verificationAgent = VerificationAgent.CreateAgent(kernel);
        toneConsistencyAgent = ToneConsistencyAgent.CreateAgent(kernel);
        biasDetectionAgent = BiasDetectionAgent.CreateAgent(kernel);
    }
    
    public Agent[] GetAgents() => [apStylebookAgent, toneConsistencyAgent, biasDetectionAgent];
    public Agent InitialAgent => GetAgents().First();
    public Agent FinalAgent => GetAgents().Last();
}

public record ChatResponse(string Author, string Reasoning, List<Feedback> Feedbacks);
public record Feedback(string OriginalText, string Recommendation, string Explanation);
public record FeedbackResponse(List<Feedback> Feedback);

public static class StringExtensions
{
    public static string GetBetween(this string text, string start, string end)
    {
        var startIndex = text.IndexOf(start, StringComparison.Ordinal);
        if (startIndex < 0) return string.Empty;
        
        startIndex += start.Length;
        var endIndex = text.IndexOf(end, startIndex, StringComparison.Ordinal);
        if (endIndex < 0) return string.Empty;
        
        return text[startIndex..endIndex];
    }
}
