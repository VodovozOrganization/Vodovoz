using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.Utilities.Extensions;
using Sms.Internal.Client.Framework;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.Options;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using Vodovoz.Tools.Orders;
using Vodovoz.Validation;
using VodovozBusiness.CachingRepositories.Employees;
using VodovozBusiness.CachingRepositories.Subdivisions;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBusiness(
			this IServiceCollection services,
			IConfiguration configuration,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) =>
			services
				.AddFeatureManagement()
				.RegisterClassesByInterfaces("Controller", serviceLifetime)
				.RegisterClassesByInterfaces("Converter", serviceLifetime)
				.RegisterClassesByInterfaces("Repository", serviceLifetime)
				.RegisterClassesByInterfaces("Service", serviceLifetime)
				.RegisterClassesByInterfaces("Handler", serviceLifetime)
				.RegisterClassesByInterfaces("Factory", serviceLifetime)

				.AddCachingRepositories()

				.ConfigureBusinessOptions(configuration)
				.AddService<RouteGeometryCalculator>(serviceLifetime)
				.AddService<IDistanceCalculator>(sp => sp.GetService<RouteGeometryCalculator>(), serviceLifetime)
				.AddService<IRouteListProfitabilityFactory, RouteListProfitabilityFactory>(serviceLifetime)
				.AddService<IFastPaymentSender, FastPaymentSender>(serviceLifetime)
				.AddService<ISmsClientChannelFactory, SmsClientChannelFactory>(serviceLifetime)
				.AddService<IDeliveryPriceCalculator, DeliveryPriceCalculator>(serviceLifetime)
				.AddService<IFastDeliveryHandler, FastDeliveryHandler>(serviceLifetime)
				.AddService<IFastDeliveryValidator, FastDeliveryValidator>(serviceLifetime)
				.AddService<ICallTaskWorker, CallTaskWorker>(serviceLifetime)
				.AddService<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance(), serviceLifetime)
				.AddService<IErrorReporter>(context => ErrorReporter.Instance, serviceLifetime)
				.AddService<OrderStateKey>(serviceLifetime)
				.AddService<OnlineOrderStateKey>(serviceLifetime)
			;

		private static IServiceCollection RegisterClassesByInterfaces(
			this IServiceCollection services,
			string classEndsWith,
			ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			services.AddServicesEndsWith(typeof(DependencyInjection).Assembly, classEndsWith, serviceLifetime);
			return services;
		}

		public static IServiceCollection ConfigureBusinessOptions(this IServiceCollection services, IConfiguration configuration) => services
			.Configure<PushNotificationSettings>(pushNotificationOptions =>
				configuration.GetSection(nameof(PushNotificationSettings)).Bind(pushNotificationOptions));

		public static IServiceCollection AddCachingRepositories(
			this IServiceCollection services)
			=> services
				.AddScoped<IEmployeeInMemoryNameWithInitialsCacheRepository, EmployeeInMemoryNameWithInitialsCacheRepository>()
				.AddScoped<IDomainEntityNodeInMemoryCacheRepository<Subdivision>, SubdivisionInMemoryTitleCacheRepository>();
	}
}
