using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using MultiAgent.Agents;

namespace MultiAgent;

public static class AgentSelection
{
    public static KernelFunctionSelectionStrategy CreateSelectionStrategy(Kernel kernel,
        ChatHistoryTruncationReducer historyReducer,
        EssayAgents essayAgents) =>
        new(CreateSelectionFunction(), kernel)
        {
            InitialAgent = essayAgents.InitialAgent,
            HistoryReducer = historyReducer,
            HistoryVariableName = "lastMessage",
            ResultParser = (result) =>
            {
                var resultValue = result.GetValue<string>() ?? nameof(VerificationAgent);
                Console.WriteLine($"selectionFunction: {resultValue}");

                return resultValue;
            }
        };
    
    private static KernelFunction CreateSelectionFunction() =>
        AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
            Examine the provided RESPONSE and choose the next participant.
            State only the name of the chosen participant without explanation.
            Never choose the participant named in the RESPONSE.

            Choose only from these participants:
            - {{{nameof(ApStylebookAgent)}}}
            - {{{nameof(ToneConsistencyAgent)}}}
            - {{{nameof(VerificationAgent)}}}

            Always follow these rules when choosing the next participant:
            - If RESPONSE is user input, it is {{{nameof(ApStylebookAgent)}}}'s turn.
            - If RESPONSE is by {{{nameof(ApStylebookAgent)}}}, it is {{{nameof(VerificationAgent)}}}'s turn.
            - If RESPONSE is by {{{nameof(VerificationAgent)}}}, it is {{{nameof(ApStylebookAgent)}}}'s turn.

            RESPONSE:
            {{$lastmessage}}
            """,
            safeParameterNames: "lastmessage");
}