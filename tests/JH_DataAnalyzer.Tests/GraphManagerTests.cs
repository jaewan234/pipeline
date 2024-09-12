using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Xunit;
using ZedGraph;

/// <summary>
/// JH_DataAnalyzer는 데이터 분석 및 시각화를 위한 네임스페이스입니다.
/// </summary>
namespace JH_DataAnalyzer.Tests
{
    /// <summary>
    /// GraphManager 클래스의 단위 테스트를 수행하는 클래스입니다.
    /// </summary>
    public class GraphManagerTests
    {
        /// <summary>
        /// 그래프 생성 기능을 테스트합니다.
        /// </summary>
        [Fact]
        public void TestGraphCreation()
        {
            // Arrange
            var graphManager = new GraphManager();
            var mockForm = new JH_DataAnalyzer();
            graphManager.Initialize(mockForm);

            // Act
            graphManager.CreateGraphForm();
            var form = graphManager.GetGraphForm();

            // Assert
            Assert.NotNull(form);
            Assert.Equal("Jahwa Data Analyzer - /", form.Text);
            Assert.Equal(new Size((int)(Screen.PrimaryScreen.WorkingArea.Width * 0.7),
                (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.7)),
                form.Size);
            Assert.Equal(FormStartPosition.CenterScreen, form.StartPosition);
            Assert.IsType<TableLayoutPanel>(form.Controls[0]);
            Assert.Equal(4, ((TableLayoutPanel)form.Controls[0]).RowCount);
            Assert.Equal(3, ((TableLayoutPanel)form.Controls[0]).ColumnCount);
        }

        /// <summary>
        /// 줌 이벤트 기능을 테스트합니다.
        /// </summary>
        [Fact]
        public void TestZoomEvent()
        {
            // Arrange
            var graphManager = new GraphManager();
            var mockForm = new JH_DataAnalyzer();
            graphManager.Initialize(mockForm);
            graphManager.CreateGraphForm();

            // Act & Assert
            Assert.NotNull(typeof(GraphManager).GetMethod("zgc_ZoomEvent"));
        }

        /// <summary>
        /// 그래프 폼 업데이트 기능을 테스트합니다.
        /// </summary>
        [Fact]
        public void TestUpdateGraphForm()
        {
            // Arrange
            var graphManager = new GraphManager();
            var mockForm = new JH_DataAnalyzer();
            graphManager.Initialize(mockForm);
            graphManager.CreateGraphForm();
            var filePaths = new List<string> { "path1.csv", "path2.csv" };
            var selectedBarcodes = new List<string> { "Barcode1", "Barcode2" };
            var selectedTestNames = new List<string> { "Test1", "Test2" };

            // Act
            graphManager.UpdateGraphForm(filePaths, selectedBarcodes, selectedTestNames);

            // Assert
            var form = graphManager.GetGraphForm();
            Assert.Equal("Jahwa Data Analyzer - Barcode1, Barcode2 / Test1, Test2", form.Text);
            Assert.Equal(new Point(0, 0), form.Location);
            Assert.Equal(12, ((TableLayoutPanel)form.Controls[0]).Controls.Count);
        }
    }

    /// <summary>
    /// 그래프 관리 및 생성을 담당하는 클래스입니다.
    /// </summary>
    public class GraphManager
    {
        private JH_DataAnalyzer JH_Form;
        private Form graphForm;
        private List<ZedGraphControl> allGraphs = new List<ZedGraphControl>();

        /// <summary>
        /// GraphManager를 초기화합니다.
        /// </summary>
        /// <param name="form">메인 폼 인스턴스</param>
        public void Initialize(JH_DataAnalyzer form)
        {
            JH_Form = form;
        }

        /// <summary>
        /// 그래프 폼을 생성합니다.
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
            graphForm.Text = "Jahwa Data Analyzer - /";

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
        /// 그래프 폼을 업데이트합니다.
        /// </summary>
        /// <param name="filePaths">파일 경로 목록</param>
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

        /// <summary>
        /// 그래프 폼 업데이트의 내부 구현입니다.
        /// </summary>
        /// <param name="filePaths">파일 경로 목록</param>
        /// <param name="selectedBarcodes">선택된 바코드 목록</param>
        /// <param name="selectedTestNames">선택된 테스트 이름 목록</param>
        private void UpdateGraphFormInternal(List<string> filePaths, List<string> selectedBarcodes, List<string> selectedTestNames)
        {
            string barcodesString = string.Join(", ", selectedBarcodes);
            string testNamesString = string.Join(", ", selectedTestNames);
            graphForm.Text = $"Jahwa Data Analyzer - {barcodesString} / {testNamesString}";
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
        /// 그래프 폼 닫기 이벤트 핸들러입니다.
        /// </summary>
        public void Closing(object sender, FormClosingEventArgs e)
        {
            allGraphs.Clear();
        }

        /// <summary>
        /// 그래프 폼을 반환합니다.
        /// </summary>
        /// <returns>그래프 폼 인스턴스</returns>
        public Form GetGraphForm()
        {
            return graphForm;
        }

        /// <summary>
        /// 줌 이벤트 핸들러입니다.
        /// </summary>
        public void zgc_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            if (IsZoomStateChanged(oldState, newState))
            {
                sender.GraphPane.XAxis.Scale.MajorStep = (sender.GraphPane.XAxis.Scale.Max - sender.GraphPane.XAxis.Scale.Min) / 10;
                SynchronizeZoom(sender);
            }
        }

