using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sms.Internal.Client.Framework;
using Vodovoz.Controllers;
using Vodovoz.EntityRepositories;
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
		public static IServiceCollection AddBusiness(
			this IServiceCollection services,
			DependencyType dependencyType = DependencyType.Scoped) => services

			.RegisterClassesByInterfaces("Controller", dependencyType)
			.RegisterClassesByInterfaces("Repository", dependencyType)
			.RegisterClassesByInterfaces("Service", dependencyType)
			.RegisterClassesByInterfaces("Handler", dependencyType)
			
			.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
			.AddScoped<RouteGeometryCalculator>()
			.AddScoped<IDistanceCalculator>(sp => sp.GetService<RouteGeometryCalculator>())
			.AddScoped<IRouteListProfitabilityFactory, RouteListProfitabilityFactory>()
			.AddScoped<IFastPaymentSender, FastPaymentSender>()
			.AddScoped<IOrganizationProvider, Stage2OrganizationProvider>()
			.AddScoped<ISmsClientChannelFactory, SmsClientChannelFactory>()
			.AddScoped<FastDeliveryHandler>()
			.AddScoped<IFastDeliveryValidator, FastDeliveryValidator>()
			.AddScoped<ICallTaskWorker, CallTaskWorker>()
			.AddScoped<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance())
			.AddScoped<IErrorReporter>(context => ErrorReporter.Instance)
		;

		private static IServiceCollection RegisterClassesByInterfaces(
			this IServiceCollection services, string classEndsWith, DependencyType dependencyType)
		{
			var settingsTypes = typeof(DependencyInjection).Assembly.GetTypes()
				.Where(t => t.IsClass
					&& t.Name.EndsWith(classEndsWith)
					&& t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"));

			foreach(var type in settingsTypes)
			{
				switch(dependencyType)
				{
					case DependencyType.Singleton:
						services.AddSingleton(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
						break;
					case DependencyType.Scoped:
						services.AddScoped(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
						break;
					case DependencyType.Transient:
						services.AddTransient(type.GetInterfaces().First(i => i.Name == $"I{type.Name}"), type);
						break;
				}
			}
			
			return services;
		}
	}

	public enum DependencyType
	{
		Singleton,
		Scoped,
		Transient
	}
}
