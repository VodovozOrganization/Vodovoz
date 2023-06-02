using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static partial class Logistic
	{
		public static class RouteList
		{
			[Display(
				Name = "Удаление заказов и маршрутных листов",
				Description = "Пользователь может удалять заказы и маршрутные листы в журналах.")]
			public static string CanDelete => "can_delete";
		}
	}
}
