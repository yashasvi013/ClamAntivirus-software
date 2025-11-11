using Microsoft.AspNetCore.Mvc;
using ClamAVMicroservice.Models;
using ClamAVMicroservice.Services;

namespace ClamAVMicroservice.Controllers
{
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    [ApiController]
    public class GenAIController : ControllerBase
    {
        private readonly IGenAIService _genAIService;

        public GenAIController(IGenAIService genAIService)
        {
            _genAIService = genAIService;
        }

        [Route("/getVirusInfo/{virusName}")]
        [Produces(typeof(GenAI))]
        [HttpGet]
        public async Task<GenAI> GetVirusInfo([FromRoute] string virusName)
        {
            return await _genAIService.GetVirusInfoAsync(virusName);
        }

        [Route("/getGPTResponse/{prompt}")]
        [Produces(typeof(GenAI))]
        [HttpGet]
        public async Task<GenAI> GetGPTResponse([FromRoute] string prompt)
        {
            return await _genAIService.GetGPTResponseAsync(prompt);
        }
    }
}
