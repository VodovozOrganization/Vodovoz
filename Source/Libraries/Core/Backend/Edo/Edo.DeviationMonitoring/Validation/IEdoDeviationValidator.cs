using Edo.DeviationMonitoring.Errors;
using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Общая часть контракта валидаторов отклонений документооборота ЭДО
	/// </summary>
	public interface IEdoDeviationValidator
	{
		/// <summary>
		/// Тип отклонения, которое фиксирует валидатор
		/// </summary>
		EdoDeviationType DeviationType { get; }

		/// <summary>
		/// Признак резервного валидатора: он фиксирует, что обработка встала,
		/// не разбираясь в причине.
		/// <para>
		/// Резервный валидатор работает, только когда к записи неприменим
		/// ни один частный валидатор. Иначе он перехватывал бы случаи
		/// с большим таймаутом: сервис фиксирует первое сработавшее отклонение,
		/// поэтому частное условие с порогом больше резервного
		/// не успело бы сработать никогда
		/// </para>
		/// </summary>
		bool IsFallback { get; }
	}

	/// <summary>
	/// Валидатор отклонения по записи определенного вида
	/// </summary>
	/// <typeparam name="TNode">Состояние проверяемой записи</typeparam>
	public interface IEdoDeviationValidator<TNode> : IEdoDeviationValidator
		where TNode : class
	{
		/// <summary>
		/// Признак того, что условие валидатора относится к записи:
		/// запись находится ровно в том состоянии, длительность которого он меряет.
		/// Применимость не означает срабатывания: таймаут может быть еще не превышен
		/// </summary>
		/// <param name="node">Состояние проверяемой записи</param>
		bool IsApplicable(TNode node);

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
		Result Validate(TNode node, EdoDeviationSource source, DateTime checkTime);
	}
}
