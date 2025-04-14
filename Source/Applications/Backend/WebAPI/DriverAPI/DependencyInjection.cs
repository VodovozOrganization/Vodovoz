using DriverAPI.Library;
using DriverAPI.Services;
using DriverAPI.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services.Interactive;
using Telemetry;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Subdivisions;
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

				// Телеметрия

				.AddApiOpenTelemetry()

				// Сервисы
				.AddSingleton<IWakeUpDriverClientService, WakeUpDriverClientService>()
				//добавляем сервисы, т.к. в методе Order.SendUpdToEmailOnFinishIfNeeded() есть их вызов
				.AddScoped<IInteractiveQuestion, ConsoleInteractiveQuestion>()
				.AddScoped<IInteractiveMessage, ConsoleInteractiveMessage>()
				.AddScoped<IInteractiveService, ConsoleInteractiveService>()

				.AddDriverApiLibrary(configuration)

				.AddScoped<ICallTaskWorker, CallTaskWorker>()
				.AddScoped<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance())
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
