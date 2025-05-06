using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Logistics
{
	public static class DistrictSet
	{
		public static Error NotFound(int id) =>
			new Error(
				typeof(DistrictSet),
				nameof(NotFound),
				$"Версия районов #{id} не найдена");
	}
}
