using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DailyChatSettingWinForm
{
	public partial class AddRandomContentForm : Form
	{
		public AddRandomContentForm()
		{
			InitializeComponent();

			RefreshPanel();
		}
		private string _selectKey = string.Empty;
		void RefreshPanel()
		{
			KeyPanel.Controls.Clear();
			ValuePanel.Controls.Clear();

			foreach (var keyValue in ShareField.EchoRule.RandomContent)
			{
				string selfKey = keyValue.Key;
				Button btn = new Button();
				btn.Text = $"{selfKey}";
				btn.Size = new Size(190, 60);
				btn.Click += (s, e) =>
				{
					_selectKey = selfKey;
					RefreshPanel();
				};
				KeyPanel.Controls.Add(btn);
			}
			if (!string.IsNullOrEmpty(_selectKey))
			{
				for (int i = 0; i < ShareField.EchoRule.RandomContent[_selectKey].Count; i++)
				{
					int selfID = i;
					string text = ShareField.EchoRule.RandomContent[_selectKey][selfID];
					Button btn = new Button();
					btn.Text = $"{(text.Length <= 50 ? text : text[..50])}";
					btn.Size = new Size(ValuePanel.Width - 20, 60);
					btn.Click += (s, e) =>
					{
						ChangeRandonContentForm form = new ChangeRandonContentForm("修改内容", $"是否要修改中的{_selectKey}这个值", ShareField.EchoRule.RandomContent[_selectKey][selfID]);
						form.ShowDialog();
						if (form.DialogResult == DialogResult.OK)
						{
							string newValue = form.InputValue;
							ShareField.EchoRule.RandomContent[_selectKey][selfID] = newValue;
							ShareField.Save();
							RefreshPanel();
						}
						else if (form.DialogResult == DialogResult.Yes && form.Deleted)
						{
							ShareField.EchoRule.RandomContent[_selectKey].Remove(ShareField.EchoRule.RandomContent[_selectKey][selfID]);
							ShareField.Save();
							RefreshPanel();
						}
						else
						{
						}

						RefreshPanel();
					};
					ValuePanel.Controls.Add(btn);
				}
			}
		}

		private void AddKey_Click(object sender, EventArgs e)
		{
			var input = Interaction.InputBox("输入你想要添加的Key", "请输入", "new key");
			if (!string.IsNullOrEmpty(input))
			{
				ShareField.EchoRule.RandomContent.Add(input, new());
				ShareField.Save();
				RefreshPanel();
			}

		}

		private void AddValue_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(_selectKey))
			{
				MessageBox.Show("未选择任何Key", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			var input = Interaction.InputBox("输入你想要添加的Value", "请输入", "new value");
			if (!string.IsNullOrEmpty(input))
			{
				ShareField.EchoRule.RandomContent[_selectKey].Add(input);
				ShareField.Save();
				RefreshPanel();
			}
		}

		private void DeleteKey_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(_selectKey))
			{
				MessageBox.Show("未选择任何Key", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (MessageBox.Show($"确认要删除Key{_selectKey}嘛？{JsonConvert.SerializeObject(ShareField.EchoRule.RandomContent[_selectKey], Formatting.Indented)}", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				ShareField.EchoRule.RandomContent.Remove(_selectKey);
				_selectKey = string.Empty;
				RefreshPanel();
				ShareField.Save();
			}
		}

		private void AddRandomContentForm_Load(object sender, EventArgs e)
		{

		}
	}
}
