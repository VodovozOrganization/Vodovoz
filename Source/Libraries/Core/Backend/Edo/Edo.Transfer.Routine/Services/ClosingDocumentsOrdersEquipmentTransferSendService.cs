using Edo.Transfer.Routine.Options;
using Edo.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Organizations;

namespace Edo.Transfer.Routine.Services
{
	public class ClosingDocumentsOrdersEquipmentTransferSendService
	{
		private readonly ILogger<ClosingDocumentsOrdersEquipmentTransferSendService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> _optionsMonitor;
		private readonly MessageService _edoMessageService;

		private readonly IEnumerable<OrderStatus> _orderStatusesToSendUpd =
			new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

		public ClosingDocumentsOrdersEquipmentTransferSendService(
			ILogger<ClosingDocumentsOrdersEquipmentTransferSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrganizationSettings organizationSettings,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IOptionsMonitor<ClosingDocumentsOrdersUpdSendSettings> optionsMonitor,
			MessageService edoMessageService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_optionsMonitor = optionsMonitor;
			_edoMessageService = edoMessageService ?? throw new ArgumentNullException(nameof(edoMessageService));
		}

		public async Task Send(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ClosingDocumentsOrdersUpdSendService)))
			{
				var orders = await GetEquipmentTransferOrdersToSendEdoRequest(uow, cancellationToken);
				var edoRequests = await CreateEquipmentTransferEdoRequests(uow, orders, cancellationToken);

				await PublishEdoRequestCreatedEvents(edoRequests);
			}
		}


		private async Task<IEnumerable<OrderEntity>> GetEquipmentTransferOrdersToSendEdoRequest(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем заказы для отправки ЭДО акта приёма-передачи оборудования");

			var orders =
				await (from order in uow.Session.Query<OrderEntity>()
					   join client in uow.Session.Query<CounterpartyEntity>() on order.Client.Id equals client.Id
					   join contract in uow.Session.Query<CounterpartyContractEntity>()
						   on order.Contract.Id equals contract.Id
					   join organization in uow.Session.Query<CounterpartyEntity>()
						   on contract.Organization.Id equals organization.Id
					   join er in uow.Session.Query<EquipmentTransferEdoRequest>() on order.Id equals er.Order.Id into edoRequests
					   from edoRequest in edoRequests.DefaultIfEmpty()
					   join defaultEdoAccount in uow.Session.Query<CounterpartyEdoAccountEntity>()
						   on new { a = client.Id, b = (int?)contract.Organization.Id, c = true }
						   equals new { a = defaultEdoAccount.Counterparty.Id, b = defaultEdoAccount.OrganizationId, c = defaultEdoAccount.IsDefault }
						   into edoAccountsByOrder
					   from edoAccountByOrder in edoAccountsByOrder.DefaultIfEmpty()
					   join defaultVodovozEdoAccount in uow.Session.Query<CounterpartyEdoAccountEntity>()
						   on new { a = client.Id, b = (int?)_organizationSettings.VodovozOrganizationId, c = true }
						   equals new { a = defaultVodovozEdoAccount.Counterparty.Id, b = defaultVodovozEdoAccount.OrganizationId, c = defaultVodovozEdoAccount.IsDefault }
					   where
						   order.PaymentType == PaymentType.Cashless
						   && order.DeliveryDate >= DateTime.Today.AddDays(-_optionsMonitor.CurrentValue.MaxDaysFromDeliveryDate)
						   && order.DeliverySchedule.Id == _deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId
						   && _orderStatusesToSendUpd.Contains(order.OrderStatus)
						   && client.IsNewEdoProcessing
						   && (edoAccountByOrder.ConsentForEdoStatus == ConsentForEdoStatus.Agree
							   || defaultVodovozEdoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
						   && !client.IsNotSendDocumentsByEdo
						   && edoRequest == null // Добавить булево из карточки КА
					   select order)
				.ToListAsync(cancellationToken);

			_logger.LogInformation($"Найдено {orders.Count()} заказов для отправки ЭДО акта приёма-передачи оборудования");

			return orders;
		}

		private async Task<IEnumerable<EquipmentTransferEdoRequest>> CreateEquipmentTransferEdoRequests(IUnitOfWork uow, IEnumerable<OrderEntity> orders, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Создаем заявки на ЭДО акта приёма-передачи оборудования");

			var edoRequests = new List<EquipmentTransferEdoRequest>();

			foreach(var order in orders)
			{
				if(order is null)
				{
					continue;
				}

				if(order.OrderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark))
				{
					_logger.LogError(
						$"В заказе \"Закр док\" имеются маркированные товары. Отправка документов по ЭДО недоступна. OrderId: {order.Id}");

					continue;
				}

				var edoRequest = CreateEquipmentTransferEdoRequests(order);

				await uow.SaveAsync(edoRequest, cancellationToken: cancellationToken);

				edoRequests.Add(edoRequest);
			}

			if(edoRequests.Any())
			{
				await uow.CommitAsync(cancellationToken);
			}

			_logger.LogInformation($"Создано {edoRequests.Count} заявок");

			return edoRequests;
		}


		private EquipmentTransferEdoRequest CreateEquipmentTransferEdoRequests(OrderEntity order)
		{
			var edoRequest = new EquipmentTransferEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Manual,
				DocumentType = EdoDocumentType.InformalOrderDocument,
				Order = order,
			};

			return edoRequest;
		}

		private async Task PublishEdoRequestCreatedEvents(IEnumerable<EdoRequest> edoRequests)
		{
			_logger.LogInformation("Отправляем события о создании новых заявок по ЭДО");

			if(edoRequests is null || !edoRequests.Any())
			{
				_logger.LogInformation("Нет новых заявок по ЭДО для отправки событий");

				return;
			}

			foreach(var edoRequest in edoRequests)
			{
				try
				{
					_logger.LogInformation($"Отправляем событие о создании новой заявки по ЭДО. RequestId: {edoRequest.Id}.");

					await _edoMessageService.PublishEdoRequestCreatedEvent(edoRequest.Id);

					_logger.LogInformation($"Событие о создании новой заявки по ЭДО отправлено успешно");
				}
				catch(Exception ex)
				{
					_logger.LogError(
						ex,
						$"Ошибка при отправке события на создание новой заявки по ЭДО. RequestId: {edoRequest.Id}.");

					continue;
				}
			}
		}
	}
}
