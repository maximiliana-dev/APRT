using System.Globalization;
using CsvHelper;

namespace aprt
{
    public class FileManager
    {
        static List<PinRecord> records = [];

        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static List<PinRecord> ProcessDirectory(string targetDirectory)
        {
            try
            {
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (string fileName in fileEntries)
                    records.AddRange(ProcessFile(fileName));

                // Recurse into subdirectories of this directory.
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                    ProcessDirectory(subdirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load PIN files because: {ex.Message}");
            }

            return records;
        }

        public static List<PinRecord> ProcessFile(string path)
        {
            try
            {
                using var reader = new StreamReader(path);
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<PinRecordMap>();
                    var records = csv.GetRecords<PinRecord>();
                    return records.ToList();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to process file '{0}'. because: {1}", path, ex.Message);
                return [];
            }
        }
    }
}