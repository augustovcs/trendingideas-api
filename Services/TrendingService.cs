using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TrendingAPI.Interfaces;

namespace TrendingAPI.Services;

public class TrendingService : ITrendingService
{

    // Constructors to be implemented

    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    private readonly string _model;

    public TrendingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        // API CONFIG HERE -> need to change the appsettings to
        // appsettings.develop

        _apiToken = configuration["ApiToken"]
                    ?? throw new Exception("ApiToken não configurado no appsettings.json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
        _httpClient.DefaultRequestHeaders.Add("Prefer", "wait");


    }



    public async Task<string> GenerateAnalysis(string basePrompt, string userPromptIdea)
    {

        // Service to be implemented 
        var fullPrompt = userPromptIdea + basePrompt;
        return null;
    }
}