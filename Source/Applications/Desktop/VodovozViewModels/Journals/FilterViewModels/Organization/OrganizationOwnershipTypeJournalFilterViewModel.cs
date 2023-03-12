using QS.Project.Filter;
using System.IO.Compression;

namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
	public class OrganizationOwnershipTypeJournalFilterViewModel : FilterViewModelBase<OrganizationOwnershipTypeJournalFilterViewModel>
	{
		bool _isArchive;
		public bool IsArchive
		{
			get => _isArchive;
			set => UpdateFilterField(ref _isArchive, value);
		}
	}
}
