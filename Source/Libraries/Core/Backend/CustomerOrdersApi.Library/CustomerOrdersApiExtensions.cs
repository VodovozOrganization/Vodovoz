using CustomerOrdersApi.Library.Factories;
using CustomerOrdersApi.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Application;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Services.Orders;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Validation;

namespace CustomerOrdersApi.Library
{
	public static class CustomerOrdersApiExtensions
	{
		public static IServiceCollection AddCustomerOrdersApiLibrary(this IServiceCollection services)
		{
			services.AddScoped<IUnitOfWork>(_ => UnitOfWorkFactory.CreateWithoutRoot("Сервис работы с заказами"))
				.AddSingleton<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>()
				.AddSingleton<ISessionProvider, DefaultSessionProvider>()
				.AddApplicationOrderServices()
				.AddLibraryDependencies();
			
			return services;
		}

		private static IServiceCollection AddLibraryDependencies(this IServiceCollection services)
		{
			services.AddScoped<IOnlineOrderFactory, OnlineOrderFactory>()
				.AddScoped<ICustomerOrdersService, CustomerOrdersService>()
				.AddScoped<INomenclatureParametersProvider, NomenclatureParametersProvider>()
				.AddSingleton<IParametersProvider, ParametersProvider>()
				.AddSingleton<ISettingsController, SettingsController>()
				.AddScoped<IEmployeeRepository, EmployeeRepository>()
				.AddScoped<IOrderDailyNumberController, OrderDailyNumberController>()
				.AddScoped<INomenclatureRepository, NomenclatureRepository>()
				.AddScoped<IOrderRepository, OrderRepository>()
				.AddScoped<IPaymentFromBankClientController, PaymentFromBankClientController>()
				.AddScoped<IPaymentItemsRepository, PaymentItemsRepository>()
				.AddScoped<IPaymentsRepository, PaymentsRepository>()
				.AddScoped<ICounterpartyContractRepository, CounterpartyContractRepository>()
				.AddScoped<IOrganizationProvider, Stage2OrganizationProvider>()
				.AddScoped<IOrganizationParametersProvider, OrganizationParametersProvider>()
				.AddScoped<IOrderParametersProvider, OrderParametersProvider>()
				.AddScoped<IGeographicGroupParametersProvider, GeographicGroupParametersProvider>()
				.AddScoped<IFastPaymentRepository, FastPaymentRepository>()
				.AddScoped<ICashReceiptRepository, CashReceiptRepository>()
				.AddScoped<ICounterpartyContractFactory, CounterpartyContractFactory>()
				.AddScoped<ICallTaskWorker, CallTaskWorker>()
				.AddScoped<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance())
				.AddScoped<ICallTaskRepository, CallTaskRepository>()
				.AddScoped<IPersonProvider, BaseParametersProvider>()
				.AddScoped<IUserService>(context => ServicesConfig.UserService)
				.AddScoped<IErrorReporter>((sp) => ErrorReporter.Instance)
				
				.AddScoped<OrderFromOnlineOrderCreator>()
				.AddScoped<IOrderFromOnlineOrderValidator, OrderFromOnlineOrderValidator>()
				.AddScoped<IGoodsPriceCalculator, GoodsPriceCalculator>()
				.AddScoped<FastDeliveryHandler>()
				.AddScoped<IPromotionalSetRepository, PromotionalSetRepository>()
				.AddScoped<IDeliveryRepository, DeliveryRepository>()
				.AddScoped<IDriverApiParametersProvider, DriverApiParametersProvider>()
				.AddScoped<IDeliveryRulesParametersProvider, DeliveryRulesParametersProvider>()
				.AddScoped<IRouteListAddressKeepingDocumentController, RouteListAddressKeepingDocumentController>()
				.AddScoped<IFastDeliveryValidator, FastDeliveryValidator>()
				;
			
			return services;
		}
	}
}
