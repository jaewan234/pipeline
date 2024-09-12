using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// JH_DataAnalyzer.Tests는 JH_DataAnalyzer 프로젝트의 단위 테스트를 위한 네임스페이스입니다.
/// </summary>
namespace JH_DataAnalyzer.Tests
{
    /// <summary>
    /// FileProcessor 클래스의 기능을 테스트하는 클래스입니다.
    /// </summary>
    /// <remarks>
    /// 이 클래스는 FileProcessor의 주요 메서드들에 대한 단위 테스트를 포함합니다.
    /// IDisposable을 구현하여 테스트 후 리소스를 정리합니다.
    /// </remarks>
    public class FileProcessorTests : IDisposable
    {
        private readonly string testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        private FileProcessor fileProcessor;

        /// <summary>
        /// FileProcessorTests 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public FileProcessorTests()
        {
            fileProcessor = new FileProcessor();
            Directory.CreateDirectory(testDataPath);
        }

        /// <summary>
        /// ProcessFolderAsync 메서드를 테스트합니다.
        /// </summary>
        /// <remarks>
        /// 이 테스트는 ProcessFolderAsync 메서드가 CSV 파일을 올바르게 처리하는지 확인합니다.
        /// </remarks>
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

        /// <summary>
        /// FindTargetFilesAsync 메서드를 테스트합니다.
        /// </summary>
        /// <remarks>
        /// 이 테스트는 FindTargetFilesAsync 메서드가 주어진 조건에 맞는 파일들을 올바르게 찾는지 확인합니다.
        /// </remarks>
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

        /// <summary>
        /// 테스트에 사용된 리소스를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(testDataPath))
            {
                Directory.Delete(testDataPath, true);
            }
        }
    }

    /// <summary>
    /// CSV 파일 처리를 담당하는 클래스입니다.
    /// </summary>
    /// <remarks>
    /// 이 클래스는 CSV 파일을 읽고, 데이터를 분석하며, 결과를 저장합니다.
    /// 주요 기능으로는 폴더 내 CSV 파일 처리, 개별 파일 처리, 중복 항목 제거 등이 있습니다.
    /// </remarks>
    public class FileProcessor
    {
        private HashSet<string> uniqueItems = new HashSet<string>();

        /// <summary>
        /// 처리된 테스트 이름들의 목록입니다.
        /// </summary>
        public List<string> TestNames { get; } = new List<string>();

        /// <summary>
        /// 처리된 바코드들의 목록입니다.
        /// </summary>
        public List<string> Barcodes { get; } = new List<string>();

        /// <summary>
        /// 처리된 테스트 시간들의 목록입니다.
        /// </summary>
        public List<string> TestTimes { get; } = new List<string>();

        /// <summary>
        /// 주어진 조건에 맞는 CSV 파일들을 찾습니다.
        /// </summary>
        /// <param name="csvFiles">검색할 CSV 파일 배열</param>
        /// <param name="testNames">찾을 테스트 이름 목록</param>
        /// <param name="barcodes">찾을 바코드 목록</param>
        /// <param name="testTimes">찾을 테스트 시간 목록</param>
        /// <returns>조건에 맞는 파일 경로 목록</returns>
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

        /// <summary>
        /// 지정된 폴더의 CSV 파일들을 처리합니다.
        /// </summary>
        /// <param name="folderPath">처리할 CSV 파일이 있는 폴더 경로</param>
        /// <remarks>
        /// 이 메서드는 지정된 폴더에서 모든 CSV 파일을 찾아 처리합니다.
        /// 각 파일은 ProcessFileAsync 메서드를 통해 개별적으로 처리됩니다.
        /// </remarks>
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

        /// <summary>
        /// 파일 이름을 파싱하여 각 부분으로 분리합니다.
        /// </summary>
        /// <param name="fileName">파싱할 파일 이름</param>
        /// <returns>파일 이름의 각 부분을 담은 문자열 배열</returns>
        private string[] ParseFileName(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName).Split('_');
        }

        /// <summary>
        /// 개별 파일을 처리합니다.
        /// </summary>
        /// <param name="parts">파일 이름의 각 부분</param>
        /// <param name="fullPath">파일의 전체 경로</param>
        /// <remarks>
        /// 이 메서드는 파일 이름에서 추출한 정보를 사용하여 테스트 이름, 바코드, 테스트 시간을 처리합니다.
        /// </remarks>
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

        /// <summary>
        /// 리스트에 중복되지 않는 항목을 추가합니다.
        /// </summary>
        /// <param name="list">항목을 추가할 리스트</param>
        /// <param name="item">추가할 항목</param>
        /// <remarks>
        /// 이 메서드는 내부적으로 HashSet을 사용하여 중복을 방지합니다.
        /// </remarks>
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
