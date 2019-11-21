using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels
{
	public class BottlesCountAdvancedWageParameterViewModel : EntityTabViewModelBase<BottlesCountAdvancedWageParameter>
	{
		public BottlesCountAdvancedWageParameterViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			ConfigureEntityPropertyChanges();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.RightSing, () => CanSetRightCount);
		}

		public bool CanSetRightCount => Entity.RightSing != null;
	}
}
