using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static class RouteListPermissions
	{
		/// <summary>
		///  Возможность менять машину в закрытии МЛ
		/// </summary>
		[Display(
			Name = "Сменить авто в закрытии МЛ",
			Description = "Разрешено изменять авто после статуса \"Доставлено\" и позже")]
		public static string CanEditCarOnCloseRouteList => "can_edit_car_on_close_route_list";
		
		/// <summary>
		/// Возможность менять водителя в закрытии МЛ
		/// </summary>
		[Display(
			Name = "Сменить водителя в закрытии МЛ",
			Description = "Разрешено изменять водителя после статуса \"Доставлено\" и позже")]
		public static string CanEditDriverOnCloseRouteList => "can_edit_driver_on_close_route_list";
		
		/// <summary>
		/// Возможность менять экспедитора в закрытии МЛ
		/// </summary>
		[Display(
			Name = "Сменить экспедитора в закрытии МЛ",
			Description = "Разрешено изменять экспедитора после статуса \"Доставлено\" и позже")]
		public static string CanEditExpeditorOnCloseRouteList => "can_edit_expeditor_on_close_route_list";
	}
}
