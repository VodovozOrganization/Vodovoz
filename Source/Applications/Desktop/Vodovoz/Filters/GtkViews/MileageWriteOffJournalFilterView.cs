using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic.MileagesWriteOff;
namespace Vodovoz.Filters.GtkViews
{
	public partial class MileageWriteOffJournalFilterView : FilterViewBase<MileageWriteOffJournalFilterViewModel>
	{
		public MileageWriteOffJournalFilterView(MileageWriteOffJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			daterangepickerPeriod.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.WriteOffDateFrom, w => w.StartDateOrNull)
				.AddBinding(vm => vm.WriteOffDateTo, w => w.EndDateOrNull)
				.InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;
			entityentryDriver.ViewModel = ViewModel.DriverEntryViewModel;
			entityentryAuthor.ViewModel = ViewModel.AuthorEntryViewModel;
		}
	}
}
