using QS.Utilities.Text;

namespace Vodovoz.ViewModels.Logistic.DriversStopLists
{
	public partial class DriversStopListsViewModel
	{
		public sealed class DriverNode
		{
			public int DriverId { get; set; }
			public string DriverName { get; set; }
			public string DriverLastName { get; set; }
			public string DriverPatronymic { get; set; }
			public string CarRegistrationNumber { get; set; }
			public decimal RouteListsDebtsSum { get; set; }
			public int UnclosedRouteListsWithDebtCount { get; set; }
			public bool IsStopListRemoved { get; set; }
			public int DriversUnclosedRouteListsMaxCount { get; set; }
			public int DriversRouteListsDebtsMaxSum { get; set; }
			public string DriverFullName => PersonHelper.PersonNameWithInitials(DriverLastName, DriverName, DriverPatronymic);
			public bool IsDriverInStopList =>
				!IsStopListRemoved
				&& ((DriversRouteListsDebtsMaxSum > 0 && RouteListsDebtsSum >= DriversRouteListsDebtsMaxSum)
					|| (DriversUnclosedRouteListsMaxCount > 0 && UnclosedRouteListsWithDebtCount >= DriversUnclosedRouteListsMaxCount));
		}
	}
}
