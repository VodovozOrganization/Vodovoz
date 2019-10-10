using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class SalesPlanViewModel : EntityTabViewModelBase<SalesPlan>
	{
		public SalesPlanViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
		}
	}
}