using QS.Project.Filter;

namespace Vodovoz.ViewModels.Logistic.MileagesWriteOff
{
	public class MileageWriteOffReasonJournalFilterViewModel : FilterViewModelBase<MileageWriteOffReasonJournalFilterViewModel>
	{
		private bool _isShowArchived;

		public bool IsShowArchived
		{
			get => _isShowArchived;
			set => UpdateFilterField(ref _isShowArchived, value);
		}
	}
}
