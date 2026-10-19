using System;
using System.Linq;
using System.Text.Json;
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

		/// <summary>
		/// Создает фабрику снимков счетов
		/// </summary>
		/// <param name="orderConverter">Конвертер заказов</param>
		/// <param name="orderWithoutShipmentConverter">Конвертер счетов без отгрузки</param>
		/// <param name="edoAccountController">Контроллер согласия клиента на ЭДО</param>
		public BillEdoRequestFactory(
			IOrderConverter orderConverter,
			IOrderWithoutShipmentConverter orderWithoutShipmentConverter,
			ICounterpartyEdoAccountController edoAccountController)
		{
			_orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			_orderWithoutShipmentConverter = orderWithoutShipmentConverter ?? throw new ArgumentNullException(nameof(orderWithoutShipmentConverter));
			_edoAccountController = edoAccountController ?? throw new ArgumentNullException(nameof(edoAccountController));
		}

		/// <summary>
		/// Создает заявку счета из сохраненного заказа
		/// </summary>
		/// <param name="order">Сохраненный заказ</param>
		/// <param name="type">Обычный или особый счет</param>
		/// <param name="pdf">PDF сохраненного счета без подписи и печати</param>
		/// <returns>Новая заявка со снимком отправляемого счета</returns>
		public InformalEdoRequest Create(Order order, OrderDocumentType type, byte[] pdf)
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
			request.DocumentSnapshot = CreateSnapshot(
				order.Id, number, document.DocumentDate.Value, info.OrderSum,
				info.ContractInfoForEdo.OrganizationInfoForEdo, info.CounterpartyInfoForEdo, pdf);
			return request;
		}

		/// <summary>
		/// Создает заявку сохраненного счета без отгрузки
		/// </summary>
		/// <param name="bill">Счет на долг, предоплату или постоплату</param>
		/// <param name="pdf">PDF сохраненного счета без подписи и печати</param>
		/// <returns>Новая заявка со снимком отправляемого счета</returns>
		public InformalEdoRequest Create(OrderWithoutShipmentBase bill, byte[] pdf)
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
			request.DocumentSnapshot = CreateSnapshot(
				0, info.BillNumber, info.CreationDate, info.Sum,
				info.OrganizationInfoForEdo, info.CounterpartyInfoForEdo, pdf);
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

		private static string CreateSnapshot(
			int orderId, string number, DateTime date, decimal sum,
			OrganizationInfoForEdo organization, CounterpartyInfoForEdo counterparty, byte[] pdf)
		{
			if(pdf == null || pdf.Length == 0)
			{
				throw new ArgumentException("PDF счета пустой", nameof(pdf));
			}
			if(string.IsNullOrWhiteSpace(organization.TaxcomEdoAccountId))
			{
				throw new InvalidOperationException("У организации счета отсутствует кабинет Такском");
			}

			return JsonSerializer.Serialize(new InfoForCreatingEdoInformalOrderDocument
			{
				MainDocumentId = Guid.NewGuid(),
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
					DocumentDate = date,
					Image = pdf
				}
			});
		}
	}
}
