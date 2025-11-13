using System;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

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
		/// Список событий документооборота
		/// </summary>
		public virtual IObservableList<TaxcomDocflowAction> Actions { get; set; } = new ObservableList<TaxcomDocflowAction>();
	}
}
