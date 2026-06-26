using Microsoft.Extensions.DependencyInjection;
using QS.Utilities.Numeric;
using Vodovoz.Core.Application.Clients.Services;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Tools;
using VodovozBusiness.Services.Clients.DeliveryPoints;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure.Cryptography;
using DriverApi.Notifications.Client;

namespace CustomerAppsApi.Library
{
	/// <summary>
	/// Методы расширения коллекции сервисов дял регистрации в контейнере зависимостей
	/// </summary>
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавление зависимостей для первой версии
		/// </summary>
		/// <param name="services">Список зависимостей</param>
		/// <returns></returns>
		public static IServiceCollection AddVersion1(this IServiceCollection services)
		{
			services
				.AddScoped<V1.Models.ISendingService, V1.Models.SendingService>()
				.AddScoped<V1.Models.IDeliveryPointService, V1.Models.DeliveryPointService>()
				.AddScoped<V1.Repositories.ICachedBottlesDebtRepository, V1.Repositories.CachedBottlesDebtRepository>()
				.AddScoped<V1.Factories.IRegisteredNaturalCounterpartyDtoFactory, V1.Factories.RegisteredNaturalCounterpartyDtoFactory>()
				.AddScoped<V1.Factories.ICounterpartyModelFactory, V1.Factories.CounterpartyModelFactory>()
				.AddScoped<V1.Factories.ICounterpartyFactory, V1.Factories.CounterpartyFactory>()
				.AddScoped<V1.Factories.INomenclatureFactory, V1.Factories.NomenclatureFactory>()
				.AddScoped<V1.Factories.IPromotionalSetFactory, V1.Factories.PromotionalSetFactory>()
				.AddScoped<V1.Factories.IRentPackageFactory, V1.Factories.RentPackageFactory>()
				.AddScoped<V1.Factories.IDeliveryPointFactory, V1.Factories.DeliveryPointFactory>()
				.AddScoped<V1.Converters.ICameFromConverter, V1.Converters.CameFromConverter>()
				.AddScoped<V1.Converters.ISourceConverter, V1.Converters.SourceConverter>()
				.AddScoped<V1.Models.ICounterpartyModel, V1.Models.CounterpartyModel>()
				.AddScoped<V1.Models.INomenclatureModel, V1.Models.NomenclatureModel>()
				.AddScoped<V1.Models.IOrderModel, V1.Models.OrderModel>()
				.AddScoped<V1.Models.IPromotionalSetModel, V1.Models.PromotionalSetModel>()
				.AddScoped<V1.Models.IWarehouseModel, V1.Models.WarehouseModel>()
				.AddScoped<V1.Models.IRentPackageModel, V1.Models.RentPackageModel>()
				.AddScoped<V1.Validators.ICounterpartyModelValidator, V1.Validators.CounterpartyModelValidator>()
				.AddScoped<V1.Validators.IDeliveryPointModelValidator, V1.Validators.DeliveryPointModelValidator>()
				.AddSingleton<V1.Services.SelfDeliveriesAddressesFrequencyRequestsHandler>()
				.AddSingleton<V1.Services.PricesFrequencyRequestsHandler>()
				.AddSingleton<V1.Services.NomenclaturesFrequencyRequestsHandler>()
				.AddSingleton<V1.Services.RentPackagesFrequencyRequestsHandler>()
				.AddCommonDependencies()
				;
			
			return services;
		}
		
		/// <summary>
		/// Добавление зависимостей для второй версии
		/// </summary>
		/// <param name="services">Список зависимостей</param>
		/// <returns></returns>
		public static IServiceCollection AddVersion2(this IServiceCollection services)
		{
			services
				.AddScoped<V2.Models.ISendingService, V2.Models.SendingService>()
				.AddScoped<V2.Models.IDeliveryPointService, V2.Models.DeliveryPointService>()
				.AddScoped<V2.Repositories.ICachedBottlesDebtRepository, V2.Repositories.CachedBottlesDebtRepository>()
				.AddScoped<V2.Factories.IRegisteredNaturalCounterpartyDtoFactory, V2.Factories.RegisteredNaturalCounterpartyDtoFactory>()
				.AddScoped<V2.Factories.ICounterpartyModelFactory, V2.Factories.CounterpartyModelFactory>()
				.AddScoped<V2.Factories.ICounterpartyFactory, V2.Factories.CounterpartyFactory>()
				.AddScoped<V2.Factories.ISaleItemFactory, V2.Factories.SaleItemFactory>()
				.AddScoped<V2.Factories.IDeliveryPointFactory, V2.Factories.DeliveryPointFactory>()
				.AddScoped<V2.Converters.ICameFromConverter, V2.Converters.CameFromConverter>()
				.AddScoped<V2.Converters.ISourceConverter, V2.Converters.SourceConverter>()
				.AddScoped<V2.Models.ICounterpartyModel, V2.Models.CounterpartyModel>()
				.AddScoped<V2.Models.INomenclatureModel, V2.Models.NomenclatureModel>()
				.AddScoped<V2.Models.IOrderModel, V2.Models.OrderModel>()
				.AddScoped<V2.Models.IWarehouseModel, V2.Models.WarehouseModel>()
				.AddScoped<V2.Validators.ICounterpartyModelValidator, V2.Validators.CounterpartyModelValidator>()
				.AddScoped<V2.Validators.IDeliveryPointModelValidator, V2.Validators.DeliveryPointModelValidator>()
				.AddSingleton<V2.Services.SelfDeliveriesAddressesFrequencyRequestsHandler>()
				.AddSingleton<V2.Services.PricesFrequencyRequestsHandler>()
				.AddSingleton<V2.Services.NomenclaturesFrequencyRequestsHandler>()
				.AddCommonDependencies()
				;

			return services;
		}

		private static IServiceCollection AddCommonDependencies(this IServiceCollection services)
		{
			services
				.AddSingleton<PhoneFormatter>(_ => new PhoneFormatter(PhoneFormat.DigitsTen))
				.AddScoped<IFreeLoaderChecker, FreeLoaderChecker>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddScoped<IDeliveryPointBuildingNumberParser, DeliveryPointBuildingNumberParser>()
				.AddScoped<IErrorReporter>(context => ErrorReporter.Instance)
				.AddScoped<IContactManagerForExternalCounterparty, ContactManagerForExternalCounterparty>()
				.AddScoped<ContactFinderForExternalCounterpartyFromOne>()
				.AddScoped<ContactFinderForExternalCounterpartyFromTwo>()
				.AddScoped<ContactFinderForExternalCounterpartyFromMany>()
				.AddDriverApiNotificationsSenders()
				;
			
			return services;
		}
	}
}
