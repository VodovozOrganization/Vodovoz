using QS.Project.Filter;

namespace Vodovoz.Presentation.ViewModels.Organisations.Journals
{
	public class FundsFilterViewModel : FilterViewModelBase<FundsFilterViewModel>
	{
		private bool _showArchived;

		public bool ShowArchived
		{
			get => _showArchived;
			set => UpdateFilterField(ref _showArchived, value);
		}
	}
}
