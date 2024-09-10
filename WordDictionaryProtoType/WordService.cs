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
                string line;
                long currentOffset = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    // Use a StringBuilder to handle the meaning extraction
                    var meaningBuilder = new StringBuilder();
                    bool insideQuotes = false;
                    int i = 0;

                    // Iterate through each character in the line
                    while (i < line.Length)
                    {
                        char currentChar = line[i];

                        // Toggle the insideQuotes flag when encountering a double quote
                        if (currentChar == '"')
                        {
                            insideQuotes = !insideQuotes;
                        }
                        else if (currentChar == ',' && !insideQuotes)
                        {
                            // If we encounter a comma outside of quotes, we've reached the end of the word
                            break;
                        }
                        else
                        {
                            // Append the character to the meaning builder
                            meaningBuilder.Append(currentChar);
                        }

                        i++;
                    }

                    // Split the word and meaning
                    string word = line.Substring(0, i).Trim().ToLower(); // Get the word
                    string meaning = meaningBuilder.ToString().Trim(); // Get the meaning

                    // Store the offset if we have a valid word and meaning
                    if (!string.IsNullOrEmpty(word) && !string.IsNullOrEmpty(meaning))
                    {
                        long offset = currentOffset; // Store the current offset
                        WordOffsets[word] = offset; // Add the word and its offset to the dictionary
                    }

                    // Update the current offset to the next line's starting index
                    currentOffset += line.Length + 1; // +1 for the newline character
                }
            }
        }
        public string GetMeaning(string word)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Dictionary.csv");

            // Check if the word exists in the dictionary
            if (WordOffsets.TryGetValue(word, out var desiredOffset))
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream))
                {
                    // Seek to the desired offset
                    stream.Seek(desiredOffset, SeekOrigin.Begin);

                    // Read the line starting from the offset
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        // Use a more robust CSV parsing approach
                        var csvLine = new List<string>();
                        var inQuotes = false;
                        var currentField = new StringBuilder();

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
