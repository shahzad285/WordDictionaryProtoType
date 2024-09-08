using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordDictionaryProtoType.Helper;

namespace WordDictionaryProtoType.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DictionaryController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DictionaryController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet(Name = "GetDefinition")]
        public async Task<IActionResult> Get(string word)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.txt");
            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            string meaning = DictionaryFile.ReadDictionary(filePath, word.ToLower().Trim());

            // Return the content as a response
            return Ok(meaning);
           
        }
    }
}
