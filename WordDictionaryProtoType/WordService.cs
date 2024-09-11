using System.Text;

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

        public void PopulateWordOffsets()
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");
            using (StreamReader reader = new StreamReader(filePath))
            {
                StringBuilder lineBuilder = new StringBuilder();
                long currentOffset = 0;
                bool inQuotes = false;
                char currentChar;

                while (reader.Peek() >= 0)
                {
                    currentChar = (char)reader.Read();
                    lineBuilder.Append(currentChar);

                    // Toggle inQuotes status when encountering a double quote
                    if (currentChar == '"')
                    {
                        inQuotes = !inQuotes;
                    }

                    // Check for end of line
                    if (currentChar == '\n' && !inQuotes)
                    {
                        string line = lineBuilder.ToString();
                        lineBuilder.Clear(); // Clear the builder for the next line

                        // Process the line to extract the word and meaning
                        int commaIndex = line.IndexOf(',');
                        if (commaIndex > 0)
                        {
                            string word = line.Substring(0, commaIndex).Trim().ToLower();
                            string meaning = line.Substring(commaIndex + 1).Trim();

                            // Store the offset if we have a valid word and meaning
                            if (!string.IsNullOrEmpty(word) && !string.IsNullOrEmpty(meaning))
                            {
                                WordOffsets[word] = currentOffset; // Store the current offset
                            }
                        }

                        // Update the current offset to the next line's starting index
                        currentOffset += line.Length + 1; // +1 for the newline character
                    }
                }
            }
        }
        public string GetMeaning(string word)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");

            // Check if the word exists in the dictionary
            if (WordOffsets.TryGetValue(word.ToLower(), out var desiredOffset))
            {
                using (var reader = new StreamReader(filePath))
                {
                    // Seek to the desired offset
                    reader.BaseStream.Seek(desiredOffset, SeekOrigin.Begin);
                    reader.DiscardBufferedData();

                    // Read the line from the current position
                    StringBuilder lineBuilder = new StringBuilder();
                    bool inQuotes = false;
                    char currentChar;

                    // Read characters until we find the end of the line
                    while (reader.Peek() >= 0)
                    {
                        currentChar = (char)reader.Read();

                        if (currentChar == '"')
                        {
                            inQuotes = !inQuotes; // Toggle inQuotes status
                        }
                        else if (currentChar == '\n' && !inQuotes)
                        {
                            break; // End of line
                        }

                        lineBuilder.Append(currentChar);
                    }

                    string line = lineBuilder.ToString();
                    if (!string.IsNullOrEmpty(line))
                    {
                        // Use a more robust CSV parsing approach
                        var csvLine = new List<string>();
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
                                csvLine.Add(currentField.ToString());
                                currentField.Clear();
                            }
                            else
                            {
                                currentField.Append(c);
                            }
                        }

                        // Add the last field
                        csvLine.Add(currentField.ToString());

                        // Ensure we have at least two fields (word and meaning)
                        if (csvLine.Count >= 2)
                        {
                            // The meaning is the second field, with outer quotes removed
                            string meaning = csvLine[1].Trim('"');

                            // Unescape any double quotes within the meaning
                            meaning = meaning.Replace("\"\"", "\"");

                            return meaning;
                        }
                    }
                }
            }

            return "Word not found";
        }
    }
}
