using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.WageCalculation;
namespace Vodovoz.Views.WageCalculation
{
	public partial class WageDistrictLevelRatesAssigningView : TabViewBase<WageDistrictLevelRatesAssigningViewModel>
	{
		public WageDistrictLevelRatesAssigningView(WageDistrictLevelRatesAssigningViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}
