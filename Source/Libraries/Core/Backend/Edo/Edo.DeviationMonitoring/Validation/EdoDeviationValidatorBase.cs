using Edo.DeviationMonitoring.Errors;
using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Базовый валидатор отклонения по превышению таймаута стадии.
	/// Наследнику достаточно указать точку отсчета, текст описания
	/// и то, к какой записи привязывается отклонение;
	/// таймаут приходит из справочника описаний отклонений
	/// </summary>
	/// <typeparam name="TNode">Состояние проверяемой записи</typeparam>
	public abstract class EdoDeviationValidatorBase<TNode>
		where TNode : class
	{
		/// <summary>
		/// Тип отклонения, которое фиксирует валидатор
		/// </summary>
		public abstract EdoDeviationType DeviationType { get; }

		/// <summary>
		/// Признак того, что проверка имеет смысл и для завершенной задачи
		/// </summary>
		public virtual bool IsAppliesToFinishedTask => false;

		/// <inheritdoc/>
		public virtual bool IsFallback => false;

		/// <summary>
		/// Условие валидатора относится к записи, если у него есть точка отсчета:
		/// именно ее отсутствие и означает, что запись не в том состоянии,
		/// длительность которого меряет валидатор
		/// </summary>
		/// <param name="node">Состояние проверяемой записи</param>
		public virtual bool IsApplicable(TNode node)
		{
			if(node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			return GetStageStartTime(node) != null;
		}

		/// <summary>
		/// Проверяет состояние записи на отклонение
		/// </summary>
		/// <param name="node">Состояние проверяемой записи</param>
		/// <param name="source">Описание отклонения из справочника, откуда берется таймаут</param>
		/// <param name="checkTime">Время проверки</param>
		/// <returns>
		/// Успешный результат, если отклонения нет;
		/// иначе <see cref="EdoDeviationError"/> с подробностями отклонения
		/// </returns>
		public virtual Result Validate(
			TNode node,
			EdoDeviationSource source,
			DateTime checkTime)
		{
			if(node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			if(source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			var stageStartTime = GetStageStartTime(node);

			if(stageStartTime is null)
			{
				return Result.Success();
			}

			var elapsed = checkTime - stageStartTime.Value;

			if(elapsed <= source.Timeout)
			{
				return Result.Success();
			}

			var deviation = new EdoDeviationValidationResult
			{
				DeviationSourceId = source.Id,
				StageStartTime = stageStartTime.Value,
				Threshold = source.Timeout,
				Details = BuildDetails(node, source.Timeout, elapsed)
			};

			FillEntity(deviation, node);

			return new EdoDeviationError(DeviationType, deviation);
		}

		/// <summary>
		/// Возвращает момент, от которого отсчитывается таймаут,
		/// или <c>null</c>, если валидатор неприменим к записи
		/// </summary>
		/// <param name="node">Состояние проверяемой записи</param>
		protected abstract DateTime? GetStageStartTime(TNode node);

		/// <summary>
		/// Формирует описание, в какие условия не уложился документооборот
		/// </summary>
		/// <param name="node">Состояние проверяемой записи</param>
		/// <param name="timeout">Превышенный таймаут</param>
		/// <param name="elapsed">Фактическая длительность стадии</param>
		protected abstract string BuildDetails(TNode node, TimeSpan timeout, TimeSpan elapsed);

		/// <summary>
		/// Проставляет в результат привязку отклонения и название стадии
		/// </summary>
		/// <param name="result">Результат проверки</param>
		/// <param name="node">Состояние проверяемой записи</param>
		protected abstract void FillEntity(EdoDeviationValidationResult result, TNode node);
	}
}
