using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoBot.Core.Interfaces;
using MoBot.Infra.PlayWright.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Tool
{
	internal class GlobalSetting
	{
		//Logger
		public static ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;

		public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
		public static ILogger CreateLogger(Type t) => LoggerFactory.CreateLogger(t.FullName ?? t.Name);

		//DataStorage
		public static IDataStorage DataStorage { get; set; }

		//Webshot
		public static IWebshot Webshot { get; set; }
		public static IWebshotRequestStore WebshotRequestStore { get; set; }

	}
}
