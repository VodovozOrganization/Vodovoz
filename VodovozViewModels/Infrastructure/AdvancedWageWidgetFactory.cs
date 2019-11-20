using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;

namespace Vodovoz.Infrastructure
{
	public class AdvancedWageWidgetFactory : IAdvancedWageWidgetFactory
	{
		public ViewModelBase GetAdvancedWageWidgetViewModel(IAdvancedWageParameter wageParameter, ICommonServices commonServices)
		{
			if(wageParameter == null) 
				throw new ArgumentNullException(nameof(wageParameter));
			if(wageParameter.Id == 0)
				return GetAdvancedWageWidgetViewModel(wageParameter.AdvancedWageParameterType, commonServices);

			return GetAdvancedWageWidgetViewModel( wageParameter.AdvancedWageParameterType, EntityUoWBuilder.ForOpen(wageParameter.Id), commonServices);
		}

		public ViewModelBase GetAdvancedWageWidgetViewModel(AdvancedWageParameterType wageParameterType, ICommonServices commonServices)
		{
			return GetAdvancedWageWidgetViewModel(wageParameterType ,EntityUoWBuilder.ForCreate(), commonServices);
		}

		public ViewModelBase GetAdvancedWageWidgetViewModel(AdvancedWageParameterType wageParameterType, IEntityUoWBuilder entityUoWBuilder, ICommonServices commonServices)
		{
			if(commonServices == null)
				throw new ArgumentNullException(nameof(commonServices));

			if(wageParameterType == AdvancedWageParameterType.DeliveryTime)
				return new DeliveryTimeAdvancedWageParameterViewModel
							(
								entityUoWBuilder,
								UnitOfWorkFactory.GetDefaultFactory,
								commonServices
							);

			if(wageParameterType == AdvancedWageParameterType.BottlesCount)
				return new BottlesCountAdvancedWageParameterViewModel
					(
								entityUoWBuilder,
								UnitOfWorkFactory.GetDefaultFactory,
								commonServices
					);

			throw new NotImplementedException(wageParameterType.ToString());
		}
	}
}
