using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Stores
{
	public static partial class SelfDeliveryDocumentErrors
	{
		public static Error IsNotFullyShiped =>
			new Error(
				typeof(SelfDeliveryDocumentErrors),
				nameof(IsNotFullyShiped),
				"Отпуск самовывоза не может быть отгружен полностью");
	}
}
