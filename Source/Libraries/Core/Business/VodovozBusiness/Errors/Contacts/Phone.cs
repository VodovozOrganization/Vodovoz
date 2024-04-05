namespace Vodovoz.Errors.Contacts
{
	public static class Phone
	{
		public static Error NotFound =>
			new Error(
				typeof(Phone),
				nameof(NotFound),
				"Телефон не найден");
	}
}
