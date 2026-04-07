using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure;

namespace Edo.Problems.Routine
{
	public class OrderContactProblemUpdateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<OrderContactProblemUpdateWorker> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		
		private bool _workInProgress;
		private readonly int _intervalMinutes = 15;
		
		public OrderContactProblemUpdateWorker(
			ILogger<OrderContactProblemUpdateWorker> logger,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger;
			_serviceScopeFactory = serviceScopeFactory;
			
			Interval = TimeSpan.FromMinutes(_intervalMinutes);
		}
		
		protected override TimeSpan Interval { get; }
		
		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				var now = DateTime.Now;
				var startDate = now.Date.AddDays(-3);
				var endDate = now;

				using var scope = _serviceScopeFactory.CreateScope();

				var edoTaskRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<EdoTask>>();
				var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
				var edoOrderContactProvider = scope.ServiceProvider.GetRequiredService<IEdoOrderContactProvider>();

				using var uow = unitOfWorkFactory.CreateWithoutRoot("Проверка исправления клиента в заказе");

				var edoTasks = (await edoTaskRepository
						.GetAsync(uow,
							x => x.CreationTime >= startDate
							     && x.CreationTime <= endDate
							     && x.Problems.Any(problem =>
								     (problem.SourceName == "OrderContactMissingException" || problem.SourceName == "Receipt.ContactValid")
								     && problem.State == TaskProblemState.Active),
							cancellationToken: stoppingToken))
					.Value.ToArray();

				foreach(var edoTask in edoTasks)
				{
					if(edoTask is OrderEdoTask orderEdoTask)
					{
						var request = orderEdoTask.FormalEdoRequest;
						var order = request.Order;

						var contact = edoOrderContactProvider.GetContact(order);

						if(order.Client != null && contact.IsValid)
						{
							var problem = orderEdoTask.Problems.FirstOrDefault(problem =>
								(problem.SourceName is "OrderContactMissingException" or "Receipt.ContactValid")
								&& problem.State == TaskProblemState.Active);
							if(problem != null)
							{
								problem.State = TaskProblemState.Solved;
								orderEdoTask.Status = EdoTaskStatus.InProgress;

								await uow.SaveAsync(problem, cancellationToken: stoppingToken);
								await uow.SaveAsync(orderEdoTask, cancellationToken: stoppingToken);
							}
						}
					}
				}

				await uow.CommitAsync(stoppingToken);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при обработке заказов в: {ErrorDateTime}",
					DateTimeOffset.Now);
			}
			
			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayTime}' мин. перед следующим запуском", nameof(OrderContactProblemUpdateWorker), _intervalMinutes);
		}
		
		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {StartTime}",
				nameof(OrderContactProblemUpdateWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(OrderContactProblemUpdateWorker),
				DateTimeOffset.Now);

			base.OnStopService();
		}
	}
}
