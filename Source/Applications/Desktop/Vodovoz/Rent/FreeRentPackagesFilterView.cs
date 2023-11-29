using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

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

			Initialize();
		}

		private void Initialize()
		{
			ycheckbuttonIsArchive.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchieved, w => w.Active)
				.InitializeFromSource();
		}
	}
}
