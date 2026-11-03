using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Settings.Edo;

namespace Edo.Withdrawal.Routine.Services
{
	/// <summary>
	/// Сервис для проверки документооборотов с истёкшим таймаутом
	/// </summary>
	public class TrueMarkTimedOutDocumentsWithdrawalService
	{
		private readonly ILogger<TrueMarkTimedOutDocumentsWithdrawalService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IEdoSettings _edoSettings;
		private readonly IEdoRepository _edoRepository;
		private readonly IPublishEndpoint _publishEndpoint;

		public TrueMarkTimedOutDocumentsWithdrawalService(
			ILogger<TrueMarkTimedOutDocumentsWithdrawalService> logger,
			IUnitOfWork uow,
			IEdoSettings edoSettings,
			IEdoRepository edoRepository,
			IPublishEndpoint publishEndpoint)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_uow = uow
				?? throw new ArgumentNullException(nameof(uow));
			_edoSettings = edoSettings
				?? throw new ArgumentNullException(nameof(edoSettings));
			_edoRepository = edoRepository
				?? throw new ArgumentNullException(nameof(edoRepository));
			_publishEndpoint = publishEndpoint
				?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		/// <summary>
		/// Обрабатывает просроченные документообороты и создать заявки на вывод из оборота
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessTimedOutDocumentTasks(CancellationToken cancellationToken)
		{
			var timedOutTasks =
				(await GetTimedOutOrderDocumentTasks(_uow, cancellationToken))
				.ToList();

			var withdrawalEdoRequests = await CreateWithdrawalEdoRequests(_uow, timedOutTasks, cancellationToken);

			await PublishWithdrawalEdoRequestCreatedEvents(withdrawalEdoRequests, cancellationToken);

			await _uow.CommitAsync(cancellationToken);
		}

		private async Task<IList<TimedOutOrderDocumentTaskNode>> GetTimedOutOrderDocumentTasks(
			IUnitOfWork uow,
			CancellationToken cancellationToken)
		{
			var timeoutDays = _edoSettings.WithdrawalDocflowTimeoutDays;

			_logger.LogInformation(
					"Ищем задачи ЭДО с отправленным более {TimeoutDays} дней назад, но не принятым клиентом документом",
					timeoutDays);

			var timedOutTasks = await _edoRepository.GetTimedOutOrderDocumentTasks(
					uow,
					timeoutDays,
					cancellationToken);

			_logger.LogInformation(
				"Найдено {TasksCount} просроченных задач",
				timedOutTasks.Count);

			return timedOutTasks;
		}

		private async Task<IList<WithdrawalEdoRequest>> CreateWithdrawalEdoRequests(
			IUnitOfWork uow,
			IList<TimedOutOrderDocumentTaskNode> documentTaskNodes,
			CancellationToken cancellationToken)
		{
			var withdrawalRequests = new List<WithdrawalEdoRequest>();

			var existingWithdrawalRequestsForOrders = await _edoRepository.GetExistingWithdrawalEdoRequestOrders(
				uow,
				documentTaskNodes.Select(x => x.Order.Id).Distinct(),
				cancellationToken);

			var orderTasks = documentTaskNodes
				.ToLookup(x => x.Order, x => x.Task);

			foreach(var orderTask in orderTasks)
			{
				var order = orderTask.Key;
				var documentTasks = orderTask.ToList();

				if(existingWithdrawalRequestsForOrders.Contains(order.Id))
				{
					_logger.LogInformation(
						"По заказу {OrderId} уже создана заявка на вывод из оборота. " +
						"Новая заявка на вывод из оборота не будет сформирована",
						order.Id);

					continue;
				}

				if(documentTasks.Count != 1)
				{
					_logger.LogInformation(
						"По заказу {OrderId} найдено {TasksCount} задач с истёкшим таймаутом. " +
						"Заявка на вывод из оборота не будет сформирована",
						order.Id,
						documentTasks.Count);

					continue;
				}

				var withdrawalRequest = CreateWithdrawalRequest(order, documentTasks.First());

				await uow.SaveAsync(withdrawalRequest, cancellationToken: cancellationToken);

				_logger.LogInformation(
					"Создана заявка на вывод из оборота {RequestId} для заказа {OrderId}",
					withdrawalRequest.Id, order.Id);

				withdrawalRequests.Add(withdrawalRequest);
			}

			return withdrawalRequests;
		}

		private WithdrawalEdoRequest CreateWithdrawalRequest(
			OrderEntity order,
			DocumentEdoTask edoTask)
		{
			var withdrawalRequest = new WithdrawalEdoRequest
			{
				Time = DateTime.Now,
				Source = EdoRequestSource.Manual,
				Type = CustomerEdoRequestType.Order,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
				BaseDocumentEdoTask = edoTask
			};

			return withdrawalRequest;
		}

		private async Task PublishWithdrawalEdoRequestCreatedEvents(
			IEnumerable<WithdrawalEdoRequest> withdrawalEdoRequest,
			CancellationToken cancellationToken)
		{
			foreach(var request in withdrawalEdoRequest)
			{
				await PublishWithdrawalEdoRequestCreatedEvent(request, cancellationToken);
			}
		}

		private async Task PublishWithdrawalEdoRequestCreatedEvent(WithdrawalEdoRequest withdrawalEdoRequest, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation(
						"Публикация события {EventName} для заявки на вывод из оборота {RequestId}",
						nameof(WithdrawalEdoRequestCreatedEvent),
						withdrawalEdoRequest.Id);

				await _publishEndpoint.Publish(
					new WithdrawalEdoRequestCreatedEvent { Id = withdrawalEdoRequest.Id },
					cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при публикации события {EventName} для заявки на вывод из оборота {RequestId}",
					nameof(WithdrawalEdoRequestCreatedEvent),
					withdrawalEdoRequest.Id);
			}
		}
	}
}
