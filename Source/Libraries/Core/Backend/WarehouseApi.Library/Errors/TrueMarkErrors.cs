using Vodovoz.Core.Domain.Results;

namespace WarehouseApi.Library.Errors
{
	public static class TrueMarkErrors
	{
		public static Error TrueMarkCodeAlreadyExists
			=> new Error(
				typeof(TrueMarkErrors),
				nameof(TrueMarkCodeAlreadyExists),
				"Код уже используется");
	}
}
