using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic.MileagesWriteOff;
namespace Vodovoz.Filters.GtkViews
{
	public partial class MileageWriteOffReasonJournalFilterView : FilterViewBase<MileageWriteOffReasonJournalFilterViewModel>
	{
		public MileageWriteOffReasonJournalFilterView(MileageWriteOffReasonJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ycheckbuttonIsShowArchived.Binding
				.AddBinding(ViewModel, vm => vm.IsShowArchived, w => w.Active)
				.InitializeFromSource();
		}
	}
}
