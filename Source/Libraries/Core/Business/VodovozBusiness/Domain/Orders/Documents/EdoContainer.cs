using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using MySqlX.XDevAPI.Common;
using NLog.LayoutRenderers;
using NPOI.SS.Formula.PTG;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.Orders.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Контейнеры электронного документооборота",
		Nominative = "Контейнер электронного документооборота",
		Prepositional = "Контейнере электронного документооборота",
		PrepositionalPlural = "Контейнерах электронного документооборота")]
	[HistoryTrace]
	public class EdoContainer : PropertyChangedBase, IDomainObject
	{
		private bool _received;
		private bool _isIncoming;
		private string _mainDocumentId;
		private string _errorDescription;
		private Guid? _internalId;
		private Guid? _docFlowId;
		private Order _order;
		private OrderWithoutShipmentForAdvancePayment _orderWithoutShipmentForAdvancePayment;
		private OrderWithoutShipmentForDebt _orderWithoutShipmentForDebt;
		private OrderWithoutShipmentForPayment _orderWithoutShipmentForPayment;
		private Counterparty _counterparty;
		private EdoDocFlowStatus _edoDocFlowStatus;
		private DateTime _created;
		private Type _type;
		
		//protected EdoContainer() { }

		public virtual int Id { get; set; }

		[Display(Name = "Доставлено")]
		public virtual bool Received
		{
			get => _received;
			set => SetField(ref _received, value);
		}
		
		[Display(Name = "Входящий?")]
		public virtual bool IsIncoming
		{
			get => _isIncoming;
			set => SetField(ref _isIncoming, value);
		}
		
		[Display(Name = "Id главного документа")]
		public virtual string MainDocumentId
		{
			get => _mainDocumentId;
			set => SetField(ref _mainDocumentId, value);
		}
		
		[Display(Name = "Внутренний Id документа в хранилище служб Такском")]
		public virtual Guid? InternalId
		{
			get => _internalId;
			set => SetField(ref _internalId, value);
		}
		
		[Display(Name = "Id документооборота")]
		public virtual Guid? DocFlowId
		{
			get => _docFlowId;
			set => SetField(ref _docFlowId, value);
		}
		
		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Счет без отгрузки на предоплату")]
		public virtual OrderWithoutShipmentForAdvancePayment OrderWithoutShipmentForAdvancePayment
		{
			get => _orderWithoutShipmentForAdvancePayment;
			set => SetField(ref _orderWithoutShipmentForAdvancePayment, value);
		}

		[Display(Name = "Cчет без отгрузки на долг")]
		public virtual OrderWithoutShipmentForDebt OrderWithoutShipmentForDebt
		{
			get => _orderWithoutShipmentForDebt;
			set => SetField(ref _orderWithoutShipmentForDebt, value);
		}

		[Display(Name = "Cчет без отгрузки на постоплату")]
		public virtual OrderWithoutShipmentForPayment OrderWithoutShipmentForPayment
		{
			get => _orderWithoutShipmentForPayment;
			set => SetField(ref _orderWithoutShipmentForPayment, value);
		}

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		
		[Display(Name = "Статус")]
		public virtual EdoDocFlowStatus EdoDocFlowStatus
		{
			get => _edoDocFlowStatus;
			set => SetField(ref _edoDocFlowStatus, value);
		}
		
		[Display(Name = "Описание ошибки")]
		public virtual string ErrorDescription
		{
			get => _errorDescription;
			set => SetField(ref _errorDescription, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime Created
		{
			get => _created;
			set => SetField(ref _created, value);
		}

		public virtual Type Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		[Display(Name = "Отправленные документы")]
		public virtual string SentDocuments => Type.GetEnumTitle();
	}

	/// <summary>
	/// WS - используется потому, что enum в NHibernate не может быть более 36 символов для 1 значения
	/// </summary>
	public enum Type
	{
		[Display(Name = "УПД")]
		Upd,
		[Display(Name = "Счёт")]
		Bill,
		[Display(Name = "Счет без отгрузки на предоплату")]
		BillWSForAdvancePayment,
		[Display(Name = "Cчет без отгрузки на долг")]
		BillWSForDebt,
		[Display(Name = "Cчет без отгрузки на постоплату")]
		BillWSForPayment
	}

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
		
		public EdoContainerBuilder EmptyContainer()
		{
			_edoContainer.Container = new byte[64];
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
