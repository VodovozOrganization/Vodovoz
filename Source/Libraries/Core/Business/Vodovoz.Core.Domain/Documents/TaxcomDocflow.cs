using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Документооборот в Такском
	/// </summary>
	public class TaxcomDocflow : PropertyChangedBase, IDomainObject
	{
		private DateTime _creationTime;
		private DateTime? _acceptingIngoingDocflowTime;
		private string _mainDocumentId;
		private Guid? _docflowId;
		private int _edoDocumentId;
		private bool _isReceived;
		private bool _isReminderToAcceptUpdEmailSent;
		private string _correctionNoticeMessage;
		private string _cancellationOfferMessage;
		private string _discrepancyMessage;
		private EdoDocFlowStatus _docFlowState = EdoDocFlowStatus.NotStarted;
		private DateTime? _stateChangeTime;
		private string _errorMessage;
		private TrueMarkTraceabilityStatus? _trueMarkTraceabilityStatus;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Время создания
		/// </summary>
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}
		
		/// <summary>
		/// Время отправки запроса на принятие входящего документооборота
		/// </summary>
		public virtual DateTime? AcceptingIngoingDocflowTime
		{
			get => _acceptingIngoingDocflowTime;
			set => SetField(ref _acceptingIngoingDocflowTime, value);
		}

		/// <summary>
		/// Внутренний идентификатор главного документа
		/// </summary>
		public virtual string MainDocumentId
		{
			get => _mainDocumentId;
			set => SetField(ref _mainDocumentId, value);
		}

		/// <summary>
		/// Идентификатор документооборота на сервере Такском
		/// </summary>
		public virtual Guid? DocflowId
		{
			get => _docflowId;
			set => SetField(ref _docflowId, value);
		}

		/// <summary>
		/// Идентификатор документа ЭДО
		/// </summary>
		public virtual int EdoDocumentId
		{
			get => _edoDocumentId;
			set => SetField(ref _edoDocumentId, value);
		}

		/// <summary>
		/// Доставлено
		/// </summary>
		public virtual bool IsReceived
		{
			get => _isReceived;
			set => SetField(ref _isReceived, value);
		}

		/// <summary>
		/// Напоминание о принятии входящего документооборота отправлено
		/// </summary>
		public virtual bool IsReminderToAcceptUpdEmailSent 
		{
			get => _isReminderToAcceptUpdEmailSent;
			set => SetField(ref _isReminderToAcceptUpdEmailSent, value);
		}

		/// <summary>
		/// Сообщение из уведомления об уточнении
		/// </summary>
		public virtual string CorrectionNoticeMessage
		{
			get => _correctionNoticeMessage;
			set => SetField(ref _correctionNoticeMessage, value);
		}

		/// <summary>
		/// Сообщение из предложения об аннулировании
		/// </summary>
		public virtual string CancellationOfferMessage
		{
			get => _cancellationOfferMessage;
			set => SetField(ref _cancellationOfferMessage, value);
		}

		/// <summary>
		/// Сообщение из документооборота принятного с расхождениями
		/// </summary>
		public virtual string DiscrepancyMessage
		{
			get => _discrepancyMessage;
			set => SetField(ref _discrepancyMessage, value);
		}

		/// <summary>
		/// Текущее состояние документооборота
		/// </summary>
		public virtual EdoDocFlowStatus DocFlowState
		{
			get => _docFlowState;
			set => SetField(ref _docFlowState, value);
		}

		/// <summary>
		/// Время изменения текущего состояния
		/// </summary>
		public virtual DateTime? StateChangeTime
		{
			get => _stateChangeTime;
			set => SetField(ref _stateChangeTime, value);
		}

		/// <summary>
		/// Сообщение об ошибке текущего состояния
		/// </summary>
		public virtual string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}

		/// <summary>
		/// Статус прослеживаемости в ЧЗ текущего состояния
		/// </summary>
		public virtual TrueMarkTraceabilityStatus? TrueMarkTraceabilityStatus
		{
			get => _trueMarkTraceabilityStatus;
			set => SetField(ref _trueMarkTraceabilityStatus, value);
		}
	}
}
