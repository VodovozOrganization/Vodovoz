﻿using CustomerAppsApi.Factories;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using CustomerAppsApi.Models;
using Microsoft.Extensions.DependencyInjection;
using QS.Project.DB;
using QS.Utilities.Numeric;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.RentPackages;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Factories;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Common;
using Vodovoz.Settings.Database.Delivery;
using Vodovoz.Settings.Database.Roboats;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Roboats;
using VodovozInfrastructure.Cryptography;

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
				.AddSingleton<IPhoneRepository, PhoneRepository>()
				.AddSingleton<IEmailRepository, EmailRepository>()
				.AddSingleton<IWarehouseRepository, WarehouseRepository>()
				.AddSingleton<IRoboatsRepository, RoboatsRepository>()
				.AddSingleton<IBottlesRepository, BottlesRepository>()
				.AddSingleton<INomenclatureRepository, NomenclatureRepository>()
				.AddSingleton<IOrderRepository, OrderRepository>()
				.AddSingleton<IStockRepository, StockRepository>()
				.AddSingleton<IDeliveryPointRepository, DeliveryPointRepository>()
				.AddSingleton<IDeliveryRepository, DeliveryRepository>()
				.AddSingleton<IPromotionalSetRepository, PromotionalSetRepository>()
				.AddSingleton<IExternalCounterpartyRepository, ExternalCounterpartyRepository>()
				.AddSingleton<IExternalCounterpartyMatchingRepository, ExternalCounterpartyMatchingRepository>()
				.AddSingleton<IRentPackageRepository, RentPackageRepository>()
				.AddSingleton<PhoneFormatter>(_ => new PhoneFormatter(PhoneFormat.DigitsTen))
				.AddSingleton<ISettingsController, SettingsController>()
				.AddSingleton<ISessionProvider, DefaultSessionProvider>()
				.AddSingleton<IRoboatsSettings, RoboatsSettings>()
				.AddSingleton<IGlobalSettings, GlobalSettings>()
				.AddSingleton<IDeliveryRulesSettings, DeliveryRulesSettings>()
				.AddSingleton<ICachedBottlesDebtRepository, CachedBottlesDebtRepository>()
				.AddSingleton<IRegisteredNaturalCounterpartyDtoFactory, RegisteredNaturalCounterpartyDtoFactory>()
				.AddSingleton<IExternalCounterpartyMatchingFactory, ExternalCounterpartyMatchingFactory>()
				.AddSingleton<IExternalCounterpartyFactory, ExternalCounterpartyFactory>()
				.AddSingleton<ICounterpartyModelFactory, CounterpartyModelFactory>()
				.AddSingleton<ICounterpartyFactory, CounterpartyFactory>()
				.AddSingleton<INomenclatureFactory, NomenclatureFactory>()
				.AddSingleton<IPromotionalSetFactory, PromotionalSetFactory>()
				.AddSingleton<IRentPackageFactory, RentPackageFactory>()
				.AddSingleton<IDeliveryPointFactory, DeliveryPointFactory>()
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
				.AddScoped<IRentPackageModel, RentPackageModel>()
				.AddScoped<IDeliveryPointService, DeliveryPointService>()
				.AddScoped<ICounterpartyModelValidator, CounterpartyModelValidator>()
				.AddScoped<IDeliveryPointModelValidator, DeliveryPointModelValidator>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddSingleton<SelfDeliveriesAddressesFrequencyRequestsHandler>()
				.AddSingleton<PricesFrequencyRequestsHandler>()
				.AddSingleton<NomenclaturesFrequencyRequestsHandler>()
				.AddSingleton<RentPackagesFrequencyRequestsHandler>();

			return services;
		}
	}
}
