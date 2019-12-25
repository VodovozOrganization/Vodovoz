using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarFilterView : FilterViewBase<CarJournalFilterViewModel>
	{
		public CarFilterView(CarJournalFilterViewModel carJournalFilterViewModel) : base(carJournalFilterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ycheckIncludeArchive.Binding.AddBinding(ViewModel, vm => vm.IncludeArchive, w => w.Active).InitializeFromSource();
		}
	}
}
