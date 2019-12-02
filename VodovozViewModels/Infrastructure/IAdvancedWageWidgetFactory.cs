using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.Infrastructure
{
	public interface IAdvancedWageWidgetFactory
	{
		ViewModelBase GetAdvancedWageWidgetViewModel(IAdvancedWageParameter wageParameter, ICommonServices commonServices);
		ViewModelBase GetAdvancedWageWidgetViewModel(AdvancedWageParameterType parameterType, IWageHierarchyNode hierarchyNode, ICommonServices commonServices);
	}
}