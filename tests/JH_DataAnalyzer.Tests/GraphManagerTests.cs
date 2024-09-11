using Xunit;
using System.Windows.Forms;
using System.Drawing.Imaging;
using ZedGraph;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing.Imaging;

namespace JH_DataAnalyzer.Tests
{
    // 테스트용 더미 클래스
    public class JH_DataAnalyzer { }

    public class GraphManagerTests
    {
        [Fact]
        public void TestGraphCreation()
        {
            // Arrange
            var graphManager = new GraphManager();
            var form = new JH_DataAnalyzer();

            // Act
            graphManager.Initialize(form);
            graphManager.CreateGraphForm();

            // Assert
            var graphForm = graphManager.GetGraphForm();
            Assert.NotNull(graphForm);
            Assert.Equal("Jahwa Data Analyzer - /", graphForm.Text);
            Assert.Equal(12, ((TableLayoutPanel)graphForm.Controls[0]).Controls.Count);
        }

        [Fact]
        public void TestUpdateGraphForm()
        {
            // Arrange
            var graphManager = new GraphManager();
            var form = new JH_DataAnalyzer();
            graphManager.Initialize(form);
            graphManager.CreateGraphForm();

            var filePaths = new List<string> { @"C:\test.csv" };
            var selectedBarcodes = new List<string> { "BC001" };
            var selectedTestNames = new List<string> { "TestName1" };

            // Act
            graphManager.UpdateGraphForm(filePaths, selectedBarcodes, selectedTestNames);

            // Assert
            var graphForm = graphManager.GetGraphForm();
            Assert.Equal("Jahwa Data Analyzer - BC001 / TestName1", graphForm.Text);
        }

        [Fact]
        public void TestZoomEvent()
        {
            // Arrange
            var graphManager = new GraphManager();
            var form = new JH_DataAnalyzer();
            graphManager.Initialize(form);
            graphManager.CreateGraphForm();

            var zgc = new ZedGraphControl();

            var oldPane = zgc.GraphPane.Clone() as GraphPane;
            oldPane.XAxis.Scale.Min = 0;
            oldPane.XAxis.Scale.Max = 200;
            oldPane.YAxis.Scale.Min = 0;
            oldPane.YAxis.Scale.Max = 100;

            // 새로운 줌 상태 설정
            var newPane = zgc.GraphPane.Clone() as GraphPane;
            var newXMin = 50;
            var newXMax = 150;
            newPane.XAxis.Scale.Min = newXMin;
            newPane.XAxis.Scale.Max = newXMax;

            // ZoomState 객체 생성
            ZoomState oldState = new ZoomState(oldPane, ZoomState.StateType.Zoom);
            ZoomState newState = new ZoomState(newPane, ZoomState.StateType.Zoom);

            // zgc_ZoomEvent 메서드 호출
            graphManager.zgc_ZoomEvent(zgc, oldState, newState);

            // Assert
            var graphForm = graphManager.GetGraphForm();
            var tableLayoutPanel = graphForm.Controls[0] as TableLayoutPanel;
            if (tableLayoutPanel != null)
            {
                foreach (var control in tableLayoutPanel.Controls)
                {
                    if (control is ZedGraphControl graphControl)
                    {
                        Assert.Equal(newXMin, graphControl.GraphPane.XAxis.Scale.Min);
                        Assert.Equal(newXMax, graphControl.GraphPane.XAxis.Scale.Max);
                    }
                }
            }
        }


        public class GraphManager
        {
            private JH_DataAnalyzer JH_Form;
            private Form graphForm;
            private List<ZedGraphControl> allGraphs = new List<ZedGraphControl>();
            private List<ZedGraphControl> tempGraphs = new List<ZedGraphControl>();

            public static string SaveFileDialogFilter { get; } = "Portable Network Graphics (*.png)|*.png|JPEG-Image (*.jpg)|*.jpg|TIFF Image (*.tiff)|*.tiff";
            public static ImageFormat[] ImageFormats { get; } = { ImageFormat.Png, ImageFormat.Jpeg, ImageFormat.Tiff };

            public void Initialize(JH_DataAnalyzer form)
            {
                JH_Form = form;
            }

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

            public void UpdateGraphForm(List<string> filePaths, List<string> selectedBarcodes, List<string> selectedTestNames)
            {
                if (graphForm.IsHandleCreated)
                {
                    graphForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
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
                    zgc.ZoomEvent += (s, oldState, newState) => zgc_ZoomEvent((ZedGraphControl)s, oldState, newState);
                }
            }

            public void Closing(object sender, FormClosingEventArgs e)
            {
                allGraphs.Clear();
            }

            public Form GetGraphForm()
            {
                return graphForm;
            }

            public void zgc_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
            {
                if (IsZoomStateChanged(oldState, newState))
                {
                    sender.GraphPane.XAxis.Scale.MajorStep = (sender.GraphPane.XAxis.Scale.Max - sender.GraphPane.XAxis.Scale.Min) / 10;
                    SynchronizeZoom(sender);
                }
            }

            private bool IsZoomStateChanged(ZoomState oldState, ZoomState newState)
            {
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

            private bool AreScalesEqual(double oldMin, double oldMax, double newMin, double newMax)
            {
                const double epsilon = 1e-10;
                return Math.Abs(oldMin - newMin) < epsilon && Math.Abs(oldMax - newMax) < epsilon;
            }

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
                        zgc.Refresh();
                        zgc.Invalidate();
                    }
                }
            }

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

}
