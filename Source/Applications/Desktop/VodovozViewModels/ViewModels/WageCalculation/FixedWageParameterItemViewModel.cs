using System;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
namespace Vodovoz.ViewModels.WageCalculation
{
	public class FixedWageParameterItemViewModel : EntityWidgetViewModelBase<FixedWageParameterItem>
	{
		public FixedWageParameterItemViewModel(FixedWageParameterItem entity, bool canEdit, ICommonServices commonServices) : base(entity, commonServices)
		{
			CanEdit = canEdit;
		}

		public bool CanEdit { get; }
	}
}
