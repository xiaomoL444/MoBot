using MoBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Tool
{
	public static class RandomImage
	{
		private static readonly IDataStorage _dataStorage = GlobalDataStorage.DataStorage;
		private static string backgroundDirectory = "background";
		private static string defaultBackground = "./Asserts/images/MyLover.png";
		public static string GetBase64()
		{
			return Convert.ToBase64String(GetBytes());
		}
		public static byte[] GetBytes()
		{
			return File.ReadAllBytes(GetImagePath());
		}
		public static string GetImagePath()
		{
			var dataDirectory = _dataStorage.GetDirectory(MoBot.Core.Models.DirectoryType.Data);
			var directory = $"{dataDirectory}/{backgroundDirectory}";
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			var imagePaths = Directory.EnumerateFiles(directory).ToList();
			if (imagePaths.Count == 0)
			{
				return defaultBackground;
			}
			return imagePaths[Random.Shared.Next(0, imagePaths.Count)];
		}
	}
}
