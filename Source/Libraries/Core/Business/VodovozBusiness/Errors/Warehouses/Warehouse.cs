using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.Warehouses
{
	public static class Warehouse
	{
		public static Error NotFound
			=> new Error(
				typeof(Warehouse),
				nameof(NotFound),
				"Склад не найден");
	}
}
