﻿using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Extensions;

namespace Vodovoz.Errors.Stores
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

		public static Error LoadingProcessStateMustBeNotStarted =>
			new Error(
				typeof(CarLoadDocument),
				nameof(LoadingProcessStateMustBeNotStarted),
				$"Статус погрузки талона должен быть в \"{CarLoadDocumentLoadOperationState.NotStarted.GetEnumDisplayName()}\"");

		public static Error CreateLoadingProcessStateMustBeNotStarted(int? id) =>
			id is null ? LoadingProcessStateMustBeNotStarted : new Error(
				typeof(CarLoadDocument),
				nameof(LoadingProcessStateMustBeNotStarted),
				$"Статус погрузки талона #{id} должен быть в \"{CarLoadDocumentLoadOperationState.NotStarted.GetEnumDisplayName()}\"");

		public static Error LoadingProcessStateMustBeInProgress =>
			new Error(
				typeof(CarLoadDocument),
				nameof(LoadingProcessStateMustBeInProgress),
				$"Статус погрузки талона должен быть в \"{CarLoadDocumentLoadOperationState.InProgress.GetEnumDisplayName()}\"");

		public static Error CreateLoadingProcessStateMustBeInProgress(int? id) =>
			id is null ? LoadingProcessStateMustBeInProgress : new Error(
				typeof(CarLoadDocument),
				nameof(LoadingProcessStateMustBeInProgress),
				$"Статус погрузки талона #{id} должен быть в \"{CarLoadDocumentLoadOperationState.InProgress.GetEnumDisplayName()}\"");

		public static Error LoadingProcessStateMustBeNotStartedOrInProgress =>
			new Error(
				typeof(CarLoadDocument),
				nameof(LoadingProcessStateMustBeNotStartedOrInProgress),
				$"Статус погрузки талона должен быть в \"{CarLoadDocumentLoadOperationState.NotStarted.GetEnumDisplayName()}\" или \"{CarLoadDocumentLoadOperationState.InProgress.GetEnumDisplayName()}\"");

		public static Error CreateLoadingProcessStateMustBeNotStartedOrInProgress(int? id) =>
			id is null ? LoadingProcessStateMustBeNotStartedOrInProgress : new Error(
				typeof(CarLoadDocument),
				nameof(LoadingProcessStateMustBeNotStartedOrInProgress),
				$"Статус погрузки талона #{id} должен быть в \"{CarLoadDocumentLoadOperationState.NotStarted.GetEnumDisplayName()}\" или \"{CarLoadDocumentLoadOperationState.InProgress.GetEnumDisplayName()}\"");

		public static Error NotAllTrueMarkCodesWasAddedIntoCarLoadDocument =>
			new Error(
				typeof(CarLoadDocument),
				nameof(NotAllTrueMarkCodesWasAddedIntoCarLoadDocument),
				"Не для всех товаров документа погрузки были добавлены коды ЧЗ");

		public static Error CreateNotAllTrueMarkCodesWasAddedIntoCarLoadDocument(int? id) =>
			id is null ? NotAllTrueMarkCodesWasAddedIntoCarLoadDocument : new Error(
				typeof(CarLoadDocument),
				nameof(NotAllTrueMarkCodesWasAddedIntoCarLoadDocument),
				$"Не для всех товаров документа погрузки #{id} были добавлены коды ЧЗ");

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

		public static Error AllOrderNomenclatureCodesAlreadyAdded =>
			new Error(
				typeof(CarLoadDocument),
				nameof(AllOrderNomenclatureCodesAlreadyAdded),
				"Коды ЧЗ номенклатуры в заказе уже добавлены уже добавлены для всех единиц товара");

		public static Error CreateAllOrderNomenclatureCodesAlreadyAdded(int? orderId, int? nomenclatureId) =>
			orderId is null || nomenclatureId is null ? AllOrderNomenclatureCodesAlreadyAdded : new Error(
				typeof(CarLoadDocument),
				nameof(AllOrderNomenclatureCodesAlreadyAdded),
				$"Коды ЧЗ номенклатуры #{nomenclatureId} в заказе #{orderId} уже добавлены для всех единиц товара");

		public static Error CarLoadDocumentStateChangeError =>
			new Error(
				typeof(CarLoadDocument),
				nameof(CarLoadDocumentStateChangeError),
				"Ошибка при изменении статуса операции погрузки талона погрузки");

		public static Error CreateCarLoadDocumentStateChangeError(int? id) =>
			id is null ? CarLoadDocumentStateChangeError : new Error(
				typeof(CarLoadDocument),
				nameof(CarLoadDocumentStateChangeError),
				$"Ошибка при изменении статуса операции погрузки талона погрузки #{id}");

		public static Error CarLoadDocumentAlreadyHasPickerError =>
			new Error(
				typeof(CarLoadDocument),
				nameof(CarLoadDocumentAlreadyHasPickerError),
				"Ошибка при изменении статуса операции погрузки талона погрузки");

		public static Error CreateCarLoadDocumentAlreadyHasPickerError(int? documentId, string pickerName, TimeSpan noActionsTimespan) =>
			documentId is null || string.IsNullOrWhiteSpace(pickerName) || noActionsTimespan == default ? CarLoadDocumentAlreadyHasPickerError : new Error(
				typeof(CarLoadDocument),
				nameof(CarLoadDocumentAlreadyHasPickerError),
				$"Талон погрузки #{documentId} уже собирает {pickerName} Подождите {Math.Ceiling(noActionsTimespan.TotalMinutes):N0} минут и попробуйте снова");
	}
}
