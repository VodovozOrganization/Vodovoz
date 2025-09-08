namespace Vodovoz.Core.Domain.Permissions
{
	public static class RouteListPermissions
	{
		/// <summary>
		///  Возможность менять машину в закрытии МЛ
		/// </summary>
		public static string CanEditCarOnCloseRouteList => "can_edit_car_on_close_route_list";
		
		/// <summary>
		/// Возможность менять водителя в закрытии МЛ
		/// </summary>
		public static string CanEditDriverOnCloseRouteList => "can_edit_driver_on_close_route_list";
	}
}
