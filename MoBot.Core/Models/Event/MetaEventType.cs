using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Event
{
	public class MetaEventType : EventPacketBase
	{
		public string meta_event_type { get; set; } = "";
	}
}
