using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Logistics
{
	public static class DistrictSetErrors
	{
		public static Error NotFound(int id) =>
			new Error(
				typeof(DistrictSetErrors),
				nameof(NotFound),
				$"Версия районов #{id} не найдена");
	}
}
