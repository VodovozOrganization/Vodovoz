using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Отклонение документооборота ЭДО от ожидаемого хода обработки
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "отклонение документооборота ЭДО",
		NominativePlural = "отклонения документооборота ЭДО"
	)]
	public class EdoTaskDeviation : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private EdoTask _edoTask;
		private FormalEdoRequest _edoRequest;
		private EdoDeviationSource _deviationSource;
		private string _stageName;
		private DateTime _stageStartTime;
		private TimeSpan _threshold;
		private string _details;
		private TaskProblemState _state;
		private DateTime _detectedTime;
		private DateTime? _resolvedTime;
		private EdoDeviationResolveReason? _resolveReason;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Задача ЭДО, по которой зафиксировано отклонение
		/// </summary>
		[Display(Name = "Задача")]
		public virtual EdoTask EdoTask
		{
			get => _edoTask;
			set => SetField(ref _edoTask, value);
		}

		/// <summary>
		/// Заявка ЭДО, по которой зафиксировано отклонение
		/// </summary>
		[Display(Name = "Заявка")]
		public virtual FormalEdoRequest EdoRequest
		{
			get => _edoRequest;
			set => SetField(ref _edoRequest, value);
		}

		/// <summary>
		/// Источник отклонения, описывающий тип отклонения и условия его срабатывания
		/// </summary>
		[Display(Name = "Источник отклонения")]
		public virtual EdoDeviationSource DeviationSource
		{
			get => _deviationSource;
			set => SetField(ref _deviationSource, value);
		}

		/// <summary>
		/// Название стадии, на которой зафиксировано отклонение
		/// </summary>
		[Display(Name = "Стадия")]
		public virtual string StageName
		{
			get => _stageName;
			set => SetField(ref _stageName, value);
		}

		/// <summary>
		/// Время, от которого отсчитывался таймаут стадии
		/// </summary>
		[Display(Name = "Начало стадии")]
		public virtual DateTime StageStartTime
		{
			get => _stageStartTime;
			set => SetField(ref _stageStartTime, value);
		}

		/// <summary>
		/// Превышенный таймаут
		/// </summary>
		[Display(Name = "Таймаут")]
		public virtual TimeSpan Threshold
		{
			get => _threshold;
			set => SetField(ref _threshold, value);
		}

		/// <summary>
		/// Детали отклонения
		/// </summary>
		[Display(Name = "Детали отклонения")]
		public virtual string Details
		{
			get => _details;
			set => SetField(ref _details, value);
		}

		/// <summary>
		/// Состояние отклонения
		/// </summary>
		[Display(Name = "Состояние")]
		public virtual TaskProblemState State
		{
			get => _state;
			set => SetField(ref _state, value);
		}

		/// <summary>
		/// Время обнаружения отклонения
		/// </summary>
		[Display(Name = "Время обнаружения")]
		public virtual DateTime DetectedTime
		{
			get => _detectedTime;
			set => SetField(ref _detectedTime, value);
		}

		/// <summary>
		/// Время, когда отклонение перестало обнаруживаться
		/// </summary>
		[Display(Name = "Время закрытия")]
		public virtual DateTime? ResolvedTime
		{
			get => _resolvedTime;
			set => SetField(ref _resolvedTime, value);
		}

		/// <summary>
		/// Причина снятия отклонения
		/// </summary>
		[Display(Name = "Причина снятия")]
		public virtual EdoDeviationResolveReason? ResolveReason
		{
			get => _resolveReason;
			set => SetField(ref _resolveReason, value);
		}

		/// <summary>
		/// Помечает отклонение снятым
		/// </summary>
		/// <param name="resolvedTime">Время снятия</param>
		/// <param name="resolveReason">Причина снятия</param>
		public virtual void Resolve(DateTime resolvedTime, EdoDeviationResolveReason resolveReason)
		{
			State = TaskProblemState.Solved;
			ResolvedTime = resolvedTime;
			ResolveReason = resolveReason;
		}
	}
}
