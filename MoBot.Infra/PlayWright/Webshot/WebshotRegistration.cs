using Microsoft.Extensions.DependencyInjection;
using MoBot.Core.Interfaces;
using MoBot.Infra.PlayWright.Interfaces;
using MoBot.Infra.Quartz.JobListener;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.PlayWright.Webshot
{
	public static class WebshotRegistration
	{
		public static IServiceCollection AddWebshot(this IServiceCollection services)
		{
			services.AddSingleton<IWebshot, Webshot>();
			return services;
		}
	}
}
