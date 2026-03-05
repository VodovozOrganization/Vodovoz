using Edo.Common.Services;
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
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IEdoSettings _edoSettings;
		private readonly IEdoRepository _edoRepository;
		private readonly IClientsTrueMarkRegistrationCheckService _trueMarkRegistrationCheckService;
		private readonly IBus _messageBus;

		public TrueMarkTimedOutDocumentsWithdrawalService(
			ILogger<TrueMarkTimedOutDocumentsWithdrawalService> logger,
			IUnitOfWorkFactory uowFactory,
			IEdoSettings edoSettings,
			IEdoRepository edoRepository,
			IClientsTrueMarkRegistrationCheckService trueMarkRegistrationCheckService,
			IBus messageBus)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory
				?? throw new ArgumentNullException(nameof(uowFactory));
			_edoSettings = edoSettings
				?? throw new ArgumentNullException(nameof(edoSettings));
			_edoRepository = edoRepository
				?? throw new ArgumentNullException(nameof(edoRepository));
			_trueMarkRegistrationCheckService = trueMarkRegistrationCheckService
				?? throw new ArgumentNullException(nameof(trueMarkRegistrationCheckService));
			_messageBus = messageBus
				?? throw new ArgumentNullException(nameof(messageBus));
		}

		/// <summary>
		/// Обрабатывает просроченные документообороты и создать заявки на вывод из оборота
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessTimedOutDocuments(CancellationToken cancellationToken)
		{
			await ProcessTimedOutDocumentsOfRegisteredInTrueMarkClients(cancellationToken);
		}

		/// <summary>
		/// Обработка просроченных ДО клиентов, зарегистрированных в ЧЗ
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task ProcessTimedOutDocumentsOfRegisteredInTrueMarkClients(CancellationToken cancellationToken)
		{
			var timeoutDays = _edoSettings.ConnectedTrueMarkClientsWithdrawalDocflowTimeoutDays;

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				_logger.LogInformation(
					"Ищем задачи ЭДО с отправленным более {TimeoutDays} дней назад, но не принятым клиентом документом. Клиент должен быть зарегистрирован в ЧЗ",
					timeoutDays);

				var timedOutTasks =
					await _edoRepository.GetTrueMarkConnectedClientsTimedOutOrderDocumentTasks(uow, timeoutDays, cancellationToken);

				_logger.LogInformation(
					"Найдены просроченные задачи ЭДО по {OrdersCount} заказам по зарегистрированным в ЧЗ клиентам",
					timedOutTasks.Count);

				var withdrawalRequests = new List<WithdrawalEdoRequest>();

				foreach(var timedOutTask in timedOutTasks)
				{
					var order = timedOutTask.Key;
					var tasks = timedOutTask.ToList();

					if(tasks.Count != 1)
					{
						_logger.LogInformation(
							"По заказу {OrderId} найдено {TasksCount} задач с истёкшим таймаутом. " +
							"Заявка на вывод из оборота не будет сформирована",
							order.Id,
							tasks.Count);

						continue;
					}

					var task = tasks.First();

					var withdrawalRequest = CreateWithdrawalRequest(order, task);

					await uow.SaveAsync(withdrawalRequest, cancellationToken: cancellationToken);

					withdrawalRequests.Add(withdrawalRequest);
				}

				await uow.CommitAsync(cancellationToken);

				await PublishWithdrawalEdoRequestCreatedEvents(withdrawalRequests, cancellationToken);

				_logger.LogInformation("Обработка ЭДО задач с истёкшим таймаутом для создания запроса на вывод из оборота выполнено");
			}
		}

		private WithdrawalEdoRequest CreateWithdrawalRequest(
			OrderEntity order,
			DocumentEdoTask edoTask)
		{
			var withdrawalRequest = new WithdrawalEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Manual,
				Type = CustomerEdoRequestType.Order,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
				BaseDocumentEdoTask = edoTask
			};

			_logger.LogInformation(
				"Создана заявка на вывод из оборота {RequestId} для заказа {OrderId}",
				withdrawalRequest.Id, order.Id);

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

				await _messageBus.Publish(
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
