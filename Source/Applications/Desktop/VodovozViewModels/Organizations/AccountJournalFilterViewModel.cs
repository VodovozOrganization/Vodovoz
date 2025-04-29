using QS.Project.Filter;

namespace Vodovoz.ViewModels.Organizations
{
	public class AccountJournalFilterViewModel : FilterViewModelBase<AccountJournalFilterViewModel>
	{
		private int? _restrictOrganizationId;
		private int? _restrictCounterpartyId;

		public int? RestrictOrganizationId
		{
			get => _restrictOrganizationId;
			set => UpdateFilterField(ref _restrictOrganizationId, value);
		}

		public int? RestrictCounterpartyId
		{
			get => _restrictCounterpartyId;
			set => UpdateFilterField(ref _restrictCounterpartyId, value);
		}
	}
}
