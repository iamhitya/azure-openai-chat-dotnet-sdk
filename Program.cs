using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;

// Azure OpenAI configuration via environment variables
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"); // e.g. https://your-resource.openai.azure.com/
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT"); // Azure OpenAI deployment name

if (string.IsNullOrWhiteSpace(endpoint)) { Console.Error.WriteLine("AZURE_OPENAI_ENDPOINT environment variable is not set."); return; }
if (string.IsNullOrWhiteSpace(apiKey)) { Console.Error.WriteLine("AZURE_OPENAI_API_KEY environment variable is not set."); return; }
if (string.IsNullOrWhiteSpace(deployment)) { Console.Error.WriteLine("AZURE_OPENAI_CHAT_DEPLOYMENT environment variable is not set."); return; }

// Create OpenAIClient targeting Azure endpoint via options
var client = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
);
var chat = client.GetChatClient(deployment);

// Maintain conversation history
var messages = new List<ChatMessage>
{
    new SystemChatMessage("You are a concise assistant.")
};

Console.WriteLine("Type your message and press Enter. Submit a blank line to exit.\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Exiting...");
        break;
    }

    messages.Add(new UserChatMessage(input));

    try
    {
        Console.Write("Assistant: \n");
        var sb = new StringBuilder();

        await foreach (var update in chat.CompleteChatStreamingAsync(messages.ToArray()))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (part.Kind == ChatMessageContentPartKind.Text && !string.IsNullOrEmpty(part.Text))
                {
                    Console.Write(part.Text);
                    sb.Append(part.Text);
                }
            }
        }

        Console.WriteLine("\n");

        messages.Add(new AssistantChatMessage(sb.ToString()));
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}\n");
    }
}
