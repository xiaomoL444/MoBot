using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Tool
{
	public class GlobalDataStorage
	{
		public static IDataStorage DataStorage { get; set; }
	}
}
