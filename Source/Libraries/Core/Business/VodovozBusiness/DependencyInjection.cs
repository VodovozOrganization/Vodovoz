using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sms.Internal.Client.Framework;
using Vodovoz.Controllers;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using Vodovoz.Validation;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBusiness(this IServiceCollection services) => services

			.RegisterClassesByInterfaces("Controller")
			.RegisterClassesByInterfaces("Repository")
			.RegisterClassesByInterfaces("Service")
			.RegisterClassesByInterfaces("Handler")
			
			.AddScoped<RouteGeometryCalculator>()
			.AddScoped<IDistanceCalculator>(sp => sp.GetService<RouteGeometryCalculator>())
			.AddScoped<IRouteListProfitabilityFactory, RouteListProfitabilityFactory>()
			.AddScoped<IFastPaymentSender, FastPaymentSender>()
			.AddScoped<IOrganizationProvider, Stage2OrganizationProvider>()
			.AddScoped<ISmsClientChannelFactory, SmsClientChannelFactory>()
			.AddScoped<FastDeliveryHandler>()
			.AddScoped<IFastDeliveryValidator, FastDeliveryValidator>()
			.AddScoped<ICallTaskWorker, CallTaskWorker>()
			.AddScoped<IErrorReporter>(context => ErrorReporter.Instance)
		;

		private static IServiceCollection RegisterClassesByInterfaces(this IServiceCollection services, string classEndsWith)
		{
			var settingsTypes = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.IsClass
					&& t.Name.EndsWith(classEndsWith)
					&& t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"));

			foreach(var type in settingsTypes)
			{
				services.AddScoped(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
			}
			
			return services;
		}
	}
}
