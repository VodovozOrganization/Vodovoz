using CustomerAppsApi.Factories;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using CustomerAppsApi.Models;
using Microsoft.Extensions.DependencyInjection;
using QS.Utilities.Numeric;
using Vodovoz.Application.Clients.Services;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Converters;
using Vodovoz.Factories;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Validation;
using VodovozBusiness.Services.Clients.DeliveryPoints;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure.Cryptography;
using DriverApi.Notifications.Client;
using Vodovoz.Application.Clients;
using VodovozBusiness.Controllers;

namespace CustomerAppsApi.Library
{
	/// <summary>
	/// Методы расширения коллекции сервисов дял регистрации в контейнере зависимостей
	/// </summary>
	public static class DependencyInjection
	{
		//TODO: переделать на подключение общих библиотек с зависимостями
		/// <summary>
		/// Добавление сервисов библиотеки
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddCustomerApiLibrary(this IServiceCollection services)
		{
			services
				.AddScoped<ISendingService, SendingService>()
				.AddSingleton<PhoneFormatter>(_ => new PhoneFormatter(PhoneFormat.DigitsTen))
				.AddScoped<ICachedBottlesDebtRepository, CachedBottlesDebtRepository>()
				.AddScoped<IRegisteredNaturalCounterpartyDtoFactory, RegisteredNaturalCounterpartyDtoFactory>()
				.AddScoped<IExternalCounterpartyMatchingFactory, ExternalCounterpartyMatchingFactory>()
				.AddScoped<IExternalCounterpartyFactory, ExternalCounterpartyFactory>()
				.AddScoped<ICounterpartyModelFactory, CounterpartyModelFactory>()
				.AddScoped<ICounterpartyFactory, CounterpartyFactory>()
				.AddScoped<INomenclatureFactory, NomenclatureFactory>()
				.AddScoped<IPromotionalSetFactory, PromotionalSetFactory>()
				.AddScoped<ICallTaskFactory, CallTaskSingletonFactory>()
				.AddScoped<IRentPackageFactory, RentPackageFactory>()
				.AddScoped<IDeliveryPointFactory, DeliveryPointFactory>()
				.AddScoped<ICameFromConverter, CameFromConverter>()
				.AddScoped<ISourceConverter, SourceConverter>()
				.AddScoped<ContactFinderForExternalCounterpartyFromOne>()
				.AddScoped<ContactFinderForExternalCounterpartyFromTwo>()
				.AddScoped<ContactFinderForExternalCounterpartyFromMany>()
				.AddScoped<IContactManagerForExternalCounterparty, ContactManagerForExternalCounterparty>()
				.AddScoped<IGoodsOnlineParametersController, GoodsOnlineParametersController>()
				.AddScoped<ICounterpartyModel, CounterpartyModel>()
				.AddScoped<INomenclatureModel, NomenclatureModel>()
				.AddScoped<IOrderModel, OrderModel>()
				.AddScoped<IPromotionalSetModel, PromotionalSetModel>()
				.AddScoped<ICallTaskWorker, CallTaskWorker>()
				.AddDriverApiNotificationsSenders()
				.AddScoped<IFastDeliveryHandler, FastDeliveryHandler>()
				.AddScoped<IRouteListAddressKeepingDocumentController, RouteListAddressKeepingDocumentController>()
				.AddScoped<IFastDeliveryValidator, FastDeliveryValidator>()
				.AddScoped<IErrorReporter>(context => ErrorReporter.Instance)
				.AddScoped<IWarehouseModel, WarehouseModel>()
				.AddScoped<IRentPackageModel, RentPackageModel>()
				.AddScoped<IDeliveryPointService, DeliveryPointService>()
				.AddScoped<ICounterpartyModelValidator, CounterpartyModelValidator>()
				.AddScoped<IDeliveryPointModelValidator, DeliveryPointModelValidator>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddScoped<INomenclatureOnlineCharacteristicsConverter, NomenclatureOnlineCharacteristicsConverter>()
				.AddScoped<ICounterpartyServiceDataHandler, CounterpartyServiceDataHandler>()
				.AddSingleton<SelfDeliveriesAddressesFrequencyRequestsHandler>()
				.AddSingleton<PricesFrequencyRequestsHandler>()
				.AddSingleton<NomenclaturesFrequencyRequestsHandler>()
				.AddSingleton<RentPackagesFrequencyRequestsHandler>()
				.AddScoped<IFreeLoaderChecker, FreeLoaderChecker>()
				.AddScoped<IDeliveryPointBuildingNumberParser, DeliveryPointBuildingNumberParser>()
				.AddScoped<IDeliveryPointBuildingNumberHandler, DeliveryPointBuildingNumberHandler>()
				.AddScoped<ICounterpartyEdoAccountController, CounterpartyEdoAccountController>();

			return services;
		}
	}
}
