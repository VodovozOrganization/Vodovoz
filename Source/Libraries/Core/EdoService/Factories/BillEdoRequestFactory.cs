using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EdoService.Library.Services;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Organizations;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.OrdersWithoutShipment;
using Vodovoz.Converters;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using VodovozBusiness.Controllers;

namespace EdoService.Library.Factories
{
	/// <summary>
	/// Создает неформализованные заявки со снимком счета и реквизитов его отправки
	/// </summary>
	public class BillEdoRequestFactory : IBillEdoRequestFactory
	{
		private readonly IOrderConverter _orderConverter;
		private readonly IOrderWithoutShipmentConverter _orderWithoutShipmentConverter;
		private readonly ICounterpartyEdoAccountController _edoAccountController;
		private readonly IBillEdoDocumentFileStorageService _fileStorageService;

		/// <summary>
		/// Создает фабрику снимков счетов
		/// </summary>
		/// <param name="orderConverter">Конвертер заказов</param>
		/// <param name="orderWithoutShipmentConverter">Конвертер счетов без отгрузки</param>
		/// <param name="edoAccountController">Контроллер согласия клиента на ЭДО</param>
		/// <param name="fileStorageService">Хранилище PDF счетов</param>
		public BillEdoRequestFactory(
			IOrderConverter orderConverter,
			IOrderWithoutShipmentConverter orderWithoutShipmentConverter,
			ICounterpartyEdoAccountController edoAccountController,
			IBillEdoDocumentFileStorageService fileStorageService)
		{
			_orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			_orderWithoutShipmentConverter = orderWithoutShipmentConverter ?? throw new ArgumentNullException(nameof(orderWithoutShipmentConverter));
			_edoAccountController = edoAccountController ?? throw new ArgumentNullException(nameof(edoAccountController));
			_fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
		}

		/// <summary>
		/// Создает заявку счета из сохраненного заказа
		/// </summary>
		/// <param name="order">Сохраненный заказ</param>
		/// <param name="type">Обычный или особый счет</param>
		/// <param name="pdf">PDF сохраненного счета без подписи и печати</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Новая заявка со снимком отправляемого счета</returns>
		public async Task<InformalEdoRequest> CreateAsync(Order order, OrderDocumentType type, byte[] pdf, CancellationToken cancellationToken = default)
		{
			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			var request = CreateOrderBillRequest(type);

			var document = order.OrderDocuments.FirstOrDefault(x => x.Type == type && x.Order.Id == order.Id)
				?? throw new InvalidOperationException("В заказе отсутствует выбранный счет");
			if(order.Id == 0 || document.Id == 0)
			{
				throw new InvalidOperationException("Перед созданием заявки необходимо сохранить заказ и счет");
			}

			ValidateConsent(order.Client, order.Contract.Organization.Id);
			var info = _orderConverter.ConvertOrderToOrderInfoForEdo(order, DocumentContainerType.Bill);
			var number = order.DeliveryDate >= new DateTime(2026, 1, 1)
				? document.DocumentOrganizationCounter?.DocumentNumber ?? order.Id.ToString()
				: order.Id.ToString();
			request.Order = order;
			request.SourceDocumentId = document.Id;
			request.DocumentSnapshot = await CreateSnapshotAsync(
				order.Id, number, document.DocumentDate.Value, info.OrderSum,
				info.ContractInfoForEdo.OrganizationInfoForEdo, info.CounterpartyInfoForEdo, pdf, cancellationToken);
			return request;
		}

		/// <summary>
		/// Создает заявку сохраненного счета без отгрузки
		/// </summary>
		/// <param name="bill">Счет на долг, предоплату или постоплату</param>
		/// <param name="pdf">PDF сохраненного счета без подписи и печати</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Новая заявка со снимком отправляемого счета</returns>
		public async Task<InformalEdoRequest> CreateAsync(OrderWithoutShipmentBase bill, byte[] pdf, CancellationToken cancellationToken = default)
		{
			if(bill == null)
			{
				throw new ArgumentNullException(nameof(bill));
			}

			ValidateConsent(bill.Client, bill.Organization.Id);
			OrderWithoutShipmentInfo info;
			InformalEdoRequest request;
			switch(bill)
			{
				case OrderWithoutShipmentForDebt debt:
					info = _orderWithoutShipmentConverter.ConvertOrderWithoutShipmentForDebtToOrderWithoutShipmentInfo(debt, bill.CreateDate.Value);
					request = new BillWithoutShipmentForDebtEdoRequest();
					break;
				case OrderWithoutShipmentForAdvancePayment advance:
					info = _orderWithoutShipmentConverter.ConvertOrderWithoutShipmentForAdvancePaymentToOrderWithoutShipmentInfo(advance, bill.CreateDate.Value);
					request = new BillWithoutShipmentForAdvancePaymentEdoRequest();
					break;
				case OrderWithoutShipmentForPayment payment:
					info = _orderWithoutShipmentConverter.ConvertOrderWithoutShipmentForPaymentToOrderWithoutShipmentInfo(payment, bill.CreateDate.Value);
					request = new BillWithoutShipmentForPaymentEdoRequest();
					break;
				default:
					throw new ArgumentException("Неизвестный тип счета без отгрузки", nameof(bill));
			}

			if(info.Id == 0)
			{
				throw new InvalidOperationException("Перед созданием заявки необходимо сохранить счет");
			}

			request.SourceDocumentId = info.Id;
			request.DocumentSnapshot = await CreateSnapshotAsync(
				0, info.BillNumber, info.CreationDate, info.Sum,
				info.OrganizationInfoForEdo, info.CounterpartyInfoForEdo, pdf, cancellationToken);
			return request;
		}

