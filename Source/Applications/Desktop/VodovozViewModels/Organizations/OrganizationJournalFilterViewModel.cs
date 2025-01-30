using QS.Project.Filter;

namespace Vodovoz.ViewModels.Organizations
{
	public class OrganizationJournalFilterViewModel : FilterViewModelBase<OrganizationJournalFilterViewModel>
	{
		private bool _isAvangardShop;

		public bool IsAvangardShop
		{
			get => _isAvangardShop;
			set => UpdateFilterField(ref _isAvangardShop, value);
		}
	}
}
