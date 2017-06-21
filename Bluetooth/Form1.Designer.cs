namespace Bluetooth
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.pbFsrCurve = new System.Windows.Forms.PictureBox();
            this.timer3 = new System.Windows.Forms.Timer(this.components);
            this.timer4 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.Time_Label = new System.Windows.Forms.Label();
            this.bGo = new System.Windows.Forms.Button();
            this.timer5 = new System.Windows.Forms.Timer(this.components);
            this.tb_pipelineSize = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lbHeight = new System.Windows.Forms.Label();
            this.btF = new System.Windows.Forms.Button();
            this.btB = new System.Windows.Forms.Button();
            this.btH = new System.Windows.Forms.Button();
            this.btR = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFsrCurve)).BeginInit();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(386, 67);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(200, 64);
            this.listBox1.TabIndex = 0;
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // tbOutput
            // 
            this.tbOutput.Location = new System.Drawing.Point(12, 12);
            this.tbOutput.Multiline = true;
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbOutput.Size = new System.Drawing.Size(368, 118);
            this.tbOutput.TabIndex = 5;
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.Color.White;
            this.pictureBox.Location = new System.Drawing.Point(641, 12);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(180, 166);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox.TabIndex = 7;
            this.pictureBox.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // pbFsrCurve
            // 
            this.pbFsrCurve.Location = new System.Drawing.Point(592, 193);
            this.pbFsrCurve.Name = "pbFsrCurve";
            this.pbFsrCurve.Size = new System.Drawing.Size(269, 231);
            this.pbFsrCurve.TabIndex = 10;
            this.pbFsrCurve.TabStop = false;
            // 
            // timer3
            // 
            this.timer3.Tick += new System.EventHandler(this.timer3_Tick);
            // 
            // timer4
            // 
            this.timer4.Tick += new System.EventHandler(this.timer4_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(97, 427);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(257, 12);
            this.label1.TabIndex = 18;
            this.label1.Text = "比例尺 ( map scale 輸入1即為圖上的1pixel為1cm )";
            //this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 423);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(79, 22);
            this.textBox1.TabIndex = 16;
            this.textBox1.Text = "Enter map scale";
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_PressKey);
            // 
            // Time_Label
            // 
            this.Time_Label.AutoSize = true;
            this.Time_Label.Location = new System.Drawing.Point(590, 448);
            this.Time_Label.Name = "Time_Label";
            this.Time_Label.Size = new System.Drawing.Size(65, 12);
            this.Time_Label.TabIndex = 19;
            this.Time_Label.Text = "探索時間：";
            // 
            // bGo
            // 
            this.bGo.Enabled = false;
            this.bGo.Location = new System.Drawing.Point(386, 12);
            this.bGo.Name = "bGo";
            this.bGo.Size = new System.Drawing.Size(200, 49);
            this.bGo.TabIndex = 4;
            this.bGo.Text = "System will be ready for 10 second(s)";
            this.bGo.UseVisualStyleBackColor = true;
            this.bGo.Click += new System.EventHandler(this.bGo_Click);
            // 
            // timer5
            // 
            this.timer5.Interval = 1;
            this.timer5.Tick += new System.EventHandler(this.timer5_Tick);
            // 
            // tb_pipelineSize
            // 
            this.tb_pipelineSize.Enabled = false;
            this.tb_pipelineSize.Location = new System.Drawing.Point(661, 424);
            this.tb_pipelineSize.Name = "tb_pipelineSize";
            this.tb_pipelineSize.ReadOnly = true;
            this.tb_pipelineSize.Size = new System.Drawing.Size(100, 22);
            this.tb_pipelineSize.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(590, 427);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 25;
            this.label2.Text = "管徑大小：";
            // 
            // lbHeight
            // 
            this.lbHeight.AutoSize = true;
            this.lbHeight.Location = new System.Drawing.Point(360, 427);
            this.lbHeight.Name = "lbHeight";
            this.lbHeight.Size = new System.Drawing.Size(143, 12);
            this.lbHeight.TabIndex = 26;
            this.lbHeight.Text = "當前車體所在高度：0公分";
            // 
            // btF
            // 
            this.btF.Location = new System.Drawing.Point(12, 136);
            this.btF.Name = "btF";
            this.btF.Size = new System.Drawing.Size(138, 23);
            this.btF.TabIndex = 27;
            this.btF.Text = "Forward";
            this.btF.UseVisualStyleBackColor = true;
            this.btF.Click += new System.EventHandler(this.btF_Click);
            // 
            // btB
            // 
            this.btB.Location = new System.Drawing.Point(154, 136);
            this.btB.Name = "btB";
            this.btB.Size = new System.Drawing.Size(139, 23);
            this.btB.TabIndex = 28;
            this.btB.Text = "Backward";
            this.btB.UseVisualStyleBackColor = true;
            this.btB.Click += new System.EventHandler(this.btB_Click);
            // 
            // btH
            // 
            this.btH.Location = new System.Drawing.Point(300, 136);
            this.btH.Name = "btH";
            this.btH.Size = new System.Drawing.Size(140, 23);
            this.btH.TabIndex = 29;
            this.btH.Text = "Half Speed";
            this.btH.UseVisualStyleBackColor = true;
            this.btH.Click += new System.EventHandler(this.btH_Click);
            // 
            // btR
            // 
            this.btR.Location = new System.Drawing.Point(446, 136);
            this.btR.Name = "btR";
            this.btR.Size = new System.Drawing.Size(140, 23);
            this.btR.TabIndex = 30;
            this.btR.Text = "Release";
            this.btR.UseVisualStyleBackColor = true;
            this.btR.Click += new System.EventHandler(this.btR_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(97, 557);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 12);
            this.label3.TabIndex = 31;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(868, 467);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btR);
            this.Controls.Add(this.btH);
            this.Controls.Add(this.btB);
            this.Controls.Add(this.btF);
            this.Controls.Add(this.lbHeight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_pipelineSize);
            this.Controls.Add(this.bGo);
            this.Controls.Add(this.Time_Label);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.pbFsrCurve);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.listBox1);
            this.KeyPreview = true;
            this.Name = "Form1";
            this.Text = "BT";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFsrCurve)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.PictureBox pbFsrCurve;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.Timer timer4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label Time_Label;
        private System.Windows.Forms.Button bGo;
        private System.Windows.Forms.Timer timer5;
        private System.Windows.Forms.TextBox tb_pipelineSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbHeight;
        private System.Windows.Forms.Button btF;
        private System.Windows.Forms.Button btB;
        private System.Windows.Forms.Button btH;
        private System.Windows.Forms.Button btR;
        private System.Windows.Forms.Label label3;
    }
}

