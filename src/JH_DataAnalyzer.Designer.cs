using System;

namespace JH_DataAnalyzer
{
    partial class JH_DataAnalyzer
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JH_DataAnalyzer));
            this.LoadTestLog = new System.Windows.Forms.Button();
            this.TestName_List = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Barcode_List = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TestTime_List = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.appendixCheckBox = new System.Windows.Forms.CheckBox();
            this.TestLogPath = new System.Windows.Forms.TextBox();
            this.ShowGraphs = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.TestTime_count = new System.Windows.Forms.Label();
            this.barcode_count = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LoadTestLog
            // 
            this.LoadTestLog.Location = new System.Drawing.Point(12, 12);
            this.LoadTestLog.Name = "LoadTestLog";
            this.LoadTestLog.Size = new System.Drawing.Size(124, 66);
            this.LoadTestLog.TabIndex = 0;
            this.LoadTestLog.Text = "Load Test Log";
            this.LoadTestLog.UseVisualStyleBackColor = true;
            this.LoadTestLog.Click += new System.EventHandler(uiManager.LoadTestLog_Click);

            // 
            // TestName_List
            // 
            this.TestName_List.FormattingEnabled = true;
            this.TestName_List.ItemHeight = 12;
            this.TestName_List.Location = new System.Drawing.Point(12, 134);
            this.TestName_List.Name = "TestName_List";
            this.TestName_List.Size = new System.Drawing.Size(221, 316);
            this.TestName_List.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Test Name";
            // 
            // Barcode_List
            // 
            this.Barcode_List.FormattingEnabled = true;
            this.Barcode_List.HorizontalScrollbar = true;
            this.Barcode_List.ItemHeight = 12;
            this.Barcode_List.Location = new System.Drawing.Point(250, 134);
            this.Barcode_List.Name = "Barcode_List";
            this.Barcode_List.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.Barcode_List.Size = new System.Drawing.Size(221, 316);
            this.Barcode_List.TabIndex = 1;
            this.Barcode_List.MouseDown += new System.Windows.Forms.MouseEventHandler(uiManager.Barcode_List_MouseDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(250, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Barcode";
            // 
            // TestTime_List
            // 
            this.TestTime_List.FormattingEnabled = true;
            this.TestTime_List.HorizontalScrollbar = true;
            this.TestTime_List.ItemHeight = 12;
            this.TestTime_List.Location = new System.Drawing.Point(489, 134);
            this.TestTime_List.Name = "TestTime_List";
            this.TestTime_List.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.TestTime_List.Size = new System.Drawing.Size(221, 316);
            this.TestTime_List.TabIndex = 1;
            this.TestTime_List.MouseDown += new System.Windows.Forms.MouseEventHandler(uiManager.TestTime_List_MouseDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(489, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "Test Time";
            // 
            // appendixCheckBox
            // 
            this.appendixCheckBox.AutoSize = true;
            this.appendixCheckBox.Location = new System.Drawing.Point(142, 32);
            this.appendixCheckBox.Name = "appendixCheckBox";
            this.appendixCheckBox.Size = new System.Drawing.Size(67, 16);
            this.appendixCheckBox.TabIndex = 3;
            this.appendixCheckBox.Text = "Append";
            this.appendixCheckBox.UseVisualStyleBackColor = true;
            // 
            // TestLogPath
            // 
            this.TestLogPath.Location = new System.Drawing.Point(215, 30);
            this.TestLogPath.Name = "TestLogPath";
            this.TestLogPath.Size = new System.Drawing.Size(365, 21);
            this.TestLogPath.TabIndex = 4;
            // 
            // ShowGraphs
            // 
            this.ShowGraphs.Location = new System.Drawing.Point(586, 12);
            this.ShowGraphs.Name = "ShowGraphs";
            this.ShowGraphs.Size = new System.Drawing.Size(122, 36);
            this.ShowGraphs.TabIndex = 5;
            this.ShowGraphs.Text = "Show All Graphs";
            this.ShowGraphs.UseVisualStyleBackColor = true;
            this.ShowGraphs.Click += new System.EventHandler(uiManager.ShowGraphs_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(586, 54);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(122, 36);
            this.button1.TabIndex = 6;
            this.button1.Text = "Show One Graph";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(uiManager.Show_One_Graph_Click);
            // 
            // TestTime_count
            // 
            this.TestTime_count.AutoSize = true;
            this.TestTime_count.Location = new System.Drawing.Point(558, 108);
            this.TestTime_count.Name = "TestTime_count";
            this.TestTime_count.Size = new System.Drawing.Size(23, 12);
            this.TestTime_count.TabIndex = 2;
            this.TestTime_count.Text = "0/0";
            // 
            // barcode_count
            // 
            this.barcode_count.AutoSize = true;
            this.barcode_count.Location = new System.Drawing.Point(308, 108);
            this.barcode_count.Name = "barcode_count";
            this.barcode_count.Size = new System.Drawing.Size(68, 12);
            this.barcode_count.TabIndex = 2;
            this.barcode_count.Text = "0/0 out of 0";
            // 
            // JH_DataAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(726, 466);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ShowGraphs);
            this.Controls.Add(this.TestLogPath);
            this.Controls.Add(this.appendixCheckBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TestTime_count);
            this.Controls.Add(this.barcode_count);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TestTime_List);
            this.Controls.Add(this.Barcode_List);
            this.Controls.Add(this.TestName_List);
            this.Controls.Add(this.LoadTestLog);
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JAHWA.ico");
            this.Icon = new System.Drawing.Icon(iconPath);
            this.Name = "JH_DataAnalyzer";
            this.Text = "Jahwa Data Analyzer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadTestLog;
        private System.Windows.Forms.ListBox TestName_List;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox Barcode_List;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox TestTime_List;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox appendixCheckBox;
        private System.Windows.Forms.TextBox TestLogPath;
        private System.Windows.Forms.Button ShowGraphs;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label TestTime_count;
        private System.Windows.Forms.Label barcode_count;
    }
}

