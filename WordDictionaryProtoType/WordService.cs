using System;
using System.Text;
using WordDictionaryProtoType.DTOs;

namespace WordDictionaryProtoType
{
    public class WordService
    {
        public Dictionary<string, long> WordOffsets { get; private set; }
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WordService(IWebHostEnvironment webHostEnvironment)
        {
            WordOffsets = new Dictionary<string, long>();
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task PopulateWordOffsets()
        {
            WordOffsets = new Dictionary<string, long>();
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");
            using (StreamReader reader = new StreamReader(filePath))
            {
                StringBuilder lineBuilder = new StringBuilder();
                long currentOffset = 0;
                long startingOffset = currentOffset;
                bool inQuotes = false;

                while (reader.Peek() >= 0)
                {
                    char currentChar = (char)reader.Read();
                    lineBuilder.Append(currentChar);
                    currentOffset += Encoding.UTF8.GetByteCount(new[] { currentChar });

                    if (currentChar == '"')
                    {
                        inQuotes = !inQuotes;
                    }

                    if ((currentChar == '\n' || (currentChar == '\r' && reader.Peek() != '\n')) && !inQuotes)
                    {
                        string line = lineBuilder.ToString();
                        lineBuilder.Clear();

                        int commaIndex = line.IndexOf(',');
                        if (commaIndex > 0)
                        {
                            string word = line.Substring(0, commaIndex).Trim().ToLower();
                            string meaning = line.Substring(commaIndex + 1).Trim();

                            if (!string.IsNullOrEmpty(word) && !string.IsNullOrEmpty(meaning))
                            {
                                WordOffsets[word] = startingOffset; // Adjust for newline
                                startingOffset = currentOffset;
                            }
                        }

                        // Adjust for CRLF
                        if (currentChar == '\r' && reader.Peek() == '\n')
                        {
                            reader.Read(); // Consume LF
                            currentOffset += Encoding.UTF8.GetByteCount(new[] { '\n' });
                        }
                    }
                }
            }
        }
        public async Task<string> GetMeaning(string word)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");

            if (WordOffsets.TryGetValue(word.ToLower(), out var desiredOffset))
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    reader.BaseStream.Seek(desiredOffset, SeekOrigin.Begin);
                    reader.DiscardBufferedData();

                    var csvLine = new StringBuilder();
                    bool inQuotes = false;

                    while (reader.Peek() >= 0)
                    {
                        char currentChar = (char)reader.Read();
                        csvLine.Append(currentChar);

                        if (currentChar == '"')
                        {
                            inQuotes = !inQuotes;
                        }

                        if (currentChar == '\n' && !inQuotes)
                        {
                            break; // End of line reached
                        }
                    }

                    string line = csvLine.ToString();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var fields = new List<string>();
                        var currentField = new StringBuilder();
                        inQuotes = false;

                        foreach (char c in line)
                        {
                            if (c == '"')
                            {
                                inQuotes = !inQuotes;
                            }
                            else if (c == ',' && !inQuotes)
                            {
                                fields.Add(currentField.ToString());
                                currentField.Clear();
                            }
                            else
                            {
                                currentField.Append(c);
                            }
                        }

                        fields.Add(currentField.ToString());

                        if (fields.Count >= 2)
                        {
                            string meaning = fields[1].Trim('"').Replace("\"\"", "\"");
                            return meaning;
                        }
                    }
                }
            }
            return "Word not found";
        }

        public async Task<string> WriteDefinitons()
        {
            int numberOfWords = 170000;
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");

            if (!System.IO.File.Exists(filePath))
            {
                return "File not found.";
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
                int meaningLength = random.Next(3, 5);

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
            await PopulateWordOffsets();

            return "Dictionary populated";
        }

        public async Task<string> UpdateDefinition(DefinitionDTO updateDefinitionDTO)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");

            if (!WordOffsets.TryGetValue(updateDefinitionDTO.Word.ToLower(), out var targetOffset))
            {
                return "Word not found";
            }

            string tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary_temp.csv");
            try
            {
                using (var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var writer = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var sortedOffsets = WordOffsets.OrderBy(kv => kv.Value).ToList();

                    long currentPosition = 0;
                    long newWordOffset = -1;
                    for (int i = 0; i < sortedOffsets.Count; i++)
                    {
                        var currentWord = sortedOffsets[i].Key;
                        var currentOffset = sortedOffsets[i].Value;
                        var nextOffset = (i < sortedOffsets.Count - 1) ? sortedOffsets[i + 1].Value : reader.Length;

                        // Copy content from current position to the start of the current word
                        if (currentOffset > currentPosition)
                        {
                            await CopyFileContentAsync(reader, writer, currentPosition, currentOffset);
                        }


                        if (currentOffset == targetOffset)
                        {
                            // This is the word we want to update
                            string newDefinition = updateDefinitionDTO.Meaning.Replace("\"", "\"\"");
                            string updatedLine = $"{updateDefinitionDTO.Word},\"{newDefinition}\"\n";
                            await writer.WriteAsync(Encoding.UTF8.GetBytes(updatedLine));
                        }
                        else
                        {
                            // Copy the original word and its definition
                            await CopyFileContentAsync(reader, writer, currentOffset, nextOffset);
                        }

                        currentPosition = nextOffset;
                    }

                    // Copy any remaining content
                    if (currentPosition < reader.Length)
                    {
                        await CopyFileContentAsync(reader, writer, currentPosition, reader.Length);
                    }
                }

                // Replace the original file with the temporary file
                File.Delete(filePath);
                File.Move(tempFilePath, filePath);

                await PopulateWordOffsets();
                return "Definition updated successfully";
            }
            catch (IOException ex)
            {
                return $"Error updating file: {ex.Message}";
            }
        }

        public async Task<string> InsertWord(DefinitionDTO definitionDTO)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");
            string tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary_temp.csv");

            // Check if the word already exists
            if (WordOffsets.ContainsKey(definitionDTO.Word.ToLower()))
            {
                return "Word already exists";
            }

            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                using (var writer = new StreamWriter(tempFilePath, false, Encoding.UTF8))
                {
                    bool wordInserted = false;
                    var csvLine = new StringBuilder();
                    bool inQuotes = false;

                    // Read the file character by character
                    while (reader.Peek() >= 0)
                    {
                        char currentChar = (char)reader.Read();
                        if (!(currentChar == '\n' && !inQuotes))
                        {
                            csvLine.Append(currentChar);
                        }

                        if (currentChar == '"')
                        {
                            inQuotes = !inQuotes;

                        }

                        // Handle line endings
                        if (currentChar == '\n' && !inQuotes)
                        {
                            string currentLine = csvLine.ToString().TrimEnd('\r'); // Trim carriage returns
                            csvLine.Clear(); // Clear csvLine for the next line

                            if (!string.IsNullOrWhiteSpace(currentLine)) // Ensure we're not writing blank lines
                            {
                                var fields = SplitCsvLine(currentLine);

                                if (fields.Count >= 2)
                                {
                                    string currentWord = fields[0].ToLower();

                                    // Insert the new word in the correct place
                                    if (!wordInserted && string.Compare(definitionDTO.Word.ToLower(), currentWord) < 0)
                                    {
                                        await writer.WriteLineAsync($"{definitionDTO.Word.ToLower()},\"{definitionDTO.Meaning.Replace("\"", "\"\"")}\"");
                                        wordInserted = true;
                                    }

                                    // Write the current line from the original file to the temporary file
                                    await writer.WriteLineAsync(currentLine);
                                }
                            }
                        }

                    }

                    // If the word wasn't inserted yet, append it to the end
                    if (!wordInserted)
                    {
                        await writer.WriteLineAsync($"{definitionDTO.Word.ToLower()},\"{definitionDTO.Meaning.Replace("\"", "\"\"")}\"");
                    }
                }

                // Replace the original file with the updated one
                File.Delete(filePath);
                File.Move(tempFilePath, filePath);

                // Recalculate word offsets after the word is inserted
                await PopulateWordOffsets();

                return "Word inserted successfully";
            }
            catch (IOException ex)
            {
                return $"Error updating file: {ex.Message}";
            }
        }

        private List<string> SplitCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Handle escaped double quote
                        currentField.Append('"');
                        i++; // Skip the next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            fields.Add(currentField.ToString());
            return fields;
        }

        private async Task CopyFileContentAsync(FileStream source, FileStream destination, long startOffset, long endOffset)
        {
            // Ensure startOffset and endOffset are non-negative
            if (startOffset < 0 || endOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startOffset), "Offsets must be non-negative.");
            }

            // Ensure startOffset is not greater than endOffset
            if (startOffset > endOffset)
            {
                throw new ArgumentException("Start offset must be less than or equal to end offset.");
            }

            byte[] buffer = new byte[8192];
            long bytesToRead = endOffset - startOffset;

            source.Position = startOffset;

            while (bytesToRead > 0)
            {
                int bytesRead = await source.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, bytesToRead));
                if (bytesRead == 0) break;

                await destination.WriteAsync(buffer, 0, bytesRead);
                bytesToRead -= bytesRead;
            }
        }


    }
}
