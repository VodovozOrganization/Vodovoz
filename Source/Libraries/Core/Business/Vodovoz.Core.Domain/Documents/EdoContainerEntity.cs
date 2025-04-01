using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Контейнеры электронного документооборота",
		Nominative = "Контейнер электронного документооборота",
		Prepositional = "Контейнере электронного документооборота",
		PrepositionalPlural = "Контейнерах электронного документооборота")]
	[HistoryTrace]
	public class EdoContainerEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private bool _received;
		private bool _isIncoming;
		private string _mainDocumentId;
		private string _errorDescription;
		private Guid? _internalId;
		private Guid? _docFlowId;
		private OrderEntity _order;
		private CounterpartyEntity _counterparty;
		private EdoDocFlowStatus _edoDocFlowStatus;
		private DateTime _created;
		private int? _edoTaskId;
		private Type _type;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}
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
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Контрагент")]
		public virtual CounterpartyEntity Counterparty
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

		public virtual int? EdoTaskId
		{
			get => _edoTaskId;
			set => SetField(ref _edoTaskId, value);
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
}
