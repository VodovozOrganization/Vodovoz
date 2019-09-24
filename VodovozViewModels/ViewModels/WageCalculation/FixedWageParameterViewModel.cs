using System;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
namespace Vodovoz.ViewModels.WageCalculation
{
	public class FixedWageParameterViewModel : EntityWidgetViewModelBase<FixedWageParameter>
	{
		public FixedWageParameterViewModel(FixedWageParameter entity, bool canEdit, ICommonServices commonServices) : base(entity, commonServices)
		{
			CanEdit = canEdit;
		}

		public bool CanEdit { get; }
	}
}
