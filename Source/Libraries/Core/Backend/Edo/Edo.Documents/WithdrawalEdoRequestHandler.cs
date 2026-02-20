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
		private readonly IPublishEndpoint _publishEndpoint;

		public WithdrawalEdoRequestHandler(
			ILogger<WithdrawalEdoRequestHandler> logger,
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<CounterpartyEdoAccountEntity> edoAccountRepository,
			IPublishEndpoint publishEndpoint)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_edoAccountRepository = edoAccountRepository ?? throw new ArgumentNullException(nameof(edoAccountRepository));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		/// <summary>
		/// Обработать событие завершения документооборота и при необходимости создать заявку на вывод из оборота
		/// </summary>
		/// <param name="documentId">Идентификатор документа ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Идентификатор созданной заявки или null, если заявка не была создана</returns>
		public async Task<int?> HandleOrderDocflowCompleted(int documentId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var document = await uow.Session.GetAsync<OrderEdoDocument>(documentId, cancellationToken);
				if(document == null)
				{
					_logger.LogWarning("Документ ЭДО с Id {DocumentId} не найден", documentId);
					return null;
				}

				var documentTask = await uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken);
				if(documentTask == null)
				{
					_logger.LogWarning("Задача ЭДО для документа {DocumentId} не найдена", documentId);
					return null;
				}

				var formalEdoRequest = documentTask.FormalEdoRequest;
				if(formalEdoRequest == null)
				{
					_logger.LogWarning("Формальная заявка ЭДО для задачи {TaskId} не найдена", documentTask.Id);
					return null;
				}

				var order = formalEdoRequest.Order;
				if(order == null)
				{
					_logger.LogWarning("Заказ для заявки {RequestId} не найден", formalEdoRequest.Id);
					return null;
				}

				var client = order.Client;
				if(client == null)
				{
					_logger.LogWarning("Клиент для заказа {OrderId} не найден", order.Id);
					return null;
				}

				var edoAccount = _edoAccountRepository
					.Get(uow, x => x.Counterparty.Id == client.Id && x.OrganizationId == order.Contract.Organization.Id)
					.FirstOrDefault();

				if(edoAccount == null)
				{
					_logger.LogWarning(
						"Не найден аккаунт ЭДО для контрагента {CounterpartyId} и организации {OrganizationId}",
						client.Id, order.Contract.Organization.Id);
					return null;
				}

				if(edoAccount.ConsentForEdoStatus != ConsentForEdoStatus.Agree)
				{
					_logger.LogInformation(
						"Контрагент {CounterpartyId} не подключён к ЭДО. Вывод из оборота не требуется",
						client.Id);
					return null;
				}

				if(client.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
				{
					_logger.LogInformation(
						"Контрагент {CounterpartyId} зарегистрирован в ЧЗ. Вывод из оборота не требуется",
						client.Id);
					return null;
				}

				var existingWithdrawalRequest = uow.Session
					.QueryOver<WithdrawalEdoRequest>()
					.Where(x => x.Order.Id == order.Id)
					.RowCount();

				if(existingWithdrawalRequest > 0)
				{
					_logger.LogInformation(
						"Заявка на вывод из оборота для заказа {OrderId} уже существует",
						order.Id);
					return null;
				}

				var withdrawalRequest = CreateWithdrawalEdoRequest(formalEdoRequest, order);

				await uow.SaveAsync(withdrawalRequest, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Создана заявка на вывод из оборота {RequestId} для заказа {OrderId}",
					withdrawalRequest.Id, order.Id);

				await _publishEndpoint.Publish(
					new WithdrawalEdoRequestCreatedEvent { Id = withdrawalRequest.Id },
					cancellationToken);

				return withdrawalRequest.Id;
			}
		}

		/// <summary>
		/// Создать заявку на вывод из оборота из заявки на отправку документов
		/// </summary>
		/// <param name="sourceRequest">Исходная заявка</param>
		/// <param name="order">Заказ</param>
		/// <returns>Заявка на вывод из оборота</returns>
		private WithdrawalEdoRequest CreateWithdrawalEdoRequest(FormalEdoRequest sourceRequest, OrderEntity order)
		{
			var withdrawalRequest = new WithdrawalEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Manual,
				Type = CustomerEdoRequestType.Order,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
				Task = sourceRequest.Task
			};

			if(sourceRequest.ProductCodes != null)
			{
				foreach(var productCode in sourceRequest.ProductCodes)
				{
					withdrawalRequest.ProductCodes.Add(productCode);
				}
			}

			return withdrawalRequest;
		}
	}
}
