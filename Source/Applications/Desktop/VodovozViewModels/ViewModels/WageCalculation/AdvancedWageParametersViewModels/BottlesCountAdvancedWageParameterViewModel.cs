using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParametersViewModels;

namespace Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels
{
	public class BottlesCountAdvancedWageParameterViewModel : EntityWidgetViewModelBase<BottlesCountAdvancedWageParameter>, IWageParameterViewModel
	{
		public ComparisonSings[] RightSingHideType => new ComparisonSings[] { ComparisonSings.More, ComparisonSings.MoreOrEqual, ComparisonSings.Equally }; 

		public BottlesCountAdvancedWageParameterViewModel(BottlesCountAdvancedWageParameter entity, ICommonServices commonServices) : base(entity, commonServices)
		{
			ConfigureEntityPropertyChanges();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.RightSing, () => CanSetRightCount);
		}

		public AdvancedWageParameter GetParameter() => Entity;

		public bool CanSetRightCount => Entity.RightSing != null;
	}
}
