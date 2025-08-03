using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyTask.Tool
{
	public static class GlobalLogger
	{
		public static ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;

		public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
		public static ILogger CreateLogger(Type t) => LoggerFactory.CreateLogger(t.FullName ?? t.Name);
	}

}
