using System;
using System.Text.Json;
using TrendingAPI.DTOs;
using System.Net.Http.Headers;
using TrendingAPI.Services;
using Microsoft.Extensions.Configuration;
using TrendingAPI.Interfaces;

namespace TrendingAPI.Services;

public class TrendingService : ITrendingService
{

    //FIELDS
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    private readonly string _model = "anthropic/claude-4-sonnet"; // Default model, can be configured in appsettings.json

    public TrendingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiToken = configuration["ApiToken"];
        if (string.IsNullOrEmpty(_apiToken))
            throw new Exception("ApiToken not configured in appsettings.json");


        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
        _httpClient.DefaultRequestHeaders.Add("Prefer", "wait");


    }

    public async Task<string> GetBasePromptService()
    {

        var basePath = AppContext.BaseDirectory;
        var jsonPath = Path.Combine(basePath, "..", "..", "..", "Configurations", "basePrompt.json");
        jsonPath = Path.GetFullPath(jsonPath);

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException("file not found");

        var json = await File.ReadAllTextAsync(jsonPath);
        var doc = JsonDocument.Parse(json);

        var parts = doc.RootElement.EnumerateObject()
            .Select(prop => $"{prop.Name}: {prop.Value.ToString()}")
            .ToList();

        var basePrompt = doc.RootElement.GetProperty("BasePrompt").GetString();
        if (string.IsNullOrEmpty(basePrompt))
            throw new Exception("Base prompt is empty or not found in the JSON file.");

        return basePrompt;
    }

    public async Task<string> GenerateAnalysis(string userPromptIdea)
{
    if (string.IsNullOrEmpty(userPromptIdea))
        throw new ArgumentException("User prompt cannot be null or empty.");

    var basePrompt = await GetBasePromptService();
    var fullPrompt = basePrompt + "\n" + userPromptIdea.Trim();

    var url = $"https://api.replicate.com/v1/models/{_model}/predictions";

    var body = new
    {
        input = new
        {
            prompt = fullPrompt,
            max_tokens = 8192,
            system_prompt = " You are a helpful assistant that generates text based on the provided prompt.",
            extended_thinking = false,
            max_image_resolution = 0.5,
            thinking_budget_tokens = 1024
        }
    };

    var json = JsonSerializer.Serialize(body);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    var aiResponse = await _httpClient.PostAsync(url, content);

    if (!aiResponse.IsSuccessStatusCode)
        throw new Exception($"Error calling AI service: {aiResponse.ReasonPhrase}");

    var responseContent = await aiResponse.Content.ReadAsStringAsync();
    using var responseJson = JsonDocument.Parse(responseContent);

    if (responseJson.RootElement.TryGetProperty("output", out var outputElement))
    {
        string rawOutput = outputElement.ValueKind switch
        {
            JsonValueKind.Array => string.Join(" ", outputElement.EnumerateArray()
                                                        .Select(e => e.GetString()?.Trim())
                                                        .Where(s => !string.IsNullOrEmpty(s))),
            JsonValueKind.String => outputElement.GetString() ?? string.Empty,
            _ => throw new Exception("Output format not supported."),
        };

        string cleaned = System.Text.RegularExpressions.Regex.Replace(rawOutput, @"\r?\n", " ");
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ");

        return cleaned.Trim();
    }

    var predictionId = responseJson.RootElement.GetProperty("id").GetString();
    if (string.IsNullOrEmpty(predictionId))
        throw new Exception("Prediction ID not found in the response.");

    var timeout = DateTime.UtcNow.AddMinutes(1);

    while (DateTime.UtcNow < timeout)
    {
        var getResponse = await _httpClient.GetAsync($"https://api.replicate.com/v1/predictions/{predictionId}");
        var getContent = await getResponse.Content.ReadAsStringAsync();

        if (!getResponse.IsSuccessStatusCode)
            throw new Exception($"Error fetching prediction status: {getResponse.ReasonPhrase}");

        using var getDoc = JsonDocument.Parse(getContent);
        var getRoot = getDoc.RootElement;

        var status = getRoot.GetProperty("status").GetString();
        if (status == "succeeded")
        {
            if (getRoot.TryGetProperty("output", out var output))
            {
                string rawOutput = output.ValueKind switch
                {
                    JsonValueKind.String => output.GetString() ?? string.Empty,
                    JsonValueKind.Array => string.Join(" ", output.EnumerateArray()
                                                    .Select(e => e.GetString()?.Trim())
                                                    .Where(s => !string.IsNullOrEmpty(s))),
                    _ => throw new Exception("Output format not supported."),
                };

                string cleaned = System.Text.RegularExpressions.Regex.Replace(rawOutput, @"\r?\n", " ");
                cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ");

                return cleaned.Trim();
            }

            throw new Exception("Output not found in the prediction response.");
        }
        else if (status == "failed")
        {
            throw new Exception("Prediction failed.");
        }

        await Task.Delay(2500);
    }

    throw new Exception("Prediction timeout: response not completed in expected time.");
}

}