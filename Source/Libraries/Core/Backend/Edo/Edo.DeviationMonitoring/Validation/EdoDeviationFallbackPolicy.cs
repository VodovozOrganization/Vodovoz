using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Правило работы резервных валидаторов отклонений
	/// <para>
	/// Сервис фиксирует первое сработавшее отклонение, поэтому решает не порядок
	/// объявления валидаторов, а то, чей таймаут истек раньше. Без отдельного правила
	/// резервный валидатор с коротким порогом перехватывал бы все частные условия
	/// с порогом длиннее своего, и они не срабатывали бы никогда
	/// </para>
	/// </summary>
	public static class EdoDeviationFallbackPolicy
	{
		/// <summary>
		/// Проверяет, применим ли к записи хотя бы один частный валидатор
		/// Пока такой есть, резервный валидатор по записи не работает:
		/// стадия документооборота известна, и ее длительность меряет частное условие
		/// </summary>
		/// <param name="logger">Журнал, куда пишутся сбои проверки применимости</param>
		/// <param name="node">Состояние проверяемой записи</param>
		/// <param name="validations">Валидаторы с описаниями из справочника</param>
		/// <param name="entityId">Код проверяемой записи</param>
		/// <typeparam name="TNode">Состояние проверяемой записи</typeparam>
		/// <typeparam name="TValidator">Вид валидатора отклонений</typeparam>
		public static bool HasApplicableSpecificValidator<TNode, TValidator>(
			ILogger logger,
			TNode node,
			IReadOnlyList<EdoDeviationValidation<TValidator>> validations,
			int entityId)
			where TNode : class
			where TValidator : class, IEdoDeviationValidator<TNode>
		{
			if(node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			if(validations is null)
			{
				throw new ArgumentNullException(nameof(validations));
			}

			if(logger is null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			foreach(var validation in validations)
			{
				if(validation.Validator.IsFallback)
				{
					continue;
				}

				try
				{
					if(validation.Validator.IsApplicable(node))
					{
						return true;
					}
				}
				catch(Exception ex)
				{
					logger.LogError(ex,
						"Ошибка проверки применимости валидатора {DeviationType} по записи {EntityId}",
						validation.Validator.DeviationType,
						entityId);
				}
			}

			return false;
		}
	}
}
