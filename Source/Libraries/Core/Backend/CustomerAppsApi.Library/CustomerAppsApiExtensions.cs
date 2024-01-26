using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using CustomerAppsApi.Models;
using CustomerAppsApi.Validators;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Factories;
using VodovozInfrastructure.Cryptography;

namespace CustomerAppsApi.Library
{
	/// <summary>
	/// Методы расширения коллекции сервисов дял регистрации в контейнере зависимостей
	/// </summary>
	public static class CustomerAppsApiExtensions
	{
		/// <summary>
		/// Добавление сервисов библиотеки
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddCustomerApiLibrary(this IServiceCollection services)
		{
			services.AddScoped(_ => UnitOfWorkFactory.CreateWithoutRoot("Сервис интеграции"))
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
				.AddSingleton<NomenclaturesFrequencyRequestsHandler>()
				.AddSingleton<PricesFrequencyRequestsHandler>()
				.AddSingleton<SelfDeliveriesAddressesFrequencyRequestsHandler>()
				.AddScoped<ICounterpartyModel, CounterpartyModel>()
				.AddScoped<INomenclatureModel, NomenclatureModel>()
				.AddScoped<IOrderModel, OrderModel>()
				.AddScoped<IRentPackageModel, RentPackageModel>()
				.AddScoped<IPromotionalSetModel, PromotionalSetModel>()
				.AddScoped<IDeliveryPointModel, DeliveryPointModel>()
				.AddScoped<IWarehouseModel, WarehouseModel>()
				.AddScoped<IDeliveryPointModelValidator, DeliveryPointModelValidator>()
				.AddScoped<ICounterpartyModelValidator, CounterpartyModelValidator>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>();

			return services;
		}
	}
}
