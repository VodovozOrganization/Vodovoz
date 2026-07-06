using Edo.Common;
using Edo.Common.Services;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Documents
{
	/// <summary>
	/// Обработчик для создания заявки на вывод кодов из оборота
	/// </summary>
	public class WithdrawalEdoRequestHandler
	{
		private readonly ILogger<WithdrawalEdoRequestHandler> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<CounterpartyEdoAccountEntity> _edoAccountRepository;
		private readonly IGenericRepository<WithdrawalEdoRequest> _withdrawalEdoRequestRepository;
		private readonly IClientsTrueMarkRegistrationCheckService _trueMarkRegistrationCheckService;
		private readonly ITrueMarkCodesValidator _trueMarkTaskCodesValidator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly IBus _messageBus;

		public WithdrawalEdoRequestHandler(
			ILogger<WithdrawalEdoRequestHandler> logger,
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<CounterpartyEdoAccountEntity> edoAccountRepository,
			IGenericRepository<WithdrawalEdoRequest> withdrawalEdoRequestRepository,
			IClientsTrueMarkRegistrationCheckService trueMarkRegistrationCheckService,
			ITrueMarkCodesValidator trueMarkTaskCodesValidator,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			IBus publishEndpoint)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory
				?? throw new ArgumentNullException(nameof(uowFactory));
			_edoAccountRepository = edoAccountRepository
				?? throw new ArgumentNullException(nameof(edoAccountRepository));
			_withdrawalEdoRequestRepository = withdrawalEdoRequestRepository
				?? throw new ArgumentNullException(nameof(withdrawalEdoRequestRepository));
			_trueMarkRegistrationCheckService = trueMarkRegistrationCheckService
				?? throw new ArgumentNullException(nameof(trueMarkRegistrationCheckService));
			_trueMarkTaskCodesValidator = trueMarkTaskCodesValidator
				?? throw new ArgumentNullException(nameof(trueMarkTaskCodesValidator));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory
				?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_messageBus = publishEndpoint
				?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		/// <summary>
		/// Обработать событие завершения документооборота и при необходимости создать заявку на вывод из оборота
		/// </summary>
		/// <param name="documentId">Идентификатор документа ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Идентификатор созданной заявки или null, если заявка не была создана</returns>
		public async Task HandleOrderDocflowCompleted(int documentId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(nameof(WithdrawalEdoRequestHandler)))
			{
				var document = await uow.Session.GetAsync<OrderEdoDocument>(documentId, cancellationToken)
					?? throw new InvalidOperationException($"Документ ЭДО с Id {documentId} не найден");

				var documentTask = await uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken)
					?? throw new InvalidOperationException($"Задача ЭДО для документа {documentId} не найдена");

				var formalEdoRequest = documentTask.FormalEdoRequest
					?? throw new InvalidOperationException($"Запрос на ЭДО для задачи {documentTask.Id} не найден");

				var order = formalEdoRequest.Order
					?? throw new InvalidOperationException($"Заказ для заявки {formalEdoRequest.Id} не найден");

				var client = order.Client
					?? throw new InvalidOperationException($"Клиент для заказа {order.Id} не найден");

				if(order.PaymentType != Vodovoz.Domain.Client.PaymentType.Cashless)
				{
					throw new InvalidOperationException(
						$"Поступило событие об окончании ДО по заказу {order.Id}. " +
						$"Но при этом заказ имеет форму оплаты {order.PaymentType}, отличную от безналичный расчет");
				}

				if(client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds)
				{
					_logger.LogInformation(
						"В карточке контрагента {client.Id} указана причина выбытия отличная от {RequiredReasonForLeaving}. Вывод из оборота не требуется",
						client.Id,
						nameof(ReasonForLeaving.ForOwnNeeds));
					return;
				}

				var edoAccount = await GetClientEdoAccount(uow, client.Id, order.Contract.Organization.Id, cancellationToken)
					?? throw new InvalidOperationException(
						$"Не найден аккаунт ЭДО для контрагента {client.Id} и организации {order.Contract.Organization.Id}");

				if(edoAccount.ConsentForEdoStatus != ConsentForEdoStatus.Agree)
				{
					throw new InvalidOperationException(
						$"Поступило событие об окончании ДО по заказу {order.Id}. " +
						$"Но при этом клиент {client.Id} имеет статус согласия на ЭДО {edoAccount.ConsentForEdoStatus} " +
						$"с нашей организацией {order.Contract.Organization.Id}");
				}

				var trueMarkCodesChecker =
					_edoTaskTrueMarkCodeCheckerFactory.Create(documentTask);
				var codesValidationResult =
					await _trueMarkTaskCodesValidator.ValidateAsync(documentTask, trueMarkCodesChecker, cancellationToken);

				if(!codesValidationResult.IsAllValid || !codesValidationResult.ReadyToSell)
				{
					_logger.LogInformation(
						"В задаче {DocumentTaskId} присутствуют коды ЧЗ, статус которых не позволяет выполнить вывод из оборота, " +
						"либо принадлежащие организации, отличной от организации в заказе",
						documentTask.Id);
					return;
				}

				var actualTrueMarkRegistrationStatusResult =
					await _trueMarkRegistrationCheckService.GetTrueMarkRegistrationStatus(client.INN, cancellationToken);

				var isClientRegistrationStatusChanged = false;

				if(actualTrueMarkRegistrationStatusResult.IsSuccess)
				{
					var actualRegistrationStatus = actualTrueMarkRegistrationStatusResult.Value;

					if(actualRegistrationStatus != client.RegistrationInChestnyZnakStatus)
					{
						client.RegistrationInChestnyZnakStatus = actualRegistrationStatus;
						isClientRegistrationStatusChanged = true;
						await uow.SaveAsync(client, cancellationToken: cancellationToken);
					}
				}

				if(CounterpartyEntity.RegisteredInTrueMarkStatuses.Contains(client.RegistrationInChestnyZnakStatus))
				{
					_logger.LogInformation(
						"Контрагент {CounterpartyId} зарегистрирован в ЧЗ. Вывод из оборота в данный момент не требуется",
						client.Id);

					if(isClientRegistrationStatusChanged)
					{
						await uow.CommitAsync(cancellationToken);
					}

					return;
				}

				var isWithdrawalRequestExists = await IsWithdrawalRequestForOrderAndDocumentTaskExists(
					uow,
					order.Id,
					documentTask.Id,
					cancellationToken);

				if(isWithdrawalRequestExists)
				{
					_logger.LogInformation(
						"Заявка на вывод из оборота для заказа {OrderId} уже существует",
						order.Id);
					return;
				}

				var codes = documentTask.Items.Select(x => x.ProductCode).ToList();

				if(!codes.Any())
				{
					_logger.LogInformation(
						"В задаче {DocumentTaskId} отсутствуют коды ЧЗ. Вывод из оборота не требуется",
						documentTask.Id);
					return;
				}

				var withdrawalRequest = CreateWithdrawalEdoRequest(order, documentTask);

				await uow.SaveAsync(withdrawalRequest, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Создана заявка на вывод из оборота {RequestId} для заказа {OrderId}",
					withdrawalRequest.Id, order.Id);

				await _messageBus.Publish(
					new WithdrawalEdoRequestCreatedEvent { Id = withdrawalRequest.Id },
					cancellationToken);
			}
		}

		private async Task<CounterpartyEdoAccountEntity> GetClientEdoAccount(
			IUnitOfWork uow,
			int clientId,
			int organizationId,
			CancellationToken cancellationToken)
		{
			var edoAccount = (await _edoAccountRepository
				.GetAsync(
					uow,
					x => x.Counterparty.Id == clientId
						&& x.OrganizationId == organizationId
						&& x.IsDefault,
					cancellationToken: cancellationToken))
				.Value
				.FirstOrDefault();

			return edoAccount;
		}

		private async Task<bool> IsWithdrawalRequestForOrderAndDocumentTaskExists(
			IUnitOfWork uow,
			int orderId,
			int documentTaskId,
			CancellationToken cancellationToken)
		{
			var isWithdrawalRequestExists = (await _withdrawalEdoRequestRepository
				.GetAsync(
					uow,
					x => x.Order.Id == orderId && x.BaseDocumentEdoTask.Id == documentTaskId,
					cancellationToken: cancellationToken))
				.Value
				.Any();

			return isWithdrawalRequestExists;
		}

		private WithdrawalEdoRequest CreateWithdrawalEdoRequest(OrderEntity order, DocumentEdoTask documentTask)
		{
			var withdrawalRequest = new WithdrawalEdoRequest
			{
				Time = DateTime.Now,
				Source = EdoRequestSource.Manual,
				Type = CustomerEdoRequestType.Order,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
				BaseDocumentEdoTask = documentTask
			};

			return withdrawalRequest;
		}
	}
}
