namespace DailyChatSettingWinForm
{
	partial class ReplyItemForm
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
			ReplyIndexPanel = new FlowLayoutPanel();
			DeleteReplyButtonn = new Button();
			AddReplyButton = new Button();
			label1 = new Label();
			MessagePanel = new FlowLayoutPanel();
			SuspendLayout();
			// 
			// ReplyIndexPanel
			// 
			ReplyIndexPanel.Location = new Point(13, 41);
			ReplyIndexPanel.Name = "ReplyIndexPanel";
			ReplyIndexPanel.Size = new Size(276, 537);
			ReplyIndexPanel.TabIndex = 0;
			// 
			// DeleteReplyButtonn
			// 
			DeleteReplyButtonn.Location = new Point(13, 647);
			DeleteReplyButtonn.Margin = new Padding(4, 3, 4, 3);
			DeleteReplyButtonn.Name = "DeleteReplyButtonn";
			DeleteReplyButtonn.Size = new Size(276, 57);
			DeleteReplyButtonn.TabIndex = 15;
			DeleteReplyButtonn.Text = "删除回复";
			DeleteReplyButtonn.UseVisualStyleBackColor = true;
			DeleteReplyButtonn.Click += DeleteReplyButtonn_Click;
			// 
			// AddReplyButton
			// 
			AddReplyButton.Location = new Point(13, 584);
			AddReplyButton.Margin = new Padding(4, 3, 4, 3);
			AddReplyButton.Name = "AddReplyButton";
			AddReplyButton.Size = new Size(276, 57);
			AddReplyButton.TabIndex = 14;
			AddReplyButton.Text = "添加回复";
			AddReplyButton.UseVisualStyleBackColor = true;
			AddReplyButton.Click += AddReplyButton_Click;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(13, 9);
			label1.Name = "label1";
			label1.Size = new Size(48, 24);
			label1.TabIndex = 0;
			label1.Text = "回复";
			// 
			// MessagePanel
			// 
			MessagePanel.AutoScroll = true;
			MessagePanel.BackColor = SystemColors.ControlLight;
			MessagePanel.Location = new Point(296, 9);
			MessagePanel.Name = "MessagePanel";
			MessagePanel.Size = new Size(793, 695);
			MessagePanel.TabIndex = 16;
			MessagePanel.WrapContents = false;
			// 
			// ReplyItemForm
			// 
			AutoScaleDimensions = new SizeF(11F, 22F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1101, 716);
			Controls.Add(MessagePanel);
			Controls.Add(label1);
			Controls.Add(DeleteReplyButtonn);
			Controls.Add(AddReplyButton);
			Controls.Add(ReplyIndexPanel);
			Font = new Font("SDK_SC_Web", 11F, FontStyle.Bold, GraphicsUnit.Point, 134);
			Margin = new Padding(4);
			Name = "ReplyItemForm";
			Text = "ReplyItemForm";
			Load += ReplyItemForm_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private FlowLayoutPanel ReplyIndexPanel;
		private Button DeleteReplyButtonn;
		private Button AddReplyButton;
		private Label label1;
		private FlowLayoutPanel MessagePanel;
	}
}