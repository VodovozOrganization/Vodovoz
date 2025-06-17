using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Clients
{
	public static partial class DeliveryPoint
	{
		/// <summary>
		/// Точка доставки не найдена
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(DeliveryPoint),
				nameof(NotFound),
				"Точка доставки не найдена");
	}
}
