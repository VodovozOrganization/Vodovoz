using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.Errors
{
	public static partial class Security
	{
		/// <summary>
		/// Ошибки авторизации
		/// </summary>
		public static class Authorization
		{
			/// <summary>
			/// Доступ к маршрутному листу запрещен
			/// У запрашивающего доступ к маршрутному листу нет для этого прав
			/// </summary>
			public static Error RouteListAccessDenied =>
				new Error(typeof(Authorization),
					nameof(RouteListAccessDenied),
					"Доступ к маршрутному листу запрещен");

			/// <summary>
			/// Доступ к заказу
			/// У запрашивающего доступ к заказу нет для этого прав
			/// </summary>
			public static Error OrderAccessDenied =>
				new Error(typeof(Authorization),
					nameof(OrderAccessDenied),
					"Доступ к заказу запрещен");
		}
	}
}
