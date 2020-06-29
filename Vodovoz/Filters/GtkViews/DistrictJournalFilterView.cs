using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.Journals.FilterViewModels;

namespace Vodovoz.Filters.GtkViews
{
	public partial class DistrictJournalFilterView : FilterViewBase<DistrictJournalFilterViewModel>
	{
		public DistrictJournalFilterView(DistrictJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			cmbDistrictsSetStatus.ShowSpecialStateAll = true;
			cmbDistrictsSetStatus.ItemsEnum = typeof(DistrictsSetStatus);
			cmbDistrictsSetStatus.Binding.AddBinding(ViewModel, vm => vm.Status, w => w.SelectedItemOrNull).InitializeFromSource();

			ycheckOnlyWithBorders.Binding.AddBinding(ViewModel, vm => vm.OnlyWithBorders, w => w.Active).InitializeFromSource();
		}
	}
}
