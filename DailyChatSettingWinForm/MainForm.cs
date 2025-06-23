using DailyChat.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Windows.Forms;

namespace DailyChatSettingWinForm
{
	public partial class MainForm : System.Windows.Forms.Form
	{

		private string FilePath
		{
			get => ShareField.FilePath;
			set
			{
				ShareField.FilePath = value;
				SeleteFileLabel.Text = value;
			}
		}
		private const string configPath = "config";

		public MainForm()
		{
			InitializeComponent();
		}

		private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{

		}

		private void SeleteFileButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();

			openFileDialog1.Title = "ѡ��DailyChat�������ļ�";
			openFileDialog1.Filter = "�����ļ�|*.json";
			openFileDialog1.RestoreDirectory = true;

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string selectedFile = openFileDialog1.FileName;
				if ((ShareField.EchoRule = ReadJson(selectedFile)) != null)
				{
					Initail();
				}
			}
		}

		private void Form_Load(object sender, EventArgs e)
		{

			if (!File.Exists(configPath))
			{
				File.Create(configPath);
			}
			var filePath = File.ReadAllText(configPath);
			if (String.IsNullOrEmpty(filePath)) return;

			if ((ShareField.EchoRule = ReadJson(filePath)) != null)
			{
				Initail();
			}
		}

		EchoRule ReadJson(string path)
		{
			try
			{
				var fileContent = File.ReadAllText(path);
				FilePath = path;
				return JsonConvert.DeserializeObject<EchoRule>(fileContent, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error, NullValueHandling = NullValueHandling.Include, TypeNameHandling = TypeNameHandling.None });
			}
			catch (Exception)
			{
				MessageBox.Show($"{path}�ļ�����ʧ�ܣ�������ѡ��", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}

		}
		void Initail()
		{
			SeleteFileLabel.Text = $"ѡ����ļ��ǣ�{FilePath}";
			File.WriteAllText(configPath, FilePath);

			RefreshPanel();
		}
		void RefreshPanel()
		{
			//���Panel��
			KeyWordPanel.Controls.Clear();

			for (int i = 0; i < ShareField.EchoRule.ReplyItems.Count; i++)
			{
				var selfIndex = i;
				Button btn = new Button();
				btn.Text = $"{ShareField.EchoRule.ReplyItems[i].KeyWord}";
				btn.Size = new Size(190, 30);
				btn.Click += (s, e) =>
				{
					ShowJsonLabel.Text = JsonConvert.SerializeObject(ShareField.EchoRule.ReplyItems[selfIndex],Formatting.Indented);
					ShareField.SeleteKeyWord = selfIndex;
					Form KeyWordForm = new KeyWordForm();
					KeyWordForm.ShowDialog();
					RefreshPanel();
				};
				KeyWordPanel.Controls.Add(btn);
			}
		}

		private void AddKeyWordButton_Click(object sender, EventArgs e)
		{
			var input = Interaction.InputBox("��������Ҫ��ӵĹؼ���", "������", "new keyword");
			if (!string.IsNullOrEmpty(input))
			{
				ShareField.EchoRule.ReplyItems.Add(new() { KeyWord = input });
				ShareField.Save();
				RefreshPanel();
			}
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			ShareField.Save();
		}
	}
}
