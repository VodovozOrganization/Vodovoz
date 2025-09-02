using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	public static partial class OrderErrors
	{
		public static class Accept
		{
			// Должно быть readonly добавить при обновлении языка на 8ю версию
			public static Error AcceptError =>
				new Error(
					typeof(Accept),
					nameof(AcceptError),
					"Во время подтверждения заказа произошла ошибка.");

			public static Error HasNoDefaultWater =>
				new Error(
					typeof(Accept),
					nameof(HasNoDefaultWater),
					"В заказе среди воды нет воды по умолчанию");
		}
	}
}
