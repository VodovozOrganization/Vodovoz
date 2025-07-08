using FastPaymentsAPI.Library.ApiClients;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Managers;
using FastPaymentsAPI.Library.Models;
using FastPaymentsAPI.Library.Notifications;
using FastPaymentsAPI.Library.Validators;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Application;
using Vodovoz.Application.Orders.Services;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Models;
using Vodovoz.Services;
using Vodovoz.Settings.FastPayments;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Settings;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI.Library
{
	public static class FastPaymentsApiExtensions
	{
		public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
		{
			services
				.AddApplication()
				.AddScoped<ISiteSettings, SiteSettings>()
				.AddScoped<SiteClient>()
				.AddScoped<MobileAppClient>()
				.AddScoped<SiteNotifier>()
				.AddScoped<MobileAppNotifier>()
				.AddScoped<NotificationModel>()
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
				.AddScoped<IOrderRequestManager, OrderRequestManager>();

			return services;
		}
	}
}
