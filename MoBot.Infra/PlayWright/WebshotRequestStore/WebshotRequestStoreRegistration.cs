using Microsoft.Extensions.DependencyInjection;
using MoBot.Infra.PlayWright.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.PlayWright.WebshotRequestStore
{
	public static class WebshotRequestStoreRegistration
	{
		public static IServiceCollection AddWebshotRequestStore(this IServiceCollection services)
		{
			services.AddSingleton<IWebshotRequestStore, WebshotRequestStore>();
			return services;
		}
	}
}
