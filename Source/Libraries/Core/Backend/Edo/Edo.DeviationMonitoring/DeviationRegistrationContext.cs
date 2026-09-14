using Edo.DeviationMonitoring.Validation;
using System;

namespace Edo.DeviationMonitoring
{
	/// <summary>
	/// Состояние одного прохода регистрации отклонений документооборота ЭДО
	/// </summary>
	internal class DeviationRegistrationContext
	{
		public DeviationRegistrationContext(EdoDeviationValidators validators, DateTime checkTime)
		{
			Validators = validators ?? throw new ArgumentNullException(nameof(validators));
			CheckTime = checkTime;
			Result = new EdoDeviationRegistrationResult();
		}

		/// <summary>
		/// Валидаторы, включенные в справочнике на момент начала прохода
		/// </summary>
		public EdoDeviationValidators Validators { get; }

		/// <summary>
		/// Время, на которое проверяются таймауты
		/// </summary>
		public DateTime CheckTime { get; }

		/// <summary>
		/// Итог прохода
		/// </summary>
		public EdoDeviationRegistrationResult Result { get; }
	}
}
