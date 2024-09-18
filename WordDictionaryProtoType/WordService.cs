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
                                WordOffsets[word] = currentOffset - Encoding.UTF8.GetByteCount(new[] { currentChar }); // Adjust for newline
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
        public string GetMeaning(string word)
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
    }
}
