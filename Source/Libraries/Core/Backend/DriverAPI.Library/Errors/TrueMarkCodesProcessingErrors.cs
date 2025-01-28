using Vodovoz.Errors;

namespace DriverAPI.Library.Errors
{
	/// <summary>
	/// Ошибки обработки кодов ЧЗ в DriverApi
	/// </summary>
	public static class TrueMarkCodesProcessingErrors
	{
		/// <summary>
		/// Количество переданных кодов ЧЗ для заказ не равно количеству ранее сохраненных кодов
		/// </summary>
		public static Error AddedAndSavedCodesCountNotEquals =>
			new Error(typeof(TrueMarkCodesProcessingErrors),
				nameof(AddedAndSavedCodesCountNotEquals),
				"Количество переданных кодов ЧЗ для заказ не равно количеству ранее сохраненных кодов");

		/// <summary>
		/// Переданные коды ЧЗ для заказа отличаются от ранее сохраненных кодов
		/// </summary>
		public static Error AddedAndSavedCodesNotEquals =>
			new Error(typeof(TrueMarkCodesProcessingErrors),
				nameof(AddedAndSavedCodesNotEquals),
				"Переданные коды ЧЗ для заказа отличаются от ранее сохраненных кодов");

		/// <summary>
		/// Не все коды ЧЗ для заказа были добавлены
		/// </summary>
		public static Error NotAllCodesAdded =>
			new Error(typeof(TrueMarkCodesProcessingErrors),
				nameof(NotAllCodesAdded),
				"Не все коды ЧЗ для заказа были добавлены");
	}
}
