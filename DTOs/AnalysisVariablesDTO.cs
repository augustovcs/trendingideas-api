namespace TrendingAPI.Controllers;


//  In production - not yet implemented or tried of
public class AnalysisVariablesDTO
{
    public List<string> VariablesList { get; set; } = new List<string>
    {
        "resumedVariable",
        "marketVariable",
        "brainstormingVariable"
    };



}