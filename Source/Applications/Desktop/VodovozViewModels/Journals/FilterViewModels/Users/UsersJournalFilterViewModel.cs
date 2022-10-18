using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Users
{
	public class UsersJournalFilterViewModel : FilterViewModelBase<UsersJournalFilterViewModel>
	{
		private bool _showDeactivatedUsers;

		public bool ShowDeactivatedUsers
		{
			get => _showDeactivatedUsers;
			set => UpdateFilterField(ref _showDeactivatedUsers, value);
		}
	}
}
