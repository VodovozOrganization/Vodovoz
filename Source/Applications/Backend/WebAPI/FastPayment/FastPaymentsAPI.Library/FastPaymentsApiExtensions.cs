using CustomerNotifications.Application.Builders;
using CustomerNotifications.Contracts;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Managers;
using FastPaymentsAPI.Library.Models;
using FastPaymentsAPI.Library.Settings;
using FastPaymentsAPI.Library.Validators;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure;
using TransactionalOutbox.Abstractions;
using Vodovoz.Core.Application;
using Vodovoz.Services;
using Vodovoz.Settings.FastPayments;
using Vodovoz.Tools.Orders;
using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI.Library
{
	public static class FastPaymentsApiExtensions
	{
		public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
		{
			services
				.AddCoreApplication()
				.AddScoped<ISiteSettings, SiteSettings>()
				.AddScoped<OrderStateKey>()
				.AddScoped<FastPaymentStatusManagerFromDesktop>()
				.AddScoped<FastPaymentStatusManagerFromDriverApp>()
				.AddScoped<FastPaymentStatusManagerFromOnline>()
				.AddTransient<FastPaymentPerformedStatusChecker>()
				.AddTransient<FastPaymentProcessingStatusChecker>()
				.AddTransient<FastPaymentStatusNotEqualResponseChecker>()
				.AddTransient<ResponseStatusPerformedChecker>()
				.AddTransient<ResponseStatusProcessingFromDesktopChecker>()
				.AddTransient<ResponseStatusProcessingFromDriverAppChecker>()
				.AddTransient<FastPaymentPerformedStatusFromOnlineChecker>()
				.AddTransient<ResponseStatusPerformedFromOnlineChecker>()
				.AddTransient<ResponseStatusProcessingFromOnlineChecker>()
				
				//backgroundServices
				.AddHostedService<FastPaymentStatusUpdater>()
				.AddHostedService<CachePaymentManager>()

				//factories
				.AddSingleton<IFastPaymentFactory, FastPaymentFactory>()

				//converters
				.AddSingleton<IOrderSumConverter, OrderSumConverter>()
				.AddSingleton<IResponseCodeConverter, ResponseCodeConverter>()
				.AddSingleton<IRequestFromConverter, RequestFromConverter>()

				.AddScoped<IFastPaymentOrderService, FastPaymentOrderService>()
				.AddScoped<IFastPaymentService, FastPaymentService>()
				.AddScoped<IEmailService, EmailService>()

				//validators
				.AddScoped<IFastPaymentValidator, FastPaymentValidator>()

				//helpers
				.AddScoped<ISignatureManager, SignatureManager>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddSingleton<IFastPaymentManager, FastPaymentManager>()
				.AddSingleton<IErrorHandler, ErrorHandler>()
				.AddSingleton(_ => new FastPaymentFileCache("/tmp/VodovozFastPaymentServiceTemp.txt"))
				.AddScoped<IOrderRequestManager, OrderRequestManager>()

				// Уведомления клиентов

				.AddScoped<IOutboxNotificationPublisher<CustomerNotificationDomainEvent>, MappingOutboxNotificationPublisher<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>>()
				.AddScoped<IIntegrationEventBuilder<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>, CustomerNotificationsIntegrationEventBuilder>()
				.AddCustomerNotificationsSettingsProvider();
			;

			return services;
		}
	}
}
