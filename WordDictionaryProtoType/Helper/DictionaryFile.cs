namespace WordDictionaryProtoType.Helper
{
    public class DictionaryFile
    {
        public static string ReadDictionary(string filePath, string searchWord)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                return "Dictionary file not found.";
            }

            // Read all lines in the file
            string[] lines = File.ReadAllLines(filePath);

            // Iterate through each line
            foreach (string line in lines)
            {
                // Split the line into word and meaning
                string[] parts = line.Split(',', 2); // Split only on the first comma
                if (parts.Length < 2) continue; // Skip if the line is malformed

                string word = parts[0].Trim();
                string meaning = parts[1].Trim();

                // Check if the word matches the search word (case-insensitive)
                if (string.Equals(word, searchWord, StringComparison.OrdinalIgnoreCase))
                {
                    return meaning; // Return the meaning if found
                }
            }

            return "Word not found in the dictionary.";
        }
    }
}
