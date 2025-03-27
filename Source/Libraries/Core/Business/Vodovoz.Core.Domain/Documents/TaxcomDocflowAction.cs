using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Действие с документооборотом
	/// </summary>
	public class TaxcomDocflowAction : PropertyChangedBase, IDomainObject
	{
		private int? _taxcomDocflowId;
		private DateTime _time;
		private EdoDocFlowStatus _state;
		private string _errorMessage;
		private bool _isReceived;

		public TaxcomDocflowAction()
		{
			State = EdoDocFlowStatus.NotStarted;
		}

		/// <summary>
		/// Id
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Id документооборота Такском
		/// </summary>
		public virtual int? TaxcomDocflowId
		{
			get => _taxcomDocflowId;
			set => SetField(ref _taxcomDocflowId, value);
		}

		/// <summary>
		/// Время
		/// </summary>
		public virtual DateTime Time
		{
			get => _time;
			set => SetField(ref _time, value);
		}

		/// <summary>
		/// Состояние
		/// </summary>
		public virtual EdoDocFlowStatus State
		{
			get => _state;
			set => SetField(ref _state, value);
		}

		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public virtual string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}

		/// <summary>
		/// Доставлено
		/// </summary>
		public virtual bool IsReceived
		{
			get => _isReceived;
			set => SetField(ref _isReceived, value);
		}
	}
}
