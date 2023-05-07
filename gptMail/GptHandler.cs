using EmailServer;
using MimeKit;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class GptHandler
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GptHandler(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient { BaseAddress = new Uri("https://api.openai.com/v1/") };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GenerateReply(MimeMessage message)
    {
        // Process the email message and create a prompt
        string prompt = CreatePrompt(message);

        // Call the GPT API
        var requestBody = new
        {
            prompt = prompt,
            model = "text-davinci-003",
            max_tokens = 1000,
            temperature = 0.7
        };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("completions", requestBody);

        string responseBody = await response.Content.ReadAsStringAsync();
        var completionResponse = JsonSerializer.Deserialize<CompletionResponse>(responseBody);

        // Extract the reply from the GPT API response
        string reply = completionResponse.Choices[0].Text;
        return reply;
    }

    private string CreatePrompt(MimeMessage message)
    {
        string trimmedMessage = Regex.Replace(message.TextBody.Trim(), @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
        trimmedMessage = Regex.Replace(trimmedMessage, "^>.*$", string.Empty, RegexOptions.Multiline).Trim();

        // Customize this prompt as needed
        string prompt = "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly.\n\n";
        prompt += "Human: " + trimmedMessage + "\nAI: ";

        return prompt;
    }
}


public class CompletionResponse
{
    [JsonPropertyName("choices")]
    public List<ChatGPTChoice>? Choices
    {
        get;
        set;
    }
    [JsonPropertyName("usage")]
    public ChatGPTUsage? Usage
    {
        get;
        set;
    }
}
public class ChatGPTUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens
    {
        get;
        set;
    }
    [JsonPropertyName("completion_token")]
    public int CompletionTokens
    {
        get;
        set;
    }
    [JsonPropertyName("total_tokens")]
    public int TotalTokens
    {
        get;
        set;
    }
}
[DebuggerDisplay("Text = {Text}")]
public class ChatGPTChoice
{
    [JsonPropertyName("text")]
    public string? Text
    {
        get;
        set;
    }
}

