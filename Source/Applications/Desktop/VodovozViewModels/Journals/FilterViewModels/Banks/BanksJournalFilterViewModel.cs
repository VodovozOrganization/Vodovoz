using System.Collections.Generic;
using QS.Banks.Domain;
using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Banks
{
	public class BanksJournalFilterViewModel : FilterViewModelBase<BanksJournalFilterViewModel>
	{
		private Account _account;
		
		public IEnumerable<int> ExcludeBanksIds { get; set; }

		public Account Account
		{
			get => _account;
			set => UpdateFilterField(ref _account, value);
		}
	}
}
