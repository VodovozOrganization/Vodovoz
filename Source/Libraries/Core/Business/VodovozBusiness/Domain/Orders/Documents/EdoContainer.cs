using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Client;

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
		private Counterparty _counterparty;
		private byte[] _container;
		private EdoDocFlowStatus _edoDocFlowStatus;
		private DateTime _created;
		private Type _type;
		
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
		
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		
		[Display(Name = "Контейнер с документами для ЭДО")]
		public virtual byte[] Container
		{
			get => _container;
			set => SetField(ref _container, value);
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

	public enum EdoDocFlowStatus
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "В процессе")]
		InProgress,
		[Display(Name = "Документооборот завершен успешно")]
		Succeed,
		[Display(Name = "Предупреждение")]
		Warning,
		[Display(Name = "Ошибка")]
		Error,
		[Display(Name = "Не начат")]
		NotStarted,
		[Display(Name = "Завершен с различиями")]
		CompletedWithDivergences,
		[Display(Name = "Не принят")]
		NotAccepted
	}
	public enum Type
	{
		[Display(Name = "УПД")]
		Upd,
		[Display(Name = "Счёт")]
		Bill
	}
}
