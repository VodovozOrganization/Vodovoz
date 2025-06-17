using Vodovoz.Core.Domain.Results;

namespace ScannedTrueMarkCodesDelayedProcessing.Library.Errors
{
	internal static class DriversScannedCodes
	{
		public static Error AddingCodeToRouteListAddressError => new Error(
			typeof(DriversScannedCodes),
			nameof(AddingCodeToRouteListAddressError),
			"Ошибка при добавлении кода к адресу МЛ");
	}
}
