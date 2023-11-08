using Mango.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pacs.Mango
{
	public static class RegistrationExtension
	{
		public static void ConfigurePacsServices(this IServiceCollection serviceCollection)
		{
			serviceCollection
				.AddScoped<ICallEventHandler, PacsMangoCallEventHandler>()
				;
		}
	}
}
