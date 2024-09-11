using Xunit;
using System.Windows.Forms;
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
        private ListBox testNameList;
        private ListBox barcodeList;
        private ListBox testTimeList;
        private Label testTimeCount;
        private TextBox testLogPath;

        public FileProcessorTests()
        {
            fileProcessor = new FileProcessor();
            testNameList = new ListBox();
            barcodeList = new ListBox();
            testTimeList = new ListBox();
            testTimeCount = new Label();
            testLogPath = new TextBox();
            fileProcessor.Initialize(testNameList, barcodeList, testTimeList, testTimeCount, testLogPath);
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
            Assert.Contains("TestName", testNameList.Items.Cast<string>());
            Assert.Contains("Barcode", barcodeList.Items.Cast<string>());
            Assert.Contains("TestTime", testTimeList.Items.Cast<string>());
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
        private HashSet<string> listBoxItems = new HashSet<string>();
        private ListBox TestName_List;
        private ListBox Barcode_List;
        private ListBox TestTime_List;
        private Label TestTime_count;
        private TextBox TestLogPath;

        public void Initialize(ListBox testNameList, ListBox barcodeList, ListBox testTimeList, Label testTimeCount, TextBox testLogPath)
        {
            TestName_List = testNameList;
            Barcode_List = barcodeList;
            TestTime_List = testTimeList;
            TestTime_count = testTimeCount;
            TestLogPath = testLogPath;
        }

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

                await AddUniqueItemToListAsync(TestName_List, testName);
                await AddUniqueItemToListAsync(Barcode_List, barcode);
                await AddUniqueItemToListAsync(TestTime_List, testTime);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error processing file {fullPath}: {ex.Message}");
            }
        }

        public async Task AddUniqueItemToListAsync(ListBox listBox, string item)
        {
            await Task.Run(() =>
            {
                if (listBoxItems.Add(item))
                {
                    listBox.Invoke((MethodInvoker)delegate
                    {
                        listBox.Items.Add(item);
                    });
                }
            });
        }
    }
}
