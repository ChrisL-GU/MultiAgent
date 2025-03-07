using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace MultiAgent.Agents;

public static class VerificationAgent
{
    private const string verificationPrompt = """
         If a violation includes a rule citing an abbreviation for "Father" then respond with:
         do not apply any rule regarding the use of "Father"; otherwise approve.
         
         IMPORTANT INSTRUCTIONS:
         - Do not include any additional details in the response, only approval or disapproval
         with why it has been disapproved.
         """;
         // Your sole responsibility is ensure the original text has not been rewritten and that
         // the format conforms to:
         //
         // 1. Original text containing the violation
         // 2. AP Stylebook rule citation
         // 3. Explanation of why the text violates the rule
         //
         // IMPORTANT: Recommendations must NOT include rewritten or corrected versions of the text.
         // """;

    public static ChatCompletionAgent CreateAgent(Kernel kernel) =>
        new()
        {
            Name = nameof(VerificationAgent),
            Instructions = verificationPrompt,
            Kernel = kernel,
            Arguments =
                new KernelArguments(new AzureOpenAIPromptExecutionSettings
                    { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
        };

}