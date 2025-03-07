using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace MultiAgent.Agents;

public static class ApStylebookAgent
{
        private const string apStylebookPrompt = """
            You are an expert AP Stylebook editor. Analyze the following text and identify any
            violations of AP style rules. Be thorough and precise in your analysis.

            For each violation:
            1. Quote the original text containing the violation
            2. State the specific AP Stylebook rule that applies
            3. Explain why the text violates the rule
            
            Format your response using JSON with the following structure:
            {
                "violations": [
                    {
                        "originalText": "Original text containing the violation",
                        "rule": "AP Stylebook rule citation",
                        "explanation": "Explanation of why the text violates the rule"
                    }
                ]
            }

            IMPORTANT INSTRUCTIONS:
            - Do NOT provide rewritten or corrected versions of the text
            - Be precise in your citations of AP Stylebook rules
            - Format your response as shown in the example JSON
        """;

        public static ChatCompletionAgent CreateAgent(Kernel kernel) =>
             new ChatCompletionAgent()
                {
                    Name = nameof(ApStylebookAgent),
                    Instructions = apStylebookPrompt,
                    Kernel = kernel,
                    Arguments =
                        new KernelArguments(new AzureOpenAIPromptExecutionSettings
                            { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
                };
}