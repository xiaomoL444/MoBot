using DailyChat.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DailyChatSettingWinForm
{
	public partial class KeyWordForm : Form
	{
		public KeyWordForm()
		{
			InitializeComponent();
		}

		void RefreshPanel()
		{

			TriggerPanel.Controls.Clear();
			WhiteListPanel.Controls.Clear();
			for (int i = 0; i < ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].Trigger.Count; i++)
			{
				var selfIndex = i;
				Button btn = new Button();
				btn.Text = $"{ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].Trigger[i]}";
				btn.Size = new Size(190, 30);
				btn.Click += (s, e) =>
				{
					ShareField.SeleteTrigger = selfIndex;
				};
				TriggerPanel.Controls.Add(btn);
			}

			for (int i = 0; i < ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].WhiteList.Count; i++)
			{
				var selfIndex = i;
				Button btn = new Button();
				btn.Text = $"{ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].WhiteList[i].UserID}";
				btn.Size = new Size(190, 30);
				btn.Click += (s, e) =>
				{
					ShareField.SeleteWhiteList = selfIndex;
					ShareField.responseType = ResponseType.WhiteList;
					Form ReplyForm = new ReplyItemForm();
					ReplyForm.ShowDialog();
				};
				WhiteListPanel.Controls.Add(btn);
			}
		}
		private void AddTriggerButton_Click(object sender, EventArgs e)
		{
			var input = Interaction.InputBox("输入你想要添加的触发器", "请输入", "new trigger");
			if (!string.IsNullOrEmpty(input))
			{
				ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].Trigger.Add(input);
				ShareField.Save();
				RefreshPanel();
			}
		}

		private void DeleteTriggerButtonn_Click(object sender, EventArgs e)
		{
			if (ShareField.SeleteTrigger == -1)
			{
				MessageBox.Show("未选择任何触发器", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (MessageBox.Show($"确认要删除序列{ShareField.SeleteKeyWord + 1}的关键词[{ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].KeyWord}]的序列{ShareField.SeleteTrigger + 1}的触发器[{ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].Trigger[ShareField.SeleteTrigger]}]嘛?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].Trigger.Remove(ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].Trigger[ShareField.SeleteTrigger]);
				ShareField.SeleteTrigger = -1;
				RefreshPanel();
				ShareField.Save();
			}
		}

		private void AddWhiteList_Click(object sender, EventArgs e)
		{
			var input = Interaction.InputBox("输入你想要添加的白名单用户", "请输入", "0");
			if (!string.IsNullOrEmpty(input))
			{
				try
				{
					long.Parse(input);
				}
				catch (Exception)
				{
					MessageBox.Show("无法解析成数字", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].WhiteList.Add(new() { UserID = long.Parse(input) });
				ShareField.Save();
				RefreshPanel();
			}
		}

		private void DeleteWhiteList_Click(object sender, EventArgs e)
		{
			if (ShareField.SeleteWhiteList == -1)
			{
				MessageBox.Show("未选择任何白名单用户");
				return;
			}
			if (MessageBox.Show($"确认要删除序列{ShareField.SeleteKeyWord + 1}的关键词[{ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].KeyWord}]的序列{ShareField.SeleteTrigger + 1}的白名单[{ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].WhiteList[ShareField.SeleteWhiteList].UserID}]嘛?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].WhiteList.Remove(ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].WhiteList[ShareField.SeleteWhiteList]);
				ShareField.SeleteWhiteList = -1;
				ShareField.Save();
				RefreshPanel();
			}
		}

		private void DeleteKeyWord_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show($"确认要删除序列{ShareField.SeleteKeyWord + 1}的关键词[{ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].KeyWord}]嘛?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				ShareField.EchoRule.ReplyItems.Remove(ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord]);
				ShareField.SeleteKeyWord = -1;
				ShareField.Save();
				this.Close();
			}
		}

		private void KeyWordForm_Load(object sender, EventArgs e)
		{
			ShareField.SeleteTrigger = -1;
			ShareField.SeleteWhiteList = -1;
			RefreshPanel();
		}

		private void NormalReplyButton_Click(object sender, EventArgs e)
		{
			ShareField.responseType = ResponseType.Normal;
			Form ReplyForm = new ReplyItemForm();

			ReplyForm.ShowDialog();
		}
	}
}
