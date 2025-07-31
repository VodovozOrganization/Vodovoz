namespace Vodovoz.Errors.Orders
{
	public static partial class FixedPrice
	{
		public static Error NotFound =>
			new Error(
				typeof(FixedPrice),
				nameof(NotFound),
				"Фикса не найдена");
	}
}
