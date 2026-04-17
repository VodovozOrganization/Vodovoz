using CustomerNotifications.Application.Providers;
using CustomerNotifications.Contracts;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using System.Collections.ObjectModel;
using System.Linq;
using TransactionalOutbox.Abstractions;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCustomerNotificationsSettingsProvider(this IServiceCollection services)
		{
			services.AddSingleton<ICustomerNotificationsSettingsProvider>(sp =>
			{
				var uowFactory = sp.GetRequiredService<IUnitOfWorkFactory>();

				using(var uow = uowFactory.CreateWithoutRoot())
				{
					var settingsDict = uow.GetAll<OnlineOrderNotificationSetting>()
						.ToDictionary(s => s.CustomerNotificationEventType);

					var readOnlySettings = new ReadOnlyDictionary<CustomerNotificationEventType, OnlineOrderNotificationSetting>(settingsDict);

					return new CustomerNotificationsSettingsProvider(readOnlySettings);
				}
			})
			.AddSingleton<IOutBoxSettingsProvider<CustomerNotificationDomainEvent>>(sp => sp.GetRequiredService<ICustomerNotificationsSettingsProvider>());

			return services;
		}
	}
}
