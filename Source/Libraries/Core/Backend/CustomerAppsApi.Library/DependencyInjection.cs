using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Utilities.Numeric;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;

namespace CustomerAppsApi.Library
{
	/// <summary>
	/// Методы расширения коллекции сервисов дял регистрации в контейнере зависимостей
	/// </summary>
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавление сервисов библиотеки
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddCustomerApiLibrary(this IServiceCollection services)
		{
			services
				.AddScoped<IUnitOfWork>(_ => UnitOfWorkFactory.CreateWithoutRoot("Сервис интеграции"))
				.AddSingleton<IPhoneRepository, PhoneRepository>()
				.AddSingleton<IEmailRepository, EmailRepository>()
				.AddSingleton<IWarehouseRepository, WarehouseRepository>()
				.AddSingleton<IRoboatsRepository, RoboatsRepository>()
				.AddSingleton<IBottlesRepository, BottlesRepository>()
				.AddSingleton<INomenclatureRepository, NomenclatureRepository>()
				.AddSingleton<IOrderRepository, OrderRepository>()
				.AddSingleton<IStockRepository, StockRepository>()
				.AddSingleton<IPromotionalSetRepository, PromotionalSetRepository>()
				.AddSingleton<IExternalCounterpartyRepository, ExternalCounterpartyRepository>()
				.AddSingleton<IExternalCounterpartyMatchingRepository, ExternalCounterpartyMatchingRepository>()
				.AddSingleton<PhoneFormatter>(_ => new PhoneFormatter(PhoneFormat.DigitsTen))
				.AddSingleton<ICounterpartySettings, CounterpartySettings>()
				.AddSingleton<ISettingsController, SettingsController>()
				.AddSingleton<ISessionProvider, DefaultSessionProvider>()
				.AddSingleton<IParametersProvider, ParametersProvider>()
				.AddSingleton<INomenclatureParametersProvider, NomenclatureParametersProvider>()
				.AddSingleton<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>()
				.AddSingleton<IRoboatsSettings, RoboatsSettings>()
				.AddSingleton<ICachedBottlesDebtRepository, CachedBottlesDebtRepository>()
				.AddSingleton<IRegisteredNaturalCounterpartyDtoFactory, RegisteredNaturalCounterpartyDtoFactory>()
				.AddSingleton<IExternalCounterpartyMatchingFactory, ExternalCounterpartyMatchingFactory>()
				.AddSingleton<IExternalCounterpartyFactory, ExternalCounterpartyFactory>()
				.AddSingleton<ICounterpartyModelFactory, CounterpartyModelFactory>()
				.AddSingleton<ICounterpartyFactory, CounterpartyFactory>()
				.AddSingleton<INomenclatureFactory, NomenclatureFactory>()
				.AddSingleton<IPromotionalSetFactory, PromotionalSetFactory>()
				.AddSingleton<ICameFromConverter, CameFromConverter>()
				.AddSingleton<ISourceConverter, SourceConverter>()
				.AddSingleton<ContactFinderForExternalCounterpartyFromOne>()
				.AddSingleton<ContactFinderForExternalCounterpartyFromTwo>()
				.AddSingleton<ContactFinderForExternalCounterpartyFromMany>()
				.AddSingleton<IContactManagerForExternalCounterparty, ContactManagerForExternalCounterparty>()
				.AddSingleton<IGoodsOnlineParametersController, GoodsOnlineParametersController>()
				.AddScoped<ICounterpartyModel, CounterpartyModel>()
				.AddScoped<INomenclatureModel, NomenclatureModel>()
				.AddScoped<IOrderModel, OrderModel>()
				.AddScoped<IPromotionalSetModel, PromotionalSetModel>()
				.AddScoped<IWarehouseModel, WarehouseModel>()
				.AddScoped<ICounterpartyModelValidator, CounterpartyModelValidator>()
				.AddSingleton<SelfDeliveriesAddressesFrequencyRequestsHandler>()
				.AddSingleton<PricesFrequencyRequestsHandler>()
				.AddSingleton<NomenclaturesFrequencyRequestsHandler>();

			return services;
		}
	}
}
