using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DailyChatSettingWinForm
{
	public partial class ChangeRandonContentForm : Form
	{

		public string InputValue => textBox.Text;
		public bool Deleted { get; private set; } = false;

		public ChangeRandonContentForm(string title, string message, string defaultValue)
		{
			InitializeComponent();

			this.Text = title;
			label.Text = message;
			textBox.Text = defaultValue;
		}

		private void OKButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void DeleteButton_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("确认要删除嘛？", "确认要删除嘛？", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
			{
				return;
			}
			Deleted = true;
			this.DialogResult = DialogResult.Yes; // 你可以自定义处理方式
			this.Close();
		}
	}
}
