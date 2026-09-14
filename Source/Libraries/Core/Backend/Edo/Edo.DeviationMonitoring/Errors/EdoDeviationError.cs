using Edo.DeviationMonitoring.Validation;
using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;

namespace Edo.DeviationMonitoring.Errors
{
	/// <summary>
	/// Обнаруженное отклонение документооборота ЭДО
	/// </summary>
	public class EdoDeviationError : Error
	{
		/// <summary>
		/// Создает ошибку по обнаруженному отклонению
		/// </summary>
		/// <param name="deviationType">Тип отклонения, он же различает коды ошибок</param>
		/// <param name="deviation">Обнаруженное отклонение документооборота ЭДО</param>
		public EdoDeviationError(EdoDeviationType deviationType, EdoDeviationValidationResult deviation)
			: base(typeof(EdoDeviationError), deviationType.ToString(), GetMessage(deviation))
		{
			Deviation = deviation;
		}

		/// <summary>
		/// Обнаруженное отклонение документооборота ЭДО
		/// </summary>
		public EdoDeviationValidationResult Deviation { get; }

		private static string GetMessage(EdoDeviationValidationResult deviation)
		{
			if(deviation is null)
			{
				throw new ArgumentNullException(nameof(deviation));
			}

			return deviation.Details;
		}
	}
}
