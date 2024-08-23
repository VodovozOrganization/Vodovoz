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

		public static Error TrueMarkCodeIsAlreadyExists =>
			new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeIsAlreadyExists),
				"Код ЧЗ уже имеется в базе. Добавляемый код является дублем");

		public static Error CreateTrueMarkCodeIsAlreadyExists(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeIsAlreadyExists : new Error(
				typeof(TrueMarkCode),
				nameof(TrueMarkCodeIsAlreadyExists),
				$"Код ЧЗ ({codeString}) уже имеется в базе. Добавляемый код является дублем");

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
	}
}
