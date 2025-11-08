# .NET Console Azure OpenAI Chat (SDK)

This repository contains a minimal .NET 9 console app that uses the official OpenAI .NET SDK configured for Azure OpenAI to run a chat session with:
- Streaming output (tokens print as they arrive — no waiting for full responses)
- Persistent conversation history (system, user, and assistant turns)

The sample targets C# 13 and .NET 9.

## Features
- Uses the `OpenAI` NuGet package (2.x) and the `ChatClient` API
- Configured for Azure OpenAI via endpoint and deployment name
- Streams assistant responses via `CompleteChatStreamingAsync(...)`
- Preserves full conversation context by appending `UserChatMessage` and `AssistantChatMessage` objects
- Exits the loop on a blank user input

## Prerequisites
- .NET 9 SDK installed
- An Azure OpenAI resource with:
  - Resource endpoint (e.g., `https://your-resource.openai.azure.com/`)
  - API key
  - A deployed chat model (deployment name, e.g., a `gpt-4o-mini` deployment)
- Network access to the Azure OpenAI endpoint

## Getting started

1) Clone/download this repo and open the solution.

2) Restore and build (Visual Studio or CLI):

```
dotnet restore
dotnet build -c Release
```

3) Set these environment variables:

- Windows (PowerShell):

```
$env:AZURE_OPENAI_ENDPOINT = "https://your-resource.openai.azure.com/"
$env:AZURE_OPENAI_API_KEY = "<your-api-key>"
$env:AZURE_OPENAI_CHAT_DEPLOYMENT = "<your-deployment-name>"
```

- macOS/Linux (bash/zsh):

```
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="<your-api-key>"
export AZURE_OPENAI_CHAT_DEPLOYMENT="<your-deployment-name>"
```

4) Run:

```
dotnet run --project Azure.OpenAI.Chat.SDK.csproj
```

You should see a prompt:

```
Type your message and press Enter. Submit a blank line to exit.
You:
```
Type a message and watch the assistant reply stream in real time. Submit a blank line to exit.

## How it works
The core logic lives in `Program.cs`:
- Create a client using an API key and Azure endpoint:
  - `var client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri(endpoint) });`
- Get a chat client from your Azure deployment:
  - `var chat = client.GetChatClient(deployment);`
- Maintain a `List<ChatMessage>` for history:
  - Start with a `SystemChatMessage` to set the assistant behavior.
  - Append each `UserChatMessage` and `AssistantChatMessage` after every turn.
- Stream responses:
  - `await foreach (var update in chat.CompleteChatStreamingAsync(messages.ToArray())) { ... }`
  - Print `Text` parts as they arrive and accumulate them into a buffer for history.

## Changing the deployment
Update your Azure deployment name via the `AZURE_OPENAI_CHAT_DEPLOYMENT` environment variable (and ensure the model is deployed to the resource referenced by `AZURE_OPENAI_ENDPOINT`).

## Non‑streaming example (reference)
If you prefer to wait for the complete response (not used in this sample), the SDK supports a non‑streaming call:

```
var result = await chat.CompleteChatAsync(messages.ToArray());
var completion = result.Value; // ChatCompletion
var text = string.Join("\n",
    Enumerable.Range(0, completion.Content.Count)
              .Select(i => completion.Content[i].Text)
              .Where(t => !string.IsNullOrEmpty(t)));
```

## Troubleshooting
- AZURE_OPENAI_ENDPOINT/API_KEY/CHAT_DEPLOYMENT not set: The app exits with an error; set the environment variables and try again.
- 401/403 errors: Verify the API key, endpoint, and that the key belongs to the target resource.
- 404/BadRequest: Confirm the deployment name exists in the specified Azure OpenAI resource and the endpoint/region is correct.
- Networking issues: Ensure outbound HTTPS access to the Azure OpenAI endpoint.
- Quotas/limits: Check your Azure OpenAI quotas and usage.

## Security notes
- Do not commit secrets (API keys) to source control.
- Prefer user or machine‑level environment variables or managed secret stores.

## Tech stack
- .NET 9
- C# 13
- `OpenAI` NuGet package (2.x), configured for Azure OpenAI

## Scripts/commands summary
- Restore: `dotnet restore`
- Build: `dotnet build`
- Run: `dotnet run --project Azure.OpenAI.Chat.SDK.csproj`
