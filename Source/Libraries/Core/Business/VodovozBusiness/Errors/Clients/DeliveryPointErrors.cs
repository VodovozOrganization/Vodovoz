using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Clients
{
	public static partial class DeliveryPointErrors
	{
		/// <summary>
		/// Точка доставки не найдена
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(DeliveryPointErrors),
				nameof(NotFound),
				"Точка доставки не найдена");
	}
}
