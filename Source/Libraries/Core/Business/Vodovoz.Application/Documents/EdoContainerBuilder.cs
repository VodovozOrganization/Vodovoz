using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Type = Vodovoz.Domain.Orders.Documents.Type;

namespace Vodovoz.Application.Documents
{
	public class EdoContainerBuilder
	{
		private readonly EdoContainer _edoContainer;
		
		private EdoContainerBuilder()
		{
			_edoContainer = new EdoContainer();
		}
		
		public EdoContainerBuilder Empty()
		{
			_edoContainer.EdoDocFlowStatus = EdoDocFlowStatus.NotStarted;
			return this;
		}
		
		public EdoContainerBuilder OrderUpd(Order order)
		{
			SetEdoContainerType(Type.Upd);
			SetOrder(order);
			return this;
		}
		
		public EdoContainerBuilder OrderBill(Order order)
		{
			SetEdoContainerType(Type.Bill);
			SetOrder(order);
			return this;
		}
		
		public EdoContainerBuilder BillWithoutShipmentForDebt(OrderWithoutShipmentForDebt orderWithoutShipment)
		{
			SetEdoContainerType(Type.BillWSForDebt);
			_edoContainer.OrderWithoutShipmentForDebt = orderWithoutShipment;
			SetCounterparty(orderWithoutShipment.Counterparty);
			return this;
		}
		
		public EdoContainerBuilder BillWithoutShipmentForPayment(OrderWithoutShipmentForPayment orderWithoutShipment)
		{
			SetEdoContainerType(Type.BillWSForPayment);
			_edoContainer.OrderWithoutShipmentForPayment = orderWithoutShipment;
			SetCounterparty(orderWithoutShipment.Counterparty);
			return this;
		}
		
		public EdoContainerBuilder BillWithoutShipmentForAdvancePayment(OrderWithoutShipmentForAdvancePayment orderWithoutShipment)
		{
			SetEdoContainerType(Type.BillWSForAdvancePayment);
			_edoContainer.OrderWithoutShipmentForAdvancePayment = orderWithoutShipment;
			SetCounterparty(orderWithoutShipment.Counterparty);
			return this;
		}
		
		public EdoContainerBuilder MainDocumentId(string mainDocumentId)
		{
			_edoContainer.MainDocumentId = mainDocumentId;
			return this;
		}

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

		private void SetEdoContainerType(Type edoContainerType) => _edoContainer.Type = edoContainerType;
	}
}
