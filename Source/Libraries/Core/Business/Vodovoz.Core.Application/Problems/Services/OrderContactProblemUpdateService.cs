using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using EdoService.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;

namespace Vodovoz.Core.Application.Problems.Services
{
	public class OrderContactProblemUpdateService : IOrderContactProblemUpdateService
	{
		private readonly ILogger<OrderContactProblemUpdateService> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IEdoRepository _edoRepository;
		private readonly IEdoService _edoService;

		public OrderContactProblemUpdateService(ILogger<OrderContactProblemUpdateService> logger,
			IServiceScopeFactory serviceScopeFactory,
			IEdoRepository edoRepository,
			IEdoService edoService)
		{
			_logger = logger ?? throw new  ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory  ?? throw new  ArgumentNullException(nameof(serviceScopeFactory));
			_edoRepository = edoRepository  ?? throw new  ArgumentNullException(nameof(edoRepository));
			_edoService = edoService  ?? throw new  ArgumentNullException(nameof(edoService));
		}

		public async Task UpdateEdoTaskWithContactProblemAsync(CancellationToken stoppingToken)
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
				.Value.ToList();

			var tasksToRemove = new List<EdoTask>();

			foreach(var edoTask in edoTasks)
			{
				if(edoTask is not OrderEdoTask orderEdoTask)
				{
					continue;
				}

				var request = orderEdoTask.FormalEdoRequest;
				var order = request.Order;

				try
				{
					var contact = edoOrderContactProvider.GetContact(order);

					if(order.Client != null && contact.IsValid)
					{
						var problem = orderEdoTask.Problems.FirstOrDefault(problem =>
							(problem.SourceName is "OrderContactMissingException" or "Receipt.ContactValid")
							&& problem.State == TaskProblemState.Active);

						if(problem == null)
						{
							continue;
						}

						problem.State = TaskProblemState.Solved;
						orderEdoTask.Status = EdoTaskStatus.InProgress;

						await uow.SaveAsync(problem, cancellationToken: stoppingToken);
						await uow.SaveAsync(orderEdoTask, cancellationToken: stoppingToken);

						await uow.CommitAsync(stoppingToken);
					}
					else
					{
						tasksToRemove.Add(edoTask);
					}
				}
				catch(Exception e)
				{
					tasksToRemove.Add(edoTask);
				}
			}

			foreach(var t in tasksToRemove)
			{
				edoTasks.Remove(t);
			}

			foreach(var edoTask in edoTasks)
			{
				if(edoTask is not OrderEdoTask orderEdoTask)
				{
					continue;
				}

				var request = orderEdoTask.FormalEdoRequest;
				var order = request.Order;

				var isActiveProblem = orderEdoTask.Problems.Any(problem => problem.State == TaskProblemState.Active);

				if(isActiveProblem)
				{
					continue;
				}

				var documents = _edoRepository.GetOrderEdoDocumentsByOrderId(uow, order.Id);

				if(documents == null || !documents.Any())
				{
					_edoService.SendDocumentTaskCreatedEvent(edoTask);

					continue;
				}

				var resendResult = _edoService.ResendEdoDocumentForOrder(order);

				if(!resendResult.IsFailure)
				{
					continue;
				}

				var errors = string.Join("\n - ", resendResult.Errors.Select(x => x.Message));

				_logger.LogError(
					"Не удалось переотправить документ для заказа {OrderId}.\nПричины:\n - {Errors}",
					order.Id,
					errors
				);
			}
		}
	}
}
