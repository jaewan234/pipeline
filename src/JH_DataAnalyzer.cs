using Microsoft.WindowsAPICodePack.Dialogs;
using ZedGraph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Image = System.Drawing.Image;

namespace JH_DataAnalyzer
{
    /// <summary>
    /// 그래프의 상태를 저장하는 구조체
    /// </summary>
    public struct GraphState
    {
        public double XMin, XMax, YMin, YMax;
        public double XMajorStep, XMinorStep, YMajorStep, YMinorStep;
        public string XAxisTitle, YAxisTitle;
    }

    /// <summary>
    /// 파일 처리를 담당하는 클래스
    /// </summary>
    public class FileProcessor
    {
        private HashSet<string> listBoxItems = new HashSet<string>();
        private ListBox TestName_List;
        private ListBox Barcode_List;
        private ListBox TestTime_List;
        private System.Windows.Forms.Label TestTime_count;
        private TextBox TestLogPath;

        /// <summary>
        /// FileProcessor를 초기화하는 메서드
        /// </summary>
        /// <param name="testNameList">테스트 이름 ListBox</param>
        /// <param name="barcodeList">바코드 ListBox</param>
        /// <param name="testTimeList">테스트 시간 ListBox</param>
        /// <param name="testTimeCount">테스트 시간 카운트 Label</param>
        /// <param name="testLogPath">테스트 로그 경로 TextBox</param>
        public void Initialize(ListBox testNameList, ListBox barcodeList, ListBox testTimeList, System.Windows.Forms.Label testTimeCount, TextBox testLogPath)
        {
            TestName_List = testNameList;
            Barcode_List = barcodeList;
            TestTime_List = testTimeList;
            TestTime_count = testTimeCount;
            TestLogPath = testLogPath;
        }

        /// <summary>
        /// 지정된 폴더의 CSV 파일들을 처리하는 메서드
        /// </summary>
        /// <param name="folderPath">처리할 폴더 경로</param>
        public void ProcessFolder(string folderPath)
        {
            var csvFiles = Directory.GetFiles(folderPath, "*.csv");
            var fileInfo = csvFiles.Select(file => new FileInfo(file))
                                   .Select(fi => new
                                   {
                                       Parts = Path.GetFileNameWithoutExtension(fi.Name).Split('_'),
                                       FullName = fi.FullName
                                   })
                                   .Where(x => x.Parts.Length >= 5);

            //파일명 분리 결과와 전체 파일경로를 전달
            foreach (var file in fileInfo)
            {
                ProcessFile(file.Parts, file.FullName);
            }
        }

        /// <summary>
        /// 개별 파일을 처리하는 메서드
        /// </summary>
        /// <param name="parts">파일 이름을 분리한 배열</param>
        /// <param name="fullPath">파일의 전체 경로</param>
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
        }

        /// <summary>
        /// ListBox에 중복되지 않는 항목을 추가하는 메서드
        /// </summary>
        /// <param name="listBox">항목을 추가할 ListBox</param>
        /// <param name="item">추가할 항목</param>
        public void AddUniqueItemToList(ListBox listBox, string item)
        {
            if (listBoxItems.Add(item))
            {
                listBox.Items.Add(item);
            }
        }

        /// <summary>
        /// 주어진 조건에 맞는 CSV 파일들을 찾는 메서드
        /// </summary>
        /// <param name="csvFiles">검색할 CSV 파일 배열</param>
        /// <param name="testNames">찾을 테스트 이름 목록</param>
        /// <param name="barcodes">찾을 바코드 목록</param>
        /// <param name="testTimes">찾을 테스트 시간 목록</param>
        /// <returns>조건에 맞는 파일 경로 목록</returns>
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

