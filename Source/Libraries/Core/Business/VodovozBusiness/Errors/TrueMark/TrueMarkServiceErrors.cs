using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.TrueMark
{
    public static class TrueMarkServiceErrors
    {
		/// <summary>
		/// Ошибка добавления кода, участвующего в аггрегации<br/>
		/// Нельзя добавить код, участвующий в аггрегации
		/// </summary>
		public static Error AggregationCodeAddError =>
			new Error(
				typeof(TrueMarkServiceErrors),
				nameof(AggregationCodeAddError),
				"Нельзя добавить код, участвующий в аггрегации");

		/// <summary>
		/// Ошибка изменения кода, участвующего в аггрегации<br/>
		/// Нельзя изменить код, участвующий в аггрегации
		/// </summary>
		public static Error AggregationCodeChangeError =>
			new Error(
				typeof(TrueMarkServiceErrors),
				nameof(AggregationCodeChangeError),
				"Нельзя изменить код, участвующий в аггрегации");

		/// <summary>
		/// Ошибка удаления кода<br/>
		/// Код запрашиваемый для удаления - отсутствует
		/// </summary>
		public static Error MissingTrueMarkCodeToDelete =>
			new Error(
				typeof(TrueMarkServiceErrors),
				nameof(MissingTrueMarkCodeToDelete),
				"Код запрашиваемый для удаления - отсутствует");
	}
}
