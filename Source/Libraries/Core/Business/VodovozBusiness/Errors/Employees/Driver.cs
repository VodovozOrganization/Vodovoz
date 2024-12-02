namespace Vodovoz.Errors.Employees
{
	public static class Driver
	{
		public static Error NotFound =>
			new Error(typeof(Driver),
				nameof(NotFound),
				"Водитель не найден");

		public static Error CantRecieveCounterpartyCalls =>
			new Error(typeof(Driver),
				nameof(CantRecieveCounterpartyCalls),
				"Водитель не может принимать звонки от контрагента");
	}
}
