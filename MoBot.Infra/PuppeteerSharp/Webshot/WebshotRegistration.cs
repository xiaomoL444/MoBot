using Microsoft.Extensions.DependencyInjection;
using MoBot.Core.Interfaces;
using MoBot.Infra.PuppeteerSharp.Interface;
using MoBot.Infra.Quartz.JobListener;
using PuppeteerSharp;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.PuppeteerSharp.Webshot
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
