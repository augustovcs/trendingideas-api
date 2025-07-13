using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrendingAPI.DTOs;
using TrendingAPI.Interfaces;

namespace TrendingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrendingAnalysisController : ControllerBase
    {
        private readonly ITrendingService _trendingService;

        public TrendingAnalysisController(ITrendingService trendingService)
        {
            _trendingService = trendingService;
        }

        [HttpGet("baseprompt_example")]
        public async Task<IActionResult> GetBasePrompt()
        {
            try
            {
                var prompt = await _trendingService.GetBasePromptService();
                return Ok(new { prompt });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Erro ao ler o prompt base.");
            }
        }

        [HttpPost("output_prompt")]
        public async Task<IActionResult> GenerateOutput([FromBody] OptionalTextDTO req)
        {
            if (string.IsNullOrWhiteSpace(req.UserPrompt))
                return BadRequest("User prompt não pode ser vazio.");

            try
            {
                var basePrompt = await _trendingService.GetBasePromptService();
                if (string.IsNullOrWhiteSpace(basePrompt))
                    return BadRequest("Prompt base indisponível.");

                var output = await _trendingService.GenerateAnalysis(req.UserPrompt);

            
                return Ok(new { output });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gerar a análise: {ex.Message}");
            }
        }

    }
}
