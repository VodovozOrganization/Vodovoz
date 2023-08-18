using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Logistic.DriversStopLists
{
	public partial class DriversStopListsViewModel
	{
		/// <summary>
		/// Порядок сортировки водителей
		/// </summary>
		public enum DriversSortOrder
		{
			[Display(Name = "По наличию стопа")]
			ByStopList,

			[Display(Name = "По кол-ву незакрытых МЛ")]
			ByUnclosedRouteListsCount
		}
	}
}
