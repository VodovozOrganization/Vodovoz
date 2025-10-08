using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;

namespace Vodovoz.Errors.Stores
{
	public static partial class CarLoadDocumentErrors
	{
		public static Error DocumentNotFound =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(DocumentNotFound),
				"Талон погрузки не найден");

		public static Error CreateDocumentNotFound(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(DocumentNotFound),
				$"Талон погрузки #{id} не найден");

		public static Error CarLoadDocumentItemNotFound =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CarLoadDocumentItemNotFound),
				"Стока талона погрузки не найдена");

		public static Error CreateCarLoadDocumentItemNotFound(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CarLoadDocumentItemNotFound),
				$"Строки талона погрузки для заказа #{id} не найдены");

		public static Error OrderNotFound =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderNotFound),
				"Заказ не найден");

		public static Error CreateOrderNotFound(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderNotFound),
				$"Заказ #{id} не найден");
		
		public static Error CreateOrderBadStatus(int? id, OrderStatus status) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderNotFound),
				$"Заказ #{id} не может быть изменен в статусе {status}");

		public static Error OrderNoNeedIndividualSetOnLoad =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderNoNeedIndividualSetOnLoad),
				"Заказ не является сетевым");

		public static Error CreateOrderNoNeedIndividualSetOnLoadPaymentIsNotCashless(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CreateOrderNoNeedIndividualSetOnLoadPaymentIsNotCashless),
				$"В заказе #{id} тип оплаты не безналичный, сканирование не требуется");

		public static Error CreateOrderNoNeedIndividualSetOnLoadClientIsNotSet(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CreateOrderNoNeedIndividualSetOnLoadClientIsNotSet),
				$"В заказе #{id} не указан контрагент");

		public static Error CreateOrderNoNeedIndividualSetOnLoadConsentForEdoIsNotAgree(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CreateOrderNoNeedIndividualSetOnLoadConsentForEdoIsNotAgree),
				$"В заказе #{id} у клиента нет согласия на отрпавки документов по ЭДО, сканирование не требуется");

		public static Error CreateOrderNoNeedIndividualSetOnLoadOrderIsNotEnRoute(int? id) =>
			id is null ? DocumentNotFound : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CreateOrderNoNeedIndividualSetOnLoadOrderIsNotEnRoute),
				$"Заказ #{id} не в статусе в пути для ЭДО");

		public static Error LoadingProcessStateMustBeNotStarted =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(LoadingProcessStateMustBeNotStarted),
				$"Статус погрузки талона должен быть в \"{CarLoadDocumentLoadOperationState.NotStarted.GetEnumDisplayName()}\"");

		public static Error CreateLoadingProcessStateMustBeNotStarted(int? id) =>
			id is null ? LoadingProcessStateMustBeNotStarted : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(LoadingProcessStateMustBeNotStarted),
				$"Статус погрузки талона #{id} должен быть в \"{CarLoadDocumentLoadOperationState.NotStarted.GetEnumDisplayName()}\"");

		public static Error LoadingProcessStateMustBeInProgress =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(LoadingProcessStateMustBeInProgress),
				$"Статус погрузки талона должен быть в \"{CarLoadDocumentLoadOperationState.InProgress.GetEnumDisplayName()}\"");

		public static Error CreateLoadingProcessStateMustBeInProgress(int? id) =>
			id is null ? LoadingProcessStateMustBeInProgress : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(LoadingProcessStateMustBeInProgress),
				$"Статус погрузки талона #{id} должен быть в \"{CarLoadDocumentLoadOperationState.InProgress.GetEnumDisplayName()}\"");

		public static Error LoadingProcessStateMustBeNotStartedOrInProgress =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(LoadingProcessStateMustBeNotStartedOrInProgress),
				$"Статус погрузки талона должен быть в \"{CarLoadDocumentLoadOperationState.NotStarted.GetEnumDisplayName()}\" или \"{CarLoadDocumentLoadOperationState.InProgress.GetEnumDisplayName()}\"");

		public static Error CreateLoadingProcessStateMustBeNotStartedOrInProgress(int? id) =>
			id is null ? LoadingProcessStateMustBeNotStartedOrInProgress : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(LoadingProcessStateMustBeNotStartedOrInProgress),
				$"Статус погрузки талона #{id} должен быть в \"{CarLoadDocumentLoadOperationState.NotStarted.GetEnumDisplayName()}\" или \"{CarLoadDocumentLoadOperationState.InProgress.GetEnumDisplayName()}\"");

		public static Error NotAllTrueMarkCodesWasAddedIntoCarLoadDocument =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(NotAllTrueMarkCodesWasAddedIntoCarLoadDocument),
				"Не для всех товаров документа погрузки были добавлены коды ЧЗ");

		public static Error CreateNotAllTrueMarkCodesWasAddedIntoCarLoadDocument(int? id) =>
			id is null ? NotAllTrueMarkCodesWasAddedIntoCarLoadDocument : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(NotAllTrueMarkCodesWasAddedIntoCarLoadDocument),
				$"Не для всех товаров документа погрузки #{id} были добавлены коды ЧЗ");

		public static Error OrderItemsExistInMultipleDocuments =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderItemsExistInMultipleDocuments),
				"Строки одного заказа сетевого клиента присутствуют в нескольких талонах погрузки");

		public static Error CreateOrderItemsExistInMultipleDocuments(int? id) =>
			id is null ? OrderItemsExistInMultipleDocuments : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderItemsExistInMultipleDocuments),
				$"Строки заказа #{id} сетевого клиента присутствуют в нескольких талонах погрузки");

		public static Error OrderDoesNotContainNomenclature =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderDoesNotContainNomenclature),
				"В сетевом заказе номенклатура не найдена");

		public static Error CreateOrderDoesNotContainNomenclature(int? orderId, int? nomenclatureId) =>
			orderId is null || nomenclatureId is null ? OrderDoesNotContainNomenclature : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderDoesNotContainNomenclature),
				$"В сетевом заказе #{orderId} номенклатура #{nomenclatureId} не найдена");

		public static Error OrderNomenclatureExistInMultipleDocumentItems =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderNomenclatureExistInMultipleDocumentItems),
				"В талоне погрузки имеется несколько строк сетевого заказа с номенклатурой");

		public static Error CreateOrderNomenclatureExistInMultipleDocumentItems(int? orderId, int? nomenclatureId) =>
			orderId is null || nomenclatureId is null ? OrderNomenclatureExistInMultipleDocumentItems : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(OrderNomenclatureExistInMultipleDocumentItems),
				$"В талоне погрузки имеется несколько строк сетевого заказа #{orderId} с номенклатурой #{nomenclatureId}");

		public static Error AllOrderNomenclatureCodesAlreadyAdded =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(AllOrderNomenclatureCodesAlreadyAdded),
				"Коды ЧЗ номенклатуры в заказе уже добавлены уже добавлены для всех единиц товара");

		public static Error CreateAllOrderNomenclatureCodesAlreadyAdded(int? orderId, int? nomenclatureId) =>
			orderId is null || nomenclatureId is null ? AllOrderNomenclatureCodesAlreadyAdded : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(AllOrderNomenclatureCodesAlreadyAdded),
				$"Коды ЧЗ номенклатуры #{nomenclatureId} в заказе #{orderId} уже добавлены для всех единиц товара");

		public static Error CarLoadDocumentStateChangeError =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CarLoadDocumentStateChangeError),
				"Ошибка при изменении статуса операции погрузки талона погрузки");

		public static Error CreateCarLoadDocumentStateChangeError(int? id) =>
			id is null ? CarLoadDocumentStateChangeError : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CarLoadDocumentStateChangeError),
				$"Ошибка при изменении статуса операции погрузки талона погрузки #{id}");

		public static Error CarLoadDocumentLogisticEventCreationError =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CarLoadDocumentLogisticEventCreationError),
				"Ошибка при создании логистического события при изменении статуса операции погрузки талона погрузки");

		public static Error CarLoadDocumentAlreadyHasPickerError =>
			new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CarLoadDocumentAlreadyHasPickerError),
				"Ошибка при изменении статуса операции погрузки талона погрузки");

		public static Error CreateCarLoadDocumentAlreadyHasPickerError(int? documentId, string pickerName, TimeSpan noActionsTimespan) =>
			documentId is null || string.IsNullOrWhiteSpace(pickerName) || noActionsTimespan == default ? CarLoadDocumentAlreadyHasPickerError : new Error(
				typeof(CarLoadDocumentErrors),
				nameof(CarLoadDocumentAlreadyHasPickerError),
				$"Талон погрузки #{documentId} уже собирает {pickerName} Подождите {Math.Ceiling(noActionsTimespan.TotalMinutes):N0} минут и попробуйте снова");
	}
}
