using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using WordDictionaryProtoType.DTOs;

namespace WordDictionaryProtoType.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DictionaryController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly WordService _wordService;
        public DictionaryController(IWebHostEnvironment webHostEnvironment, WordService wordService)
        {
            _webHostEnvironment = webHostEnvironment;
            _wordService = wordService;
        }

        [HttpGet("GetDefinition")]
        public async Task<IActionResult> Get(string word)
        {

            // Return the content as a response
            return Ok(await _wordService.GetMeaning(word.ToLower()));

        }

        [HttpPost("WriteDefinitions")]
        public async Task<IActionResult> WriteDefinitions()
        {
            return Ok(await _wordService.WriteDefinitons());
        }

        [HttpPost("UpdateDefinition")]
        public async Task<IActionResult> UpdateDefinition(UpdateDefinitionDTO updateDefinitionDTO)
        {
            return Ok(await _wordService.UpdateDefinition(updateDefinitionDTO));
        }
    }
}
