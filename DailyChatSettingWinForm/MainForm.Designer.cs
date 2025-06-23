namespace DailyChatSettingWinForm
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			openFileDialog1 = new OpenFileDialog();
			SeleteFileButton = new Button();
			SeleteFileLabel = new Label();
			label1 = new Label();
			AddKeyWordButton = new Button();
			flowLayoutPanel1 = new FlowLayoutPanel();
			KeyWordPanel = new FlowLayoutPanel();
			SaveButton = new Button();
			ShowJsonPanel = new FlowLayoutPanel();
			ShowJsonLabel = new Label();
			AddRandomContent = new Button();
			ShowJsonPanel.SuspendLayout();
			SuspendLayout();
			// 
			// openFileDialog1
			// 
			openFileDialog1.FileName = "openFileDialog1";
			openFileDialog1.FileOk += openFileDialog1_FileOk;
			// 
			// SeleteFileButton
			// 
			SeleteFileButton.Location = new Point(16, 14);
			SeleteFileButton.Margin = new Padding(6, 0, 6, 0);
			SeleteFileButton.Name = "SeleteFileButton";
			SeleteFileButton.Size = new Size(116, 30);
			SeleteFileButton.TabIndex = 0;
			SeleteFileButton.Text = "选择文件";
			SeleteFileButton.UseVisualStyleBackColor = true;
			SeleteFileButton.Click += SeleteFileButton_Click;
			// 
			// SeleteFileLabel
			// 
			SeleteFileLabel.AutoSize = true;
			SeleteFileLabel.Location = new Point(138, 17);
			SeleteFileLabel.Margin = new Padding(6, 0, 6, 0);
			SeleteFileLabel.Name = "SeleteFileLabel";
			SeleteFileLabel.Size = new Size(124, 24);
			SeleteFileLabel.TabIndex = 1;
			SeleteFileLabel.Text = "选择的文件：";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(16, 59);
			label1.Name = "label1";
			label1.Size = new Size(67, 24);
			label1.TabIndex = 3;
			label1.Text = "关键词";
			// 
			// AddKeyWordButton
			// 
			AddKeyWordButton.Location = new Point(12, 691);
			AddKeyWordButton.Name = "AddKeyWordButton";
			AddKeyWordButton.Size = new Size(226, 52);
			AddKeyWordButton.TabIndex = 4;
			AddKeyWordButton.Text = "添加关键词";
			AddKeyWordButton.UseVisualStyleBackColor = true;
			AddKeyWordButton.Click += AddKeyWordButton_Click;
			// 
			// flowLayoutPanel1
			// 
			flowLayoutPanel1.AutoScroll = true;
			flowLayoutPanel1.Location = new Point(92, 154);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new Size(0, 0);
			flowLayoutPanel1.TabIndex = 2;
			// 
			// KeyWordPanel
			// 
			KeyWordPanel.AutoScroll = true;
			KeyWordPanel.Location = new Point(16, 86);
			KeyWordPanel.Name = "KeyWordPanel";
			KeyWordPanel.Size = new Size(222, 599);
			KeyWordPanel.TabIndex = 5;
			// 
			// SaveButton
			// 
			SaveButton.Location = new Point(745, 691);
			SaveButton.Name = "SaveButton";
			SaveButton.Size = new Size(122, 52);
			SaveButton.TabIndex = 6;
			SaveButton.Text = "保存文件";
			SaveButton.UseVisualStyleBackColor = true;
			SaveButton.Click += SaveButton_Click;
			// 
			// ShowJsonPanel
			// 
			ShowJsonPanel.AutoScroll = true;
			ShowJsonPanel.Controls.Add(ShowJsonLabel);
			ShowJsonPanel.Location = new Point(244, 86);
			ShowJsonPanel.MaximumSize = new Size(623, 599);
			ShowJsonPanel.Name = "ShowJsonPanel";
			ShowJsonPanel.Size = new Size(623, 599);
			ShowJsonPanel.TabIndex = 7;
			// 
			// ShowJsonLabel
			// 
			ShowJsonLabel.AutoSize = true;
			ShowJsonLabel.Location = new Point(3, 0);
			ShowJsonLabel.MaximumSize = new Size(596, 0);
			ShowJsonLabel.Name = "ShowJsonLabel";
			ShowJsonLabel.Size = new Size(68, 24);
			ShowJsonLabel.TabIndex = 0;
			ShowJsonLabel.Text = "label2";
			// 
			// AddRandomContent
			// 
			AddRandomContent.Location = new Point(247, 691);
			AddRandomContent.Name = "AddRandomContent";
			AddRandomContent.Size = new Size(232, 52);
			AddRandomContent.TabIndex = 8;
			AddRandomContent.Text = "添加RandomContent";
			AddRandomContent.UseVisualStyleBackColor = true;
			AddRandomContent.Click += AddRandomContent_Click;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(11F, 22F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(879, 755);
			Controls.Add(AddRandomContent);
			Controls.Add(ShowJsonPanel);
			Controls.Add(SaveButton);
			Controls.Add(KeyWordPanel);
			Controls.Add(AddKeyWordButton);
			Controls.Add(label1);
			Controls.Add(flowLayoutPanel1);
			Controls.Add(SeleteFileLabel);
			Controls.Add(SeleteFileButton);
			Font = new Font("SDK_SC_Web", 11F);
			Margin = new Padding(6, 0, 6, 0);
			Name = "MainForm";
			Text = "MainForm";
			Load += Form_Load;
			ShowJsonPanel.ResumeLayout(false);
			ShowJsonPanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private OpenFileDialog openFileDialog1;
		private Button SeleteFileButton;
		private Label SeleteFileLabel;
		private Label label1;
		private Button AddKeyWordButton;
		private FlowLayoutPanel flowLayoutPanel1;
		private FlowLayoutPanel KeyWordPanel;
		private Button SaveButton;
		private FlowLayoutPanel ShowJsonPanel;
		private Label ShowJsonLabel;
		private Button AddRandomContent;
	}
}
