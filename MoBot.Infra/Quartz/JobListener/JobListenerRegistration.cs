using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.Quartz.JobListener
{
	public static class JobListenerRegistration
	{
		public static IServiceCollection AddJobListener(this IServiceCollection services)
		{
			services.AddScoped<IJobListener, JobListener>();
			return services;
		}
	}
	public class JobListener : IJobListener
	{
		private readonly ILogger<JobListener> _logger = NullLogger<JobListener>.Instance;

		public JobListener(ILogger<JobListener> logger)
		{
			_logger = logger;
		}
		public string Name => "JobListener";

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
			// ✅ 执行后触发（不管是否成功）
			var jobKey = context.JobDetail.Key;
			var triggerKey = context.Trigger.Key;
			if (jobException == null)
				_logger.LogInformation("任务{@key}执行{@trigger}完成，下一次触发时间{nextTime}", jobKey,triggerKey, context.Trigger.GetNextFireTimeUtc()?.ToLocalTime());
			else
				_logger.LogError(jobException, "任务{@key}{@trigger}执行异常", jobKey,triggerKey);
			return Task.CompletedTask;
		}
	}
}
