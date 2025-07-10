using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrendingAPI.DTOs;
using TrendingAPI.Services;
using TrendingAPI.Interfaces;


namespace TrendingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrendingAnalysisController : ControllerBase
    {
        private readonly ITrendingService _trendingApiService;

        public TrendingAnalysisController(ITrendingService trendingApiService)
        {
            _trendingApiService = trendingApiService;
        }


        // to implement here a get base prompt
        [HttpGet("baseprompt_example")]
        public async Task<IActionResult> GetBasePrompt()
        {
            try
            {
                var basePath = AppContext.BaseDirectory;
                var jsonPath = Path.Combine(basePath, "..", "..", "Settings", "basePrompt.json");
                jsonPath = Path.GetFullPath(jsonPath);

                Console.WriteLine($"[LOG] Base Prompt Path mounted: {jsonPath} ");
                if (!System.IO.File.Exists(jsonPath))
                {
                    Console.WriteLine($"[ERROR LOG] The base prompt path was not found {jsonPath}");
                    return NotFound($"Base prompt not found in: {jsonPath}");
                }
            }

            // please dont run it, possibly null or error occurrence, made this first
            catch
            {
                return null;

            }
            return null;
        }

        // to implement here a post request -> user prompt -> AI output.
        [HttpPost("output_prompt")]
        public async Task<IActionResult> GenerateOutput()
        {
            return null;
        }
    }
}