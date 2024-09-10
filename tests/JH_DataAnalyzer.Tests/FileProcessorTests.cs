using Xunit;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace JH_DataAnalyzer.Tests
{
    public class FileProcessorTests
    {
        [Fact]
        public void TestFileProcessing()
        {
            // Arrange
            var fileProcessor = new FileProcessor();
            var testNameList = new ListBox();
            var barcodeList = new ListBox();
            var testTimeList = new ListBox();
            var testTimeCount = new Label();
            var testLogPath = new TextBox();

            fileProcessor.Initialize(testNameList, barcodeList, testTimeList, testTimeCount, testLogPath);

            // 테스트용 임시 디렉토리 생성
            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                // 테스트용 CSV 파일 생성
                File.WriteAllText(Path.Combine(tempDir, "test_123_20230101_BC001_TestName.csv"), "Test content");

                // Act
                fileProcessor.ProcessFolder(tempDir);

                // Assert
                Assert.Single(testNameList.Items);
                Assert.Equal("TestName", testNameList.Items[0]);
                Assert.Single(barcodeList.Items);
                Assert.Equal("BC001", barcodeList.Items[0]);
                Assert.Single(testTimeList.Items);
                Assert.Equal("20230101", testTimeList.Items[0]);
            }
            finally
            {
                // 테스트 후 임시 디렉토리 삭제
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void TestFindTargetFiles()
        {
            // Arrange
            var fileProcessor = new FileProcessor();
            var csvFiles = new string[]
            {
                @"C:\test_123_20230101_BC001_TestName1.csv",
                @"C:\test_456_20230102_BC002_TestName2.csv",
                @"C:\test_789_20230103_BC003_TestName3.csv"
            };

            var testNames = new List<string> { "TestName1", "TestName3" };
            var barcodes = new List<string> { "BC001", "BC003" };
            var testTimes = new List<string> { "20230101", "20230103" };

            // Act
            var result = fileProcessor.FindTargetFiles(csvFiles, testNames, barcodes, testTimes);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(@"C:\test_123_20230101_BC001_TestName1.csv", result);
            Assert.Contains(@"C:\test_789_20230103_BC003_TestName3.csv", result);
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

        public List<string> FindTargetFiles(string[] csvFiles, List<string> testNames, List<string> barcodes, List<string> testTimes)
        {
            var testNameSet = new HashSet<string>(testNames);
            var barcodeSet = new HashSet<string>(barcodes);
            var testTimeSet = new HashSet<string>(testTimes);

            return csvFiles.Where(file =>
            {
                string[] parts = Path.GetFileNameWithoutExtension(file).Split('_');
                return parts.Length >= 5 &&
                       testTimeSet.Contains(parts[3]) &&
                       barcodeSet.Contains(parts[4]) &&
                       testNameSet.Contains(string.Join("_", parts.Skip(5)));
            }).ToList();
        }

        public void ProcessFolder(string folderPath)
        {
            var csvFiles = Directory.GetFiles(folderPath, "*.csv");
            var fileInfo = csvFiles
                .Select(file => new FileInfo(file))
                .Select(fi => new { Parts = Path.GetFileNameWithoutExtension(fi.Name).Split('_'), FullName = fi.FullName })
                .Where(x => x.Parts.Length >= 5);

            foreach (var file in fileInfo)
            {
                ProcessFile(file.Parts, file.FullName);
            }
        }

        public void ProcessFile(string[] parts, string fullPath)
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

            AddUniqueItemToList(TestName_List, testName);
            AddUniqueItemToList(Barcode_List, barcode);
            AddUniqueItemToList(TestTime_List, testTime);
        }

        public void AddUniqueItemToList(ListBox listBox, string item)
        {
            if (listBoxItems.Add(item))
            {
                listBox.Items.Add(item);
            }
        }
    }
}