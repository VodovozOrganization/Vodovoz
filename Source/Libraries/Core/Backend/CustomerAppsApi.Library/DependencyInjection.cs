using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Validators;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Factories;

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
			services.AddSingleton<ICachedBottlesDebtRepository, CachedBottlesDebtRepository>();
			
			services.AddSingleton<IRegisteredNaturalCounterpartyDtoFactory, RegisteredNaturalCounterpartyDtoFactory>();
			services.AddSingleton<IExternalCounterpartyMatchingFactory, ExternalCounterpartyMatchingFactory>();
			services.AddSingleton<IExternalCounterpartyFactory, ExternalCounterpartyFactory>();
			services.AddSingleton<ICounterpartyModelFactory, CounterpartyModelFactory>();
			services.AddSingleton<ICounterpartyFactory, CounterpartyFactory>();
			services.AddSingleton<INomenclatureFactory, NomenclatureFactory>();
			services.AddSingleton<IPromotionalSetFactory, PromotionalSetFactory>();
			services.AddSingleton<ICameFromConverter, CameFromConverter>();
			services.AddSingleton<ISourceConverter, SourceConverter>();
			services.AddSingleton<ContactFinderForExternalCounterpartyFromOne>();
			services.AddSingleton<ContactFinderForExternalCounterpartyFromTwo>();
			services.AddSingleton<ContactFinderForExternalCounterpartyFromMany>();
			services.AddSingleton<IContactManagerForExternalCounterparty, ContactManagerForExternalCounterparty>();
			services.AddSingleton<IGoodsOnlineParametersController, GoodsOnlineParametersController>();
			
			services.AddScoped<ICounterpartyModel, CounterpartyModel>();
			services.AddScoped<INomenclatureModel, NomenclatureModel>();
			services.AddScoped<IOrderModel, OrderModel>();
			services.AddScoped<IPromotionalSetModel, PromotionalSetModel>();
			services.AddScoped<ICounterpartyModelValidator, CounterpartyModelValidator>();

			return services;
		}
	}
}
