using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Employees
{
	public static class DriverErrors
	{
		/// <summary>
		/// Водитель не найден
		/// </summary>
		public static Error NotFound =>
			new Error(typeof(DriverErrors),
				nameof(NotFound),
				"Водитель не найден");

		/// <summary>
		/// Водитель не может принимать звонки от контрагента
		/// </summary>
		public static Error CantRecieveCounterpartyCalls =>
			new Error(typeof(DriverErrors),
				nameof(CantRecieveCounterpartyCalls),
				"Водитель не может принимать звонки от контрагента");
	}
}
