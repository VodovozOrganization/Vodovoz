using System;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
namespace Vodovoz.ViewModels.WageCalculation
{
	public class PercentWageParameterViewModel : EntityWidgetViewModelBase<PercentWageParameter>
	{
		public PercentWageParameterViewModel(PercentWageParameter entity, bool canEdit, ICommonServices commonServices) : base(entity, commonServices)
		{
			CanEdit = canEdit;

			SetPropertyChangeRelation(
				x => x.PercentWageType,
				() => RouteListPercentVisibility,
				() => ServicePercentVisibility
			);
		}

		public bool CanEdit { get; }

		public bool RouteListPercentVisibility => Entity.PercentWageType == PercentWageTypes.RouteList;

		public bool ServicePercentVisibility => Entity.PercentWageType == PercentWageTypes.Service;
	}
}
