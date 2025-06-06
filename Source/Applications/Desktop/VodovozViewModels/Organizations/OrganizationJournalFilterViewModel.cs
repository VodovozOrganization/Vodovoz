using QS.Project.Filter;

namespace Vodovoz.ViewModels.Organizations
{
	public class OrganizationJournalFilterViewModel : FilterViewModelBase<OrganizationJournalFilterViewModel>
	{
		private bool _hasAvangardShopId;
		private bool _hasCashBoxId;
		private bool _hasTaxcomEdoAccountId;


		public bool HasAvangardShopId
		{
			get => _hasAvangardShopId;
			set => UpdateFilterField(ref _hasAvangardShopId, value);
		}

		public bool HasCashBoxId
		{
			get => _hasCashBoxId;
			set => UpdateFilterField(ref _hasCashBoxId, value);
		}

		public bool HasTaxcomEdoAccountId
		{
			get => _hasTaxcomEdoAccountId;
			set => UpdateFilterField(ref _hasTaxcomEdoAccountId, value);
		}
	}
}
