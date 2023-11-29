using QS.Project.Filter;
using System;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.GeoGroup
{
	public class GeoGroupJournalFilterViewModel : FilterViewModelBase<GeoGroupJournalFilterViewModel>
	{
		private bool _isShowArchived;

		public GeoGroupJournalFilterViewModel(Action<GeoGroupJournalFilterViewModel> filterParams = null)
		{
			if(filterParams != null)
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}

		#region Properties
		public bool IsShowArchived
		{
			get => _isShowArchived;
			set => UpdateFilterField(ref _isShowArchived, value);
		}
		#endregion Properties
	}
}
