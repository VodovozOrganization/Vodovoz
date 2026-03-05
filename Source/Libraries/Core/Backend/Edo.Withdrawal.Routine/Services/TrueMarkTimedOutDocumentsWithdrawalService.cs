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
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
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
		private readonly IGenericRepository<CounterpartyEntity> _counterpartyRepository;
		private readonly IClientsTrueMarkRegistrationCheckService _trueMarkRegistrationCheckService;
		private readonly IBus _messageBus;

		public TrueMarkTimedOutDocumentsWithdrawalService(
			ILogger<TrueMarkTimedOutDocumentsWithdrawalService> logger,
			IUnitOfWorkFactory uowFactory,
			IEdoSettings edoSettings,
			IEdoRepository edoRepository,
			IGenericRepository<CounterpartyEntity> counterpartyRepository,
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
			_counterpartyRepository = counterpartyRepository
				?? throw new ArgumentNullException(nameof(counterpartyRepository));
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
		public async Task ProcessTimedOutDocumentTasks(CancellationToken cancellationToken)
		{
			var withdrawalEdoRequests = Enumerable.Empty<WithdrawalEdoRequest>();
			var clientInnsToSetRegisteredStatus = Enumerable.Empty<string>();

			using(var uow = _uowFactory.CreateWithoutRoot(nameof(TrueMarkTimedOutDocumentsWithdrawalService)))
			{
				var timedOutDocumentTasks =
					(await GetTimedOutDocumentsOfRegisteredInTrueMarkClients(uow, cancellationToken))
					.ToList();

				var timedOutTasksOfNotRegisteredInTrueMarkClients =
					await GetTimedOutDocumentsOfNotRegisteredInTrueMarkClients(uow, cancellationToken);

				clientInnsToSetRegisteredStatus =
					await GetRegisteredInTrueMarkClientsByDataFromTrueMark(timedOutTasksOfNotRegisteredInTrueMarkClients, cancellationToken);

				timedOutDocumentTasks.AddRange(
					timedOutTasksOfNotRegisteredInTrueMarkClients
						.Where(x => !clientInnsToSetRegisteredStatus.Contains(x.ClientInn)));

				withdrawalEdoRequests = await CreateWithdrawalEdoRequesrs(uow, timedOutDocumentTasks, cancellationToken);

				await uow.CommitAsync(cancellationToken);
			}

			await PublishWithdrawalEdoRequestCreatedEvents(withdrawalEdoRequests, cancellationToken);

			if(clientInnsToSetRegisteredStatus.Any())
			{
				await UpdateClientsRegistrationInTrueMarkStatus(clientInnsToSetRegisteredStatus, cancellationToken);
			}
		}

		private async Task UpdateClientsRegistrationInTrueMarkStatus(IEnumerable<string> clientInns, CancellationToken cancellationToken)
		{
			try
			{
				using(var uow = _uowFactory.CreateWithoutRoot($"Обновление статуса регистрации клиента в ЧЗ в сервисе {nameof(TrueMarkTimedOutDocumentsWithdrawalService)}"))
				{
					var clients =
						(await _counterpartyRepository.GetAsync(uow, x => clientInns.Contains(x.INN), cancellationToken: cancellationToken))
						.Value;

					foreach(var client in clients)
					{
						client.RegistrationInChestnyZnakStatus = RegistrationInChestnyZnakStatus.Registered;
						await uow.SaveAsync(client, cancellationToken: cancellationToken);
					}

					await uow.CommitAsync(cancellationToken);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обновлении статуса регистрации клиента в ЧЗ в сервисе. Сообщение: {ErrorMessage}",
					ex.Message);
			}
		}

		private async Task<IList<TimedOutOrderDocumentTaskNode>> GetTimedOutDocumentsOfRegisteredInTrueMarkClients(
			IUnitOfWork uow,
			CancellationToken cancellationToken)
		{
			var timeoutDays = _edoSettings.ConnectedTrueMarkClientsWithdrawalDocflowTimeoutDays;
			var searchMode = TimedOutDocumentTasksSearchMode.OnlyTrueMarkRegisteredClients;

			_logger.LogInformation(
					"Ищем задачи ЭДО с отправленным более {TimeoutDays} дней назад, но не принятым клиентом документом. Клиент должен быть зарегистрирован в ЧЗ",
					timeoutDays);

			var timedOutTasks = await _edoRepository.GetTrueMarkConnectedClientsTimedOutOrderDocumentTasks(
					uow,
					timeoutDays,
					searchMode,
					cancellationToken);

			_logger.LogInformation(
				"Найдены просроченные задачи ЭДО по {OrdersCount} заказам по зарегистрированным в ЧЗ клиентам",
				timedOutTasks.Count);

			return timedOutTasks;
		}

		private async Task<IList<TimedOutOrderDocumentTaskNode>> GetTimedOutDocumentsOfNotRegisteredInTrueMarkClients(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var timeoutDays = _edoSettings.NotConnectedTrueMarkClientsWithdrawalDocflowTimeoutDays;
			var searchMode = TimedOutDocumentTasksSearchMode.OnlyTrueMarkNotRegisteredClients;

			_logger.LogInformation(
					"Ищем задачи ЭДО с отправленным более {TimeoutDays} дней назад, но не принятым клиентом документом. Клиент не должен быть зарегистрирован в ЧЗ",
					timeoutDays);

			var timedOutTasks = await _edoRepository.GetTrueMarkConnectedClientsTimedOutOrderDocumentTasks(
					uow,
					timeoutDays,
					searchMode,
					cancellationToken);

			_logger.LogInformation(
				"Найдены просроченные задачи ЭДО по {OrdersCount} заказам по не зарегистрированным в ЧЗ клиентам",
				timedOutTasks.Count);

			return timedOutTasks;
		}

		private async Task<IList<string>> GetRegisteredInTrueMarkClientsByDataFromTrueMark(
			IEnumerable<TimedOutOrderDocumentTaskNode> timedOutOrderDocumentTaskNodes,
			CancellationToken cancellationToken)
		{
			var clientsInnsToCheckRegistrationInTrueMark =
				timedOutOrderDocumentTaskNodes
				.Select(x => x.ClientInn)
				.Distinct()
				.ToList();

			return await GetRegisteredInTrueMarkClientsByDataFromTrueMark(clientsInnsToCheckRegistrationInTrueMark, cancellationToken);
		}

		private async Task<IList<string>> GetRegisteredInTrueMarkClientsByDataFromTrueMark(
			IEnumerable<string> inns,
			CancellationToken cancellationToken)
		{
			var registeredInTrueMarkClients = new List<string>();

			foreach(var inn in inns)
			{
				var registrationStatusResult =
					await _trueMarkRegistrationCheckService.GetTrueMarkRegistrationStatus(inn, cancellationToken);

				if(registrationStatusResult.IsSuccess)
				{
					var registrationStatus = registrationStatusResult.Value;
					if(registrationStatus == RegistrationInChestnyZnakStatus.Registered)
					{
						registeredInTrueMarkClients.Add(inn);
					}
				}
			}

			return registeredInTrueMarkClients;
		}

		private async Task<IList<WithdrawalEdoRequest>> CreateWithdrawalEdoRequesrs(
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
				Source = CustomerEdoRequestSource.Manual,
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
