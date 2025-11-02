using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;

namespace Vodovoz.Application.Documents
{
	/// <summary>
	/// Класс для создания контейнеров по ЭДО
	/// </summary>
	public class EdoContainerBuilder
	{
		private readonly EdoContainer _edoContainer;
		
		private EdoContainerBuilder()
		{
			_edoContainer = new EdoContainer();
		}
		
		/// <summary>
		/// Установка статуса "Не начат" контейнеру <see cref="EdoDocFlowStatus"/>
		/// </summary>
		/// <returns></returns>
		public EdoContainerBuilder Empty()
		{
			_edoContainer.EdoDocFlowStatus = EdoDocFlowStatus.NotStarted;
			return this;
		}

		/// <summary>
		/// Установка нужных параметров для контейнера по отправке УПД
		/// <see cref="DocumentContainerType.Upd"/>
		/// </summary>
		/// <param name="order">Заказ, по которому отправляется УПД</param>
		/// <returns></returns>
		public EdoContainerBuilder OrderUpd(Order order)
		{
			SetEdoContainerType(DocumentContainerType.Upd);
			SetOrder(order);
			return this;
		}
		
		/// <summary>
		/// Установка нужных параметров для контейнера по отправке Счета
		/// <see cref="DocumentContainerType.Bill"/>
		/// </summary>
		/// <param name="order">Заказ, по которому отправляется Счет</param>
		/// <returns></returns>
		public EdoContainerBuilder OrderBill(Order order)
		{
			SetEdoContainerType(DocumentContainerType.Bill);
			SetOrder(order);
			return this;
		}
		
		/// <summary>
		/// Установка нужных параметров для контейнера по отправке Счета на долг
		/// <see cref="DocumentContainerType.BillWSForDebt"/>
		/// </summary>
		/// <param name="orderWithoutShipment">Счет на долг</param>
		/// <returns></returns>
		public EdoContainerBuilder BillWithoutShipmentForDebt(OrderWithoutShipmentForDebt orderWithoutShipment)
		{
			SetEdoContainerType(DocumentContainerType.BillWSForDebt);
			_edoContainer.OrderWithoutShipmentForDebt = orderWithoutShipment;
			SetCounterparty(orderWithoutShipment.Counterparty);
			return this;
		}
		
		/// <summary>
		/// Установка нужных параметров для контейнера по отправке Счета на постоплату
		/// <see cref="DocumentContainerType.BillWSForPayment"/>
		/// </summary>
		/// <param name="orderWithoutShipment">Счет на постоплату</param>
		/// <returns></returns>
		public EdoContainerBuilder BillWithoutShipmentForPayment(OrderWithoutShipmentForPayment orderWithoutShipment)
		{
			SetEdoContainerType(DocumentContainerType.BillWSForPayment);
			_edoContainer.OrderWithoutShipmentForPayment = orderWithoutShipment;
			SetCounterparty(orderWithoutShipment.Counterparty);
			return this;
		}
		
		/// <summary>
		/// Установка нужных параметров для контейнера по отправке Счета на предоплату
		/// <see cref="DocumentContainerType.BillWSForAdvancePayment"/>
		/// </summary>
		/// <param name="orderWithoutShipment">Счет на предоплату</param>
		/// <returns></returns>
		public EdoContainerBuilder BillWithoutShipmentForAdvancePayment(OrderWithoutShipmentForAdvancePayment orderWithoutShipment)
		{
			SetEdoContainerType(DocumentContainerType.BillWSForAdvancePayment);
			_edoContainer.OrderWithoutShipmentForAdvancePayment = orderWithoutShipment;
			SetCounterparty(orderWithoutShipment.Counterparty);
			return this;
		}
		
		/// <summary>
		/// Установка Id главного документа в контейнере
		/// </summary>
		/// <param name="mainDocumentId">Id главного документа</param>
		/// <returns></returns>
		public EdoContainerBuilder MainDocumentId(string mainDocumentId)
		{
			_edoContainer.MainDocumentId = mainDocumentId;
			return this;
		}

		/// <summary>
		/// Возврат созданног контейнера
		/// </summary>
		/// <returns></returns>
		public EdoContainer Build()
		{
			_edoContainer.Created = DateTime.Now;
			return _edoContainer;
		}

		public static EdoContainerBuilder Create()
			=> new EdoContainerBuilder();

		private void SetOrder(Order order)
		{
			_edoContainer.Order = order;
			SetCounterparty(order.Client);
		}

		private void SetCounterparty(Counterparty counterparty)
		{
			_edoContainer.Counterparty = counterparty;
		}

		private void SetEdoContainerType(DocumentContainerType edoContainerType) => _edoContainer.Type = edoContainerType;
	}
}
