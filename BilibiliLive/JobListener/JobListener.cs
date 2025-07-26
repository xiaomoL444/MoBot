using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.JobListener
{
	internal class JobListener : IJobListener
	{
		private ILogger _logger = GlobalLogger.CreateLogger<JobListener>();
		public string Name => throw new NotImplementedException();

		public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}

		public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}

		public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
		{
			//退出执行
			if (jobException == null)
			{
				_logger.LogInformation("任务{jobKey}执行完成，", context.JobDetail.Key);
			}
		}
	}
}
