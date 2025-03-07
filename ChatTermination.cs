using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MultiAgent;

public static class ChatTermination
{
    private const string terminationToken = "yes";
    private const int maxTurns = 6;
    
    public static KernelFunctionTerminationStrategy CreateTerminationStrategy(Kernel kernel,
        ChatHistoryTruncationReducer historyReducer, ChatCompletionAgent verificationAgent) =>
        new (CreateTerminationFunction(), kernel)
        {
            Agents = [verificationAgent],
            HistoryReducer = historyReducer,
            HistoryVariableName = "lastMessage",
            MaximumIterations = maxTurns,
            ResultParser = (result) =>
            {
                var resultValue = result.GetValue<string>();
                var resultCondition = resultValue
                    ?.Contains(terminationToken, StringComparison.OrdinalIgnoreCase) ?? false;
                Console.WriteLine($"terminationFunction ({resultCondition}): {resultValue}");

                return resultCondition;
            }
        };
    
    private static KernelFunction CreateTerminationFunction() =>
        AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
            Determine if the reviewer has approved. If so, respond with a single word: {{{terminationToken}}}.

            RESPONSE:
            {{$lastmessage}}
            """,
            safeParameterNames: "lastmessage");
        
}