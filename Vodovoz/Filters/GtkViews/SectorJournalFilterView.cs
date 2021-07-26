using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sectors;
using Vodovoz.Journals.FilterViewModels;

namespace Vodovoz.Filters.GtkViews
{
	public partial class SectorJournalFilterView : FilterViewBase<SectorJournalFilterViewModel>
	{
		public SectorJournalFilterView(SectorJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			cmbDistrictsSetStatus.ShowSpecialStateAll = true;
			cmbDistrictsSetStatus.ItemsEnum = typeof(SectorsSetStatus);
			cmbDistrictsSetStatus.Binding.AddBinding(ViewModel, vm => vm.Status, w => w.SelectedItemOrNull).InitializeFromSource();

			ycheckOnlyWithBorders.Binding.AddBinding(ViewModel, vm => vm.OnlyWithBorders, w => w.Active).InitializeFromSource();
		}
	}
}
