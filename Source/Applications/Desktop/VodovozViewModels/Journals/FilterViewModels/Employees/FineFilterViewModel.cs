using System;
using QS.Project.Filter;
using QS.Services;

namespace Vodovoz.FilterViewModels.Employees
{
	public class FineFilterViewModel : FilterViewModelBase<FineFilterViewModel>
	{
		public FineFilterViewModel(bool canEditFilter = false)
		{
			CanEditSubdivision =
			CanEditFineDate =
			CanEditRouteListDate = canEditFilter;
		}

		private bool canEditSubdivision;
		public virtual bool CanEditSubdivision {
			get => canEditSubdivision;
			set => SetField(ref canEditSubdivision, value, () => CanEditSubdivision);
		}

		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => UpdateFilterField(ref subdivision, value, () => Subdivision);
		}

		private bool canEditFineDate;
		public virtual bool CanEditFineDate {
			get => canEditFineDate;
			set => SetField(ref canEditFineDate, value, () => CanEditFineDate);
		}

		private DateTime? fineDateStart;
		public virtual DateTime? FineDateStart {
			get => fineDateStart;
			set => UpdateFilterField(ref fineDateStart, value, () => FineDateStart);
		}

		private DateTime? fineDateEnd;
		public virtual DateTime? FineDateEnd {
			get => fineDateEnd;
			set => UpdateFilterField(ref fineDateEnd, value, () => FineDateEnd);
		}

		private bool canEditRouteListDate;
		public virtual bool CanEditRouteListDate {
			get => canEditRouteListDate;
			set => SetField(ref canEditRouteListDate, value, () => CanEditRouteListDate);
		}

		private DateTime? routeListDateStart;
		public virtual DateTime? RouteListDateStart {
			get => routeListDateStart;
			set => UpdateFilterField(ref routeListDateStart, value, () => RouteListDateStart);
		}

		private DateTime? routeListDateEnd;
		public virtual DateTime? RouteListDateEnd {
			get => routeListDateEnd;
			set => UpdateFilterField(ref routeListDateEnd, value, () => RouteListDateEnd);
		}

		private int[] excludedIds;
		public virtual int[] ExcludedIds {
			get => excludedIds;
			set => UpdateFilterField(ref excludedIds, value, () => ExcludedIds);
		}
		
		private int[] findFinesWithIds;
		public virtual int[] FindFinesWithIds {
			get => findFinesWithIds;
			set => UpdateFilterField(ref findFinesWithIds, value);
		}
	}
}