        /// <summary>
        /// 선택된 바코드에 해당하는 테스트 시간 목록을 업데이트하는 메서드
        /// </summary>
        /// <param name="selectedBarcodes">선택된 바코드 목록</param>
        public void TestTimeList_update(List<string> selectedBarcodes)
        {
            TestTime_List.Items.Clear(); // 기존 항목들을 모두 지웁니다.
            HashSet<string> uniqueTestTimes = new HashSet<string>(); // 중복을 방지하기 위한 HashSet

            // 모든 CSV 파일 경로 수집
            List<string> allCsvFiles = new List<string>();
            foreach (string path in TestLogPath.Text.Split(','))
            {
                string trimmedPath = path.Trim();
                if (Directory.Exists(trimmedPath))
                {
                    allCsvFiles.AddRange(Directory.GetFiles(trimmedPath, "*.csv"));
                }
            }

            foreach (string file in allCsvFiles)
            {
                string[] parts = Path.GetFileNameWithoutExtension(file).Split('_');
                if (parts.Length >= 5)
                {
                    string barcode = parts[4];
                    if (selectedBarcodes.Contains(barcode))
                    {
                        string testTime = parts[3];
                        if (uniqueTestTimes.Add(testTime)) // 중복되지 않은 경우에만 추가
                        {
                            TestTime_List.Items.Add(testTime);
                        }
                    }
                }
            }

            // ListBox의 모든 항목을 선택합니다.
            for (int i = 0; i < TestTime_List.Items.Count; i++)
            {
                TestTime_List.SetSelected(i, true);
            }

            TestTime_count.Text = string.Format("{0}/{1}", TestTime_List.SelectedItems.Count, TestTime_List.Items.Count);
        }
    }

    public class GraphManager
    {
        JH_DataAnalyzer JH_Form;
        private Form graphForm;
        private List<ZedGraphControl> allGraphs = new List<ZedGraphControl>();
        private List<ZedGraphControl> tempGraphs = new List<ZedGraphControl>();
        public static string SaveFileDialogFilter { get; } = "Portable Network Graphics (*.png)|*.png|JPEG-Image (*.jpg)|*.jpg|TIFF Image (*.tiff)|*.tiff";
        public static ImageFormat[] ImageFormats { get; } = { ImageFormat.Png, ImageFormat.Jpeg, ImageFormat.Tiff };


        /// <summary>
        /// GraphManager 클래스를 초기화하는 메서드
        /// </summary>
        /// <param name="form">JH_DataAnalyzer 폼 인스턴스</param>
        public void Initialize(JH_DataAnalyzer form)
        {
            JH_Form = form;
        }


        /// <summary>
        /// 그래프를 표시할 새 폼을 생성하는 메서드
        /// </summary>
        public void CreateGraphForm()
        {
            graphForm = new Form();
            graphForm.Icon = new System.Drawing.Icon("JAHWA.ico");
            graphForm.FormClosing += new FormClosingEventHandler(Closing);

            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            int formWidth = (int)(workingArea.Width * 0.7);
            int formHeight = (int)(workingArea.Height * 0.7);
            graphForm.Size = new Size(formWidth, formHeight);
            graphForm.StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.RowCount = 4;
            tableLayoutPanel.ColumnCount = 3;

            for (int i = 0; i < 4; i++)
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            for (int i = 0; i < 3; i++)
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            graphForm.Controls.Add(tableLayoutPanel);
        }


        /// <summary>
        /// 그래프 폼을 업데이트하는 메서드
        /// </summary>
        /// <param name="filePaths">처리할 파일 경로 목록</param>
        /// <param name="selectedBarcodes">선택된 바코드 목록</param>
        /// <param name="selectedTestNames">선택된 테스트 이름 목록</param>
        public void UpdateGraphForm(List<string> filePaths, List<string> selectedBarcodes, List<string> selectedTestNames)
        {
            if (graphForm.IsHandleCreated)
            {
                graphForm.Invoke((MethodInvoker)delegate
                {
                    UpdateGraphFormInternal(filePaths, selectedBarcodes, selectedTestNames);
                });
            }
            else
            {
                UpdateGraphFormInternal(filePaths, selectedBarcodes, selectedTestNames);
            }
        }

        private void UpdateGraphFormInternal(List<string> filePaths, List<string> selectedBarcodes, List<string> selectedTestNames)
        {
            string barcodesString = string.Join(", ", selectedBarcodes);
            string testNamesString = string.Join(", ", selectedTestNames);
            graphForm.Text = $"Jahwa Data Analyzer - {barcodesString} / {testNamesString}";
            graphForm.Icon = new System.Drawing.Icon("JAHWA.ico");

            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            int formWidth = (int)(workingArea.Width * 0.7);
            int formHeight = (int)(workingArea.Height * 0.7);
            graphForm.Size = new Size(formWidth, formHeight);
            graphForm.StartPosition = FormStartPosition.Manual;
            graphForm.Location = new Point(0, 0);

            TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)graphForm.Controls[0];
            tableLayoutPanel.Controls.Clear();
            allGraphs.Clear();

            string[] graphTitles = { "OISX current", "OISY Current", "AF Current", "OISX Cmd, FW Pos, LaserPos", "OISY Cmd, FW Pos, LaserPos", "AF Cmd, FW Pos, LaserPos", "OISX Sensors", "OISY Sensors", "AF Sensors", "FW Positions", "Temp NTC vs Temp INT", "AF Laser TiltX vs AF Laser TiltY" };

            for (int i = 0; i < 12; i++)
            {
                ZedGraphControl zgc = CreateGraphControl(graphTitles[i], filePaths);
                tableLayoutPanel.Controls.Add(zgc, i % 3, i / 3);
                allGraphs.Add(zgc);
                zgc.ZoomEvent += new ZedGraphControl.ZoomEventHandler(zgc_ZoomEvent);
            }
        }

        /// <summary>
        /// 그래프 폼이 닫힐 때 호출되는 이벤트 핸들러
        /// </summary>
        /// <param name="sender">이벤트 발생 객체</param>
        /// <param name="e">이벤트 인자</param>
        public void Closing(object sender, FormClosingEventArgs e)
        {
            allGraphs.Clear();
        }

        /// <summary>
        /// 그래프 폼을 반환하는 메서드
        /// </summary>
        /// <param name="sender">이벤트 발생 객체</param>
        /// <param name="e">이벤트 인자</param>
        public Form GetGraphForm()
        {
            return graphForm;
        }

        /// <summary>
        /// ZedGraph 컨트롤의 줌 이벤트 핸들러
        /// </summary>
        public void zgc_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            // 줌 상태가 변경되었는지 확인
            if (IsZoomStateChanged(oldState, newState))
            {
                sender.GraphPane.XAxis.Scale.MajorStep = (sender.GraphPane.XAxis.Scale.Max - sender.GraphPane.XAxis.Scale.Min) / 10;

                SynchronizeZoom(sender); // 모든 그래프의 줌 상태 동기화
            }
        }

        /// <summary>
        /// 줌 상태가 변경되었는지 확인하는 메서드
        /// </summary>
        /// <param name="oldState">이전 줌 상태</param>
        /// <param name="newState">새로운 줌 상태</param>
        /// <returns>줌 상태 변경 여부</returns>
        private bool IsZoomStateChanged(ZoomState oldState, ZoomState newState)
        {
            // ZoomState 객체가 null이 아닌지 확인
            if (oldState == null || newState == null)
                return true;

            // GraphPane을 통해 축 스케일 비교
            GraphPane oldPane = oldState.GetType().GetProperty("GraphPane")?.GetValue(oldState) as GraphPane;
            GraphPane newPane = newState.GetType().GetProperty("GraphPane")?.GetValue(newState) as GraphPane;

            if (oldPane == null || newPane == null)
                return true;

            // X축과 Y축의 스케일 변경 여부 확인
            bool isXScaleChanged = !AreScalesEqual(oldPane.XAxis.Scale.Min, oldPane.XAxis.Scale.Max,
                                                   newPane.XAxis.Scale.Min, newPane.XAxis.Scale.Max);
            bool isYScaleChanged = !AreScalesEqual(oldPane.YAxis.Scale.Min, oldPane.YAxis.Scale.Max,
                                                   newPane.YAxis.Scale.Min, newPane.YAxis.Scale.Max);

            return isXScaleChanged || isYScaleChanged;
        }

        /// <summary>
        /// 두 축의 스케일이 동일한지 비교하는 메서드
        /// </summary>
        /// <param name="oldMin">이전 최소값</param>
        /// <param name="oldMax">이전 최대값</param>
        /// <param name="newMin">새로운 최소값</param>
        /// <param name="newMax">새로운 최대값</param>
        /// <returns>스케일이 동일한지 여부</returns>
        private bool AreScalesEqual(double oldMin, double oldMax, double newMin, double newMax)
        {
            const double epsilon = 1e-10; // 부동소수점 비교를 위한 작은 값 (오차 허용 범위)
            return Math.Abs(oldMin - newMin) < epsilon && Math.Abs(oldMax - newMax) < epsilon;
        }

        /// <summary>
        /// 모든 그래프의 줌 상태를 동기화하는 메서드
        /// </summary>
        /// <param name="sourceGraph">줌 이벤트가 발생한 원본 그래프</param>
        private void SynchronizeZoom(ZedGraphControl sourceGraph)
        {
            foreach (var zgc in allGraphs) // 원본 그래프 제외
            {
                if (zgc != sourceGraph)
                {
                    // X축 스케일 동기화
                    zgc.GraphPane.XAxis.Scale.Min = sourceGraph.GraphPane.XAxis.Scale.Min;
                    zgc.GraphPane.XAxis.Scale.Max = sourceGraph.GraphPane.XAxis.Scale.Max;
                    zgc.GraphPane.XAxis.Scale.MajorStep = sourceGraph.GraphPane.XAxis.Scale.MajorStep;
                    zgc.GraphPane.XAxis.Scale.MinorStep = sourceGraph.GraphPane.XAxis.Scale.MinorStep;

                    zgc.AxisChange();
                    zgc.Invalidate(); // 그래프 다시 그리기
                }
            }
        }

        /// <summary>
        /// ZedGraph 컨트롤을 생성하고 설정하는 메서드
        /// </summary>
        private ZedGraphControl CreateGraphControl(string title, List<string> filePaths)
        {
            ZedGraphControl zgc = new ZedGraphControl();
            zgc.Dock = DockStyle.Fill;
            zgc.IsShowCopyMessage = false;
            GraphPane myPane = zgc.GraphPane;
            zgc.IsShowPointValues = true;


            // 아이템 메뉴 재구성
            zgc.ContextMenuBuilder += (sender, menuStrip, mousePt, objState) =>
            {

                // "Set Scale to Default" 메뉴 아이템 제거
                for (int i = menuStrip.Items.Count - 1; i >= 0; i--)
                {
                    if (menuStrip.Items[i] is ToolStripMenuItem item && item.Text == "Set Scale to Default")
                    {
                        menuStrip.Items.RemoveAt(i);
                        break; // 첫 번째 일치하는 항목을 찾아 제거한 후 루프 종료
                    }
                }

                // "Copy to All Graphs" 메뉴 아이템 추가
                ToolStripMenuItem allCopyItem2 = new ToolStripMenuItem("Copy to All Graphs");
                allCopyItem2.Click += (s, e) => CopyEntireGraphWindow();
                menuStrip.Items.Add(allCopyItem2);

                // "Copy to All Graphs" 메뉴 아이템 추가
                ToolStripMenuItem allCopyItem3 = new ToolStripMenuItem("Save All Graphs");
                allCopyItem3.Click += (s, e) => CopyEntireGraphWindow3();
                menuStrip.Items.Add(allCopyItem3);

            };

            // 그래프 제목 설정
            myPane.Title.Text = title;
            myPane.Title.FontSpec.Size = 20;
            myPane.Title.FontSpec.IsBold = true;

            // X축 제목 설정
            myPane.XAxis.Title.Text = "time (msec)";
            myPane.XAxis.Title.FontSpec.Size = 20;
            myPane.XAxis.Title.FontSpec.IsBold = true;

            // Y축 제목 설정
            myPane.YAxis.Title.Text = GetYAxisTitle(title);
            myPane.YAxis.Title.FontSpec.Size = 18;
            myPane.YAxis.Title.FontSpec.IsBold = true;

            // 범례 설정
            myPane.Legend.FontSpec.Size = 14;
            myPane.Legend.FontSpec.IsBold = true;

            // X축과 Y축의 보조 눈금선 제거
            myPane.XAxis.MajorGrid.IsVisible = false;
            myPane.XAxis.MinorGrid.IsVisible = false;
            myPane.YAxis.MajorGrid.IsVisible = false;
            myPane.YAxis.MinorGrid.IsVisible = false;
            myPane.YAxis.MinorTic.IsInside = false;
            myPane.YAxis.MinorTic.IsOutside = false;
            myPane.XAxis.MinorTic.IsInside = false;
            myPane.XAxis.MinorTic.IsOutside = false;
            myPane.YAxis.MinorTic.IsAllTics = false;
            myPane.XAxis.MinorTic.IsAllTics = false;

            // 주 눈금선만 표시 (선택적)
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;
            myPane.XAxis.MajorGrid.Color = Color.DarkGray;
            myPane.YAxis.MajorGrid.Color = Color.DarkGray;
            myPane.XAxis.MajorGrid.DashOn = 1;  // 실선으로 설정
            myPane.YAxis.MajorGrid.DashOn = 1;  // 실선으로 설정
            myPane.XAxis.MajorGrid.DashOff = 0; // 실선으로 설정
            myPane.YAxis.MajorGrid.DashOff = 0; // 실선으로 설정
            myPane.XAxis.MajorGrid.PenWidth = 1f;  // 선 두께 조정
            myPane.YAxis.MajorGrid.PenWidth = 1f;  // 선 두께 조정

            //범례 중복 방지
            HashSet<string> addedLegends = new HashSet<string>();
            bool dataAdded = false;
            double maxX = 0;
            foreach (string filePath in filePaths)
            {
                double fileMax = AddDataToGraph(myPane, filePath, title, addedLegends);
                maxX = Math.Max(maxX, fileMax);
                dataAdded |= (fileMax > 0);
            }


            // 데이터가 없는 경우 0의 그래프를 출력
            if (!dataAdded)
            {

                myPane.AddCurve("No Data", new PointPairList(new[] { 0.0 }, new[] { 0.0 }), Color.Transparent, SymbolType.None);
            }

            // X축 스케일 설정
            double roundedMaxX = Math.Ceiling(maxX / 10.0) * 10;
            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = roundedMaxX;
            myPane.XAxis.Scale.MajorStep = roundedMaxX / 10;
            myPane.XAxis.Scale.MinorStep = 5;
            myPane.XAxis.Scale.Format = "F0";

            zgc.AxisChange();
            zgc.Invalidate(); //그래프 다시 그리기


            // 상태 저장
            JH_Form.InitialStates[zgc] = new GraphState
            {
                XMin = myPane.XAxis.Scale.Min,
                XMax = myPane.XAxis.Scale.Max,
                YMin = myPane.YAxis.Scale.Min,
                YMax = myPane.YAxis.Scale.Max,
                XMajorStep = myPane.XAxis.Scale.MajorStep,
                XMinorStep = myPane.XAxis.Scale.MinorStep,
                YMajorStep = myPane.YAxis.Scale.MajorStep,
                YMinorStep = myPane.YAxis.Scale.MinorStep,
                XAxisTitle = myPane.XAxis.Title.Text,
                YAxisTitle = myPane.YAxis.Title.Text
            };
            return zgc;
        }

        /// <summary>
        /// 그래프의 주 눈금 간격을 계산하는 메서드
        /// </summary>
        /// <param name="range">그래프의 데이터 범위</param>
        /// <returns>계산된 주 눈금 간격</returns>
        public double CalculateMajorStep(double range)
        {
            double step = Math.Pow(10, Math.Floor(Math.Log10(range)));

            // 범위와 간격의 비율에 따라 간격 조정
            if (range / step < 2)
                step /= 5; // 간격이 너무 크면 5로 나눔
            else if (range / step < 5)
                step /= 2; // 간격이 약간 크면 2로 나눔

            return step;
        }

        /// <summary>
        /// 그래프 창을 복사하고 저장하는 메서드
        /// </summary>
        private void CopyEntireGraphWindow()
        {
            // Alt + PrtScn 키 조합을 시뮬레이션하여 활성 창 캡처
            SendKeys.SendWait("%{PRTSC}");
        }

        /// <summary>
        /// 전체 그래프 창을 복사하고 저장하는 메서드
        /// </summary>
        private void CopyEntireGraphWindow3()
        {
            // Alt + PrtScn 키 조합을 시뮬레이션하여 활성 창 캡처
            SendKeys.SendWait("%{PRTSC}");
            // 캡처가 완료될 때까지 잠시 대기
            System.Threading.Thread.Sleep(500); // 500ms 대기
            // 클립보드에서 이미지 가져오기
            Image image = Clipboard.GetImage();
            if (image == null)
            {
                return;
            }

            // 파일 저장 대화상자 설정 및 표시
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = SaveFileDialogFilter;  // 미리 정의된 파일 형식 필터 사용
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // 초기 디렉토리 설정

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 선택된 파일 형식에 따라 이미지 저장
                        ImageFormat format = ImageFormats[saveFileDialog.FilterIndex - 1];
                        // 선택된 파일 형식에 해당하는 ImageFormat
                        image.Save(saveFileDialog.FileName, format);
                    }
                    catch (Exception ex)
                    {
                        // 저장 중 오류 발생 시 메시지 표시
                        MessageBox.Show($"이미지 저장 중 오류가 발생했습니다: {ex.Message}");
                    }
                }
            }
            // 사용한 이미지 리소스 해제
            image.Dispose();
        }

        /// <summary>
        /// CSV 파일의 데이터를 그래프에 추가하는 메서드
        /// </summary>
        /// <param name="myPane">데이터를 추가할 그래프 패널</param>
        /// <param name="filePath">CSV 파일 경로</param>
        /// <param name="graphTitle">그래프 제목</param>
        /// <param name="addedLegends">이미 추가된 범례를 추적하는 집합</param>
        /// <returns>데이터의 최대 X 값</returns>
        private double AddDataToGraph(GraphPane myPane, string filePath, string graphTitle, HashSet<string> addedLegends)
        {
            // CSV 파일의 모든 라인을 읽음
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1) return 0; // 데이터가 없으면 0 반환

            // 헤더 행을 분석하여 열 인덱스를 가져옴
            string[] headers = lines[0].Split(',');
            Dictionary<string, int> columnIndices = GetColumnIndices(headers, graphTitle);
            double maxX = 0; // X축의 최대값 추적



            // 각 열에 대해 처리
            foreach (var kvp in columnIndices)
            {
                string columnName = kvp.Key;
                int columnIndex = kvp.Value;
                PointPairList pointList = new PointPairList();

                // 레이저 위치 데이터를 저장할 리스트 생성
                List<double> laserPositions = new List<double>();

                // 각 데이터 행을 처리
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    string[] values = lines[i].Split(',');
                    double value = 0;
                    if (columnIndex != -1 && values.Length > columnIndex && double.TryParse(values[columnIndex], out double parsedValue))
                    {
                        value = parsedValue;
                    }

                    // current_DAC 값에 대해 200/1023을 곱하여 처리
                    if (columnName.Contains("current_DAC"))
                    {
                        value = value * 200 / 1023;
                    }
                    else if (columnName.Contains("APS_lsb"))
                    {
                        laserPositions.Add(value);
                    }

                    double xValue = i - 1;
                    pointList.Add(xValue, value);
                    maxX = Math.Max(maxX, xValue);
                }

                // 7, 8, 9번째 그래프에 대해 보조축 추가
                if (columnName.Contains("APS_lsb"))
                {
                    // 주축(왼쪽) 설정
                    myPane.YAxis.MajorTic.IsOpposite = false;
                    myPane.YAxis.MinorTic.IsOpposite = false;
                    myPane.YAxis.MajorGrid.IsVisible = true;

                    // 보조축(오른쪽) 설정
                    myPane.Y2Axis.IsVisible = true;
                    myPane.Y2Axis.MajorTic.IsOpposite = false;
                    myPane.Y2Axis.MinorTic.IsOpposite = false;
                    myPane.Y2Axis.MajorGrid.IsVisible = false;
                    myPane.Y2Axis.MinorTic.IsAllTics = false;

                    // 보조축 이름 설정
                    if (columnName.Contains("OISX"))
                    {
                        myPane.Y2Axis.Title.Text = "OISX Laser pos (um)";
                    }
                    else if (columnName.Contains("OISY"))
                    {
                        myPane.Y2Axis.Title.Text = "OISY Laser pos (um)";
                    }
                    else if (columnName.Contains("AF"))
                    {
                        myPane.Y2Axis.Title.Text = "AF Laser pos (um)";
                    }
                    myPane.Y2Axis.Title.FontSpec.Size = 18;
                    myPane.Y2Axis.Title.FontSpec.IsBold = true;

                    // 레이저 위치 데이터가 존재하는 경우
                    if (laserPositions.Count > 0)
                    {
                        // 레이저 위치의 최소값과 최대값 계산
                        double minLaserPos = laserPositions.Min();
                        double maxLaserPos = laserPositions.Max();

                        // 스케일 범위를 데이터의 최소값과 최대값으로 설정
                        myPane.Y2Axis.Scale.Min = minLaserPos;
                        myPane.Y2Axis.Scale.Max = maxLaserPos;

                        // 주 눈금 간격을 자동으로 계산
                        double range = maxLaserPos - minLaserPos;
                        // CalculateMajorStep 함수를 사용하여 적절한 주 눈금 간격 계산
                        myPane.Y2Axis.Scale.MajorStep = CalculateMajorStep(range);
                        // 부 눈금 간격을 주 눈금 간격의 1/5로 설정
                        myPane.Y2Axis.Scale.MinorStep = myPane.Y2Axis.Scale.MajorStep / 5;
                    }
                }

                // 그래프에 데이터 시리즈 추가
                Color lineColor = GetLineColor(columnName); // 열 이름에 따른 선 색상 결정
                LineItem curve = myPane.AddCurve(columnName, pointList, lineColor, SymbolType.None);
                curve.Line.Width = 2; // 선 두께 설정

                // 범례 중복 방지
                if (addedLegends.Contains(columnName))
                {
                    curve.Label.IsVisible = false;
                }
                else
                {
                    addedLegends.Add(columnName);
                }
            }

            return maxX; // 최대 X 값 반환
        }

        /// <summary>
        /// 그래프 제목에 따른 열 이름과 인덱스를 저장한 메서드
        /// </summary>
        private Dictionary<string, int> GetColumnIndices(string[] headers, string graphTitle)
        {
            // 열 이름과 인덱스를 저장할 딕셔너리 초기화
            Dictionary<string, int> columnIndices = new Dictionary<string, int>();
            // 그래프 제목에 따라 필요한 열을 선택하고 인덱스를 찾음
            switch (graphTitle)
            {
                case "OISX current":
                    columnIndices["OISX_current_DAC"] = Array.IndexOf(headers, "OISX_current_DAC");
                    break;
                case "OISY Current":
                    columnIndices["OISY_current_DAC"] = Array.IndexOf(headers, "OISY_current_DAC");
                    break;
                case "AF Current":
                    columnIndices["AF_current_DAC"] = Array.IndexOf(headers, "AF_current_DAC");
                    break;
                case "OISX Cmd, FW Pos, LaserPos":
                    columnIndices["OISX_command_um"] = Array.IndexOf(headers, "OISX_command_um");
                    columnIndices["OIS_X"] = Array.IndexOf(headers, "OIS_X");
                    columnIndices["Laser_OIS_X_um"] = Array.IndexOf(headers, "Laser_OIS_X_um");
                    break;
                case "OISY Cmd, FW Pos, LaserPos":
                    columnIndices["OISY_command_um"] = Array.IndexOf(headers, "OISY_command_um");
                    columnIndices["OIS_Y"] = Array.IndexOf(headers, "OIS_Y");
                    columnIndices["Laser_OIS_Y_um"] = Array.IndexOf(headers, "Laser_OIS_Y_um");
                    break;
                case "AF Cmd, FW Pos, LaserPos":
                    columnIndices["AF_command_um"] = Array.IndexOf(headers, "AF_command_um");
                    columnIndices["AF_Z"] = Array.IndexOf(headers, "AF_Z");
                    columnIndices["Laser_AF_Z_um"] = Array.IndexOf(headers, "Laser_AF_Z_um");
                    break;
                case "OISX Sensors":
                    columnIndices["OISX_APS_lsb"] = Array.IndexOf(headers, "OISX_APS_lsb");
                    columnIndices["Laser_OIS_X_um"] = Array.IndexOf(headers, "Laser_OIS_X_um");
                    break;
                case "OISY Sensors":
                    columnIndices["OISY_APS_lsb"] = Array.IndexOf(headers, "OISY_APS_lsb");
                    columnIndices["Laser_OIS_Y_um"] = Array.IndexOf(headers, "Laser_OIS_Y_um");
                    break;
                case "AF Sensors":
                    columnIndices["AF_APS_lsb"] = Array.IndexOf(headers, "AF_APS_lsb");
                    columnIndices["Laser_AF_Z_um"] = Array.IndexOf(headers, "Laser_AF_Z_um");
                    break;
                case "FW Positions":
                    columnIndices["OIS_X"] = Array.IndexOf(headers, "OIS_X");
                    columnIndices["OIS_Y"] = Array.IndexOf(headers, "OIS_Y");
                    columnIndices["AF_Z"] = Array.IndexOf(headers, "AF_Z");
                    break;
                case "Temp NTC vs Temp INT":
                    columnIndices["NTC_Temp"] = Array.IndexOf(headers, "NTC_Temp");
                    columnIndices["INT_Temp"] = Array.IndexOf(headers, "INT_Temp");
                    break;
                case "AF Laser TiltX vs AF Laser TiltY":
                    columnIndices["Laser_AF_TiltX_min"] = Array.IndexOf(headers, "Laser_AF_TiltX_min");
                    columnIndices["Laser_AF_TiltY_min"] = Array.IndexOf(headers, "Laser_AF_TiltY_min");
                    break;
            }
            return columnIndices;
        }

        /// <summary>
        /// 그래프 제목에 따라 Y축 제목을 반환하는 메서드
        /// </summary>
        /// <param name="graphTitle">그래프 제목</param>
        /// <returns>Y축 제목</returns>
        private string GetYAxisTitle(string graphTitle)
        {
            // 그래프 제목에 따라 적절한 Y축 제목 설정
            switch (graphTitle)
            {

                case "OISX current":
                    return "OISX driving current (mA)";
                case "OISY Current":
                    return "OISY driving current (mA)";
                case "AF Current":
                    return "AF driving current (mA)";
                case "OISX Cmd, FW Pos, LaserPos":
                    return "OISX positions";
                case "OISY Cmd, FW Pos, LaserPos":
                    return "OISY positions";
                case "AF Cmd, FW Pos, LaserPos":
                    return "AF positions";
                case "OISX Sensors":
                    return "OISX sensor (LSB)";
                case "OISY Sensors":
                    return "OISY sensor (LSB)";
                case "AF Sensors":
                    return "AF sensor (LSB)";
                case "FW Positions":
                    return "Linear FW Pos (um)";
                case "Temp NTC vs Temp INT":
                    return "Temperature (degC)";
                case "AF Laser TiltX vs AF Laser TiltY":
                    return "";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 열 이름에 따라 그래프 선의 색상을 반환하는 메서드
        /// </summary>
        /// <param name="columnName">CSV 파일의 열 이름</param>
        /// <returns>그래프 선의 색상</returns>
        private Color GetLineColor(string columnName)
        {
            // 열 이름에 따라 적절한 색상 설정
            switch (columnName)
            {
                case "AF_current_DAC":
                case "OISX_current_DAC":
                case "OISY_current_DAC":
                    return Color.Orange;
                case "OISX_command_um":
                case "OISY_command_um":
                case "AF_command_um":
                    return Color.Blue;
                case "OIS_X":
                case "OIS_Y":
                case "AF_Z":
                    return Color.Red;
                case "Laser_OIS_X_um":
                case "Laser_OIS_Y_um":
                case "Laser_AF_Z_um":
                    return Color.Green;
                case "OISX_APS_lsb":
                case "OISY_APS_lsb":
                case "AF_APS_lsb":
                    return Color.Blue;
                case "NTC_Temp":
                    return Color.Blue;
                case "INT_Temp":
                    return Color.Red;
                case "Laser_AF_TiltX_min":
                    return Color.Green;
                case "Laser_AF_TiltY_min":
                    return Color.Red;
                default:
                    return Color.Black;
            }
        }

        public void UpdateOneGraphForm(List<string> filePaths, List<string> selectedBarcode, List<string> selectedTestName)
        {
            JH_Form.oneGraphControl.GraphPane.CurveList.Clear();  // 기존 그래프 데이터 삭제
            JH_Form.columnCheckList.Items.Clear(); // 체크박스 리스트 초기화
            // 모든 항목 체크 해제
            for (int i = 0; i < JH_Form.columnCheckList.Items.Count; i++)
            {
                JH_Form.columnCheckList.SetItemChecked(i, false);
            }

            // 그래프 폼 제목 설정
            // fileName = Path.GetFileName(filePath);
            string barcodesString = string.Join(", ", selectedBarcode);
            string testNamesString = string.Join(", ", selectedTestName);
            JH_Form.oneGraphForm.Text = $"Jahwa Data Analyzer - {barcodesString} / {testNamesString}";
            /*

            */
            // 필터링된 헤더를 체크리스트에 추가 - 필터링 왜하는지?? desiredHeaders를 추가하면 되는거 같은데
            JH_Form.columnCheckList.Items.AddRange(JH_Form.desiredHeaders.ToArray());
            //columnCheckList.Items.AddRange(filteredHeaders.ToArray());

            // 그래프 설정
            GraphPane myPane = JH_Form.oneGraphControl.GraphPane;
            myPane.Title.Text = $"{barcodesString} / {testNamesString}";
            myPane.XAxis.Title.Text = "Time (ms)";
            myPane.YAxis.Title.Text = "";

            ApplyGraphSettings(myPane); // 그래프 설정 적용


            // 그래프 컨트롤 설정
            JH_Form.oneGraphControl.AxisChange();
            JH_Form.oneGraphControl.Invalidate();
            JH_Form.oneGraphControl.ZoomEvent += new ZedGraphControl.ZoomEventHandler(zgc_ZoomEvent);
            //oneGraphControl.MouseWheel += new MouseEventHandler(zgc_MouseWheel);
            JH_Form.currentFilePaths = filePaths; //파일 경로 저장
        }

        /// <summary>
        /// 선택된 열에 따라 그래프를 업데이트하는 메서드
        /// </summary>
        /// <param name="myPane">업데이트할 그래프 패널</param>
        public void UpdateGraph(GraphPane myPane)
        {
            // 현재 파일 경로가 유효한지 확인
            if (JH_Form.currentFilePaths == null || JH_Form.currentFilePaths.Count == 0)
            {
                return;
            }

            JH_Form.oneGraphControl.GraphPane.CurveList.Clear(); // 기존 그래프 데이터 삭제

            // 체크된 열 목록 가져오기
            List<string> checkedColumns = JH_Form.columnCheckList.CheckedItems.Cast<string>().ToList();
            HashSet<string> addedLegends = new HashSet<string>(); // 추가된 범례 추적
            HashSet<string> uniqueLegends = new HashSet<string>(); // 고유한 범례 추적


            foreach (string filePath in JH_Form.currentFilePaths)
            {
                string[] lines = File.ReadAllLines(filePath);
                string[] headers = lines[0].Split(',');
                foreach (string columnName in checkedColumns)
                {
                    int columnIndex = Array.IndexOf(headers, columnName);

                    if (columnIndex != -1)
                    {
                        PointPairList points = new PointPairList();

                        // 데이터 포인트 추가
                        for (int j = 1; j < lines.Length; j++)
                        {
                            string[] values = lines[j].Split(',');
                            if (values.Length > columnIndex && double.TryParse(values[columnIndex], out double y))
                            {
                                double x = j - 1;
                                // current_DAC 값에 대해 200/1023을 곱하여 처리
                                if (columnName.Contains("current_DAC"))
                                {
                                    y = y * 200 / 1023;
                                }
                                points.Add(x, y);
                            }
                        }

                        string legendLabel = columnName; //columnName을 범례명으로 설정
                        Color lineColor = GetLineColor2(columnName); // 열 이름에 따른 색상 선택

                        // 유효한 데이터 포인트가 있는 경우에만 곡선 추가
                        if (points != null && points.Count > 0)
                        {
                            LineItem curve = myPane.AddCurve(legendLabel, points, lineColor, SymbolType.None);
                            curve.Line.Width = 2;

                            // 범례 중복 방지
                            if (!uniqueLegends.Contains(legendLabel))
                            {
                                curve.Label.IsVisible = true;
                                uniqueLegends.Add(legendLabel);
                            }
                            else
                            {
                                curve.Label.IsVisible = false;
                            }
                            addedLegends.Add(legendLabel);
                        }
                    }
                    // X축 스케일 설정
                    GraphPane pane = JH_Form.oneGraphControl.GraphPane; // 그래프 패널 객체 가져오기
                    double data_length = lines.Length - 1; // 데이터의 총 길이 계산 (헤더 행 제외)
                    double xRoundedMax = Math.Ceiling(data_length / 10) * 10; // X축의 최대값을 10의 배수로 올림
                    JH_Form.oneGraphControl.GraphPane.XAxis.Scale.Min = 0; // X축의 최소값을 0으로 설정
                    JH_Form.oneGraphControl.GraphPane.XAxis.Scale.Max = xRoundedMax; // X축의 최대값을 계산된 올림값으로 설정
                    JH_Form.oneGraphControl.GraphPane.XAxis.Scale.MajorStep = xRoundedMax / 10; // X축의 주 눈금 간격을 전체 범위의 1/10로 설정
                    JH_Form.oneGraphControl.GraphPane.XAxis.Scale.MinorStep = JH_Form.oneGraphControl.GraphPane.XAxis.Scale.MajorStep / 5; // X축의 보조 눈금 간격을 주 눈금 간격의 1/5로 설정
                }
            }
            JH_Form.oneGraphControl.AxisChange(); // 축 변경 적용
            JH_Form.oneGraphControl.Invalidate();// 그래프 다시 그리기
        }

        /// <summary>
        /// 그래프의 시각적 설정을 적용하는 메서드
        /// </summary>
        /// <param name="myPane">설정을 적용할 그래프 패널</param>
        private void ApplyGraphSettings(GraphPane myPane)
        {
            // 제목 및 축 레이블의 폰트 설정
            myPane.Title.FontSpec.Size = 10;
            myPane.Title.FontSpec.IsBold = true;
            myPane.XAxis.Title.FontSpec.Size = 8;
            myPane.XAxis.Title.FontSpec.IsBold = true;
            myPane.YAxis.Title.FontSpec.Size = 8;
            myPane.YAxis.Title.FontSpec.IsBold = true;
            myPane.Legend.FontSpec.Size = 8;
            myPane.Legend.FontSpec.IsBold = true;

            // 그리드 라인 설정
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;
            myPane.XAxis.MajorGrid.Color = Color.DarkGray;
            myPane.YAxis.MajorGrid.Color = Color.DarkGray;
            myPane.XAxis.MajorGrid.DashOn = 1; // 실선으로 설정
            myPane.YAxis.MajorGrid.DashOn = 1;
            myPane.XAxis.MajorGrid.DashOff = 0;
            myPane.YAxis.MajorGrid.DashOff = 0;
            myPane.XAxis.MajorGrid.PenWidth = 1f; // 선 두께 설정
            myPane.YAxis.MajorGrid.PenWidth = 1f;

            // 보조 눈금 설정 (비활성화)
            myPane.YAxis.MinorTic.IsInside = false;
            myPane.YAxis.MinorTic.IsOutside = false;
            myPane.XAxis.MinorTic.IsInside = false;
            myPane.XAxis.MinorTic.IsOutside = false;
            myPane.YAxis.MinorTic.IsAllTics = false;
            myPane.XAxis.MinorTic.IsAllTics = false;
        }

        /// <summary>
        /// 열 이름에 따라 그래프 선의 색상을 반환하는 메서드
        /// </summary>
        /// <param name="columnName">CSV 파일의 열 이름</param>
        /// <returns>그래프 선의 색상</returns>
        private Color GetLineColor2(string columnName)
        {
            // 열 이름에 따라 적절한 색상 설정
            switch (columnName)
            {
                case "AF_current_DAC":
                    return Color.Blue;
                case "OISX_current_DAC":
                    return Color.Red;
                case "OISY_current_DAC":
                    return Color.Green;
                case "Laser_AF_Z_um":
                    return Color.Orange;
                case "Laser_OIS_X_um":
                    return Color.DodgerBlue;
                case "Laser_OIS_Y_um":
                    return Color.Brown;
                case "Laser_AF_TiltX_min":
                    return Color.Pink;
                case "Laser_AF_TiltY_min":
                    return Color.Cyan;
                case "AF_command_um":
                    return Color.Purple;
                case "OISX_command_um":
                    return Color.LimeGreen;
                case "OISY_command_um":
                    return Color.PowderBlue;
                case "AF_APS_lsb":
                    return Color.Teal;
                case "OISX_APS_lsb":
                    return Color.Maroon;
                case "OISY_APS_lsb":
                    return Color.Navy;
                case "NTC_Temp":
                    return Color.Blue;
                case "INT_Temp":
                    return Color.Red;
                case "b1_coil_res":
                    return Color.Green;
                case "b2_coil_res":
                    return Color.Orange;
                case "b3_coil_res":
                    return Color.Purple;
                case "OIS_X":
                    return Color.DarkOrange;
                case "OIS_Y":
                    return Color.DarkBlue;
                case "AF_Z":
                    return Color.DeepPink;
                default:
                    return Color.Black;
            }
        }


    }

    public class UIController
    {
        private FileProcessor fileManager;
        private GraphManager graphManager;
        private JH_DataAnalyzer JH_Form;

        // 추가된 필드들
        private System.Windows.Forms.Label barcode_count;
        private System.Windows.Forms.ListBox Barcode_List;
        private System.Windows.Forms.CheckBox appendixCheckBox;
        private System.Windows.Forms.TextBox TestLogPath;
        private System.Windows.Forms.ListBox TestName_List;
        private System.Windows.Forms.ListBox TestTime_List;
        private List<string> loadedFolders = new List<string>(); // ListBox에서 List<string>으로 변경
        private System.Windows.Forms.Label TestTime_count;
        private int testtime_list_lastSelectedIndex = -1;
        private int barcode_list_lastSelectedIndex = -1;


        public UIController()
        {
            // 기본 생성자
        }

        public void Initialize(JH_DataAnalyzer form, FileProcessor fileManager, GraphManager graphManager,
                       System.Windows.Forms.ListBox barcodeList, System.Windows.Forms.Label barcodeCount,
                       System.Windows.Forms.CheckBox appendixCheckBox, System.Windows.Forms.TextBox testLogPath,
                       System.Windows.Forms.ListBox testNameList, System.Windows.Forms.ListBox testTimeList,
                       System.Windows.Forms.Label testTimeCount)
        {
            JH_Form = form;
            this.fileManager = fileManager;
            this.graphManager = graphManager;

            // UI 컨트롤 초기화
            this.Barcode_List = barcodeList;
            this.barcode_count = barcodeCount;
            this.appendixCheckBox = appendixCheckBox;
            this.TestLogPath = testLogPath;
            this.TestName_List = testNameList;
            this.TestTime_List = testTimeList;
            this.TestTime_count = testTimeCount;
        }



        /// <summary>
        /// Load Test Log 버튼 클릭 이벤트 핸들러
        /// </summary>
        public void LoadTestLog_Click(object sender, EventArgs e)
        {
            //폴더 선택
            using (var dialog = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    ProcessSelectedFolder(dialog.FileName);
                }
            }

            barcode_count.Text = string.Format("0/{0} out of {0}", Barcode_List.Items.Count);
        }

        /// <summary>
        /// 선택된 폴더를 처리하는 메서드
        /// </summary>
        /// <param name="folderPath">처리할 폴더 경로</param>
        private void ProcessSelectedFolder(string folderPath)
        {
            //Append 모드가 아닐 시 리스트 초기화
            if (!appendixCheckBox.Checked)
            {
                ClearLists();
                loadedFolders.Clear();
                loadedFolders.Add(folderPath);
            }

            // 중복 폴더 추가 방지
            if (!loadedFolders.Contains(folderPath))
            {
                loadedFolders.Add(folderPath);
            }

            // 폴더 내 파일 처리
            TestLogPath.Text = string.Join(", ", loadedFolders);
            fileManager.ProcessFolder(folderPath);
        }

        /// <summary>
        /// 폴더 내 파일 처리를 위한 리스트 초기화 메서드
        /// </summary>
        public void ClearLists()
        {
            TestName_List?.Items?.Clear();
            Barcode_List?.Items?.Clear();
            TestTime_List?.Items?.Clear();
            loadedFolders?.Clear();
        }


        /// <summary>
        /// 그래프 표시 버튼 클릭 이벤트 핸들러
        /// </summary>
        public void ShowGraphs_Click(object sender, EventArgs e)
        {
            Form graphForm = graphManager.GetGraphForm();
            //Parts 선택 확인 메세지
            if (string.IsNullOrEmpty(TestLogPath.Text) || TestName_List.SelectedItem == null ||
                Barcode_List.SelectedItems.Count == 0 || TestTime_List.SelectedItems.Count == 0)
            {
                MessageBox.Show("선택이 올바른지 확인하세요.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //선택 Parts 추출
            //string selectedTestName = TestName_List.SelectedItem.ToString();
            List<string> selectedTestNames = TestName_List.SelectedItems.Cast<string>().ToList();
            List<string> selectedBarcodes = Barcode_List.SelectedItems.Cast<string>().ToList();
            List<string> selectedTestTimes = TestTime_List.SelectedItems.Cast<string>().ToList();

            //모든 CSV 파일 경로 수집
            List<string> allCsvFiles = new List<string>();
            foreach (string path in TestLogPath.Text.Split(','))
            {
                string trimmedPath = path.Trim();
                if (Directory.Exists(trimmedPath))
                {
                    allCsvFiles.AddRange(Directory.GetFiles(trimmedPath, "*.csv"));
                }
            }

            //대상 파일 찾기
            List<string> targetFiles = fileManager.FindTargetFiles(allCsvFiles.ToArray(), selectedTestNames, selectedBarcodes, selectedTestTimes);
            if (targetFiles.Count == 0)
            {
                MessageBox.Show("선택한 경로에 파일을 찾을수없습니다.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 사용자에게 그래프 생성 여부 확인
            else
            {
                DialogResult result = MessageBox.Show(string.Format("{0}개의 파일을 찾았습니다. 그래프를 그리겠습니까?", targetFiles.Count), "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == System.Windows.Forms.DialogResult.No) return;
            }

            if (graphForm == null || graphForm.IsDisposed)
            {
                // 새 그래프 폼 생성
                graphManager.CreateGraphForm();
                graphForm = graphManager.GetGraphForm(); // 새로 생성된 폼을 가져옵니다.
            }

            // 기존 그래프 폼 업데이트
            graphManager.UpdateGraphForm(targetFiles, selectedBarcodes, selectedTestNames);

            //그래프 폼 보기
            if (graphForm != null && !graphForm.Visible)
            {
                graphForm.Show();
            }

        }

        /// <summary>
        /// 단일 그래프를 표시하는 버튼 클릭 이벤트 핸들러
        /// </summary>
        public void Show_One_Graph_Click(object sender, EventArgs e)
        {
            // TestName, Barcode, TestTime 유효성 검사
            if (string.IsNullOrEmpty(TestLogPath.Text) || TestName_List.SelectedItem == null ||
               Barcode_List.SelectedItems.Count == 0 || TestTime_List.SelectedItems.Count == 0)
            {
                MessageBox.Show("선택이 올바른지 확인하세요.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 선택된 항목 저장
            //string selectedTestName = TestName_List.SelectedItem.ToString();
            //string selectedBarcode = Barcode_List.SelectedItems[0].ToString();
            //string selectedTestTime = TestTime_List.SelectedItems[0].ToString();
            List<string> selectedTestNames = TestName_List.SelectedItems.Cast<string>().ToList();
            List<string> selectedBarcodes = Barcode_List.SelectedItems.Cast<string>().ToList();
            List<string> selectedTestTimes = TestTime_List.SelectedItems.Cast<string>().ToList();
            List<string> allCsvFiles = new List<string>();

            // CSV 파일 경로 수집
            foreach (string path in TestLogPath.Text.Split(','))
            {
                string trimmedPath = path.Trim();
                if (Directory.Exists(trimmedPath))
                {
                    allCsvFiles.AddRange(Directory.GetFiles(trimmedPath, "*.csv"));

                }
            }

            // 대상 파일 찾기 왜 두번 찾는건지? 의미가 없음
            List<string> targetFiles = fileManager.FindTargetFiles(allCsvFiles.ToArray(), selectedTestNames, selectedBarcodes, selectedTestTimes);
            //string targetFile = FindTargetFile(allCsvFiles.ToArray(), selectedTestName, selectedBarcode, selectedTestTime);

            // 그래프 폼 생성 
            if (JH_Form.oneGraphForm == null || JH_Form.oneGraphForm.IsDisposed)
            {
                CreateOneGraphForm();
            }

            // 그래프 폼 업데이트
            JH_Form.oneGraphControl.IsShowCopyMessage = false;
            graphManager.UpdateOneGraphForm(targetFiles, selectedBarcodes, selectedTestNames);

            // 그래프 폼 표시
            if (!JH_Form.oneGraphForm.Visible)
            {
                JH_Form.oneGraphForm.Show();
            }
        }

        /// <summary>
        /// 단일 그래프를 표시할 새로운 폼을 생성하는 메서드
        /// </summary>
        private void CreateOneGraphForm()
        {
            //그래프 폼 생성 및 자화 아이콘 선정
            JH_Form.oneGraphForm = new Form();
            JH_Form.oneGraphForm.Icon = new System.Drawing.Icon("JAHWA.ico");

            JH_Form.oneGraphForm.FormClosing += new FormClosingEventHandler(graphManager.Closing);

            // 화면 크기의 70%로 폼 크기 설정
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            int formWidth = (int)(workingArea.Width * 0.7);
            int formHeight = (int)(workingArea.Height * 0.7);
            JH_Form.oneGraphForm.Size = new Size(formWidth, formHeight);
            JH_Form.oneGraphForm.StartPosition = FormStartPosition.CenterScreen;

            //체크박스 리스트와 그래프로 구성된 2열 레이아웃 생성
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            // 체크박스 리스트 생성 및 설정
            JH_Form.columnCheckList = new CheckedListBox(); //새로운 CheckedListBox 객체를 생성
            JH_Form.columnCheckList.Dock = DockStyle.Fill; //리스트박스가 공간을 채우도록 설정
            JH_Form.columnCheckList.CheckOnClick = true; //항목을 클릭하면 바로 체크/언체크 되도록 설정
            JH_Form.columnCheckList.DrawMode = DrawMode.OwnerDrawVariable; //리스트박스의 각 항목을 사용자 정의 방식으로 그리도록 설정
            JH_Form.columnCheckList.MeasureItem += ColumnCheckList_MeasureItem; //각 항목의 크기를 측정하는 이벤트 핸들러를 연결
            JH_Form.columnCheckList.DrawItem += ColumnCheckList_DrawItem; //각 항목을 그리는 이벤트 핸들러를 연결
            JH_Form.columnCheckList.ItemCheck += ColumnCheckList_ItemCheck; //항목의 체크 상태가 변경될 때 호출되는 이벤트 핸들러를 연결

            // ZedGraph 컨트롤 생성
            JH_Form.oneGraphControl = new ZedGraphControl();
            JH_Form.oneGraphControl.Dock = DockStyle.Fill;
            JH_Form.oneGraphControl.IsShowPointValues = true;
            // 레이아웃에 컨트롤 추가
            tableLayoutPanel.Controls.Add(JH_Form.columnCheckList, 0, 0);
            tableLayoutPanel.Controls.Add(JH_Form.oneGraphControl, 1, 0);

            JH_Form.oneGraphForm.Controls.Add(tableLayoutPanel);

            // ZedGraph 컨텍스트 메뉴 커스터마이즈
            JH_Form.oneGraphControl.ContextMenuBuilder += (sender, menuStrip, mousePt, objState) =>
            {
                // "Set Scale to Default" 메뉴 항목 제거
                for (int i = menuStrip.Items.Count - 1; i >= 0; i--)
                {
                    if (menuStrip.Items[i] is ToolStripMenuItem item && item.Text == "Set Scale to Default")
                    {
                        menuStrip.Items.RemoveAt(i);
                        break;
                    }
                }
            };
        }

        /// <summary>
        /// CheckedListBox의 각 항목 높이를 설정하는 메서드
        /// </summary>
        private void ColumnCheckList_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 30; // 각 항목의 높이를 30픽셀로 설정
        }

        /// <summary>
        /// CheckedListBox의 각 항목을 그리는 메서드
        /// </summary>
        private void ColumnCheckList_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();// 항목의 배경을 그림
            e.DrawFocusRectangle(); // 포커스 사각형 그리기
        }

        /// <summary>
        /// 체크리스트 항목의 체크 상태가 변경될 때 호출되는 이벤트 핸들러
        /// </summary>
        private void ColumnCheckList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                control.BeginInvoke(new Action(() =>
                {
                    if (JH_Form.oneGraphControl != null && JH_Form.oneGraphControl.GraphPane != null)
                    {
                        graphManager.UpdateGraph(JH_Form.oneGraphControl.GraphPane);
                        JH_Form.oneGraphControl.Invalidate();
                    }
                }));
            }
        }

        public void Barcode_List_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int index = Barcode_List.IndexFromPoint(e.Location);

                if (index != ListBox.NoMatches)
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        // Shift가 눌렸을 때 범위 선택 구현
                        if (barcode_list_lastSelectedIndex >= 0 && barcode_list_lastSelectedIndex < Barcode_List.Items.Count)
                        {
                            int startIndex = Math.Min(barcode_list_lastSelectedIndex, index) + 1;
                            int endIndex = Math.Max(barcode_list_lastSelectedIndex, index);

                            // Barcode_List.ClearSelected();
                            for (int i = startIndex; i < endIndex; i++)
                            {
                                if (Barcode_List.GetSelected(i) == false)
                                {
                                    Barcode_List.SetSelected(i, true);
                                }
                                else
                                {
                                    Barcode_List.SetSelected(i, false);
                                }

                            }
                        }
                    }
                    else
                    {
                        // Shift가 눌리지 않았을 때 단일 선택
                        barcode_list_lastSelectedIndex = index;
                    }
                }
                barcode_count.Text = string.Format("{0}/{1} out of {1}", Barcode_List.SelectedItems.Count, Barcode_List.Items.Count);

                List<string> selectedBarcodes = Barcode_List.SelectedItems.Cast<string>().ToList();
                //Test Time List update
                TestTime_List.Items.Clear();
                if (selectedBarcodes.Count != 0) fileManager.TestTimeList_update(selectedBarcodes);

            }
        }

        public void TestTime_List_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int index = TestTime_List.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        // Shift가 눌렸을 때 범위 선택 구현
                        if (testtime_list_lastSelectedIndex >= 0 && testtime_list_lastSelectedIndex < TestTime_List.Items.Count)
                        {
                            int startIndex = Math.Min(testtime_list_lastSelectedIndex, index) + 1;
                            int endIndex = Math.Max(testtime_list_lastSelectedIndex, index);

                            // TestTime_List.ClearSelected();
                            for (int i = startIndex; i < endIndex; i++)
                            {
                                if (TestTime_List.GetSelected(i) == false)
                                {
                                    TestTime_List.SetSelected(i, true);
                                }
                                else
                                {
                                    TestTime_List.SetSelected(i, false);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Shift가 눌리지 않았을 때 단일 선택
                        testtime_list_lastSelectedIndex = index;

                    }
                }

                TestTime_count.Text = string.Format("{0}/{1}", TestTime_List.SelectedItems.Count, TestTime_List.Items.Count);

            }
        }

    }
    /// <summary>
    /// JH_DataAnalyzer 클래스 : 경로 선정, 데이터 분석 및 그래프 생성을 담당하는 주요 클래스
    /// </summary>
    public partial class JH_DataAnalyzer : Form
    {
        private FileProcessor fileManager = new FileProcessor();
        private GraphManager graphManager = new GraphManager();
        private UIController uiManager = new UIController();

        public JH_DataAnalyzer()
        {
            InitializeComponent();

            // 각 클래스 초기화
            fileManager.Initialize(TestName_List, Barcode_List, TestTime_List, TestTime_count, TestLogPath);
            graphManager.Initialize(this);
            uiManager.Initialize(this, fileManager, graphManager, Barcode_List, barcode_count, appendixCheckBox, TestLogPath, TestName_List, TestTime_List, TestTime_count);
        }

        //폼 및 컨트롤 변수
        public Form graphForm;
        public Form oneGraphForm;
        public List<string> currentFilePaths;
        private List<string> loadedFolders = new List<string>();
        private List<ZedGraphControl> allGraphs = new List<ZedGraphControl>();

        //타이머 및 상태 저장 변수
        public Dictionary<ZedGraphControl, GraphState> InitialStates = new Dictionary<ZedGraphControl, GraphState>();


        //그래프 컨트롤 및 체크리스트 박스 변수
        public ZedGraphControl oneGraphControl;
        public CheckedListBox columnCheckList;
        private HashSet<string> listBoxItems = new HashSet<string>();
        List<ZedGraphControl> tempGraphs = new List<ZedGraphControl>();

        //그래프를 그리기 위한 항목 필터링을 위한 목록
        public string[] desiredHeaders = {
        "AF_current_DAC", "OISX_current_DAC", "OISY_current_DAC","Laser_AF_Z_um", "Laser_OIS_X_um", "Laser_OIS_Y_um",
        "Laser_AF_TiltX_min", "Laser_AF_TiltY_min", "AF_command_um", "OISX_command_um", "OISY_command_um",
        "AF_APS_lsb", "OISX_APS_lsb", "OISY_APS_lsb", "NTC_Temp", "INT_Temp", "b1_coil_res",
        "b2_coil_res", "b3_coil_res","OIS_X","OIS_Y","AF_Z"};

    }
}