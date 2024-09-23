﻿using System.Text;
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

        public async Task<string> UpdateDefinition(UpdateDefinitionDTO updateDefinitionDTO)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");
            if (!WordOffsets.TryGetValue(updateDefinitionDTO.Word.ToLower(), out var desiredOffset))
            {
                return "Word not found";
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            using (var reader = new StreamReader(stream, Encoding.UTF8, true, -1, true))
            using (var writer = new StreamWriter(stream, Encoding.UTF8, -1, true))
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

                    if (fields.Count < 2)
                    {
                        return "Error reading the line";
                    }

                    string word = fields[0];
                    string oldDefinition = fields[1].Trim('"').Replace("\"\"", "\"");

                    // Prepare the new line
                    string newDefinition = updateDefinitionDTO.Meaning.Replace("\"", "\"\"");
                    string newLine = $"{word},\"{newDefinition}\"\n";

                    // Calculate the difference in length
                    long lengthDifference = newLine.Length - line.Length;

                    if (lengthDifference != 0)
                    {
                        long currentPosition = stream.Position;
                        long fileLength = stream.Length;

                        if (lengthDifference > 0)
                        {
                            // New line is longer, shift right
                            stream.SetLength(fileLength + lengthDifference);
                            for (long pos = fileLength - 1; pos >= currentPosition; pos--)
                            {
                                stream.Seek(pos, SeekOrigin.Begin);
                                int byteValue = stream.ReadByte();
                                stream.Seek(pos + lengthDifference, SeekOrigin.Begin);
                                stream.WriteByte((byte)byteValue);
                            }
                        }
                        else
                        {
                            // New line is shorter, shift left
                            for (long pos = currentPosition; pos < fileLength; pos++)
                            {
                                stream.Seek(pos, SeekOrigin.Begin);
                                int byteValue = stream.ReadByte();
                                stream.Seek(pos + lengthDifference, SeekOrigin.Begin);
                                stream.WriteByte((byte)byteValue);
                            }
                            stream.SetLength(fileLength + lengthDifference);
                        }
                    }

                    // Write the new line
                    stream.Seek(desiredOffset, SeekOrigin.Begin);
                    byte[] newLineBytes = Encoding.UTF8.GetBytes(newLine);
                    await stream.WriteAsync(newLineBytes, 0, newLineBytes.Length);

                    // Write the new line
                    stream.Seek(desiredOffset, SeekOrigin.Begin);
                    await writer.WriteAsync(newLine);
                }

                // Update the WordOffsets dictionary
                await PopulateWordOffsets();
                return "Definition updated successfully";
            }
        }
    }
}
