namespace TrendingAPI.Interfaces;

public interface ITrendingService
{
    Task<string> GenerateAnalysis(string fullPrompt);
    Task<string> GetBasePromptService();
    
}