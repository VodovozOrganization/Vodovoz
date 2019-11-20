using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels
{
	public class DeliveryTimeAdvancedWageParameterViewModel : EntityTabViewModelBase<DeliveryTimeAdvancedWageParameter>
	{
		public DeliveryTimeAdvancedWageParameterViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
		}
	}
}
