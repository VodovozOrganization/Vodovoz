using DocumentFormat.OpenXml.Office2010.Excel;
using System.Security.Cryptography;

namespace Vodovoz.Errors.Store
{
	public static partial class CarLoadDocument
	{
		public static Error DocumentNotFound =>
			new Error(
				typeof(CarLoadDocument),
				nameof(DocumentNotFound),
				"Талон погрузки не найден");

		public static Error CreateDocumentNotFound(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocument),
				nameof(DocumentNotFound),
				$"Талон погрузки #{id} не найден");
		public static Error OrderNotFound =>
			new Error(
				typeof(CarLoadDocument),
				nameof(OrderNotFound),
				"Заказ не найден");

		public static Error CreateOrderNotFound(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocument),
				nameof(OrderNotFound),
				$"Заказ #{id} не найден");

		public static Error LoadingIsAlreadyInProgress =>
			new Error(
				typeof(CarLoadDocument),
				nameof(LoadingIsAlreadyInProgress),
				"Талон погрузки уже в процессе погрузки");

		public static Error CreateLoadingIsAlreadyInProgress(int? id) =>
			id is null ? LoadingIsAlreadyInProgress : new Error(
				typeof(CarLoadDocument),
				nameof(LoadingIsAlreadyInProgress),
				$"Талон погрузки #{id} уже в процессе погрузки");

		public static Error LoadingIsAlreadyDone =>
			new Error(
				typeof(CarLoadDocument),
				nameof(LoadingIsAlreadyDone),
				"Талон погрузки уже полностью погружен");

		public static Error CreateLoadingIsAlreadyDone(int? id) =>
			id is null ? LoadingIsAlreadyDone : new Error(
				typeof(CarLoadDocument),
				nameof(LoadingIsAlreadyDone),
				$"Талон погрузки #{id} уже полностью погружен");

		public static Error OrderItemsExistInMultipleDocuments =>
			new Error(
				typeof(CarLoadDocument),
				nameof(OrderItemsExistInMultipleDocuments),
				"Строки одного заказа сетевого клиента присутствуют в нескольких талонах погрузки");

		public static Error CreateOrderItemsExistInMultipleDocuments(int? id) =>
			id is null ? OrderItemsExistInMultipleDocuments : new Error(
				typeof(CarLoadDocument),
				nameof(OrderItemsExistInMultipleDocuments),
				$"Строки заказа #{id} сетевого клиента присутствуют в нескольких талонах погрузки");

		public static Error OrderDoesNotContainNomenclature =>
			new Error(
				typeof(CarLoadDocument),
				nameof(OrderDoesNotContainNomenclature),
				"В сетевом заказе номенклатура не найдена");

		public static Error CreateOrderDoesNotContainNomenclature(int? orderId, int? nomenclatureId) =>
			orderId is null || nomenclatureId is null ? OrderDoesNotContainNomenclature : new Error(
				typeof(CarLoadDocument),
				nameof(OrderDoesNotContainNomenclature),
				$"В сетевом заказе #{orderId} номенклатура #{nomenclatureId} не найдена");

		public static Error OrderNomenclatureExistInMultipleDocumentItems =>
			new Error(
				typeof(CarLoadDocument),
				nameof(OrderNomenclatureExistInMultipleDocumentItems),
				"В талоне погрузки имеется несколько строк сетевого заказа с номенклатурой");

		public static Error CreateOrderNomenclatureExistInMultipleDocumentItems(int? orderId, int? nomenclatureId) =>
			orderId is null || nomenclatureId is null ? OrderNomenclatureExistInMultipleDocumentItems : new Error(
				typeof(CarLoadDocument),
				nameof(OrderNomenclatureExistInMultipleDocumentItems),
				$"В талоне погрузки имеется несколько строк сетевого заказа #{orderId} с номенклатурой #{nomenclatureId}");

		public static Error TrueMarkCodeStringIsNotValid =>
			new Error(
				typeof(CarLoadDocument),
				nameof(TrueMarkCodeStringIsNotValid),
				"Полученная строка кода ЧЗ невалидна");

		public static Error CreateTrueMarkCodeStringIsNotValid(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeStringIsNotValid : new Error(
				typeof(CarLoadDocument),
				nameof(TrueMarkCodeStringIsNotValid),
				$"Полученная строка кода ЧЗ ({codeString}) невалидна");

		public static Error TrueMarkCodeIsAlreadyExists =>
			new Error(
				typeof(CarLoadDocument),
				nameof(TrueMarkCodeIsAlreadyExists),
				"Код ЧЗ уже имеется в базе. Добавляемый код является дублем");

		public static Error CreateTrueMarkCodeIsAlreadyExists(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeIsAlreadyExists : new Error(
				typeof(CarLoadDocument),
				nameof(TrueMarkCodeIsAlreadyExists),
				$"Код ЧЗ ({codeString}) уже имеется в базе. Добавляемый код является дублем");

		public static Error TrueMarkCodeGtinIsNotEqualsNomenclatureGtin =>
			new Error(
				typeof(CarLoadDocument),
				nameof(TrueMarkCodeGtinIsNotEqualsNomenclatureGtin),
				"Значение GTIN переданного кода не соответствует значению GTIN для указанной номенклатуры");

		public static Error CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(string codeString) =>
			string.IsNullOrEmpty(codeString) ? TrueMarkCodeGtinIsNotEqualsNomenclatureGtin : new Error(
				typeof(CarLoadDocument),
				nameof(TrueMarkCodeGtinIsNotEqualsNomenclatureGtin),
				$"Значение GTIN переданного кода ({codeString}) не соответствует значению GTIN для указанной номенклатуры");
	}
}
