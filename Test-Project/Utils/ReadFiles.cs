using Newtonsoft.Json;
using Test_Project.TestFile;

namespace TestProject.Utils
{
    public static class ReadFiles
    {
        public static dynamic? jsonFile { get; set; }
        public static List<string> customerCsvColumns = new List<string>() { "ID", "Email", "First Name", "Last Name", "Phone", "Bill To Address 1", "Bill To Address 2", "Bill To City", "Bill To State", "Bill To Postal Code", "Bill To Country", "Ship To Address 1", "Ship To Address 2", "Ship To City", "Ship To State", "Ship To Postal Code", "Ship To Country", "Title", "Registered", "Organization", "Audience Rewards Number", "Marketing Opt In 1", "Marketing Opt In 2", "Is Anonymized", "Last Updated" };

        public static List<string> GetCSVColumnValues(string csvFilePath, string columnName)
        {
            var fileOutput = File.ReadAllLines(csvFilePath);
            List<string> values = new List<string>();

            int columnIndex = Array.IndexOf(fileOutput[0].Split(','), columnName);

            if (columnIndex != -1)
            {
                foreach (string line in fileOutput.Skip(1))
                {
                    string[] delimitedLine = line.Split(',');
                    values.Add(delimitedLine[columnIndex]);
                }
            }
            else
            {
                throw new Exception($"ColumnName {columnName} Not Found in CSV File");
            }
            return values;
        }

        public static bool VerifyCSVColumnIsPresent(string csvFilePath, string columnName)
        {
            var fileOutput = File.ReadAllLines(csvFilePath);

            int columnIndex = Array.IndexOf(fileOutput[0].Split(','), columnName);
            return columnIndex != -1;
        }

        public static bool VerifyAllCSVColumnsIsPresent(string csvFilePath)
        {
            string[] readAllColumns = File.ReadAllLines(csvFilePath);

            var csvHeaders = readAllColumns[0].Split(',').ToList();

            var isMissingHeaders = customerCsvColumns.SequenceEqual(csvHeaders);

            return isMissingHeaders;
        }

        public static void ReadJsonFile(string jsonFileIn)
        {
            if (File.Exists(jsonFileIn))
            {
                jsonFile = JsonConvert.DeserializeObject(File.ReadAllText(jsonFileIn));
            }
            else
            {
                BaseTest.LogMessage($"JSON Directory dosen't exists ");
            }
        }

        public static string getValuesForGivenKey(string keyName, string sectionName = null)
        {
            var json = JsonConvert.DeserializeObject(jsonFile.ToString());
            return !string.IsNullOrWhiteSpace(sectionName) ? (string)json[sectionName][0][keyName] : (string)json[keyName];
        }
    }
}