        /// <summary>
        /// 줌 상태가 변경되었는지 확인합니다.
        /// </summary>
        /// <param name="oldState">이전 줌 상태</param>
        /// <param name="newState">새로운 줌 상태</param>
        /// <returns>줌 상태 변경 여부</returns>
        private bool IsZoomStateChanged(ZoomState oldState, ZoomState newState)
        {
            if (oldState == null || newState == null)
                return true;
            GraphPane oldPane = oldState.GetType().GetProperty("GraphPane")?.GetValue(oldState) as GraphPane;
            GraphPane newPane = newState.GetType().GetProperty("GraphPane")?.GetValue(newState) as GraphPane;
            if (oldPane == null || newPane == null)
                return true;
            bool isXScaleChanged = !AreScalesEqual(oldPane.XAxis.Scale.Min, oldPane.XAxis.Scale.Max, newPane.XAxis.Scale.Min, newPane.XAxis.Scale.Max);
            bool isYScaleChanged = !AreScalesEqual(oldPane.YAxis.Scale.Min, oldPane.YAxis.Scale.Max, newPane.YAxis.Scale.Min, newPane.YAxis.Scale.Max);
            return isXScaleChanged || isYScaleChanged;
        }

        /// <summary>
        /// 두 스케일이 동일한지 확인합니다.
        /// </summary>
        /// <param name="oldMin">이전 최소값</param>
        /// <param name="oldMax">이전 최대값</param>
        /// <param name="newMin">새로운 최소값</param>
        /// <param name="newMax">새로운 최대값</param>
        /// <returns>스케일 동일 여부</returns>
        private bool AreScalesEqual(double oldMin, double oldMax, double newMin, double newMax)
        {
            const double epsilon = 1e-10;
            return Math.Abs(oldMin - newMin) < epsilon && Math.Abs(oldMax - newMax) < epsilon;
        }

        /// <summary>
        /// 모든 그래프의 줌을 동기화합니다.
        /// </summary>
        /// <param name="sourceGraph">줌 이벤트가 발생한 원본 그래프</param>
        private void SynchronizeZoom(ZedGraphControl sourceGraph)
        {
            foreach (var zgc in allGraphs)
            {
                if (zgc != sourceGraph)
                {
                    zgc.GraphPane.XAxis.Scale.Min = sourceGraph.GraphPane.XAxis.Scale.Min;
                    zgc.GraphPane.XAxis.Scale.Max = sourceGraph.GraphPane.XAxis.Scale.Max;
                    zgc.GraphPane.XAxis.Scale.MajorStep = sourceGraph.GraphPane.XAxis.Scale.MajorStep;
                    zgc.GraphPane.XAxis.Scale.MinorStep = sourceGraph.GraphPane.XAxis.Scale.MinorStep;
                    zgc.AxisChange();
                    zgc.Invalidate();
                }
            }
        }

        /// <summary>
        /// 그래프 컨트롤을 생성합니다.
        /// </summary>
        /// <param name="title">그래프 제목</param>
        /// <param name="filePaths">파일 경로 목록</param>
        /// <returns>생성된 ZedGraphControl 인스턴스</returns>
        private ZedGraphControl CreateGraphControl(string title, List<string> filePaths)
        {
            ZedGraphControl zgc = new ZedGraphControl();
            zgc.Dock = DockStyle.Fill;
            zgc.IsShowCopyMessage = false;
            GraphPane myPane = zgc.GraphPane;
            zgc.IsShowPointValues = true;
            zgc.ContextMenuBuilder += (sender, menuStrip, mousePt, objState) =>
            {
                for (int i = menuStrip.Items.Count - 1; i >= 0; i--)
                {
                    if (menuStrip.Items[i] is ToolStripMenuItem item && item.Text == "Set Scale to Default")
                    {
                        menuStrip.Items.RemoveAt(i);
                        break;
                    }
                }
                ToolStripMenuItem allCopyItem2 = new ToolStripMenuItem("Copy to All Graphs");
                allCopyItem2.Click += (s, e) => CopyEntireGraphWindow();
                menuStrip.Items.Add(allCopyItem2);
                ToolStripMenuItem allCopyItem3 = new ToolStripMenuItem("Save All Graphs");
                allCopyItem3.Click += (s, e) => CopyEntireGraphWindow3();
                menuStrip.Items.Add(allCopyItem3);
            };
            myPane.Title.Text = title;
            myPane.Title.FontSpec.Size = 20;
            myPane.Title.FontSpec.IsBold = true;
            myPane.XAxis.Title.Text = "time (msec)";
            myPane.XAxis.Title.FontSpec.Size = 20;
            myPane.XAxis.Title.FontSpec.IsBold = true;
            myPane.YAxis.Title.Text = GetYAxisTitle(title);
            myPane.YAxis.Title.FontSpec.Size = 18;
            myPane.YAxis.Title.FontSpec.IsBold = true;
            myPane.Legend.FontSpec.Size = 14;
            myPane.Legend.FontSpec.IsBold = true;
            // Here you would add the actual data to the graph based on the filePaths
            return zgc;
        }

        /// <summary>
        /// Y축 제목을 반환합니다.
        ///

        private string GetYAxisTitle(string graphTitle)
            {
                // Implement logic to return appropriate Y-axis title based on graph title
                return "Y-axis Title"; // Placeholder
            }

            private void CopyEntireGraphWindow()
            {
                // Implement logic to copy entire graph window
            }

            private void CopyEntireGraphWindow3()
            {
                // Implement logic to save all graphs
            }
        }
    }
