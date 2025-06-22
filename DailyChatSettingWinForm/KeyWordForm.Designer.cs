namespace DailyChatSettingWinForm
{
	partial class KeyWordForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			DeleteTriggerButtonn = new Button();
			AddTriggerButton = new Button();
			label2 = new Label();
			TriggerPanel = new FlowLayoutPanel();
			DeleteWhiteList = new Button();
			AddWhiteList = new Button();
			WhiteListPanel = new FlowLayoutPanel();
			label5 = new Label();
			DeleteKeyWord = new Button();
			NormalReplyButton = new Button();
			SuspendLayout();
			// 
			// DeleteTriggerButtonn
			// 
			DeleteTriggerButtonn.Location = new Point(15, 705);
			DeleteTriggerButtonn.Margin = new Padding(4, 3, 4, 3);
			DeleteTriggerButtonn.Name = "DeleteTriggerButtonn";
			DeleteTriggerButtonn.Size = new Size(276, 57);
			DeleteTriggerButtonn.TabIndex = 13;
			DeleteTriggerButtonn.Text = "删除触发词";
			DeleteTriggerButtonn.UseVisualStyleBackColor = true;
			DeleteTriggerButtonn.Click += DeleteTriggerButtonn_Click;
			// 
			// AddTriggerButton
			// 
			AddTriggerButton.Location = new Point(15, 641);
			AddTriggerButton.Margin = new Padding(4, 3, 4, 3);
			AddTriggerButton.Name = "AddTriggerButton";
			AddTriggerButton.Size = new Size(276, 57);
			AddTriggerButton.TabIndex = 12;
			AddTriggerButton.Text = "添加触发词";
			AddTriggerButton.UseVisualStyleBackColor = true;
			AddTriggerButton.Click += AddTriggerButton_Click;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(15, 10);
			label2.Margin = new Padding(4, 0, 4, 0);
			label2.Name = "label2";
			label2.Size = new Size(67, 24);
			label2.TabIndex = 11;
			label2.Text = "触发词";
			// 
			// TriggerPanel
			// 
			TriggerPanel.AutoScroll = true;
			TriggerPanel.Location = new Point(15, 39);
			TriggerPanel.Margin = new Padding(4, 3, 4, 3);
			TriggerPanel.Name = "TriggerPanel";
			TriggerPanel.Size = new Size(271, 595);
			TriggerPanel.TabIndex = 10;
			// 
			// DeleteWhiteList
			// 
			DeleteWhiteList.Location = new Point(298, 705);
			DeleteWhiteList.Name = "DeleteWhiteList";
			DeleteWhiteList.Size = new Size(226, 57);
			DeleteWhiteList.TabIndex = 21;
			DeleteWhiteList.Text = "删除白名单";
			DeleteWhiteList.UseVisualStyleBackColor = true;
			DeleteWhiteList.Click += DeleteWhiteList_Click;
			// 
			// AddWhiteList
			// 
			AddWhiteList.Location = new Point(298, 641);
			AddWhiteList.Name = "AddWhiteList";
			AddWhiteList.Size = new Size(226, 57);
			AddWhiteList.TabIndex = 20;
			AddWhiteList.Text = "添加白名单";
			AddWhiteList.UseVisualStyleBackColor = true;
			AddWhiteList.Click += AddWhiteList_Click;
			// 
			// WhiteListPanel
			// 
			WhiteListPanel.AutoScroll = true;
			WhiteListPanel.Location = new Point(293, 39);
			WhiteListPanel.Name = "WhiteListPanel";
			WhiteListPanel.Size = new Size(231, 595);
			WhiteListPanel.TabIndex = 18;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new Point(293, 9);
			label5.Name = "label5";
			label5.Size = new Size(67, 24);
			label5.TabIndex = 19;
			label5.Text = "白名单";
			// 
			// DeleteKeyWord
			// 
			DeleteKeyWord.Location = new Point(681, 12);
			DeleteKeyWord.Name = "DeleteKeyWord";
			DeleteKeyWord.Size = new Size(162, 83);
			DeleteKeyWord.TabIndex = 22;
			DeleteKeyWord.Text = "删除该关键词";
			DeleteKeyWord.UseVisualStyleBackColor = true;
			DeleteKeyWord.Click += DeleteKeyWord_Click;
			// 
			// NormalReplyButton
			// 
			NormalReplyButton.Location = new Point(530, 641);
			NormalReplyButton.Name = "NormalReplyButton";
			NormalReplyButton.Size = new Size(226, 57);
			NormalReplyButton.TabIndex = 23;
			NormalReplyButton.Text = "普通回复";
			NormalReplyButton.UseVisualStyleBackColor = true;
			NormalReplyButton.Click += NormalReplyButton_Click;
			// 
			// KeyWordForm
			// 
			AutoScaleDimensions = new SizeF(11F, 22F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(855, 774);
			Controls.Add(NormalReplyButton);
			Controls.Add(DeleteKeyWord);
			Controls.Add(DeleteWhiteList);
			Controls.Add(AddWhiteList);
			Controls.Add(WhiteListPanel);
			Controls.Add(label5);
			Controls.Add(DeleteTriggerButtonn);
			Controls.Add(AddTriggerButton);
			Controls.Add(label2);
			Controls.Add(TriggerPanel);
			Font = new Font("SDK_SC_Web", 11F, FontStyle.Bold, GraphicsUnit.Point, 134);
			Margin = new Padding(4, 3, 4, 3);
			Name = "KeyWordForm";
			Text = "KeyWordForm";
			Load += KeyWordForm_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button DeleteTriggerButtonn;
		private Button AddTriggerButton;
		private Label label2;
		private FlowLayoutPanel TriggerPanel;
		private Button DeleteWhiteList;
		private Button AddWhiteList;
		private FlowLayoutPanel WhiteListPanel;
		private Label label5;
		private Button DeleteKeyWord;
		private Button NormalReplyButton;
	}
}