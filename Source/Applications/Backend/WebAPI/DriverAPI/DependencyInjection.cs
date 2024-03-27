using DriverAPI.Library;
using DriverAPI.Services;
using DriverAPI.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Models.TrueMark;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;

namespace DriverAPI
{
	/// <summary>
	/// Конфигурация контейнера зависимостей
	/// </summary>
	public static class DependencyInjection
	{
		/// <summary>
		/// Основная конфигурация DriverApi
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public static IServiceCollection AddDriverApi(this IServiceCollection services, IConfiguration configuration) =>
			services.AddScoped<IUnitOfWork>((sp) => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot())
				// Сервисы для контроллеров

				// ErrorReporter
				.AddScoped<IErrorReporter>((sp) => ErrorReporter.Instance)
				.AddScoped<TrueMarkWaterCodeParser>()
				.AddScoped<TrueMarkCodesPool, TrueMarkTransactionalCodesPool>()

				// Сервисы
				.AddSingleton<IWakeUpDriverClientService, WakeUpDriverClientService>()

				// Репозитории водовоза
				.AddScoped<ITrackRepository, TrackRepository>()
				.AddScoped<IComplaintsRepository, ComplaintsRepository>()
				.AddScoped<IRouteListRepository, RouteListRepository>()
				.AddScoped<IStockRepository, StockRepository>()
				.AddScoped<IRouteListItemRepository, RouteListItemRepository>()
				.AddScoped<IOrderRepository, OrderRepository>()
				.AddScoped<IEmployeeRepository, EmployeeRepository>()
				.AddScoped<IFastPaymentRepository, FastPaymentRepository>()
				.AddScoped<ICarRepository, CarRepository>()

				.AddDriverApiLibrary(configuration)

				.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))

				.AddScoped<ICallTaskWorker, CallTaskWorker>()
				.AddScoped<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance())
				.AddScoped<ICallTaskRepository, CallTaskRepository>()
				.AddDriverApiHostedServices();

		/// <summary>
		/// Добавление сервисок работающих в фоновом режиме
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddDriverApiHostedServices(this IServiceCollection services) =>
			services.AddHostedService<WakeUpNotificationSenderService>();
	}
}
