namespace Vodovoz.Errors.Employees
{
	public static class Driver
	{
		/// <summary>
		/// Водитель не найден
		/// </summary>
		public static Error NotFound =>
			new Error(typeof(Driver),
				nameof(NotFound),
				"Водитель не найден");

		/// <summary>
		/// Водитель не может принимать звонки от контрагента
		/// </summary>
		public static Error CantRecieveCounterpartyCalls =>
			new Error(typeof(Driver),
				nameof(CantRecieveCounterpartyCalls),
				"Водитель не может принимать звонки от контрагента");
	}
}
