using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class LogisticPermissions
	{
		public static class RouteList
		{
			/// <summary>
			/// Удаление заказов и маршрутных листов - Пользователь может удалять заказы и маршрутные листы в журналах.
			/// </summary>
			[Display(
				Name = "Удаление заказов и маршрутных листов",
				Description = "Пользователь может удалять заказы и маршрутные листы в журналах.")]
			public static string CanDelete => "can_delete";

			/// <summary>
			/// Подтверждение МЛ с перегрузом - Пользователь может подтверждать МЛ, суммарный вес товаров и оборудования в котором превышает грузоподъемность автомобиля.
			/// </summary>
			[Display(
				Name = "Подтверждение МЛ с перегрузом",
				Description = "Пользователь может подтверждать МЛ, суммарный вес товаров и оборудования в котором превышает грузоподъемность автомобиля.")]
			public static string CanConfirmOverweighted => "can_confirm_routelist_with_overweight";

			/// <summary>
			/// Редактирование стоимости доставки в МЛ - Редактирование стоимости доставки в МЛ
			/// </summary>
			[Display(
				Name = "Редактирование стоимости доставки в МЛ",
				Description = "Редактирование стоимости доставки в МЛ")]
			public static string CanChangeRouteListFixedPrice => "can_change_route_list_fixed_price";

			/// <summary>
			/// Создание МЛ в прошлом периоде - Создание МЛ в прошлом периоде
			/// </summary>
			[Display(
				Name = "Создание МЛ в прошлом периоде",
				Description = "Создание МЛ в прошлом периоде")]
			public static string CanCreateRouteListInPastPeriod => "can_create_routelist_in_past_period";

			/// <summary>
			/// Просмотр рентабельности МЛ - Просмотр рентабельности МЛ
			/// </summary>
			[Display(
				Name = "Просмотр рентабельности МЛ",
				Description = "Просмотр рентабельности МЛ")]
			public static string CanReadRouteListProfitability => "can_read_route_list_profitability";

			/// <summary>
			/// Возможность вернуть статус МЛ в путь
			/// </summary>
			[Display(
				Name = "Возможность вернуть статус МЛ в путь")]
			public static string CanReturnRouteListToEnRouteStatus => "can_retrieve_routelist_en_route";

			/// <summary>
			/// Логистика. Ведение МЛ. Изменение времени доставки - Пользователь может изменять время доставки в диалоге ведения маршрутного листа
			/// </summary>
			[Display(
				Name = "Логистика. Ведение МЛ. Изменение времени доставки",
				Description = "Пользователь может изменять время доставки в диалоге ведения маршрутного листа")]
			public static string CanChangeDeliveryTime => "logistic_changedeliverytime";
			
			/// <summary>
			/// Логистика. Создание МЛ. Создание МЛ без заказов
			/// </summary>
			[Display(
				Name = "Логистика. Создание МЛ. Создание МЛ без заказов",
				Description = "Пользователь может создавать МЛ без заказов")]
			public static string CanCreateRouteListWithoutOrders => "can_create_route_list_without_orders";

			/// <summary>
			/// Возможность изменять водителя в диалоге создания МЛ
			/// </summary>
			[Display(
				Name = "Возможность изменять водителя в диалоге создания МЛ",
				Description = "Пользователь может изменить водителя в новом МЛ")]
			public static string CanChangeDriverInRouteList => "can_change_driver_in_route_list";
		}
	}
}
