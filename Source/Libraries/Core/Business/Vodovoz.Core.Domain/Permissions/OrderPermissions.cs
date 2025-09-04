using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Права заказы
	/// </summary>
	public static partial class OrderPermissions
	{
		/// <summary>
		/// Может активировать акцию скидка на второй заказ
		/// </summary>
		[Display(
			Name = "Может активировать акцию скидка на второй заказ",
			Description = "Может активировать акцию скидка на второй заказ")]
		public static string CanActivateClientsSecondOrderDiscount => "can_activate_clients_second_order_discount";

		/// <summary>
		/// Пользователь может формировать заказ для ликвидированного контрагента
		/// </summary>
		[Display(
			Name = "Пользователь может формировать заказ для ликвидированного контрагента",
			Description = "Пользователь может формировать заказ для ликвидированного контрагента")]
		public static string CanFormOrderWithLiquidatedCounterparty =>
			"can_form_order_with_liquidated_counterparty";

		/// <summary>
		/// Настройка для редактирование поля "Ожидает до" в заказах<br/>
		/// Разрешить управлять настройкой для редактирования поля "Ожидает до" в заказах
		/// </summary>
		[Display(
			Name = "Настройка для редактирование поля \"Ожидает до\" в заказах",
			Description = "Разрешить управлять настройкой для редактирования поля \"Ожидает до\" в заказах")]
		public static string CanEditOrderWaitUntil => nameof(CanEditOrderWaitUntil);

		/// <summary>
		/// Настройка для редактирование поля "Ожидает до" в заказах<br/>
		/// Разрешить управлять настройкой для редактирования поля "Ожидает до" в заказах
		/// </summary>
		[Display(
			Name = "Настройка для редактирования лимита бутылей 19л для доставки за час\"",
			Description = "Разрешить управлять настройкой лимита бутылей 19л для доставки за час\"")]
		public static string CanEditFastDelivery19LBottlesLimit => nameof(CanEditFastDelivery19LBottlesLimit);

		/// <summary>
		/// Изменение заказа при наличии кассового чека<br/>
		/// Пользователь может изменять товары, цены, контрагента в заказе даже при наличии кассового чека по данному заказу
		/// </summary>
		[Display(
			Name = "Изменение заказа при наличии кассового чека",
			Description = "Пользователь может изменять товары, цены, контрагента в заказе даже при наличии кассового чека по данному заказу")]
		public static string CanChangeOrderAfterRecieptCreated => nameof(CanChangeOrderAfterRecieptCreated);

		/// <summary>
		/// Изменение цены и скидки в закрытии МЛ и самовывозов - Возможность изменять цену и скидку в заказе из закрытия МЛ и в редактировании самовывоза
		/// </summary>
		[Display(
			Name = "Изменение цены и скидки в закрытии МЛ и самовывозов",
			Description = "Возможность изменять цену и скидку в заказе из закрытия МЛ и в редактировании самовывоза")]
		public static string CanEditPriceDiscountFromRouteListAndSelfDelivery => "can_edit_price_discount_from_route_list_and_self_delivery";

		/// <summary>
		/// Изменение товаров на вкладке "Товары" в заказе, который уже добавлен в МЛ
		/// Разрешить изменять товары на вкладке \"Товары\" в заказе, который уже добавлен в МЛ
		/// </summary>
		[Display(
			Name = "Изменение товаров на вкладке \"Товары\" в заказе, который уже добавлен в МЛ",
			Description = "Разрешить изменять товары на вкладке \"Товары\" в заказе, который уже добавлен в МЛ")]
		public static string CanEditGoodsInRouteList => "can_edit_goods_in_route_list";

		/// <summary>
		/// Разрешено формировать заявки с залогом без предоплаты
		/// Возможность формировать заявки с залогом без предоплаты
		/// </summary>
		[Display(
			Name = "Разрешено формировать заявки с залогом без предоплаты",
			Description = "Возможность формировать заявки с залогом без предоплаты")]
		public static string CanFormOrderWithDepositWithoutPayment => "can_form_order_with_deposit_without_payment";
		/// <summary>
		/// Выбор сотрудника в отчёте по дате создания заказа
		/// Разрешить выбор сотрудника в отчёте по дате создания заказа
		/// </summary>
		[Display(
			Name = "Выбор сотрудника в отчёте по дате создания заказа",
			Description = "Разрешить выбор сотрудника в отчёте по дате создания заказа")]
		public static string CanEditEmployeeInOrderCreationDateReport => nameof(CanEditEmployeeInOrderCreationDateReport);
	}
}
