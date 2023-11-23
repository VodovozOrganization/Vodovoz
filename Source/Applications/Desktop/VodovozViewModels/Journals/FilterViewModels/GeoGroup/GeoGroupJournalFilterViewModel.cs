using QS.Project.Filter;
using System;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.GeoGroup
{
	public class GeoGroupJournalFilterViewModel : FilterViewModelBase<GeoGroupJournalFilterViewModel>
	{
		private bool? _isArchive;

		public GeoGroupJournalFilterViewModel(Action<GeoGroupJournalFilterViewModel> filterParams = null)
		{
			if(filterParams != null)
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}

		#region Properties
		public bool? IsArchive
		{
			get => _isArchive;
			set => UpdateFilterField(ref _isArchive, value);
		}
		#endregion Properties
	}
}
