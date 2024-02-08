using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Validators;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Validation;

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
			services.AddSingleton<ICachedBottlesDebtRepository, CachedBottlesDebtRepository>()
				.AddSingleton<IRegisteredNaturalCounterpartyDtoFactory, RegisteredNaturalCounterpartyDtoFactory>()
				.AddSingleton<IExternalCounterpartyMatchingFactory, ExternalCounterpartyMatchingFactory>()
				.AddSingleton<IExternalCounterpartyFactory, ExternalCounterpartyFactory>()
				.AddSingleton<ICounterpartyModelFactory, CounterpartyModelFactory>()
				.AddSingleton<ICounterpartyFactory, CounterpartyFactory>()
				.AddSingleton<INomenclatureFactory, NomenclatureFactory>()
				.AddSingleton<IPromotionalSetFactory, PromotionalSetFactory>()
				.AddSingleton<IOnlineOrderFactory, OnlineOrderFactory>()
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
				.AddScoped<ICounterpartyModelValidator, CounterpartyModelValidator>()
				.AddScoped<OrderFromOnlineOrderCreator>()
				.AddScoped<ICallTaskWorker, CallTaskWorker>()
				.AddScoped<ICounterpartyContractRepository, CounterpartyContractRepository>()
				.AddScoped<ICounterpartyContractFactory, CounterpartyContractFactory>()
				.AddScoped<FastDeliveryHandler>()
				.AddScoped<OrderFromOnlineOrderValidator>()
				.AddScoped<IDriverApiParametersProvider, DriverApiParametersProvider>()
				.AddScoped<IDeliveryRulesParametersProvider, DeliveryRulesParametersProvider>()
				.AddScoped<IRouteListAddressKeepingDocumentController, RouteListAddressKeepingDocumentController>()
				.AddScoped<IFastDeliveryValidator, FastDeliveryValidator>()
				;

			return services;
		}
	}
}
