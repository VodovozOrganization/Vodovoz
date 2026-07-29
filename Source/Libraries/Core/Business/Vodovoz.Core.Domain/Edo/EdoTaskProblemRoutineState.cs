using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Состояние повторной обработки ЭДО-проблемы.
	/// </summary>
	public class EdoTaskProblemRoutineState : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private EdoTaskProblem _problem;
		private int _retryCount;
		private DateTime? _lastRetryTime;

		/// <summary>
		/// Код.
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// ЭДО-проблема.
		/// </summary>
		[Display(Name = "Проблема ЭДО")]
		public virtual EdoTaskProblem Problem
		{
			get => _problem;
			set => SetField(ref _problem, value);
		}

		/// <summary>
		/// Количество опубликованных попыток повторной обработки.
		/// </summary>
		[Display(Name = "Количество попыток")]
		public virtual int RetryCount
		{
			get => _retryCount;
			set => SetField(ref _retryCount, value);
		}

		/// <summary>
		/// Время последней опубликованной попытки повторной обработки.
		/// </summary>
		[Display(Name = "Время последней попытки")]
		public virtual DateTime? LastRetryTime
		{
			get => _lastRetryTime;
			set => SetField(ref _lastRetryTime, value);
		}
	}
}
