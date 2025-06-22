using DailyChat.Models;
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

namespace DailyChatSettingWinForm
{
	public partial class ReplyItemForm : Form
	{
		public ReplyItemForm()
		{
			InitializeComponent();
		}

		private void ReplyItemForm_Load(object sender, EventArgs e)
		{
			Initial();
		}

		private int _seleteReplyIndex = -1;
		private Response response;
		void Initial()
		{
			switch (ShareField.responseType)
			{
				case ResponseType.WhiteList:
					response = ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].WhiteList[ShareField.SeleteWhiteList];
					if (response.message.Count != 0)
						_seleteReplyIndex = 0;
					break;
				case ResponseType.Normal:
					if (ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].Normal.Count <= 0)
					{
						ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].Normal.Add(new());
					}
					response = ShareField.EchoRule.ReplyItems[ShareField.SeleteKeyWord].Normal[0];

					if (response.message.Count != 0)
						_seleteReplyIndex = 0;
					break;
				case ResponseType.BlackList:
					break;
				default:
					break;
			}
			RefreshPanel();
		}
		void RefreshPanel()
		{
			ReplyIndexPanel.Controls.Clear();
			MessagePanel.Controls.Clear();


			for (int i = 0; i < response.message.Count; i++)
			{
				var selfIndex = i;
				Button btn = new Button();
				btn.Text = $"回复{i}";
				btn.Size = new Size(190, 30);
				btn.Click += (s, e) =>
				{
					_seleteReplyIndex = selfIndex;
					RefreshPanel();

				};
				ReplyIndexPanel.Controls.Add(btn);

			}
			if (_seleteReplyIndex != -1)
			{
				//刷新子消息
				for (int i = 0; i < response.message[_seleteReplyIndex].MessageChains.Count; i++)
				{
					var selfIndex = i;
					FlowLayoutPanel innerPanel = new FlowLayoutPanel();
					innerPanel.Size = new Size(200, MessagePanel.Height - 30);
					innerPanel.Location = new Point(0, 0); // 相对 panel1 左上角的位置
					innerPanel.BackColor = Color.GhostWhite;
					innerPanel.AutoScroll = true;
					innerPanel.FlowDirection = FlowDirection.TopDown;

					for (int j = 0; j < response.message[_seleteReplyIndex].MessageChains[selfIndex].MessageItems.Count; j++)
					{
						var msgChainIndex = j;
						//给panel添加消息链的元素
						FlowLayoutPanel messageChainPanel = new();
						messageChainPanel.Width = innerPanel.Width - 10; // 比 panel 小 10px
						messageChainPanel.Left = (innerPanel.Width - messageChainPanel.Width) / 2;
						messageChainPanel.Height = 120; // 随意高度
						messageChainPanel.BackColor = Color.NavajoWhite;

						//消息链添加

						ComboBox comboBox = new ComboBox();
						comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
						comboBox.Items.AddRange(Enum.GetNames(typeof(MessageItemType)));
						comboBox.SelectedIndex = (int)response.message[_seleteReplyIndex].MessageChains[selfIndex].MessageItems[msgChainIndex].MessageItemType;
						comboBox.Width = 150;
						comboBox.Height = 30;

						// 设置水平居中
						comboBox.Top = 5; // 垂直位置自己定
						comboBox.Left = (messageChainPanel.Width - comboBox.Width) / 2;
						comboBox.SelectedIndexChanged += (s, e) =>
						{
							if (Enum.TryParse<MessageItemType>(comboBox.Text, out var result))
							{
								response.message[_seleteReplyIndex].MessageChains[selfIndex].MessageItems[msgChainIndex].MessageItemType = result;
								ShareField.Save();
								RefreshPanel();
							}
							else
							{
								MessageBox.Show("无法转换为 enum");
							}
						};

						messageChainPanel.Controls.Add(comboBox);

						TextBox textBox = new();
						textBox.Text = response.message[_seleteReplyIndex].MessageChains[selfIndex].MessageItems[msgChainIndex].content;
						textBox.Width = 150;
						textBox.Height = 80;
						textBox.Left = (messageChainPanel.Width - comboBox.Width) / 2;
						textBox.Multiline = true;
						textBox.WordWrap = true;
						textBox.TextChanged += (s, e) =>
						{
							response.message[_seleteReplyIndex].MessageChains[selfIndex].MessageItems[msgChainIndex].content = textBox.Text;
							ShareField.Save();
						};
						messageChainPanel.Controls.Add(textBox);

						innerPanel.Controls.Add(messageChainPanel);
					}

					//给这个panel添加添加按钮元素
					Button inner_AddMsgItemBtn = new Button();
					inner_AddMsgItemBtn.Text = "添加消息链";
					inner_AddMsgItemBtn.Location = new Point(0, 10);
					inner_AddMsgItemBtn.Width = innerPanel.Width - 10; // 比 panel 小 10px
					inner_AddMsgItemBtn.Height = 60; // 随意高度
													 // 水平居中
					inner_AddMsgItemBtn.Left = (innerPanel.Width - inner_AddMsgItemBtn.Width) / 2;

					inner_AddMsgItemBtn.Click += (s, e) =>
					{
						response.message[_seleteReplyIndex].MessageChains[selfIndex].MessageItems.Add(new());
						ShareField.Save();
						RefreshPanel();
					};

					innerPanel.Controls.Add(inner_AddMsgItemBtn);

					//给这个panel添加删除按钮元素
					Button inner_DeleteMsgItemBtn = new Button();
					inner_DeleteMsgItemBtn.Text = "删除该子消息";
					inner_DeleteMsgItemBtn.Location = new Point(0, 10);
					inner_DeleteMsgItemBtn.Width = innerPanel.Width - 10; // 比 panel 小 10px
					inner_DeleteMsgItemBtn.Height = 40; // 随意高度
														// 水平居中
					inner_DeleteMsgItemBtn.BackColor = Color.IndianRed;
					inner_DeleteMsgItemBtn.Left = (innerPanel.Width - inner_DeleteMsgItemBtn.Width) / 2;
					inner_DeleteMsgItemBtn.Click += (s, e) =>
					{
						if (MessageBox.Show($"真的要删除该消息链{JsonConvert.SerializeObject(response.message[_seleteReplyIndex].MessageChains[selfIndex], Formatting.Indented)}嘛？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
						{
							response.message[_seleteReplyIndex].MessageChains.Remove(response.message[_seleteReplyIndex].MessageChains[selfIndex]);
							RefreshPanel();
						}
					}
			;
					innerPanel.Controls.Add(inner_DeleteMsgItemBtn);


					MessagePanel.Controls.Add(innerPanel);
				}

				Button btn = new Button();
				btn.Text = $"添加子消息";
				btn.Size = new Size(100, MessagePanel.Height - 30);
				btn.Click += (s, e) =>
				{
					response.message[_seleteReplyIndex].MessageChains.Add(new());
					ShareField.Save();
					RefreshPanel();
				};
				btn.BackColor = Color.White;
				MessagePanel.Controls.Add(btn);
			}
		}

		private void AddReplyButton_Click(object sender, EventArgs e)
		{
			response.message.Add(new());
			ShareField.Save();
			RefreshPanel();
		}

		private void DeleteReplyButtonn_Click(object sender, EventArgs e)
		{
			if (_seleteReplyIndex == -1)
			{
				MessageBox.Show("未选择任何回复");
				return;
			}
			if (MessageBox.Show($"确认要删除回复{_seleteReplyIndex}嘛?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				response.message.Remove(response.message[_seleteReplyIndex]);
				_seleteReplyIndex = -1;
				ShareField.Save();
				RefreshPanel();
			}
		}
	}
}
