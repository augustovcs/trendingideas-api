namespace TrendingAPI.Interfaces;

public interface ITrendingService
{
    Task<string> GenerateAnalysis(string basePrompt, string userPromptIdea);
}