namespace Vodovoz.Errors.Employees
{
	public static class Driver
	{
		public static Error NotFound =>
			new Error(typeof(Driver),
				nameof(NotFound),
				"Водитель не найден");
	}
}