		/// <summary>
		/// Создает заявку, ожидающую подготовки PDF в воркере.
		/// </summary>
		/// <param name="type">Тип счета.</param>
		/// <param name="sourceDocumentId">Идентификатор сохраненного счета.</param>
		/// <param name="order">Заказ для обычного или особого счета.</param>
		/// <returns>Заявка без подготовленного снимка.</returns>
		public static InformalEdoRequest CreatePending(OrderDocumentType type, int sourceDocumentId, OrderEntity order = null)
		{
			if(sourceDocumentId <= 0)
			{
				throw new ArgumentException("Перед отправкой необходимо сохранить счет", nameof(sourceDocumentId));
			}

			InformalEdoRequest request;
			switch(type)
			{
				case OrderDocumentType.BillWSForDebt:
					request = new BillWithoutShipmentForDebtEdoRequest();
					break;
				case OrderDocumentType.BillWSForAdvancePayment:
					request = new BillWithoutShipmentForAdvancePaymentEdoRequest();
					break;
				case OrderDocumentType.BillWSForPayment:
					request = new BillWithoutShipmentForPaymentEdoRequest();
					break;
				default:
					request = CreateOrderBillRequest(type);
					break;
			}
			request.SourceDocumentId = sourceDocumentId;
			request.Order = order;
			return request;
		}

		private static InformalEdoRequest CreateOrderBillRequest(OrderDocumentType type)
		{
			switch(type)
			{
				case OrderDocumentType.Bill:
					return new OrderBillEdoRequest();
				case OrderDocumentType.SpecialBill:
					return new SpecialOrderBillEdoRequest();
				default:
					throw new ArgumentException("Требуется обычный или особый счет", nameof(type));
			}
		}

		private void ValidateConsent(Counterparty client, int organizationId)
		{
			var account = _edoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(client, organizationId);
			if(account == null || account.ConsentForEdoStatus != ConsentForEdoStatus.Agree
				|| string.IsNullOrWhiteSpace(account.PersonalAccountIdInEdo))
			{
				throw new InvalidOperationException("У клиента отсутствует согласие на ЭДО или номер кабинета по организации счета");
			}
		}

		private async Task<string> CreateSnapshotAsync(
			int orderId, string number, DateTime date, decimal sum,
			OrganizationInfoForEdo organization, CounterpartyInfoForEdo counterparty, byte[] pdf, CancellationToken cancellationToken)
		{
			if(pdf == null || pdf.Length == 0)
			{
				throw new ArgumentException("PDF счета пустой", nameof(pdf));
			}
			if(string.IsNullOrWhiteSpace(organization.TaxcomEdoAccountId))
			{
				throw new InvalidOperationException("У организации счета отсутствует кабинет Такском");
			}

			var documentId = Guid.NewGuid();
			var snapshot = new InfoForCreatingEdoInformalOrderDocument
			{
				MainDocumentId = documentId,
				IsBill = true,
				OrganizationInfoForEdo = new EdoParticipantInfo
				{
					Inn = organization.Inn,
					Kpp = organization.Kpp,
					OrganizationFullName = organization.FullName,
					TaxcomEdoAccountId = organization.TaxcomEdoAccountId
				},
				CounterpartyInfoForEdo = new EdoParticipantInfo
				{
					Inn = counterparty.Inn,
					Kpp = counterparty.Kpp,
					OrganizationFullName = counterparty.FullName,
					TaxcomEdoAccountId = counterparty.PersonalAccountIdInEdo
				},
				Sum = sum,
				FileData = new OrderDocumentFileData
				{
					OrderId = orderId,
					BillNumber = number,
					FileName = $"Счёт №{number} от {date:dd.MM.yyyy}.pdf",
					DocumentDate = date
				}
			};
			var serializedSnapshot = JsonSerializer.Serialize(snapshot);
			await _fileStorageService.SaveAsync(documentId, pdf, cancellationToken);
			return serializedSnapshot;
		}
	}
}
