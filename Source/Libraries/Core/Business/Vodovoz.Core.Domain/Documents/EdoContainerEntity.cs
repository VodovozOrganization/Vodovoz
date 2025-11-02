using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Контейнер электронного документооборота
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "Контейнеры электронного документооборота",
		Nominative = "Контейнер электронного документооборота",
		Prepositional = "Контейнере электронного документооборота",
		PrepositionalPlural = "Контейнерах электронного документооборота",
		Accusative = "Контейнер электронного документооборота",
		AccusativePlural = "Контейнеры электронного документооборота",
		Genitive = "Контейнера электронного документооборота",
		GenitivePlural = "Контейнеров электронного документооборота")]
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
		private DocumentContainerType _type;

		/// <summary>
		/// Код контейнера
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Доставлено
		/// </summary>
		[Display(Name = "Доставлено")]
		public virtual bool Received
		{
			get => _received;
			set => SetField(ref _received, value);
		}

		/// <summary>
		/// Входящий? (true - входящий, false - исходящий)
		/// </summary>
		[Display(Name = "Входящий?")]
		public virtual bool IsIncoming
		{
			get => _isIncoming;
			set => SetField(ref _isIncoming, value);
		}

		/// <summary>
		/// Id главного документа
		/// </summary>
		[Display(Name = "Id главного документа")]
		public virtual string MainDocumentId
		{
			get => _mainDocumentId;
			set => SetField(ref _mainDocumentId, value);
		}

		/// <summary>
		/// Id документа в хранилище служб Такском
		/// </summary>
		[Display(Name = "Внутренний Id документа в хранилище служб Такском")]
		public virtual Guid? InternalId
		{
			get => _internalId;
			set => SetField(ref _internalId, value);
		}

		/// <summary>
		/// Id документооборота
		/// </summary>
		[Display(Name = "Id документооборота")]
		public virtual Guid? DocFlowId
		{
			get => _docFlowId;
			set => SetField(ref _docFlowId, value);
		}

		/// <summary>
		/// Заказ
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual CounterpartyEntity Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Статус документооборота
		/// </summary>
		[Display(Name = "Статус")]
		public virtual EdoDocFlowStatus EdoDocFlowStatus
		{
			get => _edoDocFlowStatus;
			set => SetField(ref _edoDocFlowStatus, value);
		}

		/// <summary>
		/// Описание ошибки
		/// </summary>
		[Display(Name = "Описание ошибки")]
		public virtual string ErrorDescription
		{
			get => _errorDescription;
			set => SetField(ref _errorDescription, value);
		}

		/// <summary>
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime Created
		{
			get => _created;
			set => SetField(ref _created, value);
		}

		/// <summary>
		/// Id задачи ЭДО
		/// </summary>
		[Display(Name = "Id задачи ЭДО")]
		public virtual int? EdoTaskId
		{
			get => _edoTaskId;
			set => SetField(ref _edoTaskId, value);
		}

		/// <summary>
		/// Тип документа
		/// </summary>
		[Display(Name = "Тип документа")]
		public virtual DocumentContainerType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		/// <summary>
		/// Отправленные документы
		/// </summary>
		[Display(Name = "Отправленные документы")]
		public virtual string SentDocuments => Type.GetEnumTitle();
	}
}
