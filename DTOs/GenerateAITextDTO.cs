using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TrendingAPI.DTOs;


public class GenerateAITextDTO
{
    //FIXED 

    public string BasePrompt { get; set; } = string.Empty;
    //NOT FIXED
    public string UserPrompt { get; set; } = string.Empty;

}
