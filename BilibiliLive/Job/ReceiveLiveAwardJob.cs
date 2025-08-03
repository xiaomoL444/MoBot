using BilibiliLive.Constant;
using BilibiliLive.Manager;
using BilibiliLive.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Job
{
	sealed class ReceiveLiveAwardJob : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			await EraManager.ReceiveFinallylEraAward(Constants.OPGroupID, EraAwardType.Live);
			return;
		}
	}
}
