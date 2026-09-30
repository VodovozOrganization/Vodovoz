using System;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Валидатор отклонения вместе с его описанием из справочника
	/// </summary>
	/// <typeparam name="TValidator">Вид валидатора отклонений</typeparam>
	public class EdoDeviationValidation<TValidator>
		where TValidator : class
	{
		public EdoDeviationValidation(TValidator validator, EdoDeviationSource source)
		{
			Validator = validator ?? throw new ArgumentNullException(nameof(validator));
			Source = source ?? throw new ArgumentNullException(nameof(source));
		}

		/// <summary>
		/// Валидатор
		/// </summary>
		public TValidator Validator { get; }

		/// <summary>
		/// Описание отклонения из справочника
		/// </summary>
		public EdoDeviationSource Source { get; }
	}
}
