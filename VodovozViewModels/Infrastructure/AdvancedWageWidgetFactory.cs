using System;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;

namespace Vodovoz.Infrastructure
{
	public class AdvancedWageWidgetFactory : IAdvancedWageWidgetFactory
	{
		public ViewModelBase GetAdvancedWageWidgetViewModel(IAdvancedWageParameter entity, ICommonServices commonServices)
			=> CreateWageWidget(commonServices, entity.AdvancedWageParameterType, entity);

		public ViewModelBase GetAdvancedWageWidgetViewModel(AdvancedWageParameterType parameterType, IWageHierarchyNode hierarchyNode, ICommonServices commonServices)
			=> CreateWageWidget(commonServices, parameterType, hierarchyNode: hierarchyNode);
			
		protected ViewModelBase CreateWageWidget(ICommonServices commonServices, AdvancedWageParameterType parameterType, IAdvancedWageParameter entity = null, IWageHierarchyNode hierarchyNode = null)
		{
			if(commonServices == null)
				throw new ArgumentNullException(nameof(commonServices));

			if(parameterType == AdvancedWageParameterType.DeliveryTime)
				return new DeliveryTimeAdvancedWageParameterViewModel
							(
								(DeliveryTimeAdvancedWageParameter)(entity ?? new DeliveryTimeAdvancedWageParameter { Parent = hierarchyNode }),
								commonServices
							);

			if(parameterType == AdvancedWageParameterType.BottlesCount)
				return new BottlesCountAdvancedWageParameterViewModel
					(
								(BottlesCountAdvancedWageParameter)(entity ?? new BottlesCountAdvancedWageParameter { Parent = hierarchyNode }),
								commonServices
					);

			throw new NotImplementedException(entity.AdvancedWageParameterType.ToString());
		}
	}
}
