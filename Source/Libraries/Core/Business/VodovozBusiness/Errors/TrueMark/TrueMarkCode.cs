namespace Vodovoz.Errors.TrueMark
{
	public static class TrueMarkCode
	{

		public static Error TrueMarkCodeStringIsNotValid =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeStringIsNotValid),
				"Полученная строка кода ЧЗ невалидна");

		public static Error CreateTrueMarkCodeStringIsNotValid(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeStringIsNotValid : new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeStringIsNotValid),
				$"Полученная строка кода ЧЗ ({codeString}) невалидна");

		public static Error TrueMarkCodeIsAlreadyUsed =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeIsAlreadyUsed),
				"Код ЧЗ уже имеется в базе. Добавляемый код является дублем");

		public static Error CreateTrueMarkCodeIsAlreadyUsed(int waterCodeId) =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeIsAlreadyUsed),
				$"Код ЧЗ (Id = {waterCodeId}) уже был использован. Добавляемый код является дублем");

		public static Error TrueMarkCodeGtinIsNotEqualsNomenclatureGtin =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeGtinIsNotEqualsNomenclatureGtin),
				"Значение GTIN переданного кода не соответствует значению GTIN для указанной номенклатуры");

		public static Error CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeGtinIsNotEqualsNomenclatureGtin : new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeGtinIsNotEqualsNomenclatureGtin),
				$"Значение GTIN переданного кода ({codeString}) не соответствует значению GTIN для указанной номенклатуры");

		public static Error TrueMarkCodesGtinsNotEqual =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodesGtinsNotEqual),
				"Значения GTIN переданных кодов не равны");

		public static Error CreateTrueMarkCodesGtinsNotEqual(string codeString1, string codeString2) =>
			string.IsNullOrEmpty(codeString1) || string.IsNullOrEmpty(codeString2) ? TrueMarkCodesGtinsNotEqual : new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodesGtinsNotEqual),
				$"Значения GTIN переданных кодов ({codeString1}) и ({codeString2}) не равны");

		public static Error TrueMarkCodeForCarLoadDocumentItemNotFound =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeForCarLoadDocumentItemNotFound),
				"Код ЧЗ не найден среди добавленных в строке документа погрузки");

		public static Error CreateTrueMarkCodeForCarLoadDocumentItemNotFound(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeForCarLoadDocumentItemNotFound : new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeForCarLoadDocumentItemNotFound),
				$"Код ЧЗ ({codeString}) не найден среди добавленных в строке документа погрузки");

		public static Error TrueMarkCodeIsNotIntroduced =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeIsNotIntroduced),
				"Код ЧЗ не в обороте");

		public static Error TrueMarkCodeOwnerInnIsNotCorrect =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeOwnerInnIsNotCorrect),
				"По данным ЧЗ владельцем кода является сторонняя организация");

		public static Error CreateTrueMarkCodeOwnerInnIsNotCorrect(string inn) =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeOwnerInnIsNotCorrect),
				$"По данным ЧЗ владельцем кода является сторонняя организация с ИНН {inn}");

		public static Error TrueMarkApiRequestError =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkApiRequestError),
				"Ошибка при выполнении запроса к API честного знака");

		public static Error CreateTrueMarkApiRequestError(string message) =>
			string.IsNullOrEmpty(TrueMarkApiRequestError) ? TrueMarkApiRequestError : new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkApiRequestError),
				$"{message}");

		/// <summary>
		/// Количество переданных кодов ЧЗ для заказ не равно количеству ранее сохраненных кодов
		/// </summary>
		public static Error AddedAndSavedCodesCountNotEquals =>
			new Error(typeof(TrueMarkCode),
				nameof(AddedAndSavedCodesCountNotEquals),
				"Количество переданных кодов ЧЗ для заказ не равно количеству ранее сохраненных кодов");

		/// <summary>
		/// Переданные коды ЧЗ для заказа отличаются от ранее сохраненных кодов
		/// </summary>
		public static Error AddedAndSavedCodesNotEquals =>
			new Error(typeof(TrueMarkCode),
				nameof(AddedAndSavedCodesNotEquals),
				"Переданные коды ЧЗ для заказа отличаются от ранее сохраненных кодов");

		/// <summary>
		/// Не все коды ЧЗ для заказа были добавлены
		/// </summary>
		public static Error NotAllCodesAdded =>
			new Error(typeof(TrueMarkCode),
				nameof(NotAllCodesAdded),
				"Не все коды ЧЗ для заказа были добавлены");

		/// <summary>
		/// Нужное количество кодов ЧЗ для заказа уже было добавлено
		/// </summary>
		public static Error AllCodesAlreadyAdded =>
			new Error(typeof(TrueMarkCode),
				nameof(AllCodesAlreadyAdded),
				"Нужное количество кодов ЧЗ для заказа уже было добавлено");

		public static Error TrueMarkCodeForRouteListItemNotFound =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeForRouteListItemNotFound),
				"Код ЧЗ не найден среди добавленных к адресу доставки");
	}
}
