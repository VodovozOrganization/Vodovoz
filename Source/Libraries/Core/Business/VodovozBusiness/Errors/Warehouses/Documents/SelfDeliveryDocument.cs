using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.Warehouses.Documents
{
	public static class SelfDeliveryDocument
	{
		public static Error NotFound
			=> new Error(
				typeof(SelfDeliveryDocument),
				nameof(NotFound),
				"Документ отпуска самовывоза не найден");
	}
}
