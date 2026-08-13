using Edo.Common;
using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services;
using Edo.Problem.Routine.Services.CodeDuplicatedProblem;
using Edo.Problem.Routine.Services.CodePoolMissingProblem;
using Edo.Problem.Routine.Services.Common;
using Edo.Problem.Routine.Services.OrderSelfDeliveryPaidProblem;
using Edo.Problem.Routine.Services.OrderStatusProblem;
using Edo.Problem.Routine.Services.ReceiptContactProblem;
using Edo.Problems;
using Edo.Transport;
using Microsoft.Extensions.DependencyInjection;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;

namespace Edo.Problem.Routine
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавить сервисы обработки проблем в ЭДО в коллекцию сервисов
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Коллекция сервисов</returns>
		public static IServiceCollection AddEdoProblemRoutineServices(this IServiceCollection services)
		{
			services
				.AddCoreDataRepositories()
				.AddCore()
				.AddEdo()
				.AddEdoProblemRegistration()
				.AddEdoNotifications();

			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
			services.AddScoped<MessageService>();
 			services.AddScoped<EdoProblemRoutineNotificationFactory>();
			services.AddScoped<IEdoProblemRoutineNotificationService, EdoProblemRoutineNotificationService>();

			services
				.AddOrderSelfDeliveryPaidProblem()
				.AddOrderFiscalDocumentSendErrorProblem()
				.AddReceiptNightSendProblem()
				.AddOrderStatusProblem()
				.AddCodeDuplicatedProblem()
				.AddReceiptContactProblem()
				;

			return services;
		}

		/// <summary>
		/// Добавить сервисы обработки проблем в ЭДО в коллекцию сервисов
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Коллекция сервисов</returns>
		public static IServiceCollection AddEdoProblemRoutine(this IServiceCollection services)
		{
			services.AddEdoProblemRoutineServices();
			services.AddEdoMassTransit();

			return services;
		}

		private static IServiceCollection AddOrderSelfDeliveryPaidProblem(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureOrderSelfDeliveryPaidProblemWorkerOptions>();
			services.AddScoped<OrderSelfDeliveryPaidProblemService>();

			return services;
		}

		private static IServiceCollection AddOrderFiscalDocumentSendErrorProblem(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureFiscalDocumentSendErrorProblemWorkerOptions>();
			services.AddScoped<FiscalDocumentSendErrorProblemService>();

			return services;
		}

		private static IServiceCollection AddOrderStatusProblem(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureOrderStatusProblemWorkerOptions>();
			services.AddScoped<OrderStatusProblemService>();

			return services;
		}

		private static IServiceCollection AddCodeDuplicatedProblem(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureCodeDuplicatedProblemWorkerOptions>();
			services.AddScoped<ICodeDuplicatedProblemService, CodeDuplicatedProblemService>();

			return services;
		}

		private static IServiceCollection AddReceiptNightSendProblem(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureReceiptNightSendProblemWorkerOptions>();
			services.AddScoped<ReceiptNightSendProblemService>();

			return services;
		}

		private static IServiceCollection AddReceiptContactProblem(this IServiceCollection services) =>
			services
				.ConfigureOptions<ConfigureReceiptContactProblemWorkerOptions>()
				.AddScoped<IReceiptContactProblemSourceProvider, ReceiptContactProblemSourceProvider>()
				.AddScoped<IReceiptContactProblemService, ReceiptContactProblemService>()
				.AddScoped<IReceiptEdoTaskResendService, ReceiptEdoTaskResendService>()
				.AddScoped<IReceiptContactProblemNotificationService, ReceiptContactProblemNotificationService>();
		
		public static IServiceCollection AddOrderEdoCodePoolMissingProblem(this IServiceCollection services)
		{
			services
				.ConfigureOptions<ConfigureCodePoolMissingProblemWorkerOptions>()
				.AddScoped<ICodePoolMissingProblemService, CodePoolMissingProblemService>()
				.AddEdoProblemRegistration();;

			return services;
		}
	}
}
