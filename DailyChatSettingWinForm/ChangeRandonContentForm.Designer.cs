namespace DailyChatSettingWinForm
{
	partial class ChangeRandonContentForm
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
			label = new Label();
			textBox = new TextBox();
			OKButton = new Button();
			CancelButton = new Button();
			DeleteButton = new Button();
			SuspendLayout();
			// 
			// label
			// 
			label.AutoSize = true;
			label.Font = new Font("SDK_SC_Web", 15F, FontStyle.Bold, GraphicsUnit.Point, 134);
			label.Location = new Point(23, 32);
			label.Margin = new Padding(4, 0, 4, 0);
			label.Name = "label";
			label.Size = new Size(90, 31);
			label.TabIndex = 0;
			label.Text = "label1";
			// 
			// textBox
			// 
			textBox.AllowDrop = true;
			textBox.Font = new Font("SDK_SC_Web", 19F, FontStyle.Bold, GraphicsUnit.Point, 134);
			textBox.Location = new Point(12, 88);
			textBox.Multiline = true;
			textBox.Name = "textBox";
			textBox.Size = new Size(905, 242);
			textBox.TabIndex = 1;
			// 
			// OKButton
			// 
			OKButton.Location = new Point(945, 32);
			OKButton.Name = "OKButton";
			OKButton.Size = new Size(181, 69);
			OKButton.TabIndex = 2;
			OKButton.Text = "确认修改";
			OKButton.UseVisualStyleBackColor = true;
			OKButton.Click += OKButton_Click;
			// 
			// CancelButton
			// 
			CancelButton.Location = new Point(945, 135);
			CancelButton.Name = "CancelButton";
			CancelButton.Size = new Size(181, 69);
			CancelButton.TabIndex = 3;
			CancelButton.Text = "取消";
			CancelButton.UseVisualStyleBackColor = true;
			CancelButton.Click += CancelButton_Click;
			// 
			// DeleteButton
			// 
			DeleteButton.BackColor = Color.Red;
			DeleteButton.ForeColor = SystemColors.Control;
			DeleteButton.Location = new Point(945, 232);
			DeleteButton.Name = "DeleteButton";
			DeleteButton.Size = new Size(181, 66);
			DeleteButton.TabIndex = 4;
			DeleteButton.Text = "删除该元素";
			DeleteButton.UseVisualStyleBackColor = false;
			DeleteButton.Click += DeleteButton_Click;
			// 
			// ChangeRandonContentForm
			// 
			AutoScaleDimensions = new SizeF(11F, 22F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1151, 342);
			Controls.Add(DeleteButton);
			Controls.Add(CancelButton);
			Controls.Add(OKButton);
			Controls.Add(textBox);
			Controls.Add(label);
			Font = new Font("SDK_SC_Web", 11F, FontStyle.Bold, GraphicsUnit.Point, 134);
			Margin = new Padding(4, 3, 4, 3);
			Name = "ChangeRandonContentForm";
			Text = "ChangeRandonContentForm";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label label;
		private TextBox textBox;
		private Button OKButton;
		private Button CancelButton;
		private Button DeleteButton;
	}
}