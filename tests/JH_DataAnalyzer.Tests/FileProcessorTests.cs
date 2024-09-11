using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JH_DataAnalyzer.Tests
{
    public class FileProcessorTests : IDisposable
    {
        private readonly string testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        private FileProcessor fileProcessor;

        public FileProcessorTests()
        {
            fileProcessor = new FileProcessor();
            Directory.CreateDirectory(testDataPath);
        }

        [Fact]
        public async Task TestProcessFolder()
        {
            // Arrange
            File.WriteAllText(Path.Combine(testDataPath, "2023_01_01_TestTime_Barcode_TestName.csv"), "Some content");

            // Act
            await fileProcessor.ProcessFolderAsync(testDataPath);

            // Assert
            Assert.Contains("TestName", fileProcessor.TestNames);
            Assert.Contains("Barcode", fileProcessor.Barcodes);
            Assert.Contains("TestTime", fileProcessor.TestTimes);
        }

        [Fact]
        public async Task TestFindTargetFiles()
        {
            // Arrange
            File.WriteAllText(Path.Combine(testDataPath, "2023_01_01_TestTime1_Barcode1_TestName1.csv"), "");
            File.WriteAllText(Path.Combine(testDataPath, "2023_01_01_TestTime2_Barcode2_TestName2.csv"), "");

            var testNames = new List<string> { "TestName1", "TestName2" };
            var barcodes = new List<string> { "Barcode1", "Barcode2" };
            var testTimes = new List<string> { "TestTime1", "TestTime2" };

            // Act
            var files = await fileProcessor.FindTargetFilesAsync(Directory.GetFiles(testDataPath, "*.csv"), testNames, barcodes, testTimes);

            // Assert
            Assert.Equal(2, files.Count);
        }

        public void Dispose()
        {
            if (Directory.Exists(testDataPath))
            {
                Directory.Delete(testDataPath, true);
            }
        }
    }

    public class FileProcessor
    {
        private HashSet<string> uniqueItems = new HashSet<string>();
        public List<string> TestNames { get; } = new List<string>();
        public List<string> Barcodes { get; } = new List<string>();
        public List<string> TestTimes { get; } = new List<string>();

        public async Task<List<string>> FindTargetFilesAsync(string[] csvFiles, List<string> testNames, List<string> barcodes, List<string> testTimes)
        {
            var testNameSet = new HashSet<string>(testNames);
            var barcodeSet = new HashSet<string>(barcodes);
            var testTimeSet = new HashSet<string>(testTimes);

            return await Task.Run(() => csvFiles.Where(file =>
            {
                string[] parts = ParseFileName(file);
                return parts.Length >= 5 &&
                       testTimeSet.Contains(parts[3]) &&
                       barcodeSet.Contains(parts[4]) &&
                       testNameSet.Contains(string.Join("_", parts.Skip(5)));
            }).ToList());
        }

        public async Task ProcessFolderAsync(string folderPath)
        {
            var csvFiles = Directory.GetFiles(folderPath, "*.csv");
            var fileInfoTasks = csvFiles.Select(async file =>
            {
                var fi = new FileInfo(file);
                var parts = ParseFileName(fi.Name);
                return new { Parts = parts, FullName = fi.FullName };
            });

            var fileInfos = await Task.WhenAll(fileInfoTasks);
            var validFiles = fileInfos.Where(x => x.Parts.Length >= 5);

            foreach (var file in validFiles)
            {
                await ProcessFileAsync(file.Parts, file.FullName);
            }
        }

        private string[] ParseFileName(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName).Split('_');
        }

        public async Task ProcessFileAsync(string[] parts, string fullPath)
        {
            try
            {
                string testTime = parts[3];
                string barcode = parts[4];
                string testName = string.Join("_", parts.Skip(5));

                if (parts[3].StartsWith("JH") || parts[3].StartsWith("JG"))
                {
                    testTime = parts[2];
                    barcode = parts[3];
                    testName = string.Join("_", parts.Skip(4));
                }

                await AddUniqueItemAsync(TestNames, testName);
                await AddUniqueItemAsync(Barcodes, barcode);
                await AddUniqueItemAsync(TestTimes, testTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {fullPath}: {ex.Message}");
            }
        }

        public async Task AddUniqueItemAsync(List<string> list, string item)
        {
            await Task.Run(() =>
            {
                if (uniqueItems.Add(item))
                {
                    list.Add(item);
                }
            });
        }
    }
}
