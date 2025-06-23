namespace DailyChatSettingWinForm
{
	partial class AddRandomContentForm
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
			KeyPanel = new FlowLayoutPanel();
			label1 = new Label();
			label2 = new Label();
			ValuePanel = new FlowLayoutPanel();
			AddKey = new Button();
			DeleteKey = new Button();
			AddValue = new Button();
			SuspendLayout();
			// 
			// KeyPanel
			// 
			KeyPanel.Location = new Point(13, 36);
			KeyPanel.Margin = new Padding(4, 3, 4, 3);
			KeyPanel.Name = "KeyPanel";
			KeyPanel.Size = new Size(306, 495);
			KeyPanel.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(13, 9);
			label1.Name = "label1";
			label1.Size = new Size(46, 24);
			label1.TabIndex = 1;
			label1.Text = "Key";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(330, 9);
			label2.Name = "label2";
			label2.Size = new Size(61, 24);
			label2.TabIndex = 2;
			label2.Text = "Value";
			// 
			// ValuePanel
			// 
			ValuePanel.Location = new Point(330, 36);
			ValuePanel.Margin = new Padding(4, 3, 4, 3);
			ValuePanel.Name = "ValuePanel";
			ValuePanel.Size = new Size(1140, 563);
			ValuePanel.TabIndex = 1;
			// 
			// AddKey
			// 
			AddKey.Location = new Point(12, 537);
			AddKey.Name = "AddKey";
			AddKey.Size = new Size(307, 62);
			AddKey.TabIndex = 3;
			AddKey.Text = "添加Key";
			AddKey.UseVisualStyleBackColor = true;
			AddKey.Click += AddKey_Click;
			// 
			// DeleteKey
			// 
			DeleteKey.BackColor = Color.Firebrick;
			DeleteKey.ForeColor = SystemColors.ButtonHighlight;
			DeleteKey.Location = new Point(12, 609);
			DeleteKey.Name = "DeleteKey";
			DeleteKey.Size = new Size(307, 62);
			DeleteKey.TabIndex = 4;
			DeleteKey.Text = "删除Key";
			DeleteKey.UseVisualStyleBackColor = false;
			DeleteKey.Click += DeleteKey_Click;
			// 
			// AddValue
			// 
			AddValue.Location = new Point(330, 609);
			AddValue.Name = "AddValue";
			AddValue.Size = new Size(307, 62);
			AddValue.TabIndex = 5;
			AddValue.Text = "添加Value";
			AddValue.UseVisualStyleBackColor = true;
			AddValue.Click += AddValue_Click;
			// 
			// AddRandomContentForm
			// 
			AutoScaleDimensions = new SizeF(11F, 22F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1483, 683);
			Controls.Add(AddValue);
			Controls.Add(DeleteKey);
			Controls.Add(AddKey);
			Controls.Add(ValuePanel);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(KeyPanel);
			Font = new Font("SDK_SC_Web", 11F, FontStyle.Bold, GraphicsUnit.Point, 134);
			Margin = new Padding(4, 3, 4, 3);
			Name = "AddRandomContentForm";
			Text = "AddRandomContentForm";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private FlowLayoutPanel KeyPanel;
		private Label label1;
		private Label label2;
		private FlowLayoutPanel ValuePanel;
		private Button AddKey;
		private Button DeleteKey;
		private Button AddValue;
	}
}