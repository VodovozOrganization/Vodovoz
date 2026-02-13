using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.Errors.TrueMark
{
	public static class TrueMarkCodeErrors
	{
		public static Error TrueMarkCodeParsingError =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeParsingError),
				"Ошибка получения кода");

		public static Error MissingPersistedTrueMarkCode =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(MissingPersistedTrueMarkCode),
				"Отсутствует сохраненный код ЧЗ");

		public static Error TrueMarkCodeStringIsNotValid =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeStringIsNotValid),
				"Полученная строка кода ЧЗ невалидна");

		public static Error TrueMarkCodeNotCheckedInTrueMark =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeNotCheckedInTrueMark),
				"Код не получилось проверить в ЧЗ");

		public static Error CreateTrueMarkCodeStringIsNotValid(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeStringIsNotValid : new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeStringIsNotValid),
				$"Полученная строка кода ЧЗ ({codeString}) невалидна");

		public static Error TrueMarkCodeIsAlreadyUsed =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeIsAlreadyUsed),
				"Код ЧЗ уже имеется в базе. Добавляемый код является дублем");

		public static Error CreateTrueMarkCodeIsAlreadyUsed(int waterCodeId) =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeIsAlreadyUsed),
				$"Код ЧЗ (Id = {waterCodeId}) уже был использован. Добавляемый код является дублем");

		public static Error CreateTrueMarkCodeIsAlreadyUsedInOrder(int orderId) =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeIsAlreadyUsed),
				$"Код ЧЗ уже был использован в заказе {orderId}");

		public static Error TrueMarkCodeGtinIsNotEqualsNomenclatureGtin =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeGtinIsNotEqualsNomenclatureGtin),
				"Значение GTIN переданного кода не соответствует значению GTIN для указанной номенклатуры");

		public static Error CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeGtinIsNotEqualsNomenclatureGtin : new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeGtinIsNotEqualsNomenclatureGtin),
				$"Значение GTIN переданного кода ({codeString}) не соответствует значению GTIN для указанной номенклатуры");

		public static Error TrueMarkCodesGtinsNotEqual =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodesGtinsNotEqual),
				"Значения GTIN переданных кодов не равны");

		public static Error CreateTrueMarkCodesGtinsNotEqual(string codeString1, string codeString2) =>
			string.IsNullOrEmpty(codeString1) || string.IsNullOrEmpty(codeString2) ? TrueMarkCodesGtinsNotEqual : new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodesGtinsNotEqual),
				$"Значения GTIN переданных кодов ({codeString1}) и ({codeString2}) не равны");

		public static Error TrueMarkCodeForCarLoadDocumentItemNotFound =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeForCarLoadDocumentItemNotFound),
				"Код ЧЗ не найден среди добавленных в строке документа погрузки");

		public static Error CreateTrueMarkCodeForCarLoadDocumentItemNotFound(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeForCarLoadDocumentItemNotFound : new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeForCarLoadDocumentItemNotFound),
				$"Код ЧЗ ({codeString}) не найден среди добавленных в строке документа погрузки");

		public static Error TrueMarkCodeIsNotIntroduced =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeIsNotIntroduced),
				"Код ЧЗ не в обороте");

		public static Error TrueMarkCodeOwnerInnIsNotCorrect =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeOwnerInnIsNotCorrect),
				"По данным ЧЗ владельцем кода является сторонняя организация");

		public static Error CreateTrueMarkCodeOwnerInnIsNotCorrect(string inn) =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeOwnerInnIsNotCorrect),
				$"По данным ЧЗ владельцем кода является сторонняя организация с ИНН {inn}");

		public static Error TrueMarkCodeIsExpired =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeIsExpired),
				$"Срок годности истек");

		public static Error TrueMarkApiRequestError =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkApiRequestError),
				"Ошибка при выполнении запроса к API честного знака");

		public static Error CreateTrueMarkApiRequestError(string message) =>
			string.IsNullOrEmpty(TrueMarkApiRequestError) ? TrueMarkApiRequestError : new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkApiRequestError),
				$"{message}");

		/// <summary>
		/// Количество переданных кодов ЧЗ для заказ не равно количеству ранее сохраненных кодов
		/// </summary>
		public static Error AddedAndSavedCodesCountNotEquals =>
			new Error(typeof(TrueMarkCodeErrors),
				nameof(AddedAndSavedCodesCountNotEquals),
				"Количество переданных кодов ЧЗ для заказ не равно количеству ранее сохраненных кодов");

		/// <summary>
		/// Переданные коды ЧЗ для заказа отличаются от ранее сохраненных кодов
		/// </summary>
		public static Error AddedAndSavedCodesNotEquals =>
			new Error(typeof(TrueMarkCodeErrors),
				nameof(AddedAndSavedCodesNotEquals),
				"Переданные коды ЧЗ для заказа отличаются от ранее сохраненных кодов");

		/// <summary>
		/// Не все коды ЧЗ для заказа были добавлены
		/// </summary>
		public static Error NotAllCodesAdded =>
			new Error(typeof(TrueMarkCodeErrors),
				nameof(NotAllCodesAdded),
				"Не все коды ЧЗ для заказа были добавлены");

		/// <summary>
		/// Нужное количество кодов ЧЗ для заказа уже было добавлено
		/// </summary>
		public static Error AllCodesAlreadyAdded =>
			new Error(typeof(TrueMarkCodeErrors),
				nameof(AllCodesAlreadyAdded),
				"Нужное количество кодов ЧЗ для заказа уже было добавлено");

		public static Error TrueMarkCodeForRouteListItemNotFound =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodeForRouteListItemNotFound),
				"Код ЧЗ не найден среди добавленных к адресу доставки");

		public static Error TrueMarkCodesHaveToBeAddedInWarehouse =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodesHaveToBeAddedInWarehouse),
				"Коды ЧЗ сетевого заказа должны добавляться на складе");

		public static Error AggregatedCode =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(AggregatedCode),
				"Код ЧЗ участвует в агрегации");

		public static Error CodeExpired =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(CodeExpired),
				"Истек срок годности товара");

		/// <summary>
		/// Код ЧЗ уже был добавлен
		/// </summary>
		public static Error StagingTrueMarkCodeDuplicate =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(StagingTrueMarkCodeDuplicate),
				"Код ЧЗ уже был добавлен");

		/// <summary>
		/// В связанный документ уже были добавлены коды ЧЗ. Добавление кода для промежуточного хранения невозможно
		/// </summary>
		public static Error RelatedDocumentHasTrueMarkCodes =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(RelatedDocumentHasTrueMarkCodes),
				"В связанный документ уже были добавлены коды ЧЗ. Добавление кода для промежуточного хранения невозможно");

		/// <summary>
		/// Суммарное количество кодов превышает необходимое для строки заказа
		/// </summary>
		public static Error TrueMarkCodesCountMoreThenInOrderItem =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(TrueMarkCodesCountMoreThenInOrderItem),
				"Суммарное количество кодов превышает необходимое для строки заказа");

		/// <summary>
		/// Номенклатура с Gtin полученного кода отсутствует в заказе
		/// </summary>
		public static Error GtinNomenclatureNotFoundInOrder =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(GtinNomenclatureNotFoundInOrder),
				"Номенклатура с Gtin полученного кода отсутствует в заказе");

		/// <summary>
		/// Номенклатура с Gtin полученного кода отсутствует в заказе
		/// </summary>
		public static Error CreateGtinNomenclatureNotFoundInOrder(string nomenclatureName, string gtin) =>
			new Error(
				typeof(TrueMarkCodeErrors),
				nameof(GtinNomenclatureNotFoundInOrder),
				$"Номенклатура {nomenclatureName} с данным Gtin {gtin} отсутствует в заказе.");
	}
}
