using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.Errors
{
	internal static class TrueMark
	{
		public static Error AggregatedCodeRemovalAttempt => new Error(
			typeof(TrueMark),
			nameof(AggregatedCodeRemovalAttempt),
			"Нельзя удалить код, участвующий в аггрегации");

		public static Error AggregatedCodeChangeAttempt => new Error(
			typeof(TrueMark),
			nameof(AggregatedCodeChangeAttempt),
			"Нельзя изменить код, участвующий в аггрегации");

		public static Error ToAggregatedCodeChangeAttempt => new Error(
			typeof(TrueMark),
			nameof(ToAggregatedCodeChangeAttempt),
			"Нельзя изменить код на код, участвующий в аггрегации");

		public static Error CodesAlreadyInUse => new Error(
			typeof(TrueMark),
			nameof(CodesAlreadyInUse),
			"Отсканированные коды уже использованы");
	}
}
