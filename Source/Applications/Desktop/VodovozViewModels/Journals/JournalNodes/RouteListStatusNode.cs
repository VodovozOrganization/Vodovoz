using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListStatusNode : PropertyChangedBase
	{
		private bool selected;
		public virtual bool Selected {
			get => selected;
			set => SetField(ref selected, value);
		}

		public RouteListStatus RouteListStatus { get; }

		public string Title => RouteListStatus.GetEnumTitle();

		public RouteListStatusNode(RouteListStatus routeListStatus)
		{
			RouteListStatus = routeListStatus;
		}
	}
}
