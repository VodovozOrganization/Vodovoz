using System.ComponentModel;
using QS.Project.Filter;
using QS.Views.GtkUI;

namespace Vodovoz.Rent
{
	[ToolboxItem(true)]
	public partial class FreeRentPackagesFilterView
	 : FilterViewBase<FreeRentPackagesFilterViewModel>
	{
		public FreeRentPackagesFilterView(FreeRentPackagesFilterViewModel viewModel)
			: base(viewModel)
		{
			Build();
		}
	}

	public class FreeRentPackagesFilterViewModel : FilterViewModelBase<FreeRentPackagesFilterViewModel>
	{
	}
}
