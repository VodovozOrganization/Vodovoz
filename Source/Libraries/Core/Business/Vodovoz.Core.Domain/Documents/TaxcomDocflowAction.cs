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
		private EdoDocFlowStatus _docFlowState;
		private TrueMarkTraceabilityStatus? _trueMarkTraceabilityStatus;
		private string _errorMessage;

		public TaxcomDocflowAction()
		{
			DocFlowState = EdoDocFlowStatus.NotStarted;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Идентификатор документооборота Такском
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
		/// Состояние документооборота
		/// </summary>
		public virtual EdoDocFlowStatus DocFlowState
		{
			get => _docFlowState;
			set => SetField(ref _docFlowState, value);
		}
		
		/// <summary>
		/// Статус прослеживаемости в ЧЗ
		/// </summary>
		public virtual TrueMarkTraceabilityStatus? TrueMarkTraceabilityStatus
		{
			get => _trueMarkTraceabilityStatus;
			set => SetField(ref _trueMarkTraceabilityStatus, value);
		}

		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public virtual string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}
	}
}
