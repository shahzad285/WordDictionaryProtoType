using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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

        [HttpGet(Name = "GetDefinition")]
        public async Task<IActionResult> Get(string word)
        {
            
            // Return the content as a response
            return Ok(_wordService.GetMeaning(word.ToLower()));

        }

        [HttpPost(Name = "WriteDefinitions")]
        public async Task<IActionResult> WriteDefinitions()
        {
            int numberOfWords = 170000;
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            StringBuilder csvContent = new StringBuilder();
            
            string[] meaningWords = {
        "apple", "book", "cat", "dog", "elephant", "flower", "guitar", "house", "ice cream", "jungle",
        "kite", "lemon", "mountain", "notebook", "ocean", "pencil", "quilt", "river", "sun", "tree",
        "umbrella", "violin", "waterfall", "xylophone", "yacht", "zebra", "airplane", "balloon", "camera",
        "dolphin", "eagle", "firefly", "giraffe", "helicopter", "island", "jellyfish", "kangaroo", "lighthouse",
        "moonlight", "nest", "oasis", "pyramid", "quantum", "rainbow", "submarine", "telescope", "unicorn",
        "volcano", "windmill", "x-ray", "yak", "zeppelin", "algorithm", "binary", "code", "data", "encryption",
        "firewall", "gigabyte", "hardware", "internet", "javascript", "keyboard", "laptop", "monitor", "network",
        "operating system", "processor", "query", "router", "software", "terabyte", "upload", "virtual reality",
        "website", "xml", "year 2000", "zip file"
    };

            string[] punctuation = { ",", "\\n", "\"" };
            Random random = new Random();

            string[] dataTemplates = {
        "id: {0}, type: {1}",
        "count: {0}, category: {1}",
        "score: {0}, label: {1}",
        "index: {0}, name: {1}",
        "priority: {0}, status: {1}"
    };

            for (int i = 1; i <= numberOfWords; i++)
            {
                string word = "Word" + i;

                List<string> meaningParts = new List<string>();
                int meaningLength = random.Next(3,5);

                for (int j = 0; j < meaningLength; j++)
                {
                    if (random.Next(4) == 0)  // 1 in 4 chance to add data
                    {
                        string template = dataTemplates[random.Next(dataTemplates.Length)];
                        int randomNumber = random.Next(1, 1001);
                        string randomType = meaningWords[random.Next(meaningWords.Length)];
                        string data = string.Format(template, randomNumber, randomType);
                        meaningParts.Add($"\"{data}\"");
                    }
                    else
                    {
                        meaningParts.Add(meaningWords[random.Next(meaningWords.Length)]);
                    }

                    if (j < meaningLength - 1 && random.Next(2) == 0)
                    {
                        meaningParts.Add(punctuation[random.Next(punctuation.Length)]);
                    }
                }

                string meaning = string.Join(" ", meaningParts);
                meaning = meaning.Replace("\"", "\"\"");

                csvContent.AppendLine($"{word},\"{meaning}\"");
            }

            await System.IO.File.WriteAllTextAsync(filePath, csvContent.ToString());
            Console.WriteLine($"CSV file '{filePath}' created with {numberOfWords} words and meanings.");

            return Ok();
        }
    }
}
